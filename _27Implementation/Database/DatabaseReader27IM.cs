using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using FDCommon.LibBase.LibBase14LB;

namespace ERD2DB.Implementation.Database
{
    public class DatabaseReader27IM
    {
        private readonly string _connectionString;
        private readonly Logging14LB _logger;

        public event EventHandler<DatabasesLoadedEventArgs> DatabasesLoaded;
        public event EventHandler<string> ConnectionError;

        public DatabaseReader27IM(string server, string username = null, string password = null)
        {
            _logger = Logging14LB.Instance;

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = server,
                IntegratedSecurity = string.IsNullOrEmpty(username),
                TrustServerCertificate = true
            };

            if (!string.IsNullOrEmpty(username))
            {
                builder.UserID = username;
                builder.Password = password;
            }

            _connectionString = builder.ConnectionString;
        }

        public async Task ConnectAndLoadDatabasesAsync()
        {
            try
            {
                _logger.LogMethodEntry(nameof(ConnectAndLoadDatabasesAsync), nameof(DatabaseReader27IM));

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var databases = new List<DatabaseInfo>();

                    using (var command = new SqlCommand(@"
                        SELECT 
                            name,
                            database_id,
                            create_date,
                            state_desc,
                            user_access_desc,
                            recovery_model_desc
                        FROM sys.databases 
                        WHERE database_id > 4 
                        ORDER BY name", connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                databases.Add(new DatabaseInfo
                                {
                                    Name = reader["name"].ToString(),
                                    DatabaseId = Convert.ToInt32(reader["database_id"]),
                                    CreateDate = Convert.ToDateTime(reader["create_date"]),
                                    State = reader["state_desc"].ToString(),
                                    UserAccess = reader["user_access_desc"].ToString(),
                                    RecoveryModel = reader["recovery_model_desc"].ToString()
                                });
                            }
                        }
                    }

                    // Get size information for each database
                    foreach (var db in databases)
                    {
                        try
                        {
                            using (var command = new SqlCommand($@"
                                USE [{db.Name}];
                                SELECT 
                                    SUM(size * 8.0 / 1024) as SizeMB,
                                    (SELECT COUNT(*) FROM sys.tables) as TableCount
                                FROM sys.database_files;", connection))
                            {
                                using (var reader = await command.ExecuteReaderAsync())
                                {
                                    if (await reader.ReadAsync())
                                    {
                                        db.SizeMB = Convert.ToDouble(reader["SizeMB"]);
                                        db.TableCount = Convert.ToInt32(reader["TableCount"]);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Failed to get size info for database {db.Name}", ex);
                            db.SizeMB = 0;
                            db.TableCount = 0;
                        }
                    }

                    DatabasesLoaded?.Invoke(this, new DatabasesLoadedEventArgs(databases));
                    _logger.LogInformation($"Loaded {databases.Count} databases");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to connect to database server", ex);
                ConnectionError?.Invoke(this, ex.Message);
                throw;
            }
            finally
            {
                _logger.LogMethodExit(nameof(ConnectAndLoadDatabasesAsync), nameof(DatabaseReader27IM));
            }
        }

        public async Task<List<TableInfo>> GetDatabaseTablesAsync(string databaseName)
        {
            try
            {
                _logger.LogMethodEntry(nameof(GetDatabaseTablesAsync), nameof(DatabaseReader27IM));

                var tables = new List<TableInfo>();

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    connection.ChangeDatabase(databaseName);

                    using (var command = new SqlCommand(@"
                        SELECT 
                            s.name as SchemaName,
                            t.name as TableName,
                            p.rows as RowCount,
                            (SELECT COUNT(*) FROM sys.columns c WHERE c.object_id = t.object_id) as ColumnCount
                        FROM sys.tables t
                        INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                        INNER JOIN sys.partitions p ON t.object_id = p.object_id
                        WHERE p.index_id IN (0,1)
                        ORDER BY s.name, t.name", connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                tables.Add(new TableInfo
                                {
                                    SchemaName = reader["SchemaName"].ToString(),
                                    TableName = reader["TableName"].ToString(),
                                    RowCount = Convert.ToInt64(reader["RowCount"]),
                                    ColumnCount = Convert.ToInt32(reader["ColumnCount"])
                                });
                            }
                        }
                    }

                    _logger.LogInformation($"Loaded {tables.Count} tables from database {databaseName}");
                }

                return tables;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get tables from database {databaseName}", ex);
                throw;
            }
            finally
            {
                _logger.LogMethodExit(nameof(GetDatabaseTablesAsync), nameof(DatabaseReader27IM));
            }
        }
    }

    public class DatabaseInfo
    {
        public string Name { get; set; }
        public int DatabaseId { get; set; }
        public DateTime CreateDate { get; set; }
        public string State { get; set; }
        public string UserAccess { get; set; }
        public string RecoveryModel { get; set; }
        public double SizeMB { get; set; }
        public int TableCount { get; set; }
    }

    public class TableInfo
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public long RowCount { get; set; }
        public int ColumnCount { get; set; }
    }

    public class DatabasesLoadedEventArgs : EventArgs
    {
        public List<DatabaseInfo> Databases { get; }

        public DatabasesLoadedEventArgs(List<DatabaseInfo> databases)
        {
            Databases = databases;
        }
    }
}