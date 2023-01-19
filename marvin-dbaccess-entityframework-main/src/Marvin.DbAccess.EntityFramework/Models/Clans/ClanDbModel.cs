using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marvin.DbAccess.EntityFramework.Models.Clans;

public class ClanDbModel : IDbEntity<ClanDbModel>
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long ClanId { get; set; }
    
    public string ClanName { get; set; }
    public string ClanCallsign { get; set; }
    public int ClanLevel { get; set; }
    public int MemberCount { get; set; }
    public int MembersOnline { get; set; }
    public bool ForcedScan { get; set; }
    public bool IsTracking { get; set; }
    public DateTime JoinedOn { get; set; }
    public DateTime? LastScan { get; set; }
    public bool IsPatron { get; set; }

    public static Action<EntityTypeBuilder<ClanDbModel>> GetBinder()
    {
        return (builder) =>
        {
            builder.ToTable("clan");

            builder.HasKey(x => x.ClanId);

            builder
                .Property(x => x.ClanId)
                .HasColumnName("clan_id");

            builder
                .Property(x => x.ClanName)
                .HasColumnName("clan_name");

            builder
                .Property(x => x.ClanCallsign)
                .HasColumnName("clan_callsign");

            builder
                .Property(x => x.ClanLevel)
                .HasColumnName("clan_level");

            builder
                .Property(x => x.MemberCount)
                .HasColumnName("member_count");

            builder
                .Property(x => x.MembersOnline)
                .HasColumnName("members_online");

            builder
                .Property(x => x.ForcedScan)
                .HasColumnName("forced_scan");

            builder
                .Property(x => x.IsTracking)
                .HasColumnName("is_tracking");

            builder
                .Property(x => x.JoinedOn)
                .HasColumnName("joined_on")
                .HasColumnType("timestamp");

            builder
                .Property(x => x.LastScan)
                .HasColumnName("last_scan")
                .HasColumnType("timestamp");

            builder
                .Property(x => x.IsPatron)
                .HasColumnName("patreon");
        };
    }
}