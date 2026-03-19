using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DesignReview.Domain.Configuration
{
    /// <summary>
    /// Database provider enumeration.
    /// </summary>
    public enum DbProvider
    {
        SqlServer,
        Sqlite,
        PostgreSQL,
        MySql
    }

    /// <summary>
    /// Base class for entity configurations. Provides common configuration patterns.
    /// </summary>
    public abstract class EntityConfigurationBase<T> : IEntityTypeConfiguration<T> where T : class
    {
        private readonly DbProvider dbProvider;

        protected EntityConfigurationBase(DbProvider dbProvider = DbProvider.SqlServer)
        {
            this.dbProvider = dbProvider;
        }

        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            ConfigureEntity(builder);
        }

        protected abstract void ConfigureEntity(EntityTypeBuilder<T> builder);

        protected bool IsSqlite => dbProvider == DbProvider.Sqlite;
        protected bool IsSqlServer => dbProvider == DbProvider.SqlServer;
        protected bool IsPostgreSQL => dbProvider == DbProvider.PostgreSQL;
        protected bool IsMySql => dbProvider == DbProvider.MySql;
    }
}
