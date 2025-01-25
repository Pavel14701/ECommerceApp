using RabbitMQ.Client;

public class EventMessage
{
    public Guid EventId { get; set; }
    public string? EventType { get; set; }
    public object? Data { get; set; }
}
