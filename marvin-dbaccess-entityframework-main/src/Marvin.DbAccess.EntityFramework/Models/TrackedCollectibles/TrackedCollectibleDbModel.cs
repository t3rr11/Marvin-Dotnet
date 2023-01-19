using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marvin.DbAccess.EntityFramework.Models.TrackedCollectibles;

public class TrackedCollectibleDbModel : IDbEntity<TrackedCollectibleDbModel>
{
    public uint Hash { get; set; }
    public bool IsBroadcasting { get; set; }
    public string? CustomDescription { get; set; }
    public string? Type { get; set; }
    public string? DisplayName { get; set; }
    public string? CustomName { get; set; }

    public static Action<EntityTypeBuilder<TrackedCollectibleDbModel>> GetBinder()
    {
        return (builder) =>
        {
            builder.ToTable("tracked_collectibles");

            builder.HasKey(x => x.Hash);

            builder
                .Property(x => x.Hash)
                .HasColumnName("hash");

            builder
                .Property(x => x.IsBroadcasting)
                .HasColumnName("is_broadcasting");

            builder
                .Property(x => x.CustomDescription)
                .HasColumnName("custom_description");

            builder
                .Property(x => x.Type)
                .HasColumnName("type");

            builder
                .Property(x => x.DisplayName)
                .HasColumnName("display_name");

            builder
                .Property(x => x.CustomName)
                .HasColumnName("custom_name");
        };
    }
}