using System.ComponentModel.DataAnnotations;

namespace Order.Model
{
    public class CreateOrderItemRequest
    {
        [NotEmptyGuid]
        public Guid ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public int Quantity { get; set; }
    }
}
