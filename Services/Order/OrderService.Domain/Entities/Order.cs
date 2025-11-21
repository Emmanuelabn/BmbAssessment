namespace OrderService.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
        public Guid ClientId { get; set; }
        public DateTime OrderDate { get; set; }
        public Guid LoggedInEmployeeId { get; set; }
    }
}
