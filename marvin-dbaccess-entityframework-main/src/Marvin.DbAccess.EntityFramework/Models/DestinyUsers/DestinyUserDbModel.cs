using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marvin.DbAccess.EntityFramework.Models.DestinyUsers;

public class DestinyUserDbModel : IDbEntity<DestinyUserDbModel>
{
    public long MembershipId { get; set; }
    public long? ClanId { get; set; }
    public string DisplayName { get; set; }
    public int TimePlayed { get; set; }
    public DateTime ClanJoinDate { get; set; }
    public DateTime LastPlayed { get; set; }
    public DateTime LastUpdated { get; set; }
    public bool IsPrivate { get; set; }
    public uint? CurrentActivity { get; set; }
    public DateTime? DateActivityStarted { get; set; }
    public Dictionary<uint, DestinyUserMetricDbModel> Metrics { get; set; }
    public Dictionary<uint, DestinyUserRecordDbModel> Records { get; set; }
    public Dictionary<uint, DestinyUserProgressionDbModel> Progressions { get; set; }
    public List<uint> Items { get; set; }
    public List<uint> RecentItems { get; set; }
    public DestinyUserComputedDataDbModel ComputedData { get; set; }

    public static Action<EntityTypeBuilder<DestinyUserDbModel>> GetBinder()
    {
        return (builder) =>
        {
            builder.ToTable("destiny_user");

            builder.HasKey(x => x.MembershipId);

            builder
                .Property(x => x.MembershipId)
                .HasColumnName("membership_id");

            builder
                .Property(x => x.ClanId)
                .HasColumnName("clan_id");

            builder
                .Property(x => x.DisplayName)
                .HasColumnName("display_name");

            builder
                .Property(x => x.TimePlayed)
                .HasColumnName("time_played");

            builder
                .Property(x => x.ClanJoinDate)
                .HasColumnName("clan_join_date")
                .HasColumnType("timestamp");

            builder
                .Property(x => x.LastPlayed)
                .HasColumnName("last_played")
                .HasColumnType("timestamp");

            builder
                .Property(x => x.LastUpdated)
                .HasColumnName("last_updated")
                .HasColumnType("timestamp");

            builder
                .Property(x => x.IsPrivate)
                .HasColumnName("private");

            builder
                .Property(x => x.CurrentActivity)
                .HasColumnName("current_activity")
                .HasConversion(
                    x => x.ToString(),
                    x => string.IsNullOrEmpty(x) == true ? null : uint.Parse(x));

            builder
                .Property(x => x.DateActivityStarted)
                .HasColumnName("date_activity_started")
                .HasColumnType("timestamp");

            builder
                .Property(x => x.Metrics)
                .HasColumnName("metrics")
                .HasColumnType("jsonb");

            builder
                .Property(x => x.Records)
                .HasColumnName("records")
                .HasColumnType("jsonb");

            builder
                .Property(x => x.Progressions)
                .HasColumnName("progressions")
                .HasColumnType("jsonb");

            builder
                .Property(x => x.Items)
                .HasColumnName("items")
                .HasColumnType("jsonb");

            builder
                .Property(x => x.RecentItems)
                .HasColumnName("recent_items")
                .HasColumnType("jsonb");

            builder
                .Property(x => x.ComputedData)
                .HasColumnName("computed_data")
                .HasColumnType("jsonb");
        };
    }
}