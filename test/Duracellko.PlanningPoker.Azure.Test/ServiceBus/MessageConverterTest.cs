﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Azure.Messaging.ServiceBus;
using Duracellko.PlanningPoker.Azure.ServiceBus;
using Duracellko.PlanningPoker.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Azure.Test.ServiceBus
{
    [TestClass]
    public class MessageConverterTest
    {
        private const string SenderId = "3d1c7636-ae1d-4288-b1e1-0dccc8989722";
        private const string RecipientId = "10243241-802e-4d66-b4fc-55c76c23bcb2";
        private const string TeamName = "My Team";
        private const string Team1Json = "{\"Name\":\"My Team\",\"State\":1,\"AvailableEstimations\":[{\"Value\":0.0},{\"Value\":0.5},{\"Value\":1.0},{\"Value\":2.0},{\"Value\":3.0},{\"Value\":5.0},{\"Value\":8.0},{\"Value\":13.0},{\"Value\":20.0},{\"Value\":40.0},{\"Value\":100.0},{\"Value\":\"Infinity\"},{\"Value\":null}],\"Members\":[{\"Name\":\"Duracellko\",\"MemberType\":2,\"Messages\":[],\"LastMessageId\":3,\"LastActivity\":\"2020-05-24T14:46:48.1509407Z\",\"IsDormant\":false,\"Estimation\":null},{\"Name\":\"Me\",\"MemberType\":1,\"Messages\":[],\"LastMessageId\":2,\"LastActivity\":\"2020-05-24T14:47:40.119354Z\",\"IsDormant\":false,\"Estimation\":{\"Value\":20.0}}],\"EstimationResult\":{\"Duracellko\":null,\"Me\":{\"Value\":20.0}}}";
        private const string Team2Json = "{\"Name\":\"My Team\",\"State\":1,\"AvailableEstimations\":[{\"Value\":0.0},{\"Value\":0.5},{\"Value\":1.0},{\"Value\":2.0},{\"Value\":3.0},{\"Value\":5.0},{\"Value\":8.0},{\"Value\":13.0},{\"Value\":20.0},{\"Value\":40.0},{\"Value\":100.0},{\"Value\":\"Infinity\"},{\"Value\":null}],\"Members\":[{\"Name\":\"Duracellko\",\"MemberType\":2,\"Messages\":[],\"LastMessageId\":9,\"LastActivity\":\"2020-05-24T14:53:07.6381166Z\",\"IsDormant\":false,\"Estimation\":{\"Value\":2.0}},{\"Name\":\"Me\",\"MemberType\":1,\"Messages\":[],\"LastMessageId\":8,\"LastActivity\":\"2020-05-24T14:53:05.8193334Z\",\"IsDormant\":false,\"Estimation\":{\"Value\":5.0}},{\"Name\":\"Test\",\"MemberType\":1,\"Messages\":[{\"Id\":4,\"MessageType\":6,\"MemberName\":\"Duracellko\",\"EstimationResult\":null},{\"Id\":5,\"MessageType\":6,\"MemberName\":\"Me\",\"EstimationResult\":null}],\"LastMessageId\":5,\"LastActivity\":\"2020-05-24T14:52:40.0708949Z\",\"IsDormant\":false,\"Estimation\":null}],\"EstimationResult\":{\"Duracellko\":{\"Value\":2.0},\"Me\":{\"Value\":5.0},\"Test\":null}}";

        [TestMethod]
        public void ConvertToServiceBusMessage_Null_ArgumentNullException()
        {
            var target = new MessageConverter();
            Assert.ThrowsException<ArgumentNullException>(() => target.ConvertToServiceBusMessage(null!));
        }

        [TestMethod]
        public void ConvertToNodeMessage_Null_ArgumentNullException()
        {
            var target = new MessageConverter();
            Assert.ThrowsException<ArgumentNullException>(() => target.ConvertToNodeMessage(null!));
        }

        [TestMethod]
        public void ConvertToServiceBusMessageAndBack_ScrumTeamMessage()
        {
            var scrumTeamMessage = new ScrumTeamMessage(TeamName, MessageType.EstimationStarted);
            var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage)
            {
                SenderNodeId = SenderId,
                Data = scrumTeamMessage
            };

            var result = ConvertToServiceBusMessageAndBack(nodeMessage);
            var resultData = (ScrumTeamMessage)result.Data!;

            Assert.AreEqual(MessageType.EstimationStarted, resultData.MessageType);
            Assert.AreEqual(TeamName, resultData.TeamName);
        }

        [TestMethod]
        public void ConvertToServiceBusMessageAndBack_ScrumTeamMemberMessage()
        {
            var scrumTeamMessage = new ScrumTeamMemberMessage(TeamName, MessageType.MemberJoined)
            {
                MemberType = "Observer",
                MemberName = "Test person",
                SessionId = Guid.NewGuid()
            };
            var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage)
            {
                SenderNodeId = SenderId,
                Data = scrumTeamMessage
            };

            var result = ConvertToServiceBusMessageAndBack(nodeMessage);
            var resultData = (ScrumTeamMemberMessage)result.Data!;

            Assert.AreEqual(MessageType.MemberJoined, resultData.MessageType);
            Assert.AreEqual(TeamName, resultData.TeamName);
            Assert.AreEqual(scrumTeamMessage.MemberType, resultData.MemberType);
            Assert.AreEqual(scrumTeamMessage.MemberName, resultData.MemberName);
            Assert.AreEqual(scrumTeamMessage.SessionId, resultData.SessionId);
        }

        [DataTestMethod]
        [DataRow(8.0)]
        [DataRow(0.5)]
        [DataRow(0.0)]
        [DataRow(null)]
        [DataRow(double.PositiveInfinity)]
        public void ConvertToServiceBusMessageAndBack_ScrumTeamMemberEstimationMessage(double? estimation)
        {
            var scrumTeamMessage = new ScrumTeamMemberEstimationMessage(TeamName, MessageType.MemberEstimated)
            {
                MemberName = "Scrum Master",
                Estimation = estimation
            };
            var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage)
            {
                SenderNodeId = SenderId,
                Data = scrumTeamMessage
            };

            var result = ConvertToServiceBusMessageAndBack(nodeMessage);
            var resultData = (ScrumTeamMemberEstimationMessage)result.Data!;

            Assert.AreEqual(MessageType.MemberEstimated, resultData.MessageType);
            Assert.AreEqual(TeamName, resultData.TeamName);
            Assert.AreEqual(scrumTeamMessage.MemberName, resultData.MemberName);
            Assert.AreEqual(scrumTeamMessage.Estimation, resultData.Estimation);
        }

        [TestMethod]
        public void ConvertToServiceBusMessageAndBack_TeamCreated()
        {
            var nodeMessage = new NodeMessage(NodeMessageType.TeamCreated)
            {
                SenderNodeId = SenderId,
                Data = Encoding.UTF8.GetBytes(Team1Json)
            };

            var result = ConvertToServiceBusMessageAndBack(nodeMessage);

            var resultJson = Encoding.UTF8.GetString((byte[])result.Data!);
            Assert.AreEqual(Team1Json, resultJson);
        }

        [TestMethod]
        public void ConvertToServiceBusMessageAndBack_RequestTeamList()
        {
            var nodeMessage = new NodeMessage(NodeMessageType.RequestTeamList)
            {
                SenderNodeId = SenderId
            };

            var result = ConvertToServiceBusMessageAndBack(nodeMessage);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1011:Closing square brackets should be spaced correctly", Justification = "Nullable array")]
        public void ConvertToServiceBusMessageAndBack_TeamList()
        {
            var teamList = new[] { TeamName, "Test", "Hello, World!" };
            var nodeMessage = new NodeMessage(NodeMessageType.TeamList)
            {
                SenderNodeId = SenderId,
                RecipientNodeId = RecipientId,
                Data = teamList
            };

            var result = ConvertToServiceBusMessageAndBack(nodeMessage);

            CollectionAssert.AreEqual(teamList, (string[]?)result.Data);
        }

        [TestMethod]
        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1011:Closing square brackets should be spaced correctly", Justification = "Nullable array")]
        public void ConvertToServiceBusMessageAndBack_RequestTeams()
        {
            var teamList = new[] { TeamName };
            var nodeMessage = new NodeMessage(NodeMessageType.RequestTeams)
            {
                SenderNodeId = SenderId,
                RecipientNodeId = RecipientId,
                Data = teamList
            };

            var result = ConvertToServiceBusMessageAndBack(nodeMessage);

            CollectionAssert.AreEqual(teamList, (string[]?)result.Data);
        }

        [TestMethod]
        public void ConvertToServiceBusMessageAndBack_InitializeTeam()
        {
            var nodeMessage = new NodeMessage(NodeMessageType.InitializeTeam)
            {
                SenderNodeId = SenderId,
                RecipientNodeId = RecipientId,
                Data = Encoding.UTF8.GetBytes(Team2Json)
            };

            var result = ConvertToServiceBusMessageAndBack(nodeMessage);

            var resultJson = Encoding.UTF8.GetString((byte[])result.Data!);
            Assert.AreEqual(Team2Json, resultJson);
        }

        private static NodeMessage ConvertToServiceBusMessageAndBack(NodeMessage nodeMessage)
        {
            var target = new MessageConverter();
            var serviceBusMessage = target.ConvertToServiceBusMessage(nodeMessage);

            Assert.IsNotNull(serviceBusMessage);

            var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
                serviceBusMessage.Body,
                serviceBusMessage.MessageId,
                subject: serviceBusMessage.Subject,
                contentType: serviceBusMessage.ContentType,
                properties: serviceBusMessage.ApplicationProperties);
            var result = target.ConvertToNodeMessage(serviceBusReceivedMessage);

            Assert.IsNotNull(result);
            Assert.AreNotSame(nodeMessage, result);
            Assert.AreEqual(nodeMessage.MessageType, result.MessageType);
            Assert.AreEqual(nodeMessage.SenderNodeId, result.SenderNodeId);
            Assert.AreEqual(nodeMessage.RecipientNodeId, result.RecipientNodeId);

            if (nodeMessage.Data == null)
            {
                Assert.IsNull(result.Data);
            }
            else
            {
                Assert.IsNotNull(result.Data);
                Assert.AreEqual(nodeMessage.Data.GetType(), result.Data.GetType());
            }

            return result;
        }
    }
}
