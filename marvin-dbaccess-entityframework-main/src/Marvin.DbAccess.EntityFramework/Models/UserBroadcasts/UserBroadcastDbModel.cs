using Marvin.DbAccess.EntityFramework.Models.Broadcasts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marvin.DbAccess.EntityFramework.Models.UserBroadcasts;

public class UserBroadcastDbModel : BroadcastBase, IDbEntity<UserBroadcastDbModel>
{
    /// <summary>
    ///     Destiny membership
    /// </summary>
    public long MembershipId { get; set; }

    /// <summary>
    ///     Destiny definition hash to be reported
    /// </summary>
    public long DefinitionHash { get; set; }

    /// <summary>
    ///     This can be used to store any generic data you might ever need in this life, just don't abuse this too much
    ///     <para />
    ///     Example: store completions of activity while reporting raid exotic acquisition ("raidCompletions": 10)
    /// </summary>
    public Dictionary<string, string>? AdditionalData { get; set; }

    public static Action<EntityTypeBuilder<UserBroadcastDbModel>> GetBinder()
    {
        return (builder) =>
        {
            builder.ToTable("user_broadcasts");

            builder.HasKey(x => new
            {
                x.GuildId, 
                x.ClanId, 
                x.Type, 
                x.MembershipId, 
                x.DefinitionHash
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
                .Property(x => x.MembershipId)
                .HasColumnName("membership_id");

            builder
                .Property(x => x.DefinitionHash)
                .HasColumnName("hash");

            builder
                .Property(x => x.AdditionalData)
                .HasColumnName("additional_data")
                .HasColumnType("jsonb");
        };
    }
}