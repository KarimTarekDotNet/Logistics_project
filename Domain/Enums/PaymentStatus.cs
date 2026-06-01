namespace Domain.Enums
{
    public enum PaymentStatus
    {
        Pending,
        PartiallyPaid,
        Paid,
        Draft,
        Cancelled,
        Refunded
    }
    public enum InvoiceType
    {
        Base,
        NewElements
    }
}