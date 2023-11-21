using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models;

public class CourseForCreationDto : IValidatableObject
{
    [Required(ErrorMessage = "You should fill out a little.")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "The title shouldn't have more than 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1500, ErrorMessage = "The description shouldn't have more than 1500 characters.")]
    public string? Description { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Title.Equals(Description))
            yield return new ValidationResult("The provided description should be different from the title.", new string[] { "Course" });
    }
}

