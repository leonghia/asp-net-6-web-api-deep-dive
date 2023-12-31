﻿namespace CourseLibrary.API.ResourceParameters
{
    public class AuthorResourceParameter
    {
        private const int _maxPageSize = 20;
        public string? MainCategory { get; set; }
        public string? SearchQuery { get; set; }
        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > _maxPageSize ? _maxPageSize : value;
        }
        public int PageNumber { get; set; } = 1;

        public string? OrderBy { get; set; } = "Name";
        public string? Fields { get; set; }
    }
}
