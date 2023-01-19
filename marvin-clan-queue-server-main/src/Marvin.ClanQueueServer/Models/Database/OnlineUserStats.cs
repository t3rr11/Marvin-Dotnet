using System.ComponentModel.DataAnnotations;

namespace Marvin.ClanQueueServer.Models.Database;

public class OnlineUserStats
{
    public int Currently { get; set; }
    public int Daily { get; set; }
    public int Halfweek { get; set; }
    public int Weekly { get; set; }
    public int Fortnightly { get; set; }
    public int Monthly { get; set; }
}