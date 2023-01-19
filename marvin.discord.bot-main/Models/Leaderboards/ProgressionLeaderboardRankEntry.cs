namespace Marvin.Bot.Models.Leaderboards;

public class ProgressionLeaderboardRankEntry
{
    public string DisplayName { get; set; }
    public long MembershipId { get; set; }
    public int Progress { get; set; }
    public int CurrentResetCount { get; set; }
}