using HospitalManagement.Domain.BaseTypes;

namespace HospitalManagement.Domain.Entities
{
    public class Patient:BaseAuditEntity
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public DateOnly DateOfBirth { get; set; }

        public string? PhoneNumber { get; set; }

        public string Email { get; set; } = null!;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}