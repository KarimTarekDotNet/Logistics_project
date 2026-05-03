namespace Application.DTOs.Shipments.API
{
    public record TaxVerificationResult
    {
        public bool IsValid { get; set; }
        public bool IsVerified { get; set; }
        public string? CompanyName { get; set; }
        public string? Address { get; set; }
        public string? Provider { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Message { get; set; }
        public string? ReferenceId { get; set; }

        public static TaxVerificationResult Failed(string message) => new()
        {
            IsValid = false,
            IsVerified = false,
            Provider = "Lookuptax",
            ErrorMessage = message
        };
    }

    public class LookuptaxResponse
    {
        public string? ReferenceId { get; set; }
        public string? CountryIso { get; set; }
        public string? Tin { get; set; }
        public LookuptaxValidation? Validation { get; set; }
    }

    public class LookuptaxValidation
    {
        public LookuptaxValidationResult? Overall { get; set; }
        public LookuptaxValidationResult? Format { get; set; }
    }

    public class LookuptaxValidationResult
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
    }
}
