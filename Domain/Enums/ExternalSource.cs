namespace Domain.Enums
{
    public enum ExternalSource
    {
        N8N,
        Carrier_Api,
        Email_Import
    }

    public enum Status
    {
        Pending,
        Processing,
        Processed,
        Failed
    }
}