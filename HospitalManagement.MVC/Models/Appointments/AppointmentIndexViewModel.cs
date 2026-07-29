using HospitalManagement.MVC.Dtos.Responses;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalManagement.MVC.Models.Appointments
{
    public class AppointmentIndexViewModel
    {
        public required AppointmentFilterViewModel Filter { get; set; }
        public required PagedResponse<AppointmentResponse> Page { get; set; }

        public IEnumerable<SelectListItem> DoctorOptions { get; set; } = [];
        public IEnumerable<SelectListItem> PatientOptions { get; set; } = [];
        public IEnumerable<SelectListItem> StatusOptions { get; set; } = [];
    }
}
