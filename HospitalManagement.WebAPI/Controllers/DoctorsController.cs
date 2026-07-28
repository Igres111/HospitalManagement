using HospitalManagement.Application.Requests;
using HospitalManagement.Application.Responses;
using HospitalManagement.Application.Services;
using HospitalManagement.Domain.Enums;
using HospitalManagement.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly DoctorService _doctorService;

        public DoctorsController(DoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        /// <summary>
        /// Creates a new doctor.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/doctors
        ///     {
        ///         "firstName": "John",
        ///         "lastName": "Smith",
        ///         "specialization": "Cardiology",
        ///         "phoneNumber": "+995555123456",
        ///         "email": "john.smith@hospital.com"
        ///     }
        /// </remarks>
        /// <response code="201">Doctor successfully created.</response>
        /// <response code="400">Request validation failed.</response>
        /// <response code="401">Authentication token is missing or invalid.</response>
        [HttpPost]
        [ProducesResponseType(typeof(DoctorResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<DoctorResponse>> Create([FromBody] CreateDoctorRequest body, CancellationToken cancellationToken)
        {
            var response = await _doctorService.CreateAsync(body, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, response);
        }

        /// <summary>
        /// Returns a searchable, sortable and paginated list of active doctors.
        /// </summary>
        /// <remarks>
        /// Sample requests:
        ///
        ///     GET /api/doctors
        ///
        ///     GET /api/doctors?search=john
        ///
        ///     GET /api/doctors?pageNumber=2&amp;pageSize=10
        ///
        ///     GET /api/doctors?sortBy=lastName&amp;sortDescending=true
        ///
        ///     GET /api/doctors?search=smith&amp;sortBy=firstName&amp;pageNumber=1&amp;pageSize=5
        ///
        /// Supported sorting fields:
        /// id, firstName, lastName, specialization, email and createdAt.
        /// </remarks>
        /// <response code="200">Doctors successfully returned.</response>
        /// <response code="400">Pagination, search or sorting parameters are invalid.</response>
        /// <response code="401">Authentication token is missing or invalid.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<DoctorResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedResponse<DoctorResponse>>> GetAll([FromQuery] GetDoctorsRequest request, CancellationToken cancellationToken)
        {
            var response = await _doctorService.GetAllAsync(
                request,
                cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Returns an active doctor by ID.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/doctors/4
        /// </remarks>
        /// <response code="200">Doctor successfully returned.</response>
        /// <response code="400">Doctor ID is invalid.</response>
        /// <response code="401">Authentication token is missing or invalid.</response>
        /// <response code="404">Doctor was not found.</response>
        [HttpGet("{doctorId:int}")]
        [ProducesResponseType(typeof(DoctorResponse),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DoctorResponse>> GetById([FromRoute] int doctorId,CancellationToken cancellationToken)
        {
            var response = await _doctorService.GetByIdAsync(doctorId,cancellationToken);

            return Ok(response);
        }

        [HttpPut("{doctorId:int}")]
        [ProducesResponseType(typeof(DoctorResponse),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status409Conflict)]
        public async Task<ActionResult<DoctorResponse>> Update([FromRoute] int doctorId, [FromBody] UpdateDoctorRequest body, CancellationToken cancellationToken)
        {
            var response = await _doctorService.UpdateAsync(
                doctorId,
                body,
                cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Soft-deletes an active doctor.
        /// </summary>
        /// <remarks>
        /// A doctor cannot be deleted while they have future scheduled appointments.
        ///
        /// Only administrators can perform this operation.
        ///
        /// Sample request:
        ///
        ///     DELETE /api/doctors/4
        /// </remarks>
        /// <response code="204">Doctor successfully deleted.</response>
        /// <response code="400">Doctor ID is invalid.</response>
        /// <response code="401">Authentication token is missing or invalid.</response>
        /// <response code="403">The user is not an administrator.</response>
        /// <response code="404">Doctor was not found.</response>
        /// <response code="409">Doctor has future appointments.</response>
        [HttpDelete("{doctorId:int}")]
        [Authorize(Roles = nameof(UserRole.Administrator))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete([FromRoute] int doctorId,CancellationToken cancellationToken)
        {
            await _doctorService.DeleteAsync(doctorId,cancellationToken);

            return NoContent();
        }
    }
}