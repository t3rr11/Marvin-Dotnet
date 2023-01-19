namespace Marvin.Bot.Models.Leaderboards;

public class LeaderboardSeasonRankEntry
{
    public string DisplayName { get; set; }
    public long MembershipId { get; set; }
    public int SeasonRank { get; set; }
    public int SeasonRankOverflow { get; set; }
}