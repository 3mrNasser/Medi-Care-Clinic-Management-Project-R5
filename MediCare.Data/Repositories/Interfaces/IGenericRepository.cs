using System.Linq.Expressions;

namespace MediCare.Data.Repositories.Interfaces;

/// <summary>
/// Repository Pattern: basic data access for one entity type. Changes are saved by
/// <see cref="IUnitOfWork.SaveChangesAsync"/>, not by the repository.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IGenericRepository<T> where T : class
{
    /// <summary>The entity with this primary key, or <c>null</c>.</summary>
    Task<T?> GetByIdAsync(int id);

    /// <summary>Every row of the entity.</summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>Rows matching <paramref name="predicate"/>, loading the given navigation properties.</summary>
    /// <param name="predicate">Filter translated to SQL.</param>
    /// <param name="includes">Navigation properties to load, e.g. <c>a =&gt; a.DoctorProfile</c>.</param>
    Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes);

    /// <summary>Adds a new entity (inserted on save).</summary>
    Task AddAsync(T entity);

    /// <summary>Marks an entity as changed (updated on save).</summary>
    void Update(T entity);

    /// <summary>Marks an entity for deletion (deleted on save).</summary>
    void Remove(T entity);
}
