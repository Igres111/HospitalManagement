namespace HospitalManagement.MVC.Dtos.Requests
{
    public record CreateAppointmentRequest(
        int DoctorId,
        int PatientId,
        DateTime AppointmentDate);
}
