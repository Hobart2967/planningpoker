﻿namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Planning Poker application error codes.
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>
        /// Scrum Team with the specified name does not exist.
        /// </summary>
        public const string ScrumTeamNotExist = nameof(ScrumTeamNotExist);

        /// <summary>
        /// Scrum Team with the specified name cannot be created, because it already exists.
        /// </summary>
        public const string ScrumTeamAlreadyExists = nameof(ScrumTeamAlreadyExists);

        /// <summary>
        /// Member with the specified name cannot be added to the team,
        /// because a member with the same name already exists.
        /// </summary>
        public const string MemberAlreadyExists = nameof(MemberAlreadyExists);
    }
}
