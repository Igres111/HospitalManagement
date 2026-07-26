using HospitalManagement.Domain.BaseTypes;
using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Domain.Entities
{
    public class User:BaseAuditEntity
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public UserRole Role { get; set; }
    }
}