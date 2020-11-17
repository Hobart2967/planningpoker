﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Duracellko.PlanningPoker.Azure.Configuration;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Logging;

namespace Duracellko.PlanningPoker.Azure.ServiceBus
{
    /// <summary>
    /// Sends and receives messages from Azure service bus.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Destructor is placed together with Dispose.")]
    public class AzureServiceBus : IServiceBus, IDisposable
    {
        private const string DefaultTopicName = "PlanningPoker";
        private const string SubscriptionPingPropertyName = "SubscriptionPing";

        private static readonly TimeSpan _serviceBusTokenTimeOut = TimeSpan.FromMinutes(1);

        private readonly Subject<NodeMessage> _observableMessages = new Subject<NodeMessage>();
        private readonly ConcurrentDictionary<string, DateTime> _nodes = new ConcurrentDictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        private readonly ILogger<AzureServiceBus> _logger;

        private volatile string _nodeId;
        private string _connectionString;
        private string _topicName;
        private TopicClient _topicClient;
        private SubscriptionClient _subscriptionClient;
        private System.Timers.Timer _subscriptionsMaintenanceTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureServiceBus"/> class.
        /// </summary>
        /// <param name="messageConverter">The message converter.</param>
        /// <param name="configuration">The configuration of planning poker for Azure platform.</param>
        /// <param name="logger">Logger instance to log events.</param>
        public AzureServiceBus(IMessageConverter messageConverter, IAzurePlanningPokerConfiguration configuration, ILogger<AzureServiceBus> logger)
        {
            MessageConverter = messageConverter ?? throw new ArgumentNullException(nameof(messageConverter));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets a message converter to convert messages from NodeMessage to BrokeredMessage and vice versa.
        /// </summary>
        public IMessageConverter MessageConverter { get; private set; }

        /// <summary>
        /// Gets a configuration of planning poker for Azure platform.
        /// </summary>
        public IAzurePlanningPokerConfiguration Configuration { get; private set; }

        /// <summary>
        /// Gets an observable object receiving messages from service bus.
        /// </summary>
        public IObservable<NodeMessage> ObservableMessages
        {
            get
            {
                return _observableMessages;
            }
        }

        /// <summary>
        /// Sends a message to service bus.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Log error.")]
        public async Task SendMessage(NodeMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var topicClient = _topicClient;
            if (topicClient == null)
            {
                throw new InvalidOperationException("AzureServiceBus is not initialized.");
            }

            var topicMessage = MessageConverter.ConvertToBrokeredMessage(message);

            try
            {
                await topicClient.SendAsync(topicMessage);
                _logger.LogDebug(Resources.AzureServiceBus_Debug_SendMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Resources.AzureServiceBus_Error_SendMessage);
            }
        }

        /// <summary>
        /// Register for receiving messages from other nodes.
        /// </summary>
        /// <param name="nodeId">Current node ID.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Register(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
            {
                throw new ArgumentNullException(nameof(nodeId));
            }

            _nodeId = nodeId;
            _connectionString = Configuration.ServiceBusConnectionString;
            _topicName = Configuration.ServiceBusTopic;
            if (string.IsNullOrEmpty(_topicName))
            {
                _topicName = DefaultTopicName;
            }

            await CreateSubscription();
            _topicClient = new TopicClient(_connectionString, _topicName);

            await SendSubscriptionIsAliveMessage();
            _subscriptionsMaintenanceTimer = new System.Timers.Timer(Configuration.SubscriptionMaintenanceInterval.TotalMilliseconds);
            _subscriptionsMaintenanceTimer.Elapsed += SubscriptionsMaintenanceTimerOnElapsed;
            _subscriptionsMaintenanceTimer.Start();
        }

        /// <summary>
        /// Stop receiving messages from other nodes.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Unregister()
        {
            if (_subscriptionsMaintenanceTimer != null)
            {
                _subscriptionsMaintenanceTimer.Dispose();
                _subscriptionsMaintenanceTimer = null;
            }

            if (_subscriptionClient != null)
            {
                await _subscriptionClient.CloseAsync();
                _subscriptionClient = null;
            }

            if (!_observableMessages.IsDisposed)
            {
                _observableMessages.OnCompleted();
                _observableMessages.Dispose();
            }

            if (_topicClient != null)
            {
                await _topicClient.CloseAsync();
                _topicClient = null;
            }

            await DeleteSubscription();
        }

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all unmanaged and optionally managed resources.
        /// </summary>
        /// <param name="disposing"><c>True</c> if disposing not using GC; otherwise <c>false</c>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Unregister().Wait();
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AzureServiceBus"/> class.
        /// </summary>
        ~AzureServiceBus()
        {
            Dispose(false);
        }

