namespace HospitalManagement.Application.Requests
{
    public record CreateAppointmentRequest(
           int DoctorId,
           int PatientId,
           DateTime AppointmentDate);
}