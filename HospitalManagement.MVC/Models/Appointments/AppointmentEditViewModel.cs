using HospitalManagement.MVC.Dtos;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalManagement.MVC.Models.Appointments
{
    public class AppointmentEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Doctor is required.")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Patient is required.")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Appointment date is required.")]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public AppointmentStatus Status { get; set; }

        public IEnumerable<SelectListItem> DoctorOptions { get; set; } = [];
        public IEnumerable<SelectListItem> PatientOptions { get; set; } = [];
        public IEnumerable<SelectListItem> StatusOptions { get; set; } = [];
    }
}
