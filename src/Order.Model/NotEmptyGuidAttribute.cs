using System.ComponentModel.DataAnnotations;

namespace Order.Model
{
    /// <summary>
    /// Guid is a value type, so [Required] is satisfied by Guid.Empty.
    /// This rejects the empty Guid, which is never a valid identifier here.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class NotEmptyGuidAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            return value is Guid guid && guid != Guid.Empty;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The {name} field must be a non-empty GUID.";
        }
    }
}
