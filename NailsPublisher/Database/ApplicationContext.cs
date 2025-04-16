namespace NailsPublisher.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class ApplicationContext : DbContext
{
    public DbSet<EntityList.Chat> Chats => Set<EntityList.Chat>();

    public ApplicationContext()
    {
        Database.Migrate();
    }

    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
        
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=database.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EntityList.Chat>()
            .HasMany(u => u.Users)
            .WithOne(i => i.Chat)
            .HasForeignKey(i => i.ChatId);
        
        modelBuilder.Entity<EntityList.User>()
            .HasIndex(i => i.ChatId);
        
        modelBuilder.Entity<EntityList.User>()
            .HasMany(p => p.Posts)
            .WithOne(u => u.User)
            .HasForeignKey(i => i.UserId);
        
        modelBuilder.Entity<EntityList.Post>()
            .HasIndex(i => i.UserId);
        
        modelBuilder.Entity<EntityList.User>()
            .HasMany(p => p.OpenDates)
            .WithOne(u => u.User)
            .HasForeignKey(i => i.UserId);
        
        modelBuilder.Entity<EntityList.OpenDate>()
            .HasIndex(i => i.UserId);
        
        modelBuilder.Entity<EntityList.User>()
            .HasMany(p => p.Products)
            .WithOne(u => u.User)
            .HasForeignKey(i => i.UserId);
        
        modelBuilder.Entity<EntityList.Product>()
            .HasIndex(i => i.UserId);
    }
}

public class ApplicationContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
{
    public ApplicationContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
        optionsBuilder.UseSqlite("Data Source=database.db");
        return new ApplicationContext(optionsBuilder.Options);
    }
}