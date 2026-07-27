using HospitalManagement.Application.Interfaces;
using HospitalManagement.Infrastructure.Persistence;

namespace HospitalManagement.Infrastructure.Implementations.BaseRepository;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext Context;

    public Repository(ApplicationDbContext context)
    {
        Context = context;
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await Context.Set<T>().FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task AddAsync(T entity,CancellationToken cancellationToken)
    {
        await Context.Set<T>().AddAsync(entity, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return Context.SaveChangesAsync(cancellationToken);
    }
}