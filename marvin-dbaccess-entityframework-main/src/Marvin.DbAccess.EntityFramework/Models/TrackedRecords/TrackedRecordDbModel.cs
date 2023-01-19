using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marvin.DbAccess.EntityFramework.Models.TrackedRecords;

public class TrackedRecordDbModel : IDbEntity<TrackedRecordDbModel>
{
    public uint Hash { get; set; }
    public string DisplayName { get; set; }
    public bool IsTracking { get; set; }
    public bool IsCharacterScoped { get; set; }

    public static Action<EntityTypeBuilder<TrackedRecordDbModel>> GetBinder()
    {
        return (builder) =>
        {
            builder.ToTable("tracked_records");

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

            builder
                .Property(x => x.IsCharacterScoped)
                .HasColumnName("character_scoped");
        };
    }
}