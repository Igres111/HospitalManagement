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
    public class PatientsController : ControllerBase
    {
        private readonly PatientService _patientService;

        public PatientsController(PatientService patientService)
        {
            _patientService = patientService;
        }

        /// <summary>
        /// Creates a new patient.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/patients
        ///     {
        ///         "firstName": "Anna",
        ///         "lastName": "Brown",
        ///         "dateOfBirth": "1995-04-18",
        ///         "phoneNumber": "+995555123456",
        ///         "email": "anna.brown@example.com"
        ///     }
        /// </remarks>
        /// <response code="201">Patient successfully created.</response>
        /// <response code="400">Request validation failed.</response>
        /// <response code="401">Authentication token is missing or invalid.</response>
        /// <response code="409">An active patient with the same email already exists.</response>
        [HttpPost]
        [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<PatientResponse>> Create([FromBody] CreatePatientRequest body, CancellationToken cancellationToken)
        {
            var response = await _patientService.CreateAsync(body, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, response);
        }

        /// <summary>
        /// Returns a searchable, sortable, and paginated list of active patients.
        /// </summary>
        /// <remarks>
        /// Sample requests:
        ///
        ///     GET /api/patients
        ///
        ///     GET /api/patients?search=anna
        ///
        ///     GET /api/patients?pageNumber=2&amp;pageSize=10
        ///
        ///     GET /api/patients?sortBy=lastName&amp;sortDescending=true
        ///
        /// Supported sorting fields:
        /// id, firstName, lastName, dateOfBirth, email, createdAt.
        /// </remarks>
        /// <response code="200">Patients successfully returned.</response>
        /// <response code="400">Pagination, search, or sorting parameters are invalid.</response>
        /// <response code="401">Authentication token is missing or invalid.</response>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(PagedResponse<PatientResponse>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedResponse<PatientResponse>>> GetAll([FromQuery] GetPatientsRequest request,CancellationToken cancellationToken)
        {
            var response = await _patientService.GetAllAsync(request,cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Returns an active patient by ID.
        /// </summary>
        /// <param name="patientId">Patient ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="200">Patient successfully returned.</response>
        /// <response code="401">Authentication token is missing or invalid.</response>
        /// <response code="404">Patient was not found.</response>
        [HttpGet("{patientId:int}")]
        [ProducesResponseType(typeof(PatientResponse),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PatientResponse>> GetById([FromRoute] int patientId, CancellationToken cancellationToken)
        {
            var response = await _patientService.GetByIdAsync(patientId, cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Partially updates an active patient.
        /// </summary>
        /// <remarks>
        /// Only fields provided with non-null values are updated.
        ///
        /// Sample request:
        ///
        ///     PUT /api/patients/10
        ///     {
        ///         "firstName": "Nino",
        ///         "email": "nino@example.com"
        ///     }
        /// </remarks>
        /// <param name="patientId">Patient ID.</param>
        /// <param name="request">Patient fields to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="200">Patient successfully updated.</response>
        /// <response code="400">Request validation failed.</response>
        /// <response code="401">Authentication token is missing or invalid.</response>
        /// <response code="404">Patient was not found.</response>
        /// <response code="409">Another active patient already uses the email.</response>
        [HttpPut("{patientId:int}")]
        [ProducesResponseType(typeof(PatientResponse),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status409Conflict)]
        public async Task<ActionResult<PatientResponse>> Update([FromRoute] int patientId, [FromBody] UpdatePatientRequest request, CancellationToken cancellationToken)
        {
            var response = await _patientService.UpdateAsync(
                patientId,
                request,
                cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Soft-deletes an active patient.
        /// </summary>
        /// <remarks>
        /// A patient with future scheduled appointments cannot be deleted.
        /// Only administrators can delete patients.
        /// </remarks>
        /// <param name="patientId">Patient ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="204">Patient successfully deleted.</response>
        /// <response code="401">Authentication token is missing or invalid.</response>
        /// <response code="403">The authenticated user is not an administrator.</response>
        /// <response code="404">Patient was not found.</response>
        /// <response code="409">Patient has future scheduled appointments.</response>
        [HttpDelete("{patientId:int}")]
        [Authorize(Roles = nameof(UserRole.Administrator))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse),StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete([FromRoute] int patientId,CancellationToken cancellationToken)
        {
            await _patientService.DeleteAsync(patientId, cancellationToken);

            return NoContent();
        }
    }
}