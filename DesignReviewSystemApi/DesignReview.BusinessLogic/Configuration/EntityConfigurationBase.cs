// This file has been moved to DesignReview.Domain.Configuration
// Keeping this file for backward compatibility - it will be removed in future versions
using DesignReview.Domain.Configuration;

namespace DesignReview.BusinessLogic.Configuration
{
    /// <summary>
    /// Database provider enumeration (moved to Domain.Configuration).
    /// </summary>
    public enum DbProvider
    {
        SqlServer = Domain.Configuration.DbProvider.SqlServer,
        Sqlite = Domain.Configuration.DbProvider.Sqlite,
        PostgreSQL = Domain.Configuration.DbProvider.PostgreSQL,
        MySql = Domain.Configuration.DbProvider.MySql
    }

    /// <summary>
    /// Base class for entity configurations (moved to Domain.Configuration).
    /// </summary>
    public abstract class EntityConfigurationBase<T> : Domain.Configuration.EntityConfigurationBase<T> where T : class
    {
        protected EntityConfigurationBase(DbProvider dbProvider = DbProvider.SqlServer) 
            : base((Domain.Configuration.DbProvider)dbProvider)
        {
        }
    }
}
