using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Application.Requests
{
    public record UpdateAppointmentRequest(
        int? DoctorId,
        int? PatientId,
        DateTime? AppointmentDate,
        AppointmentStatus? Status);
}