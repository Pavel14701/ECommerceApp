using System.Threading.Tasks;

public interface IMessageSender
{
    Task<T> SendCommandAndGetResponse<T>(string exchange, string routingKey, object command);
}
