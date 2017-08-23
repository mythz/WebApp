using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Funq;
using ServiceStack;
using ServiceStack.Text;
using ServiceStack.Configuration;
using ServiceStack.OrmLite;
using ServiceStack.Data;
using ServiceStack.Redis;
using ServiceStack.IO;
using ServiceStack.Aws.S3;
using ServiceStack.VirtualPath;
using ServiceStack.Templates;

using Amazon.S3;
using Amazon;

namespace TemplateWebsites
{
    public class Program
    {
        public static IAppSettings AppSettings;
        public static void Main(string[] args)
        {
            var webSettings = args.Length > 0 
                ? args[0]
                : "web.settings";

            var appSettingsPath = $"~/{webSettings}".MapAbsolutePath();
            if (args.Length > 0 && !File.Exists(appSettingsPath))
            {
                Console.WriteLine($"'{appSettingsPath}' does not exist");
                return;
            }

            var usingWebSettings = File.Exists(appSettingsPath);
            if (usingWebSettings)
                Console.WriteLine($"Using '{webSettings}'");

            AppSettings = new MultiAppSettings(usingWebSettings
                    ? new TextFileSettings(appSettingsPath)
                    : new DictionarySettings(),
                new EnvironmentVariableSettings());

            var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var webRoot = Directory.Exists(wwwrootPath)
                ? wwwrootPath
                : Directory.GetCurrentDirectory();

            var port = AppSettings.Get("port", "5000");
            var contentRoot = AppSettings.Get("contentRoot", Directory.GetCurrentDirectory());
            if (contentRoot.StartsWith("~/"))
                contentRoot = contentRoot.MapAbsolutePath();

            var useWebRoot = AppSettings.Get("webRoot", webRoot);
            if (useWebRoot.StartsWith("~/"))
                useWebRoot = useWebRoot.MapAbsolutePath();

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(contentRoot)
                .UseWebRoot(useWebRoot)
                .UseStartup<Startup>()
                .UseUrls($"http://localhost:{port}/")
                .Build();

            host.Run();
        }
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseServiceStack(new AppHost());
        }
    }

    public class MyServices : Service 
    {
    }

    public class AppHost : AppHostBase
    {
        public static string ResolveValue(string value)
        {
            if (value?.StartsWith("$") == true)
            {
                var envValue = Environment.GetEnvironmentVariable(value.Substring(1));
                if (!string.IsNullOrEmpty(envValue))
                    return envValue;
            }
            return value;
        }

        public string GetAppSetting(string name) => ResolveValue(AppSettings.GetString(name));

        public T GetAppSetting<T>(string name, T defaultValue)
        {
            var value = AppSettings.GetString(name);
            if (value == null)
                return defaultValue;

            var resolvedValue = ResolveValue(value);
            return resolvedValue.FromJsv<T>();
        }

        public AppHost()
            : base(Program.AppSettings.Get("name", "ServiceStack Template Website"), typeof(MyServices).GetAssembly()) 
        { 
            AppSettings = Program.AppSettings;
        }

        public OrmLiteConnectionFactory GetDbFactory(string dbProvider, string connectionString)
        {
            if (dbProvider == null || connectionString == null)
                return null;

            switch (dbProvider.ToLower())
            {
                case "sqlite":
                    if (connectionString.StartsWith("~/"))
                    {
                        var file = VirtualFiles.GetFile(connectionString.Substring(2));
                        if (file != null)
                            connectionString = file.RealPath;
                    }
                    if (!File.Exists(connectionString))
                        throw new FileNotFoundException($"SQLite database not found at '{connectionString}'");
                    return new OrmLiteConnectionFactory(connectionString, SqliteDialect.Provider);
                case "mssql":
                case "sqlserver":
                    return new OrmLiteConnectionFactory(connectionString, SqlServerDialect.Provider);
                case "sqlserver2012":
                    return new OrmLiteConnectionFactory(connectionString, SqlServer2012Dialect.Provider);
                case "sqlserver2014":
                    return new OrmLiteConnectionFactory(connectionString, SqlServer2014Dialect.Provider);
                case "sqlserver2016":
                    return new OrmLiteConnectionFactory(connectionString, SqlServer2016Dialect.Provider);
                case "sqlserver2017":
                    return new OrmLiteConnectionFactory(connectionString, SqlServer2017Dialect.Provider);
                case "mysql":
                    return new OrmLiteConnectionFactory(connectionString, MySqlDialect.Provider);
                case "pgsql":
                case "postgres":
                case "postgresql":
                    return new OrmLiteConnectionFactory(connectionString, PostgreSqlDialect.Provider);
            }

            throw new NotSupportedException($"Unknown DB Provider '{dbProvider}'");
        }

        public class AwsConfig
        {
            public string AccessKey { get; set; }
            public string SecretKey { get; set; }
            public string Region { get; set; }
        }

        public class S3Config : AwsConfig
        {
            public string Bucket { get; set; }
        }

        public class FileSystemMappingConfig
        {
            public string Alias { get; set; }
            public string Path { get; set; }
        }

        public IVirtualPathProvider GetVirtualFiles(string provider, string config)
        {
            if (provider == null)
                return null;

            switch (provider.ToLower())
            {
                case "fs":
                case "filesystem":
                    if (config.StartsWith("~/"))
                    {
                        var dir = VirtualFiles.GetDirectory(config.Substring(2));
                        if (dir != null)
                            config = dir.RealPath;
                    }
                    return new FileSystemVirtualFiles(config);
                case "s3":
                case "s3virtualfiles":
                    var s3Config = config.FromJsv<S3Config>();
                    var region = RegionEndpoint.GetBySystemName(ResolveValue(s3Config.Region));
                    var awsClient = new AmazonS3Client(
                        ResolveValue(s3Config.AccessKey), 
                        ResolveValue(s3Config.SecretKey), 
                        region);
                    return new S3VirtualFiles(awsClient, ResolveValue(s3Config.Bucket));
                case "mapping":
                case "filesystemmapping":
                    var fsConfig = config.FromJsv<FileSystemMappingConfig>();
                    return new FileSystemMapping(ResolveValue(fsConfig.Alias), ResolveValue(fsConfig.Path));
            }

            throw new NotSupportedException($"Unknown VirtualFiles Provider '{provider}'");
        }

        IVirtualPathProvider vfs;

        public override List<IVirtualPathProvider> GetVirtualFileSources()
        {
            if (vfs == null)
                return base.GetVirtualFileSources();

            var fileSources = base.GetVirtualFileSources();
            fileSources.Add(vfs);
            return fileSources;
        }

        public override void Configure(Container container)
        {
            SetConfig(new HostConfig {
                DebugMode = GetAppSetting("debug", true),
            });

            var feature = new TemplatePagesFeature {
                PageFormats = { new MarkdownPageFormat() },
                ApiPath = GetAppSetting("apiPath") ?? "/api",
                CheckForModifiedPages = GetAppSetting("checkForModifiedPages", true),
            };

            var dbFactory = GetDbFactory(GetAppSetting("db"), GetAppSetting("db.connection"));
            if (dbFactory != null)
            {
                container.Register<IDbConnectionFactory>(dbFactory);
                feature.TemplateFilters.Add(new TemplateDbFilters());
            }

            var redisConnString = GetAppSetting("redis.connection");
            if (redisConnString != null)
            {
                container.Register<IRedisClientsManager>(c => new RedisManagerPool(redisConnString));
                feature.TemplateFilters.Add(new TemplateRedisFilters { 
                    RedisManager = container.Resolve<IRedisClientsManager>()
                });
            }

            vfs = GetVirtualFiles(GetAppSetting("files"), GetAppSetting("files.config"));
            if (vfs is IVirtualFiles writableFs)
                VirtualFiles = writableFs;

            var checkForModifiedPagesAfter = GetAppSetting("checkForModifiedPagesAfter");
            if (checkForModifiedPagesAfter != null)
                feature.CheckForModifiedPagesAfter = checkForModifiedPagesAfter.ConvertTo<TimeSpan>();

            var checkForModifiedPagesAfterSecs = GetAppSetting("checkForModifiedPagesAfterSecs");
            if (checkForModifiedPagesAfterSecs != null)
                feature.CheckForModifiedPagesAfter = TimeSpan.FromSeconds(checkForModifiedPagesAfterSecs.ConvertTo<int>());

            var defaultFileCacheExpirySecs = GetAppSetting("defaultFileCacheExpirySecs");
            if (defaultFileCacheExpirySecs != null)
                feature.Args[TemplateConstants.DefaultFileCacheExpiry] = TimeSpan.FromSeconds(defaultFileCacheExpirySecs.ConvertTo<int>());

            var defaultUrlCacheExpirySecs = GetAppSetting("defaultUrlCacheExpirySecs");
            if (defaultUrlCacheExpirySecs != null)
                feature.Args[TemplateConstants.DefaultUrlCacheExpiry] = TimeSpan.FromSeconds(defaultUrlCacheExpirySecs.ConvertTo<int>());

            var contextArgKeys = AppSettings.GetAllKeys().Where(x => x.StartsWith("args."));
            foreach (var key in contextArgKeys)
            {
                var name = key.RightPart('.');
                var value = GetAppSetting(key);
                feature.Args[name] = value;
            }

            Plugins.Add(feature);
        }
    }
}
