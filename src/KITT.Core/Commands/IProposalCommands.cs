﻿namespace KITT.Core.Commands;

public interface IProposalCommands
{
    Task AcceptProposalAsync(Guid proposalId);

    Task RefuseProposalAsync(Guid proposalId);

    Task RejectProposalAsync(Guid proposalId);

    Task ScheduleProposalAsync(
        Guid proposalId,
        string userId,
        string twitchChannel,
        string title,
        string slug,
        DateTime scheduleDate,
        TimeSpan startingTime, TimeSpan endingTime,
        string hostingChannelUrl,
        string streamingAbstract);
}
