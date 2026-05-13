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
        private readonly IInvoiceService invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            this.invoiceService = invoiceService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Staff");

            var invoice = await invoiceService.GetByIdAsync(id, userId, isPrivileged);
            if (invoice == null)
                return NotFound();

            return Ok(invoice);
        }

        [HttpGet("shipment/{shipmentId}")]
        public async Task<IActionResult> GetByShipmentId(Guid shipmentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Staff");

            var invoices = await invoiceService.GetByShipmentIdAsync(shipmentId, userId, isPrivileged);
            if (!invoices.Any())
                return NotFound();

            return Ok(invoices);
        }

        [HttpPost]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create(CreateInvoiceRequest request)
        {
            var createdInvoice = await invoiceService.CreateAsync(request);

            return CreatedAtAction(nameof(GetById), new { id = createdInvoice.Id }, createdInvoice);
        }

        [HttpPatch("{id}/cancel")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> Cancel(Guid id, CancelInvoiceRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Staff");

            var cancelledInvoice = await invoiceService.CancelAsync(id, userId, isPrivileged, request.Reason);
            if (cancelledInvoice == null)
                return NotFound();

            return Ok(cancelledInvoice);
        }

        [HttpPatch("{id}/mark-as-paid")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> MarkAsPaid(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Staff");

            var paidInvoice = await invoiceService.MarkAsPaidAsync(id, userId, isPrivileged);
            if (paidInvoice == null)
                return NotFound();

            return Ok(paidInvoice);
        }

        [HttpPatch("{id}/mark-as-partially-paid")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> MarkAsPartiallyPaid(Guid id)
        {
            var paidInvoice = await invoiceService.MarkAsPartiallyPaidAsync(id);
            if (paidInvoice == null)
                return NotFound();

            return Ok(paidInvoice);
        }

        [HttpPatch("{id}/mark-as-refunded")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> MarkAsRefunded(Guid id)
        {
            var paidInvoice = await invoiceService.MarkAsRefundedAsync(id);
            if (paidInvoice == null)
                return NotFound();

            return Ok(paidInvoice);
        }

        [HttpDelete("{id}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await invoiceService.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return Ok("Invoice deleted successfully.");
        }
    }
}