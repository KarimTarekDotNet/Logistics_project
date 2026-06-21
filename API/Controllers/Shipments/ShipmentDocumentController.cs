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
    public class ShipmentDocumentController : ControllerBase
    {
        private readonly IShipmentDocumentService shipmentDocumentService;

        public ShipmentDocumentController(IShipmentDocumentService shipmentDocumentService)
        {
            this.shipmentDocumentService = shipmentDocumentService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Staff");

            var document = await shipmentDocumentService.GetByIdAsync(id, userId, isPrivileged);

            if (document == null)
                return NotFound();

            return Ok(document);
        }

        [HttpGet("shipment/{shipmentId}")]
        public async Task<IActionResult> GetByShipmentId(Guid shipmentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Staff");

            var documents = await shipmentDocumentService.GetByShipmentIdAsync(shipmentId, userId, isPrivileged);

            if (!documents.Any())
                return NotFound();

            return Ok(documents);
        }

        [HttpPost("shipment/{shipmentId}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload(Guid shipmentId, [FromForm] UploadShipmentDocumentRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Staff");

            var uploadedDocument = await shipmentDocumentService.UploadAsync(shipmentId, request, userId, isPrivileged);

            return CreatedAtAction(nameof(GetById), new { id = uploadedDocument.Id }, uploadedDocument);
        }

        [HttpDelete("{id}")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Staff");

            await shipmentDocumentService.DeleteAsync(id, userId, isPrivileged);

            return Ok("Shipment document deleted successfully.");
        }
    }
}