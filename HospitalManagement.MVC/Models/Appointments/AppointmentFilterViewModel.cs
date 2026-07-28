using HospitalManagement.MVC.Dtos;

namespace HospitalManagement.MVC.Models.Appointments
{
    public class AppointmentFilterViewModel
    {
        public int? DoctorId { get; set; }
        public int? PatientId { get; set; }
        public AppointmentStatus? Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}