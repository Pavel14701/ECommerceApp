public class CreateOrderCommand
{
    public Guid CommandId { get; set; }
    public required Order Order { get; set; }
}

public class ApplyDiscountCommand
{
    public Guid CommandId { get; set; }
    public Guid OrderId { get; set; }
    public required Discount Discount { get; set; }
}

public class GetOrderByIdQuery
{
    public Guid QueryId { get; set; }
    public Guid OrderId { get; set; }
}
