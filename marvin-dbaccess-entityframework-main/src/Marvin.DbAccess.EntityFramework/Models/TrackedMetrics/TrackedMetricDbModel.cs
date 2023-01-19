using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marvin.DbAccess.EntityFramework.Models.TrackedMetrics;

public class TrackedMetricDbModel : IDbEntity<TrackedMetricDbModel>
{
    public uint Hash { get; set; }
    public string? DisplayName { get; set; }
    public bool IsTracking { get; set; }

    public static Action<EntityTypeBuilder<TrackedMetricDbModel>> GetBinder()
    {
        return (builder) =>
        {
            builder.ToTable("tracked_metrics");

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