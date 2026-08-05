using System.ComponentModel.DataAnnotations;

namespace Order.Model
{
    public class CreateOrderRequest
    {
        [NotEmptyGuid]
        public Guid ResellerId { get; set; }

        [NotEmptyGuid]
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Left nullable so a missing "items" property is reported by
        /// [Required] as a validation error rather than by the JSON deserializer.
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "An order must contain at least one item.")]
        public IList<CreateOrderItemRequest>? Items { get; set; }
    }
}
