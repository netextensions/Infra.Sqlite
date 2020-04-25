using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace NetExtensions
{
    public static class SqliteExtension
    {
        public static IServiceCollection AddSqliteDb<TContext>(this IServiceCollection services, string connectionString) where TContext : DbContext
        {
            services.AddDbContext<TContext>(c => c.UseSqlite(connectionString));
            var options = new DbContextOptionsBuilder<TContext>()
                .UseSqlite(connectionString)
                .Options;

            using var context = CreateContext(options);
            context.Database.Migrate();

            services.AddSingleton(sp => options);
            return services;
        }

        private static TContext CreateContext<TContext>(DbContextOptions<TContext> options) where TContext : DbContext => (TContext)Activator.CreateInstance(typeof(TContext), options);

        public static (string ConnectionString, string DbFilePath) ConnectionStringBuilder(string defaultDatabaseFileName, string connectionSetting)
        {
            var connectionString = PrepareConnectionString(defaultDatabaseFileName, connectionSetting);
            var builder = new SqliteConnectionStringBuilder { ConnectionString = connectionString };
            var dbPath = builder.Values.OfType<string>().FirstOrDefault();
            return (connectionString, dbPath);
        }

        private static string PrepareConnectionString(string defaultDatabaseFileName, string connectionSetting)
        {
            var connectionString = ChooseDataSource(defaultDatabaseFileName, connectionSetting);
            return (string.Concat(connectionString.Where(c => !char.IsWhiteSpace(c))).ToLowerInvariant()).StartsWith("datasource=")
                ? connectionString : $"Data Source={connectionString}";
        }

        private static string ChooseDataSource(string defaultDatabaseFileName, string connectionSetting)
        {
            return !string.IsNullOrWhiteSpace(connectionSetting) ? connectionSetting
                : $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\{defaultDatabaseFileName}";
        }
    }
}
