using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marvin.DbAccess.EntityFramework.Models.ClansToScan;

public class ClanToScanDbModel : IDbEntity<ClanToScanDbModel>
{
    public long ClanId { get; set; }
    public ulong GuildId { get; set; }
    public ulong? ChannelId { get; set; }

    public static Action<EntityTypeBuilder<ClanToScanDbModel>> GetBinder()
    {
        return (builder) =>
        {
            builder.ToTable("clans_to_scan");

            builder.HasKey(x => new { x.GuildId, x.ClanId });

            builder
                .Property(x => x.GuildId)
                .HasColumnName("guild_id")
                .HasConversion(
                    x => x.ToString(),
                    x => ulong.Parse(x));

            builder.Property(x => x.ClanId)
                .HasColumnName("clan_id");

            builder.Property(x => x.ChannelId)
                .HasColumnName("channel_id")
                .HasConversion(x => x.ToString(),
                    x => string.IsNullOrEmpty(x) == true ? null : ulong.Parse(x));
        };
    }
}