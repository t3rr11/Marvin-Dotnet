using Marvin.DbAccess.EntityFramework.Models.Broadcasts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marvin.DbAccess.EntityFramework.Models.ClanBroadcasts;

public class ClanBroadcastDbModel : BroadcastBase, IDbEntity<ClanBroadcastDbModel>
{
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    
    public static Action<EntityTypeBuilder<ClanBroadcastDbModel>> GetBinder()
    {
        return (builder) =>
        {
            builder.ToTable("clan_broadcasts");

            builder.HasKey(x => new
            {
                x.GuildId, 
                x.ClanId, 
                x.Type
            });
            
            builder
                .Property(x => x.GuildId)
                .HasColumnName("guild_id").HasConversion(
                    x => x.ToString(),
                    x => ulong.Parse(x));

            builder
                .Property(x => x.ClanId)
                .HasColumnName("clan_id");

            builder
                .Property(x => x.Type)
                .HasColumnName("type");

            builder
                .Property(x => x.WasAnnounced)
                .HasColumnName("was_announced");

            builder
                .Property(x => x.Date)
                .HasColumnName("date")
                .HasColumnType("timestamp");

            builder
                .Property(x => x.OldValue)
                .HasColumnName("old_value");
            
            builder
                .Property(x => x.NewValue)
                .HasColumnName("new_value");
        };
    }
}