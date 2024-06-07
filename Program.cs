using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

// docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Abcd5678" -p 1433:1433 mcr.microsoft.com/mssql/server
using var context = new BlogContext();
await context.Database.EnsureCreatedAsync();
await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE MyEntity");

const int timeoutSeconds = 1;
context.Database.SetCommandTimeout(timeoutSeconds);
var exceptionCount = 0;
for (int i = 0; i < 60; i++)
{
    try
    {
        await context.Database.ExecuteSqlRawAsync(
        """
        INSERT INTO MyEntity VALUES (1);
        WAITFOR DELAY '00:00:02';
        INSERT INTO MyEntity VALUES (2);
        """, CreateCancellationToken(TimeSpan.FromSeconds(timeoutSeconds)));
    }
    catch (SqlException e)
    {
        Console.WriteLine(e.Message);
        exceptionCount++;
    }
}

Console.WriteLine("Exception count: " + exceptionCount);
Console.WriteLine("Value '1' count: " + context.MyEntity.Where(e => e.X == 1).Count());
Console.WriteLine("Value '2' count: " + context.MyEntity.Where(e => e.X == 2).Count());


static CancellationToken CreateCancellationToken(TimeSpan cancelAfter)
{
    var cts = new CancellationTokenSource();
    cts.CancelAfter(cancelAfter);
    return cts.Token;
}

public class BlogContext : DbContext
{
    public DbSet<MyEntity> MyEntity { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .UseSqlServer("Server=localhost;User=SA;Password=Abcd5678;Connect Timeout=60;ConnectRetryCount=0;Encrypt=false");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MyEntity>().HasNoKey();
    }
}

public class MyEntity
{
    public int X { get; set; }
}
