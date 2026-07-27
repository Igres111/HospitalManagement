using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Application.Requests
{
    public record GetAppointmentsRequest(
        int? DoctorId,
        int? PatientId,
        AppointmentStatus? Status,
        DateTime? DateFrom,
        DateTime? DateTo,
        string? SortBy,
        bool SortDescending = false,
        int PageNumber = 1,
        int PageSize = 10);
}