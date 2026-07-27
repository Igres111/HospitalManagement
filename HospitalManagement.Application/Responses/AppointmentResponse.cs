using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Application.Responses
{
    public record AppointmentResponse(
            int Id,
            int DoctorId,
            string DoctorName,
            string DoctorLastName,
            int PatientId,
            string PatientName,
            string PatientLastName,
            DateTime AppointmentDate,
            AppointmentStatus Status,
            int CreatedByUserId,
            DateTime CreatedAt);
}