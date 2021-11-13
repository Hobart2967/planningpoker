﻿namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Result data of reconnect operation.
    /// </summary>
    public class ReconnectTeamResult : TeamResult
    {
        /// <summary>
        /// Gets or sets the last message ID for the member.
        /// </summary>
        /// <value>
        /// The last message ID.
        /// </value>
        public long LastMessageId { get; set; }

        /// <summary>
        /// Gets or sets the last selected estimation by member.
        /// </summary>
        /// <value>
        /// The selected estimation.
        /// </value>
        public Estimation? SelectedEstimation { get; set; }
    }
}
