using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IO;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Security;
using NJsonSchema;
using NJsonSchema.Generation;
using NSwag.Generation.Processors.Contexts;
using RabbitMQ.Client;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

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

        // Регистрация IHttpContextAccessor
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        // Регистрация обработчиков команд
        services.AddSingleton<ICommandHandler, CommandHandler>();

        // Регистрация сервисов
        // News
        services.AddScoped<ICreateNewsService, CreateNewsService>();
        services.AddScoped<IUpdateNewsService, UpdateNewsService>();
        services.AddScoped<IReadNewsService, ReadNewsService>();
        services.AddScoped<IDeleteNewsService, DeleteNewsService>();
        // Products
        services.AddScoped<IProductCreateService, ProductCreateService>();
        services.AddScoped<IProductUpdateService, ProductUpdateService>();
        services.AddScoped<IProductReadService, ProductReadService>();
        services.AddScoped<IProductDeleteService, ProductDeleteService>();
        // Auth
        services.AddScoped<IAuthService, AuthService>();
        // User
        services.AddScoped<IUserService, UserService>();
        // Email
        services.AddTransient<IEmailService, EmailService>();
        // Broker Common
        services.AddSingleton<IProcessedEventService, RedisProcessedEventService>();
        services.AddSingleton<IMessageSender, MessageSender>();
        services.AddSingleton<ICommandHandler, CommandHandler>();

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

            configure.OperationProcessors.Add(new AddFormFileOperationProcessor());
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

        // Настройка для обслуживания статических файлов вашего Vue.js приложения и вьюх
        var uploadsPath = Configuration.GetSection("StaticFiles:UploadsPath").Value;
        var viewsPath = Configuration.GetSection("StaticFiles:ViewsPath").Value;

        if (string.IsNullOrEmpty(uploadsPath))
        {
            throw new ArgumentNullException(nameof(uploadsPath), "Uploads path cannot be null or empty.");
        }

        if (string.IsNullOrEmpty(viewsPath))
        {
            throw new ArgumentNullException(nameof(viewsPath), "Views path cannot be null or empty.");
        }
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), uploadsPath)),
            RequestPath = "/uploads"
        });

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), viewsPath)),
            RequestPath = "/views"
        });

        // Перенаправление на index.html для всех необработанных запросов
        //app.UseSpa(spa =>
        //{
        //    spa.Options.SourcePath = "ClientApp"; // Убедитесь, что ваш путь к приложению Vue.js указан верно

        //    if (env.IsDevelopment())
        //    {
        //        spa.UseProxyToSpaDevelopmentServer("http://localhost:8080"); // Убедитесь, что ваш сервер разработки Vue.js указан верно
        //    }
        //});

        app.UseOpenApi();
        app.UseSwaggerUi();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        // Запуск прослушивания команд
        var scopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
        using (var scope = scopeFactory.CreateScope())
        {
            var eventHandlers = scope.ServiceProvider.GetServices<IEventHandler>();
            foreach (var handler in eventHandlers)
            {
                handler.StartListening();
            }
        }
    }

    public class AddFormFileOperationProcessor : IOperationProcessor
    {
        public bool Process(OperationProcessorContext context)
        {
            foreach (var parameter in context.OperationDescription.Operation.Parameters)
            {
                if (parameter.Kind == OpenApiParameterKind.FormData && parameter.Name == "file")
                {
                    parameter.Schema = new JsonSchema { Type = JsonObjectType.String, Format = "binary" };
                }
            }
            return true;
        }
    }
}
