using HospitalManagement.MVC.Dtos;

namespace HospitalManagement.MVC.Dtos.Requests;

public record UpdateAppointmentRequest(
    int? DoctorId,
    int? PatientId,
    DateTime? AppointmentDate,
    AppointmentStatus? Status);