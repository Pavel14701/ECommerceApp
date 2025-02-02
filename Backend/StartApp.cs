using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IO;
using NSwag;
using NSwag.Generation.Processors.Security;
using NJsonSchema;
using RabbitMQ.Client;
using Microsoft.Extensions.FileProviders;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Настройка базы данных
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

        // Настройка фабрики DbContext
        services.AddSingleton<IDbContextFactory, DbContextFactory>();

        // Настройка Redis
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var configuration = Configuration["Redis:Connection"] ?? throw new InvalidOperationException("Redis connection string is not configured.");
            return ConnectionMultiplexer.Connect(configuration);
        });

        // Настройка RabbitMQ
        services.AddSingleton(sp =>
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                DispatchConsumersAsync = true // Включить асинхронных потребителей
            };
            return factory;
        });

        services.AddSingleton(sp =>
        {
            var factory = sp.GetRequiredService<ConnectionFactory>();
            return factory.CreateConnection();
        });

        services.AddSingleton(sp =>
        {
            var connection = sp.GetRequiredService<IConnection>();
            return connection.CreateModel();
        });

        services.AddSingleton<IConsumerInitializer, ConsumerInitializer>();
        services.AddSingleton<IRabbitMQInitializer, RabbitMQInitializer>();

        // Регистрация AuthCommandHandler как scoped
        services.AddScoped<IEventHandler, AuthCommandHandler>();

        // Регистрация других сервисов
        services.AddSingleton<ICommandHandler, CommandHandler>();
        services.AddSingleton<IProcessedEventService, RedisProcessedEventService>();
        services.AddSingleton<IMessageSender, MessageSender>();
        services.AddScoped<ICreateNewsService, CreateNewsService>();
        services.AddScoped<IUpdateNewsService, UpdateNewsService>();
        services.AddScoped<IReadNewsService, ReadNewsService>();
        services.AddScoped<IDeleteNewsService, DeleteNewsService>();
        services.AddScoped<IProductCreateService, ProductCreateService>();
        services.AddScoped<IProductUpdateService, ProductUpdateService>();
        services.AddScoped<IProductReadService, ProductReadService>();
        services.AddScoped<IProductDeleteService, ProductDeleteService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddTransient<IEmailService, EmailService>();

        // Регистрация IHttpContextAccessor
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        // Настройка JWT аутентификации
        var key = Encoding.ASCII.GetBytes(Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured."));
        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = false;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = Configuration["Jwt:Issuer"],
                ValidAudience = Configuration["Jwt:Audience"]
            };
        });

        services.AddControllers();

        // Настройка OpenAPI (Swagger)
        services.AddOpenApiDocument(configure =>
        {
            configure.Title = "My API";
            configure.Version = "v1";
            configure.AddSecurity("JWT", new List<string>(), new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.ApiKey,
                Name = "Authorization",
                In = OpenApiSecurityApiKeyLocation.Header,
                Description = "Type into the textbox: Bearer {your JWT token}."
            });

            configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
        });

        // Регистрация IFileProvider
        var viewsPath = Configuration.GetSection("StaticFiles:ViewsPath").Value;
        services.AddSingleton<IFileProvider>(new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), viewsPath)));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseOpenApi();
        app.UseSwaggerUi();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        // Инициализация RabbitMQ
        var scopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
        using (var scope = scopeFactory.CreateScope())
        {
            var rabbitMQInitializer = scope.ServiceProvider.GetRequiredService<IRabbitMQInitializer>();
            rabbitMQInitializer.Initialize();

            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Startup>>();

            // Запуск прослушивания команд
            var eventHandlers = scope.ServiceProvider.GetServices<IEventHandler>();
            foreach (var handler in eventHandlers)
            {
                logger.LogInformation("Initializing StartListening for handler: {HandlerType}", handler.GetType().Name);
                handler.StartListening();
                logger.LogInformation("Started listening on queues for handler: {HandlerType}", handler.GetType().Name);
            }
        }
    }
}
