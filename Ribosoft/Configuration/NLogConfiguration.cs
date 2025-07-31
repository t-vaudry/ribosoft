using NLog;
using NLog.Config;
using NLog.Targets;
using System.Diagnostics.CodeAnalysis;

namespace Ribosoft.Configuration
{
    [ExcludeFromCodeCoverage]
    public static class NLogConfiguration
    {
        /// <summary>
        /// Configures NLog with database logging support based on the configured Entity Framework provider.
        /// Falls back to file-only logging if database configuration is not available.
        /// </summary>
        /// <param name="configuration">The application configuration</param>
        public static void Configure(IConfiguration configuration)
        {
            var providerName = configuration["EntityFrameworkProvider"];
            string? connectionString = null;
            string? dbProvider = null;
            string? commandText = null;
            
            if (providerName == "Npgsql")
            {
                connectionString = configuration.GetConnectionString("NpgsqlConnection");
                dbProvider = "Npgsql.NpgsqlConnection, Npgsql";
                // Use PostgreSQL-compatible syntax with double quotes
                commandText = @"INSERT INTO ""ActivityLogs"" (""Timestamp"", ""LogLevel"", ""Category"", ""Message"", ""Exception"", ""UserName"", ""IpAddress"", ""RequestPath"", ""RequestMethod"", ""Properties"") 
                               VALUES (@timestamp::timestamptz, @level, @logger, @message, @exception, @username, @ipaddress, @requestpath, @requestmethod, @properties)";
            }
            else if (providerName == "SqlServer")
            {
                connectionString = configuration.GetConnectionString("SqlServerConnection");
                dbProvider = "System.Data.SqlClient.SqlConnection, System.Data.SqlClient";
                // Use SQL Server syntax with square brackets
                commandText = @"INSERT INTO [ActivityLogs] ([Timestamp], [LogLevel], [Category], [Message], [Exception], [UserName], [IpAddress], [RequestPath], [RequestMethod], [Properties]) 
                               VALUES (@timestamp, @level, @logger, @message, @exception, @username, @ipaddress, @requestpath, @requestmethod, @properties)";
            }
            
            if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(dbProvider) && !string.IsNullOrEmpty(commandText))
            {
                ConfigureWithDatabase(connectionString, dbProvider, commandText);
            }
            else
            {
                // Fall back to file-only configuration
                LogManager.Setup().LoadConfigurationFromFile("nlog.config");
            }
        }
        
        private static void ConfigureWithDatabase(string connectionString, string dbProvider, string commandText)
        {
            var config = new LoggingConfiguration();
            
            // Load existing configuration from file
            var fileConfig = new XmlLoggingConfiguration("nlog.config");
            
            // Copy existing targets and rules (excluding database target)
            foreach (var target in fileConfig.AllTargets)
            {
                if (target.Name != "database")
                {
                    config.AddTarget(target);
                }
            }
            
            foreach (var rule in fileConfig.LoggingRules)
            {
                if (!rule.Targets.Any(t => t.Name == "database"))
                {
                    config.LoggingRules.Add(rule);
                }
            }
            
            // Create and configure database target
            var databaseTarget = CreateDatabaseTarget(connectionString, dbProvider, commandText);
            config.AddTarget(databaseTarget);
            
            // Add database logging rules
            AddDatabaseLoggingRules(config, databaseTarget);
            
            // Apply the configuration
            LogManager.Configuration = config;
        }
        
        private static DatabaseTarget CreateDatabaseTarget(string connectionString, string dbProvider, string commandText)
        {
            var databaseTarget = new DatabaseTarget("database")
            {
                ConnectionString = connectionString,
                DBProvider = dbProvider,
                CommandText = commandText,
                KeepConnection = false
            };
            
            // Add database parameters
            AddDatabaseParameters(databaseTarget);
            
            return databaseTarget;
        }
        
        private static void AddDatabaseParameters(DatabaseTarget databaseTarget)
        {
            databaseTarget.Parameters.Add(new DatabaseParameterInfo("@timestamp", "${date:universalTime=true:format=yyyy-MM-dd HH\\:mm\\:ss.fff zzz}"));
            
            // Convert short log levels to full names to match ActivityLogService format
            var levelLayout = "${replace:inner=${replace:inner=${replace:inner=${replace:inner=${level}:searchFor=Info:replaceWith=Information}:searchFor=Warn:replaceWith=Warning}:searchFor=Error:replaceWith=Error}:searchFor=Debug:replaceWith=Debug}";
            databaseTarget.Parameters.Add(new DatabaseParameterInfo("@level", levelLayout));
            
            databaseTarget.Parameters.Add(new DatabaseParameterInfo("@logger", "${logger}"));
            databaseTarget.Parameters.Add(new DatabaseParameterInfo("@message", "${message}"));
            databaseTarget.Parameters.Add(new DatabaseParameterInfo("@exception", "${exception:format=tostring}"));
            databaseTarget.Parameters.Add(new DatabaseParameterInfo("@username", "${aspnet-user-identity}"));
            databaseTarget.Parameters.Add(new DatabaseParameterInfo("@ipaddress", "${aspnet-request-ip}"));
            databaseTarget.Parameters.Add(new DatabaseParameterInfo("@requestpath", "${aspnet-request-url:IncludeHost=false:IncludePort=false:IncludeQueryString=false}"));
            databaseTarget.Parameters.Add(new DatabaseParameterInfo("@requestmethod", "${aspnet-request-method}"));
            databaseTarget.Parameters.Add(new DatabaseParameterInfo("@properties", "${all-event-properties}"));
        }
        
        private static void AddDatabaseLoggingRules(LoggingConfiguration config, DatabaseTarget databaseTarget)
        {
            // Application-specific logging
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, databaseTarget, "Ribosoft.*");
            config.AddRule(NLog.LogLevel.Warn, NLog.LogLevel.Fatal, databaseTarget, "NCBI.Datasets.API.*");
            
            // Hangfire - specific rules only (no broad "Hangfire.*" to avoid duplicates)
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, databaseTarget, "Hangfire.PostgreSql.*");
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, databaseTarget, "Hangfire.SqlServer.*");
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, databaseTarget, "Hangfire.Processing.*");
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, databaseTarget, "Hangfire.Server.*");
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, databaseTarget, "Hangfire.BackgroundJobServer");
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, databaseTarget, "Hangfire.Storage.*");
            
            // Microsoft framework logs - important events only
            config.AddRule(NLog.LogLevel.Warn, NLog.LogLevel.Fatal, databaseTarget, "Microsoft.AspNetCore.Authentication.*");
            config.AddRule(NLog.LogLevel.Warn, NLog.LogLevel.Fatal, databaseTarget, "Microsoft.AspNetCore.Authorization.*");
            config.AddRule(NLog.LogLevel.Warn, NLog.LogLevel.Fatal, databaseTarget, "Microsoft.EntityFrameworkCore.*");
            
            // Capture errors from other Microsoft/System components
            config.AddRule(NLog.LogLevel.Error, NLog.LogLevel.Fatal, databaseTarget, "Microsoft.*");
            config.AddRule(NLog.LogLevel.Error, NLog.LogLevel.Fatal, databaseTarget, "System.*");
        }
    }
}
