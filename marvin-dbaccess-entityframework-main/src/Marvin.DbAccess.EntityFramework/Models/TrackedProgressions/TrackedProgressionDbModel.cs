using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marvin.DbAccess.EntityFramework.Models.TrackedProgressions;

public class TrackedProgressionDbModel : IDbEntity<TrackedProgressionDbModel>
{
    public uint Hash { get; set; }
    public string? DisplayName { get; set; }
    public bool IsTracking { get; set; }
    
    public static Action<EntityTypeBuilder<TrackedProgressionDbModel>> GetBinder()
    {
        return (builder) =>
        {
            builder.ToTable("tracked_progressions");

            builder.HasKey(x => x.Hash);

            builder
                .Property(x => x.Hash)
                .HasColumnName("hash");

            builder
                .Property(x => x.DisplayName)
                .HasColumnName("display_name");

            builder
                .Property(x => x.IsTracking)
                .HasColumnName("is_tracking");
        };
    }
}