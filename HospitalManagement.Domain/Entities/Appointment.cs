using HospitalManagement.Domain.BaseTypes;
using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Domain.Entities
{
    public class Appointment:BaseAuditEntity
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public DateTime AppointmentDate { get; set; }

        public AppointmentStatus Status { get; set; }
        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; } = null!;
    }
}