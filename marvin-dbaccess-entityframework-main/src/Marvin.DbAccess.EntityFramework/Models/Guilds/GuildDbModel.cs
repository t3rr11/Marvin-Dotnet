using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marvin.DbAccess.EntityFramework.Models.Guilds;

[Index(nameof(GuildId), IsUnique = true)]
public class GuildDbModel : IDbEntity<GuildDbModel>
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ulong GuildId { get; set; }

    public string GuildName { get; set; }

    public ulong OwnerId { get; set; }

    public string? OwnerAvatar { get; set; }

    public bool IsTracking { get; set; }

    public List<long> Clans { get; set; }

    public DateTime JoinedOn { get; set; }

    public GuildBroadcastsConfigDbModel BroadcastsConfig { get; set; }

    public GuildAnnouncementsConfigDbModel AnnouncementsConfig { get; set; }

    public static Action<EntityTypeBuilder<GuildDbModel>> GetBinder()
    {
        return (builder) =>
        {
            builder.ToTable("guild");

            builder.HasKey(x => x.GuildId);

            builder
                .Property(x => x.GuildId)
                .HasColumnName("guild_id")
                .HasConversion(
                    x => x.ToString(),
                    x => ulong.Parse(x));

            builder
                .Property(x => x.GuildName)
                .HasColumnName("guild_name");

            builder
                .Property(x => x.OwnerId)
                .HasColumnName("owner_id")
                .HasConversion(
                    x => x.ToString(),
                    x => ulong.Parse(x));

            builder
                .Property(x => x.OwnerAvatar)
                .HasColumnName("owner_avatar");

            builder
                .Property(x => x.IsTracking)
                .HasColumnName("is_tracking");

            builder
                .Property(x => x.Clans)
                .HasColumnName("clans")
                .HasColumnType("jsonb");

            builder
                .Property(x => x.JoinedOn)
                .HasColumnName("joined_on")
                .HasColumnType("timestamp");

            builder
                .Property(x => x.BroadcastsConfig)
                .HasColumnName("broadcasts_config").HasColumnType("jsonb");

            builder
                .Property(x => x.AnnouncementsConfig)
                .HasColumnName("announcements_config").HasColumnType("jsonb");
        };
    }
}