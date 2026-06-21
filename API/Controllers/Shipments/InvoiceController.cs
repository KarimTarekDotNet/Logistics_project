using API.Extensions;
using Application.DTOs.Shipments.Core;
using Application.Interfaces.Services.Shipments.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace API.Controllers.Shipments
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("ReadPolicy")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IInvoicePaymentService _invoicePaymentService;

        public InvoiceController(IInvoiceService invoiceService, IInvoicePaymentService invoicePaymentService)
        {
            _invoiceService = invoiceService;
            _invoicePaymentService = invoicePaymentService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();
            var result = await _invoiceService.GetByIdAsync(id, userId, isPrivileged);
            return result.ToActionResult(this);
        }

        [HttpGet("payments/{invoiceId}")]
        public async Task<IActionResult> GetPaymentsByInvoiceId(Guid invoiceId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Staff");
            var payments = await _invoicePaymentService.GetPaymentsByInvoiceIdAsync(invoiceId, userId, isPrivileged);
            if (!payments.Any()) return NotFound();
            return Ok(payments);
        }

        [HttpGet("shipment/{shipmentId}")]
        public async Task<IActionResult> GetByShipmentId(Guid shipmentId)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();
            var result = await _invoiceService.GetByShipmentIdAsync(shipmentId, userId, isPrivileged);
            return result.ToActionResult(this);
        }

        [HttpPost("{shipmentId}")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> Create(Guid shipmentId)
        {
            var result = await _invoiceService.CreateOrUpdateDraftInvoiceAsync(shipmentId, getCurrentUser());
            return result.ToCreatedResult(this, nameof(GetById), new { id = result.Value?.Id });
        }

        [HttpPatch("{id}/cancel")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> Cancel(Guid id, CancelInvoiceRequest request)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();
            var result = await _invoiceService.CancelAsync(id, userId, isPrivileged, request.Reason);
            return result.ToActionResult(this);
        }

        [HttpPatch("{id}/confirm")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> Confirm(Guid id)
        {
            var result = await _invoiceService.ConfirmAsync(id, getCurrentUser());
            return result.ToActionResult(this);
        }

        [HttpPatch("{id}/mark-as-paid")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> MarkAsPaid(Guid id, [FromBody] CreateInvoicePaymentRequest request)
        {
            var paidInvoice = await _invoicePaymentService.MarkAsPaidAsync(id, request);
            if (paidInvoice == null) return NotFound();
            return Ok(paidInvoice);
        }

        [HttpPatch("{id}/mark-as-partially-paid")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> MarkAsPartiallyPaid(Guid id, [FromBody] CreateInvoicePaymentRequest request)
        {
            var paidInvoice = await _invoicePaymentService.MarkAsPartiallyPaidAsync(id, request);
            if (paidInvoice == null) return NotFound();
            return Ok(paidInvoice);
        }

        [HttpPatch("{id}/mark-as-refunded")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> MarkAsRefunded(Guid id)
        {
            var paidInvoice = await _invoicePaymentService.MarkAsRefundedAsync(id);
            if (paidInvoice == null) return NotFound();
            return Ok(paidInvoice);
        }

        [HttpDelete("{id}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _invoiceService.DeleteAsync(id, getCurrentUser());
            if (result.IsSuccess) return Ok(new { message = "Invoice deleted successfully." });
            return result.ToActionResult(this);
        }

        private string getCurrentUser() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User not found");

        private (string userId, bool isPrivileged) GetCurrentUserContext()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Staff");
            return (userId, isPrivileged);
        }
    }
}
