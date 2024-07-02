﻿using System;

namespace Duracellko.PlanningPoker.Domain;

/// <summary>
/// Event arguments for message received event.
/// </summary>
public class MessageReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageReceivedEventArgs"/> class.
    /// </summary>
    /// <param name="message">The received message.</param>
    public MessageReceivedEventArgs(Message message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    /// <summary>
    /// Gets the received message.
    /// </summary>
    /// <value>The received message.</value>
    public Message Message { get; private set; }
}
