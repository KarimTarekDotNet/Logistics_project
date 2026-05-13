using Application.DTOs.Shipments.Core;
using Domain.Entities.Shipments;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.ApplicationRules.Shipments
{
    public static class ShipmentTrackingRules
    {
        public static void ApplyTrackingUpdate(Shipment shipment, UpdateShipmentTrackingRequest request)
        {

            if (!CanUpdateTracking(shipment.Status))
                throw new BusinessRuleException($"Tracking cannot be updated when shipment status is {shipment.Status.ToString()}.");

            if (request.EstimatedDeparture.HasValue && request.EstimatedArrival.HasValue &&
                request.EstimatedArrival.Value < request.EstimatedDeparture.Value)
            {
                throw new BusinessRuleException("Estimated arrival cannot be before estimated departure.");
            }

            if (request.ActualDeparture.HasValue && request.ActualArrival.HasValue &&
                request.ActualArrival.Value < request.ActualDeparture.Value)
            {
                throw new BusinessRuleException("Actual arrival cannot be before actual departure.");
            }

            if (!string.IsNullOrWhiteSpace(request.BookingNumber))
                shipment.BookingNumber = request.BookingNumber.Trim();

            if (!string.IsNullOrWhiteSpace(request.VesselName))
                shipment.VesselName = request.VesselName.Trim();

            if (!string.IsNullOrWhiteSpace(request.VoyageNumber))
                shipment.VoyageNumber = request.VoyageNumber.Trim();

            if (!string.IsNullOrWhiteSpace(request.CurrentCheckpoint))
                shipment.CurrentCheckpoint = request.CurrentCheckpoint.Trim();

            if (request.EstimatedDeparture.HasValue)
                shipment.EstimatedDeparture = request.EstimatedDeparture.Value;

            if (request.EstimatedArrival.HasValue)
                shipment.EstimatedArrival = request.EstimatedArrival.Value;

            if (request.ActualDeparture.HasValue)
                shipment.ActualDeparture = request.ActualDeparture.Value;

            if (request.ActualArrival.HasValue)
                shipment.ActualArrival = request.ActualArrival.Value;
        }

        public static bool CanUpdateTracking(ShipmentStatus status)
        {
            return status is
                ShipmentStatus.BookingRequested or
                ShipmentStatus.BookingConfirmed or
                ShipmentStatus.DraftBLApproved or
                ShipmentStatus.DraftBLReceived or
                ShipmentStatus.PaymentPending or
                ShipmentStatus.PaymentCompleted or
                ShipmentStatus.ShippingInstructionsSubmitted or
                ShipmentStatus.TelexReleased;
        }
    }
}
