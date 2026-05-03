using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    public class RateParameters : QueryParameters
    {
        public bool? OnlyActive { get; set; } = false;

        // Unified Filters
        public string? CarrierName { get; set; }
        public string? ContainerTypeName { get; set; }
        public string? FromPortName { get; set; }
        public string? ToPortName { get; set; }

        [Range(typeof(decimal), "0.01", "9999999999999999", ErrorMessage = "Min price must be greater than 0")]
        public decimal? MinPrice { get; set; }

        [Range(typeof(decimal), "0.01", "9999999999999999", ErrorMessage = "Max price must be greater than 0")]
        public decimal? MaxPrice { get; set; }

        [MaxLength(4, ErrorMessage = "Currency code cannot exceed 4 characters")]
        public string? Currency { get; set; }

        public DateTimeOffset? ValidFrom { get; set; }
        public DateTimeOffset? ValidTo { get; set; }
        public DateTimeOffset? CreatedFrom { get; set; }
        public DateTimeOffset? CreatedTo { get; set; }
        public bool? OnlyCurrentlyValid { get; set; }
    }
}
