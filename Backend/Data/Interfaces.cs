public interface ISessionFactory
{
    ApplicationDbContext GetSession();
    void ReturnSession(ApplicationDbContext context);
}
