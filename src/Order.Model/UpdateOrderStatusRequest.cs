using System.ComponentModel.DataAnnotations;

namespace Order.Model
{
    public class UpdateOrderStatusRequest
    {
        /// <summary>
        /// Left nullable so a missing "status" property is reported by
        /// [Required] as a validation error rather than by the JSON deserializer.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "A status name is required.")]
        public string? Status { get; set; }
    }
}
