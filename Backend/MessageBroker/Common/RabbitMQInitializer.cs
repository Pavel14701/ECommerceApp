using RabbitMQ.Client;

public class RabbitMQInitializer : IRabbitMQInitializer
{
    private readonly IModel _channel;

    public RabbitMQInitializer(IModel channel)
    {
        _channel = channel;
    }

    public void Initialize()
    {
        _channel.ExchangeDeclare(exchange: "auth.exchange", type: ExchangeType.Direct, durable: true, autoDelete: false, arguments: null);
        _channel.ExchangeDeclare(exchange: "news.exchange", type: ExchangeType.Direct, durable: true, autoDelete: false, arguments: null);
        _channel.ExchangeDeclare(exchange: "orders.exchange", type: ExchangeType.Direct, durable: true, autoDelete: false, arguments: null);
        _channel.ExchangeDeclare(exchange: "products.exchange", type: ExchangeType.Direct, durable: true, autoDelete: false, arguments: null);
        _channel.ExchangeDeclare(exchange: "users.exchange", type: ExchangeType.Direct, durable: true, autoDelete: false, arguments: null);





        _channel.QueueDeclare(queue: "auth.authenticate", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "auth.refreshToken", durable: true, exclusive: false, autoDelete: false, arguments: null);

        _channel.QueueDeclare(queue: "news.create", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "news.addimage", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "news.uploadimage", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "news.delete", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "news.delete.image", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "news.getall", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "news.getbyid", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "news.update", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "news.update.image", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "news.update.title", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "news.update.publishdate", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "news.update.contenttext", durable: true, exclusive: false, autoDelete: false, arguments: null);

        _channel.QueueDeclare(queue: "orders.getById", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "orders.applyDiscount", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "orders.create", durable: true, exclusive: false, autoDelete: false, arguments: null);

        _channel.QueueDeclare(queue: "products.uploadimage", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "products.create", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "products.delete", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "products.deleteimage", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "products.getbyname", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "products.getbycategory", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "products.getbyid", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "products.getall", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "products.update.product", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "products.update.image", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "products.update.description", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "products.update.stock", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "products.update.price", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "products.update.category", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "products.update.subcategory", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "products.update.name", durable: true, exclusive: false, autoDelete: false, arguments: null);

        _channel.QueueDeclare(queue: "users.updatepassword", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "users.delete", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "users.confirmemail", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "users.register", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "users.updateusername", durable: true, exclusive: false, autoDelete: false, arguments: null);




        _channel.QueueBind(queue: "auth.authenticate", exchange: "auth.exchange", routingKey: "auth.authenticate");
        _channel.QueueBind(queue: "auth.refreshToken", exchange: "auth.exchange", routingKey: "auth.refreshToken");


        _channel.QueueBind(queue: "news.create", exchange: "news.exchange", routingKey: "news.create");
        _channel.QueueBind(queue: "news.addimage", exchange: "news.exchange", routingKey: "news.addimage");
        _channel.QueueBind(queue: "news.uploadimage", exchange: "news.exchange", routingKey: "news.uploadimage");
        _channel.QueueBind(queue: "news.delete", exchange: "news.exchange", routingKey: "news.delete");
        _channel.QueueBind(queue: "news.delete.image", exchange: "news.exchange", routingKey: "news.delete.image");
        _channel.QueueBind(queue: "news.getall", exchange: "news.exchange", routingKey: "news.getall");
        _channel.QueueBind(queue: "news.getbyid", exchange: "news.exchange", routingKey: "news.getbyid");
        _channel.QueueBind(queue: "news.update", exchange: "news.exchange", routingKey: "news.update");
        _channel.QueueBind(queue: "news.update.image", exchange: "news.exchange", routingKey: "news.update.image");
        _channel.QueueBind(queue: "news.update.title", exchange: "news.exchange", routingKey: "news.update.title");
        _channel.QueueBind(queue: "news.update.publishdate", exchange: "news.exchange", routingKey: "news.update.publishdate");
        _channel.QueueBind(queue: "news.update.contenttext", exchange: "news.exchange", routingKey: "news.update.contenttext");


        _channel.QueueBind(queue: "orders.getById", exchange: "orders.exchange", routingKey: "orders.getById");
        _channel.QueueBind(queue: "orders.applyDiscount", exchange: "orders.exchange", routingKey: "orders.applyDiscount");
        _channel.QueueBind(queue: "orders.create", exchange: "orders.exchange", routingKey: "orders.create");


        _channel.QueueBind(queue: "products.uploadimage", exchange: "products.exchange", routingKey: "products.uploadimage");
        _channel.QueueBind(queue: "products.create", exchange: "products.exchange", routingKey: "products.create");
        _channel.QueueBind(queue: "products.delete", exchange: "products.exchange", routingKey: "products.delete");
        _channel.QueueBind(queue: "products.deleteimage", exchange: "products.exchange", routingKey: "products.deleteimage");
        _channel.QueueBind(queue: "products.getbyname", exchange: "products.exchange", routingKey: "products.getbyname");
        _channel.QueueBind(queue: "products.getbycategory", exchange: "products.exchange", routingKey: "products.getbycategory");
        _channel.QueueBind(queue: "products.getbyid", exchange: "products.exchange", routingKey: "products.getbyid");
        _channel.QueueBind(queue: "products.getall", exchange: "products.exchange", routingKey: "products.getall");
        _channel.QueueBind(queue: "products.update.product", exchange: "products.exchange", routingKey: "products.update.product");
        _channel.QueueBind(queue: "products.update.image", exchange: "products.exchange", routingKey: "products.update.image");
        _channel.QueueBind(queue: "products.update.description", exchange: "products.exchange", routingKey: "products.update.description");
        _channel.QueueBind(queue: "products.update.stock", exchange: "products.exchange", routingKey: "products.update.stock");
        _channel.QueueBind(queue: "products.update.price", exchange: "products.exchange", routingKey: "products.update.price");
        _channel.QueueBind(queue: "products.update.category", exchange: "products.exchange", routingKey: "products.update.category");
        _channel.QueueBind(queue: "products.update.subcategory", exchange: "products.exchange", routingKey: "products.update.subcategory");
        _channel.QueueBind(queue: "products.update.name", exchange: "products.exchange", routingKey: "products.update.name");


        _channel.QueueBind(queue: "users.updatepassword", exchange: "users.exchange", routingKey: "users.updatepassword");
        _channel.QueueBind(queue: "users.delete", exchange: "users.exchange", routingKey: "users.delete");
        _channel.QueueBind(queue: "users.confirmemail", exchange: "users.exchange", routingKey: "users.confirmemail");
        _channel.QueueBind(queue: "users.register", exchange: "users.exchange", routingKey: "users.register");
        _channel.QueueBind(queue: "users.updateusername", exchange: "users.exchange", routingKey: "users.updateusername");
    }
}