        private async Task CreateSubscription()
        {
            if (_subscriptionClient == null)
            {
                var managementClient = new ManagementClient(_connectionString);
                var subscriptionDescription = new SubscriptionDescription(_topicName, _nodeId)
                {
                    DefaultMessageTimeToLive = _serviceBusTokenTimeOut
                };
                await managementClient.CreateSubscriptionAsync(subscriptionDescription);

                _subscriptionClient = new SubscriptionClient(_connectionString, _topicName, _nodeId);
                _logger.LogDebug(Resources.AzureServiceBus_Debug_SubscriptionCreated, _topicName, _nodeId);

                string sqlPattern = "{0} <> '{2}' AND ({1} IS NULL OR {1} = '{2}')";
                string senderIdPropertyName = ServiceBus.MessageConverter.SenderIdPropertyName;
                string recipientIdPropertyName = ServiceBus.MessageConverter.RecipientIdPropertyName;
                var filter = new SqlFilter(string.Format(CultureInfo.InvariantCulture, sqlPattern, senderIdPropertyName, recipientIdPropertyName, _nodeId));
                await _subscriptionClient.AddRuleAsync("RecipientFilter", filter);

                _subscriptionClient.RegisterMessageHandler(ReceiveMessage, ex => Task.CompletedTask);
            }
        }

        private async Task DeleteSubscription()
        {
            if (_nodeId != null)
            {
                await DeleteSubscription(_nodeId);
                _logger.LogDebug(Resources.AzureServiceBus_Debug_SubscriptionDeleted, _topicName, _nodeId);
                _nodeId = null;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Log error and try again.")]
        private async Task ReceiveMessage(Message message, CancellationToken cancellationToken)
        {
            if (message != null && _nodeId != null)
            {
                _logger.LogDebug(Resources.AzureServiceBus_Debug_MessageReceived, _topicName, _nodeId, message.MessageId);

                try
                {
                    if (message.UserProperties.ContainsKey(SubscriptionPingPropertyName))
                    {
                        ProcessSubscriptionIsAliveMessage(message);
                    }
                    else
                    {
                        var nodeMessage = MessageConverter.ConvertToNodeMessage(message);
                        _observableMessages.OnNext(nodeMessage);
                    }

                    await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
                    _logger.LogInformation(Resources.AzureServiceBus_Info_MessageProcessed, _topicName, _nodeId, message.MessageId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, Resources.AzureServiceBus_Error_MessageError, _topicName, _nodeId, message.MessageId);
                    await _subscriptionClient.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Log error.")]
        private async void SubscriptionsMaintenanceTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                await SendSubscriptionIsAliveMessage();
                await DeleteInactiveSubscriptions();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Resources.AzureServiceBus_Error_SubscriptionsMaintenance, _nodeId);
            }
        }

        private void ProcessSubscriptionIsAliveMessage(Message message)
        {
            var subscriptionLastActivityTime = (DateTime)message.UserProperties[SubscriptionPingPropertyName];
            var subscriptionId = (string)message.UserProperties[ServiceBus.MessageConverter.SenderIdPropertyName];
            _logger.LogDebug(Resources.AzureServiceBus_Debug_SubscriptionAliveMessageReceived, _topicName, _nodeId, subscriptionId);
            _nodes[subscriptionId] = subscriptionLastActivityTime;
        }

        [SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code", Justification = "topicClient can be null, when constructor fails.")]
        private async Task SendSubscriptionIsAliveMessage()
        {
            var message = new Message();
            message.UserProperties[SubscriptionPingPropertyName] = DateTime.UtcNow;
            message.UserProperties[ServiceBus.MessageConverter.SenderIdPropertyName] = _nodeId;
            TopicClient topicClient = null;
            try
            {
                topicClient = new TopicClient(_connectionString, _topicName);
                await topicClient.SendAsync(message);
                _logger.LogDebug(Resources.AzureServiceBus_Debug_SubscriptionAliveSent, _nodeId);
            }
            finally
            {
                if (topicClient != null)
                {
                    await topicClient.CloseAsync();
                }
            }
        }

        private async Task DeleteInactiveSubscriptions()
        {
            var subscriptions = await GetTopicSubscriptions();
            foreach (var subscription in subscriptions)
            {
                if (!string.Equals(subscription, _nodeId, StringComparison.OrdinalIgnoreCase))
                {
                    // if subscription is new, then assume that it has been created very recently and
                    // this node has not received notification about it yet
                    _nodes.TryAdd(subscription, DateTime.UtcNow);

                    DateTime lastSubscriptionActivity;
                    if (_nodes.TryGetValue(subscription, out lastSubscriptionActivity))
                    {
                        if (lastSubscriptionActivity < DateTime.UtcNow - Configuration.SubscriptionInactivityTimeout)
                        {
                            await DeleteSubscription(subscription);
                            _nodes.TryRemove(subscription, out lastSubscriptionActivity);
                            _logger.LogDebug(Resources.AzureServiceBus_Debug_InactiveSubscriptionDeleted, subscription, _nodeId);
                        }
                    }
                }
            }
        }

        private async Task<IEnumerable<string>> GetTopicSubscriptions()
        {
            var managementClient = new ManagementClient(_connectionString);
            var subscriptions = await managementClient.GetSubscriptionsAsync(_topicName);
            return subscriptions.Select(s => s.SubscriptionName);
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Subscription will be deleted next time.")]
        private async Task DeleteSubscription(string name)
        {
            try
            {
                var managementClient = new ManagementClient(_connectionString);
                var existsSubcription = await managementClient.SubscriptionExistsAsync(_topicName, name);
                if (existsSubcription)
                {
                    await managementClient.DeleteSubscriptionAsync(_topicName, name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(Resources.AzureServiceBus_Warning_SubscriptionDeleteFailed, _topicName, name, ex.Message);
            }
        }
    }
}
