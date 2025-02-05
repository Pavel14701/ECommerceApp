using System.Collections.Concurrent;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Added for logging

public class SessionFactory : ISessionFactory
{
    private readonly IConfiguration _configuration;
    private readonly ConcurrentQueue<ApplicationDbContext> _sessionPool;
    private readonly int _maxSize;
    private readonly int _overflowSize;
    private readonly ILogger<SessionFactory> _logger; // Added for logging

    public SessionFactory(IConfiguration configuration, ILogger<SessionFactory> logger)
    {
        _configuration = configuration;
        _maxSize = _configuration.GetValue<int>("SessionPool:MaxSize");
        _overflowSize = _configuration.GetValue<int>("SessionPool:OverflowSize");
        _sessionPool = new ConcurrentQueue<ApplicationDbContext>();
        _logger = logger;

        for (int i = 0; i < _maxSize; i++)
        {
            _sessionPool.Enqueue(CreateNewSession());
        }
    }

    private ApplicationDbContext CreateNewSession()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"));

        _logger.LogInformation("Created a new session.");
        return new ApplicationDbContext(optionsBuilder.Options);
    }

    public ApplicationDbContext GetSession()
    {
        if (_sessionPool.TryDequeue(out var context))
        {
            _logger.LogInformation("Reusing an existing session.");
            return context;
        }
        else
        {
            _logger.LogInformation("Session pool is empty, creating a new session.");
            return CreateNewSession();
        }
    }

    public void ReturnSession(ApplicationDbContext context)
    {
        if (_sessionPool.Count < _maxSize + _overflowSize)
        {
            _sessionPool.Enqueue(context);
            _logger.LogInformation("Session returned to the pool.");
        }
        else
        {
            context.Dispose();
            _logger.LogInformation("Session pool is full, disposed of the session.");
        }
    }
}

public class SessionIterator
{
    private readonly ISessionFactory _sessionFactory;
    private readonly ILogger<SessionIterator> _logger;

    public SessionIterator(ISessionFactory sessionFactory, ILogger<SessionIterator> logger)
    {
        _sessionFactory = sessionFactory;
        _logger = logger;
    }

    // Метод для записи данных (с транзакцией)
    public async Task ExecuteAsync(Func<ApplicationDbContext, Task> action)
    {
        var context = _sessionFactory.GetSession();
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            await action(context);
            await transaction.CommitAsync();
            _logger.LogInformation("Transaction committed successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Transaction rolled back due to an error.");
            throw;
        }
        finally
        {
            _sessionFactory.ReturnSession(context);
        }
    }

    // Метод для чтения данных (без явного коммита транзакции)
    public async Task<T> ReadAsync<T>(Func<ApplicationDbContext, Task<T>> query)
    {
        var context = _sessionFactory.GetSession();
        try
        {
            var result = await query(context);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during query execution.");
            throw;
        }
        finally
        {
            _sessionFactory.ReturnSession(context);
        }
    }

    // Методы для выполнения SQL-запросов
    public async Task<int> ExecuteScalarAsync(ApplicationDbContext context, string sql, params NpgsqlParameter[] parameters)
    {
        try
        {
            using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            if (command.Connection is null)
            {
                throw new InvalidOperationException("The database connection is null.");
            }
            if (command.Connection.State != System.Data.ConnectionState.Open)
            {
                await command.Connection.OpenAsync();
            }

            foreach (var parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }

            var result = await command.ExecuteScalarAsync();
            _logger.LogInformation("ExecuteScalarAsync completed successfully.");
            return result != null ? Convert.ToInt32(result) : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during ExecuteScalarAsync.");
            throw;
        }
    }

    public async Task ExecuteSqlRawAsync(ApplicationDbContext context, string sql, params NpgsqlParameter[] parameters)
    {
        try
        {
            await context.Database.ExecuteSqlRawAsync(sql, parameters);
            _logger.LogInformation("ExecuteSqlRawAsync completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during ExecuteSqlRawAsync.");
            throw;
        }
    }
}
