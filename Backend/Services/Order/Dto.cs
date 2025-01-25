public class CreateOrderResultDto
{
    public bool Success { get; set; }
    public required string Message { get; set; }
    public Order? Order { get; set; }
}

public class ApplyDiscountResultDto
{
    public bool Success { get; set; }
    public required string Message { get; set; }
    public Guid OrderId { get; set; }
    public Discount? Discount { get; set; }
}

public class GetOrderByIdResultDto
{
    public bool Success { get; set; }
    public required string Message { get; set; }
    public Order? Order { get; set; }
}
