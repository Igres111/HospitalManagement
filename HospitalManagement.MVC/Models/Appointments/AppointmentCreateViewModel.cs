using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalManagement.MVC.Models.Appointments;

public class AppointmentCreateViewModel
{
    [Required(ErrorMessage = "Doctor is required.")]
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "Patient is required.")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "Appointment date is required.")]
    [DataType(DataType.DateTime)]
    public DateTime AppointmentDate { get; set; }

    public IEnumerable<SelectListItem> DoctorOptions { get; set; } = [];
    public IEnumerable<SelectListItem> PatientOptions { get; set; } = [];
}