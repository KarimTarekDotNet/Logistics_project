namespace Domain.Enums
{
    public enum ShipmentStatus
    {
        Created,
        ClientConfirmed,

        BookingRequested,
        BookingConfirmed,

        ShippingInstructionsSubmitted,

        DraftBLReceived,
        DraftBLApproved,

        PaymentPending,
        PaymentCompleted,

        TelexReleased,

        Delivered,
        Closed,

        Cancelled,
        OnHold
    }
}