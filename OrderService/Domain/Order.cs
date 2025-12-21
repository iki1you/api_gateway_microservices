namespace Domain
{
    public class Order : BaseEntity
    {
        public long UserId { get; set; }
        public long ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalCost { get; set; }
    }
}
