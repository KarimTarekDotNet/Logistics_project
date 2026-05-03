using Domain.Enums;

namespace Application.ApplicationRules.Shipments
{
    public static class ShipmentStatusRules
    {
        public static bool CanTransition(ShipmentStatus current, ShipmentStatus next)
        {
            if (current == ShipmentStatus.Closed || current == ShipmentStatus.Cancelled)
                return false;

            if (next == ShipmentStatus.Cancelled || next == ShipmentStatus.OnHold)
                return true;

            return current switch
            {
                ShipmentStatus.Created => next == ShipmentStatus.ClientConfirmed,
                ShipmentStatus.ClientConfirmed => next == ShipmentStatus.BookingRequested,
                ShipmentStatus.BookingRequested => next == ShipmentStatus.BookingConfirmed,
                ShipmentStatus.BookingConfirmed => next == ShipmentStatus.ShippingInstructionsSubmitted,
                ShipmentStatus.ShippingInstructionsSubmitted => next == ShipmentStatus.DraftBLReceived,
                ShipmentStatus.DraftBLReceived => next == ShipmentStatus.DraftBLApproved,
                ShipmentStatus.DraftBLApproved => next == ShipmentStatus.PaymentPending,
                ShipmentStatus.PaymentPending => next == ShipmentStatus.PaymentCompleted,
                ShipmentStatus.PaymentCompleted => next == ShipmentStatus.TelexReleased,
                ShipmentStatus.TelexReleased => next == ShipmentStatus.Delivered,
                ShipmentStatus.Delivered => next == ShipmentStatus.Closed,
                _ => false
            };
        }
    }
}
