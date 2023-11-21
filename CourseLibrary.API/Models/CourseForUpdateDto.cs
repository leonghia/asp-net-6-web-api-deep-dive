using CourseLibrary.API.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models
{
    [TitleAndDescriptionNotTheSame]
    public class CourseForUpdateDto /*: IValidatableObject*/
    {
        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1500, MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if (Title.Equals(Description))
        //        yield return new ValidationResult("The provided description should be different from the title.", new string[] { "Course" });
        //}
    }
}
