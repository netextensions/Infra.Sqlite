using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace NetExtensions.Infra.Sqlite
{
    public static class SqliteExtension
    {
        public static IServiceCollection AddSqlite<TContext>(this IServiceCollection services, string connectionSetting) where TContext : DbContext
        {
            var (ConnectionString, _) = ConnectionStringBuilder(connectionSetting);
            services.AddDbContext<TContext>(c => c.UseSqlite(ConnectionString));
            var options = new DbContextOptionsBuilder<TContext>().UseSqlite(ConnectionString).Options;
            CreateContext(options).Database.Migrate();
            return services.AddSingleton(sp => options);
        }

        public static (string ConnectionString, string DbFilePath) ConnectionStringBuilder(string connectionSetting)
        {
            var connectionString = PrepareConnectionString(connectionSetting);
            var builder = new SqliteConnectionStringBuilder { ConnectionString = connectionString };
            return (connectionString, builder.Values.OfType<string>().FirstOrDefault());
        }

        private static string PrepareConnectionString(string connectionSetting)
        {
            return string.Concat(connectionSetting.Where(c => !char.IsWhiteSpace(c))).ToLowerInvariant().StartsWith("datasource=")
                ? connectionSetting : $"Data Source={connectionSetting}";
        }
        private static TContext CreateContext<TContext>(DbContextOptions<TContext> options) where TContext : DbContext => (TContext)Activator.CreateInstance(typeof(TContext), options);

    }
}
