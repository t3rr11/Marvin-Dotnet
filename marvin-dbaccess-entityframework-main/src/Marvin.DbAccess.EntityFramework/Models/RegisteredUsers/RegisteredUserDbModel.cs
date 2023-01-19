using DotNetBungieAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marvin.DbAccess.EntityFramework.Models.RegisteredUsers;

public class RegisteredUserDbModel : IDbEntity<RegisteredUserDbModel>
{
    public ulong UserId { get; set; }
    public string Username { get; set; }
    public BungieMembershipType Platform { get; set; }
    public DateTime CreatedAt { get; set; }
    public long MembershipId { get; set; }

    public static Action<EntityTypeBuilder<RegisteredUserDbModel>> GetBinder()
    {
        return (builder) =>
        {
            builder.ToTable("registered_users");

            builder.HasKey(x => x.UserId);

            builder
                .Property(x => x.UserId)
                .HasColumnName("user_id")
                .HasConversion(
                    x => x.ToString(),
                    x => ulong.Parse(x));

            builder
                .Property(x => x.Username)
                .HasColumnName("username");

            builder
                .Property(x => x.Platform)
                .HasColumnName("platform");

            builder
                .Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp");

            builder
                .Property(x => x.MembershipId)
                .HasColumnName("membership_id");
        };
    }
}