namespace CourseLibrary.API.Utilities
{
    public class PropertyMappingValue
    {
        public IEnumerable<string> DestinationProperties { get; init; }
        public bool Revert { get; init; } = false;
    }
}
