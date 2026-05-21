using Application.DTOs.Shipments.Core;
using Application.Interfaces.Services.Shipments.Core;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace API.Controllers.Shipments
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("ReadPolicy")]
    [Authorize]
    public class ShipmentController : ControllerBase
    {
        private readonly IShipmentQueryService _shipmentQueryService;
        private readonly IShipmentCommandService _shipmentCommandService;
        private readonly IShipmentLifecycleService _shipmentLifecycleService;
        private readonly IShipmentHoldService _shipmentHoldService;
        private readonly IShipmentCancellationService _shipmentCancellationService;
        private readonly IShipmentTrackingService _shipmentTrackingService;
        private readonly IShipmentTimelineService _shipmentTimelineService;

        public ShipmentController(IShipmentQueryService shipmentQueryService,
        IShipmentCommandService shipmentCommandService, IShipmentLifecycleService shipmentLifecycleService,
        IShipmentHoldService shipmentHoldService, IShipmentCancellationService shipmentCancellationService,
        IShipmentTrackingService shipmentTrackingService, IShipmentTimelineService shipmentTimelineService)
        {
            _shipmentQueryService = shipmentQueryService;
            _shipmentCommandService = shipmentCommandService;
            _shipmentLifecycleService = shipmentLifecycleService;
            _shipmentHoldService = shipmentHoldService;
            _shipmentCancellationService = shipmentCancellationService;
            _shipmentTrackingService = shipmentTrackingService;
            _shipmentTimelineService = shipmentTimelineService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll([FromQuery] ShipmentParameters parameters)
        {
            var shipments = await _shipmentQueryService.GetAllAsync(parameters);
            return Ok(shipments);
        }

        [HttpGet("Count")]
        [AllowAnonymous]
        public async Task<IActionResult> Count()
        {
            return Ok(await _shipmentQueryService.CountAsync());
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetAllForCurrentUser([FromQuery] ShipmentParameters parameters)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var shipments = await _shipmentQueryService.GetAllForUserAsync(userId, parameters);
            return Ok(shipments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();
            var shipment = await _shipmentQueryService.GetByIdAsync(id, userId, isPrivileged);
            if (shipment == null)
                return NotFound();

            return Ok(shipment);
        }

        [HttpGet("{id}/timeline")]
        public async Task<IActionResult> GetShipmentTimeline(Guid id, [FromQuery] QueryParameters query)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();
            var result = await _shipmentTimelineService
            .GetShipmentTimelineAsync(id, query, userId, isPrivileged);
            if (!result.Any())
                return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Create([FromBody] CreateShipmentRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var createdShipment = await _shipmentCommandService.CreateAsync(userId, request);
            return CreatedAtAction(nameof(GetById), new { id = createdShipment.Id }, createdShipment);
        }

        [HttpPut("{id}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateShipmentRequest request)
        {
            var updatedShipment = await _shipmentCommandService.UpdateAsync(id, request);
            if (updatedShipment == null)
                return NotFound();

            return Ok(updatedShipment);
        }

        [HttpDelete("{id}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var deleted = await _shipmentCommandService.DeleteAsync(id, userId);
            if (!deleted)
                return NotFound();

            return Ok("shipment deleted successfully");
        }

        [HttpPatch("{id:guid}/confirm-client")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ConfirmClient(Guid id, [FromBody] ChangeShipmentStatusRequest request)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();

            var result = await _shipmentLifecycleService.ConfirmClientAsync(id, userId, isPrivileged, request);

            return result == null ? NotFound() : Ok(result);
        }

        [HttpPatch("{id:guid}/request-booking")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> RequestBooking(Guid id, [FromBody] ChangeShipmentStatusRequest request)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();

            var result = await _shipmentLifecycleService.RequestBookingAsync(id, userId, isPrivileged, request);

            return result == null ? NotFound() : Ok(result);
        }

        [HttpPatch("{id:guid}/confirm-booking")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ConfirmBooking(Guid id, [FromBody] ChangeShipmentStatusRequest request)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();

            var result = await _shipmentLifecycleService.ConfirmBookingAsync(id, userId, isPrivileged, request);

            return result == null ? NotFound() : Ok(result);
        }

        [HttpPatch("{id:guid}/submit-shipping-instructions")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> SubmitShippingInstructions(Guid id, [FromBody] ChangeShipmentStatusRequest request)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();

            var result = await _shipmentLifecycleService.SubmitShippingInstructionsAsync(id, userId, isPrivileged, request);

            return result == null ? NotFound() : Ok(result);
        }

        [HttpPatch("{id:guid}/receive-draft-bl")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ReceiveDraftBl(Guid id, [FromBody] ChangeShipmentStatusRequest request)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();

            var result = await _shipmentLifecycleService.ReceiveDraftBlAsync(id, userId, isPrivileged, request);

            return result == null ? NotFound() : Ok(result);
        }

        [HttpPatch("{id:guid}/approve-draft-bl")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ApproveDraftBl(Guid id, [FromBody] ChangeShipmentStatusRequest request)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();

            var result = await _shipmentLifecycleService.ApproveDraftBlAsync(id, userId, isPrivileged, request);

            return result == null ? NotFound() : Ok(result);
        }

        [HttpPatch("{id:guid}/mark-payment-pending")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> MarkPaymentPending(Guid id, [FromBody] ChangeShipmentStatusRequest request)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();

            var result = await _shipmentLifecycleService.MarkPaymentPendingAsync(id, userId, isPrivileged, request);

            return result == null ? NotFound() : Ok(result);
        }

        [HttpPatch("{id:guid}/confirm-payment")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ConfirmPayment(Guid id, [FromBody] ChangeShipmentStatusRequest request)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();

            var result = await _shipmentLifecycleService.ConfirmPaymentAsync(id, userId, isPrivileged, request);

            return result == null ? NotFound() : Ok(result);
        }

        [HttpPatch("{id:guid}/release-telex")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ReleaseTelex(Guid id, [FromBody] ChangeShipmentStatusRequest request)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();

            var result = await _shipmentLifecycleService.ReleaseTelexAsync(id, userId, isPrivileged, request);

            return result == null ? NotFound() : Ok(result);
        }

        [HttpPatch("{id:guid}/complete-delivery")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CompleteDelivery(Guid id, [FromBody] ChangeShipmentStatusRequest request)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();

            var result = await _shipmentLifecycleService.CompleteDeliveryAsync(id, userId, isPrivileged, request);

            return result == null ? NotFound() : Ok(result);
        }

        [HttpPatch("{id:guid}/close")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Close(Guid id, [FromBody] ChangeShipmentStatusRequest request)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();

            var result = await _shipmentLifecycleService.CloseAsync(id, userId, isPrivileged, request);

            return result == null ? NotFound() : Ok(result);
        }

        [HttpPatch("{id:guid}/put-on-hold")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> PutOnHold(Guid id, [FromBody] ChangeShipmentStatusRequest request)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();

            var result = await _shipmentHoldService.PutOnHoldAsync(id, userId, isPrivileged, request);

            return result == null ? NotFound() : Ok(result);
        }

        [HttpPatch("{id:guid}/resume-from-hold")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ResumeFromHold(Guid id, [FromBody] ChangeShipmentStatusRequest request)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();

            var result = await _shipmentHoldService.ResumeFromHoldAsync(id, userId, isPrivileged, request);

            return result == null ? NotFound() : Ok(result);
        }

        [HttpPatch("{id:guid}/cancellation")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Cancellation(Guid id, [FromBody] ChangeShipmentStatusRequest request)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();

            var result = await _shipmentCancellationService.CancelAsync(id, userId, isPrivileged, request);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPut("{id:guid}/tracking")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateTracking(Guid id, UpdateShipmentTrackingRequest request)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();

            var result = await _shipmentTrackingService.UpdateTrackingAsync(id, userId, isPrivileged, request);
            return result == null ? NotFound() : Ok(result);
        }
        private (string userId, bool isPrivileged) GetCurrentUserContext()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Staff");

            return (userId, isPrivileged);
        }
    }
}