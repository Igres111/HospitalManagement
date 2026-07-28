using HospitalManagement.MVC.Dtos.Responses;

namespace HospitalManagement.MVC.Models.Doctors
{
    public class DoctorIndexViewModel
    {
        public required PagedResponse<DoctorResponse> Page { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
    }
}