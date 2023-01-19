namespace Marvin.DbAccess.Models.Fireteams;

public class FireteamData
{
    public string GuildId { get; set; }
    public string ChannelId { get; set; }
    public string MessageId { get; set; }
    public string LeaderId { get; set; }
    public List<ulong> MemberIds { get; set; }
    public long ActivityHash { get; set; }
}