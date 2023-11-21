using System.Text.Json.Serialization;

namespace CourseLibrary.API.Utilities
{
    
    public class PaginationMetadata
    {
        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }
        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }
        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; }
        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }      
    }
}
