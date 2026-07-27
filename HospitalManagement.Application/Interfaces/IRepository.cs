namespace HospitalManagement.Application.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task AddAsync(T entity, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}