using HospitalManagement.Application.Requests;
using HospitalManagement.Application.Responses;
using HospitalManagement.Application.Services;
using HospitalManagement.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppointmentService _appointmentService;

        public AppointmentsController(
            AppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        /// <summary>
        /// Creates a new appointment.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/appointments
        ///     {
        ///         "doctorId": 3,
        ///         "patientId": 7,
        ///         "appointmentDate": "2026-08-10T10:30:00Z"
        ///     }
        /// </remarks>
        /// <response code="201">Appointment successfully created.</response>
        /// <response code="400">Request validation failed.</response>
        /// <response code="401">Authentication token is missing or invalid.</response>
        /// <response code="404">Doctor or patient was not found.</response>
        /// <response code="409">
        /// Appointment overlaps or receptionist booking limit was reached.
        /// </response>
        [HttpPost]
        [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AppointmentResponse>> Create([FromBody] CreateAppointmentRequest request, CancellationToken cancellationToken)
        {
            var response = await _appointmentService.CreateAsync(request, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, response);
        }

        /// <summary>
        /// Returns a filtered, sorted, and paginated list of appointments.
        /// </summary>
        /// <remarks>
        /// Sample requests:
        ///
        ///     GET /api/appointments
        ///
        ///     GET /api/appointments?doctorId=1002
        ///
        ///     GET /api/appointments?patientId=1003
        ///
        ///     GET /api/appointments?status=Scheduled
        ///
        ///     GET /api/appointments?dateFrom=2026-07-28T00:00:00Z&amp;dateTo=2026-07-31T23:59:59Z
        ///
        ///     GET /api/appointments?sortBy=appointmentDate&amp;sortDescending=true&amp;pageNumber=1&amp;pageSize=10
        ///
        /// Supported sorting fields:
        /// id, appointmentDate, status, createdAt, doctorName, patientName.
        /// </remarks>
        /// <response code="200">Appointments successfully returned.</response>
        /// <response code="400">Filtering, sorting, or pagination parameters are invalid.</response>
        /// <response code="401">Authentication token is missing or invalid.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<AppointmentResponse>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedResponse<AppointmentResponse>>> GetAll([FromQuery] GetAppointmentsRequest request, CancellationToken cancellationToken)
        {
            var response = await _appointmentService.GetAllAsync(request, cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Returns an active appointment by ID.
        /// </summary>
        /// <param name="appointmentId">Appointment ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="200">Appointment successfully returned.</response>
        /// <response code="400">Appointment ID is invalid.</response>
        /// <response code="401">Authentication token is missing or invalid.</response>
        /// <response code="404">Appointment was not found.</response>
        [HttpGet("{appointmentId:int}")]
        [ProducesResponseType(typeof(AppointmentResponse),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AppointmentResponse>> GetById([FromRoute] int appointmentId,CancellationToken cancellationToken)
        {
            var response = await _appointmentService.GetByIdAsync(appointmentId, cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Partially updates an active appointment.
        /// </summary>
        /// <remarks>
        /// Only fields provided with non-null values are updated.
        ///
        /// Sample request:
        ///
        ///     PUT /api/appointments/1005
        ///     {
        ///         "doctorId": 1002,
        ///         "appointmentDate": "2026-07-30T12:00:00Z"
        ///     }
        /// </remarks>
        /// <param name="appointmentId">Appointment ID.</param>
        /// <param name="request">Appointment fields to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="200">Appointment successfully updated.</response>
        /// <response code="400">Request validation failed.</response>
        /// <response code="401">Authentication token is missing or invalid.</response>
        /// <response code="404">Appointment, doctor, or patient was not found.</response>
        /// <response code="409">
        /// Appointment cannot be modified because of its status or because it overlaps.
        /// </response>
        [HttpPut("{appointmentId:int}")]
        [ProducesResponseType(typeof(AppointmentResponse),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AppointmentResponse>> Update([FromRoute] int appointmentId, [FromBody] UpdateAppointmentRequest request, CancellationToken cancellationToken)
        {
            var response = await _appointmentService.UpdateAsync(
                appointmentId,
                request,
                cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Soft-deletes an active appointment.
        /// </summary>
        /// <remarks>
        /// Completed appointments cannot be deleted.
        /// </remarks>
        /// <param name="appointmentId">Appointment ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="204">Appointment successfully deleted.</response>
        /// <response code="400">Appointment ID is invalid.</response>
        /// <response code="401">Authentication token is missing or invalid.</response>
        /// <response code="404">Appointment was not found.</response>
        /// <response code="409">Completed appointment cannot be deleted.</response>
        [HttpDelete("{appointmentId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete([FromRoute] int appointmentId, CancellationToken cancellationToken)
        {
            await _appointmentService.DeleteAsync(appointmentId,cancellationToken);

            return NoContent();
        }
    }
}