using Domain.Enums;

namespace Application.ApplicationRules.Shipments
{
    public static class ShipmentStatusRules
    {
        public static bool CanTransition(ShipmentStatus current, ShipmentStatus next)
        {
            if (IsTerminal(current))
                return false;

            return current switch
            {
                ShipmentStatus.Created => CanMoveFromCreated(next),
                ShipmentStatus.ClientConfirmed => CanMoveFromClientConfirmed(next),
                ShipmentStatus.BookingRequested => CanMoveFromBookingRequested(next),
                ShipmentStatus.BookingConfirmed => CanMoveFromBookingConfirmed(next),
                ShipmentStatus.ShippingInstructionsSubmitted => CanMoveFromShippingInstructionsSubmitted(next),
                ShipmentStatus.DraftBLReceived => CanMoveFromDraftBlReceived(next),
                ShipmentStatus.DraftBLApproved => CanMoveFromDraftBlApproved(next),
                ShipmentStatus.PaymentPending => CanMoveFromPaymentPending(next),
                ShipmentStatus.PaymentCompleted => CanMoveFromPaymentCompleted(next),
                ShipmentStatus.TelexReleased => CanMoveFromTelexReleased(next),
                ShipmentStatus.Delivered => CanMoveFromDelivered(next),
                ShipmentStatus.OnHold => false,
                _ => false
            };
        }

        private static bool IsTerminal(ShipmentStatus current)
        {
            return current == ShipmentStatus.Closed
                || current == ShipmentStatus.Cancelled;
        }

        private static bool CanMoveFromCreated(ShipmentStatus next)
        {
            return next == ShipmentStatus.ClientConfirmed
                || next == ShipmentStatus.Cancelled
                || next == ShipmentStatus.OnHold;
        }

        private static bool CanMoveFromClientConfirmed(ShipmentStatus next)
        {
            return next == ShipmentStatus.BookingRequested
                || next == ShipmentStatus.Cancelled
                || next == ShipmentStatus.OnHold;
        }

        private static bool CanMoveFromBookingRequested(ShipmentStatus next)
        {
            return next == ShipmentStatus.BookingConfirmed
                || next == ShipmentStatus.Cancelled
                || next == ShipmentStatus.OnHold;
        }

        private static bool CanMoveFromBookingConfirmed(ShipmentStatus next)
        {
            return next == ShipmentStatus.ShippingInstructionsSubmitted
                || next == ShipmentStatus.Cancelled
                || next == ShipmentStatus.OnHold;
        }

        private static bool CanMoveFromShippingInstructionsSubmitted(ShipmentStatus next)
        {
            return next == ShipmentStatus.DraftBLReceived
                || next == ShipmentStatus.Cancelled
                || next == ShipmentStatus.OnHold;
        }

        private static bool CanMoveFromDraftBlReceived(ShipmentStatus next)
        {
            return next == ShipmentStatus.DraftBLApproved
                || next == ShipmentStatus.Cancelled
                || next == ShipmentStatus.OnHold;
        }

        private static bool CanMoveFromDraftBlApproved(ShipmentStatus next)
        {
            return next == ShipmentStatus.PaymentPending
                || next == ShipmentStatus.Cancelled
                || next == ShipmentStatus.OnHold;
        }

        private static bool CanMoveFromPaymentPending(ShipmentStatus next)
        {
            return next == ShipmentStatus.PaymentCompleted
                || next == ShipmentStatus.Cancelled
                || next == ShipmentStatus.OnHold;
        }

        private static bool CanMoveFromPaymentCompleted(ShipmentStatus next)
        {
            return next == ShipmentStatus.TelexReleased
                || next == ShipmentStatus.OnHold;
        }

        private static bool CanMoveFromTelexReleased(ShipmentStatus next)
        {
            return next == ShipmentStatus.Delivered
                || next == ShipmentStatus.OnHold;
        }

        private static bool CanMoveFromDelivered(ShipmentStatus next)
        {
            return next == ShipmentStatus.Closed;
        }


        public static bool CanModifyCharges(ShipmentStatus status)
        {
            return status is not (
                ShipmentStatus.PaymentCompleted or
                ShipmentStatus.TelexReleased or
                ShipmentStatus.Delivered or
                ShipmentStatus.Closed or
                ShipmentStatus.Cancelled
            );
        }

        public static bool CanModifyItems(ShipmentStatus status)
        {
            return status is not (
                ShipmentStatus.ShippingInstructionsSubmitted or
                ShipmentStatus.PaymentCompleted or
                ShipmentStatus.TelexReleased or
                ShipmentStatus.Delivered or
                ShipmentStatus.Closed or
                ShipmentStatus.Cancelled
            );
        }

        public static bool CanCreateInvoice(ShipmentStatus status)
        {
            return status is not (
                ShipmentStatus.Created or
                ShipmentStatus.Cancelled or
                ShipmentStatus.Closed
            );
        }

        public static bool CanPayInvoice(ShipmentStatus status)
        {
            return status is not (
                ShipmentStatus.Created or
                ShipmentStatus.Cancelled or
                ShipmentStatus.Closed
            );
        }

        public static bool CanPartiallyPayInvoice(ShipmentStatus status)
        {
            return CanPayInvoice(status);
        }

        public static bool CanRefundInvoice(ShipmentStatus status)
        {
            return status is not (
                ShipmentStatus.Created or
                ShipmentStatus.ClientConfirmed or
                ShipmentStatus.BookingRequested or
                ShipmentStatus.Cancelled or
                ShipmentStatus.TelexReleased or
                ShipmentStatus.Delivered or
                ShipmentStatus.Closed
            );
        }

        public static bool CanCancelInvoice(ShipmentStatus status)
        {
            return status is not (
                ShipmentStatus.PaymentCompleted or
                ShipmentStatus.TelexReleased or
                ShipmentStatus.Delivered or
                ShipmentStatus.Closed or
                ShipmentStatus.Cancelled
            );
        }
    }
}
