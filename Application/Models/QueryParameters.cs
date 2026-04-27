using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    public class QueryParameters
    {
        private const int MaxPageSize = 50;
        private const int MinPageSize = 1;
        private const int DefaultPageSize = 10;

        private int _pageSize = DefaultPageSize;
        private int _pageNumber = 1;

        [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1")]
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        [Range(1, 50, ErrorMessage = "Page size must be between 1 and 50")]
        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (value < MinPageSize)
                    _pageSize = MinPageSize;
                else if (value > MaxPageSize)
                    _pageSize = MaxPageSize;
                else
                    _pageSize = value;
            }
        }

        [MaxLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
        public string? Search { get; set; }

        [MaxLength(50, ErrorMessage = "Sort field cannot exceed 50 characters")]
        public string? SortBy { get; set; } = "name";
    }
}
