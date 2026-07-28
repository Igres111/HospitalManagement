using HospitalManagement.MVC.Dtos.Responses;

namespace HospitalManagement.MVC.Models.Patients
{
    public class PatientIndexViewModel
    {
        public required PagedResponse<PatientResponse> Page { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
    }
}