using CourseLibrary.API.Models;
using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.ValidationAttributes
{
    public class TitleAndDescriptionNotTheSame : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (validationContext.ObjectInstance is not CourseForUpdateDto course)
            {
                throw new NotSupportedException($"Attribute {nameof(TitleAndDescriptionNotTheSame)} must be applied to a {nameof(CourseForUpdateDto)} or derived type.");
            }

            if (course.Title.Equals(course.Description))
            {
                return new ValidationResult("The provided description should be different from the title.", new string[] { nameof(CourseForUpdateDto) });
            }

            return ValidationResult.Success;
        }
    }
}
