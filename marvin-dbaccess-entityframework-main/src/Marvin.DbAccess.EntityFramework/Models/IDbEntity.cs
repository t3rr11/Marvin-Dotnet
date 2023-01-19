using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marvin.DbAccess.EntityFramework.Models;

public interface IDbEntity<T>
    where T : class
{
    static abstract Action<EntityTypeBuilder<T>> GetBinder();
}