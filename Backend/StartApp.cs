using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
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
        services.AddRabbitMqConfiguration(Configuration);

        // Настройка JWT
        services.AddJwtConfiguration(Configuration);

        // Настройка Swagger
        services.AddSwaggerConfiguration();

        // Регистрация других сервисов
        services.AddSingleton<ICommandHandler, CommandHandler>();
        services.AddSingleton<IProcessedEventService, RedisProcessedEventService>();
        services.AddSingleton<IMessageSender, MessageSender>();

        services.AddScoped<IEmailService, EmailService>();

        services.AddScoped<IEventHandler, AuthCommandHandler>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<ICreateNewsService, CreateNewsService>();
        services.AddScoped<IUpdateNewsService, UpdateNewsService>();
        services.AddScoped<IReadNewsService, ReadNewsService>();
        services.AddScoped<IDeleteNewsService, DeleteNewsService>();

        services.AddScoped<IEventHandler, NewsCreateCommandHandler>();
        services.AddScoped<IEventHandler, NewsUpdateCommandHandler>();
        services.AddScoped<IEventHandler, NewsReadCommandHandler>();
        services.AddScoped<IEventHandler, NewsDeleteCommandHandler>();

        services.AddScoped<IProductCreateService, ProductCreateService>();
        services.AddScoped<IProductUpdateService, ProductUpdateService>();
        services.AddScoped<IProductReadService, ProductReadService>();
        services.AddScoped<IProductDeleteService, ProductDeleteService>();

        services.AddScoped<IEventHandler, ProductCreateCommandHandler>();
        services.AddScoped<IEventHandler, ProductUpdateCommandHandler>();
        services.AddScoped<IEventHandler, ProductReadCommandHandler>();
        services.AddScoped<IEventHandler, ProductDeleteCommandHandler>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEventHandler, UserCommandHandler>();

        // Регистрация IHttpContextAccessor
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        services.AddControllers();

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

        RabbitMqConfig.InitializeRabbitMq(app);
    }
}
