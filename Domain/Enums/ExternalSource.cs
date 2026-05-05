namespace Domain.Enums
{
    public enum ExternalSource
    {
        n8n,
        carrier_api,
        email_import
    }

    public enum Status
    {
        Pending,
        Processed,
        Failed
    }
}