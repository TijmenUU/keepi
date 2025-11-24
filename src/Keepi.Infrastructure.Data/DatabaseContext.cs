using EntityFramework.Exceptions.Sqlite;
using Keepi.Infrastructure.Data.Entries;
using Keepi.Infrastructure.Data.InvoiceItems;
using Keepi.Infrastructure.Data.Projects;
using Keepi.Infrastructure.Data.UserInvoiceItemCustomizations;
using Keepi.Infrastructure.Data.Users;
using Microsoft.EntityFrameworkCore;

namespace Keepi.Infrastructure.Data;

internal class DatabaseContext : DbContext
{
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<ProjectEntity> Projects { get; set; }
    public DbSet<InvoiceItemEntity> InvoiceItems { get; set; }
    public DbSet<UserEntryEntity> UserEntries { get; set; }
    public DbSet<UserInvoiceItemCustomizationEntity> UserInvoiceItemCustomizations { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Adds strongly typed exceptions
        // See https://github.com/Giorgi/EntityFramework.Exceptions/blob/main/EntityFramework.Exceptions.Sqlite/SqliteExceptionProcessorInterceptor.cs
        optionsBuilder.UseExceptionProcessor();
    }
}
