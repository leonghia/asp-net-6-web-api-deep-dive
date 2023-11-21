using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;
using CourseLibrary.API.Utilities;

namespace CourseLibrary.API.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private readonly Dictionary<string, PropertyMappingValue> _authorPropertyMapping = new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
        {
            { "Id", new PropertyMappingValue { DestinationProperties = new string[] { "Id" } } },
            { "MainCategory", new PropertyMappingValue { DestinationProperties = new string[] { "MainCategory" } } },
            { "Age", new PropertyMappingValue { DestinationProperties = new string[] { "DateOfBirth" }, Revert = true } },
            { "Name", new PropertyMappingValue { DestinationProperties = new string[] { "FirstName", "LastName" } } },
        };

        private readonly IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();


        public PropertyMappingService()
        {
            _propertyMappings.Add(new PropertyMapping<AuthorDto, Author> { MappingDictionary = _authorPropertyMapping });
        }

        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            // Get matching mapping
            var matchingMapping = _propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();

            if (matchingMapping.Count() == 1)
            {
                return matchingMapping.First().MappingDictionary;
            }

            throw new Exception($"Cannot find exact property mapping instance for <{typeof(TSource)}, {typeof(TDestination)}>");
        }

        public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                return true;
            }

            // the string is separated by "," so we split it
            var fieldsAfterSplit = fields.Split(',');

            // run through the fields clauses
            foreach (var field in fieldsAfterSplit)
            {
                // trim
                var trimmedField = field.Trim();

                // remove everything after the first " " - if the fields are coming from an orderBy string, "asc" or "desc" must be ignored
                var indexOfFirstSpace = trimmedField.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ? trimmedField : trimmedField.Remove(indexOfFirstSpace);

                // find the matching property
                if (!propertyMapping.ContainsKey(propertyName))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
