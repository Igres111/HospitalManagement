using HospitalManagement.Domain.Entities;

namespace HospitalManagement.Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken);
}