using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Funq;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.OrmLite;
using System;
using ServiceStack.Data;
using ServiceStack.Redis;
using System.Collections.Generic;
using ServiceStack.IO;
using ServiceStack.Aws.S3;
using Amazon.S3;
using Amazon;
using ServiceStack.VirtualPath;

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

    public class MyServices : Service {}

    public class AppHost : AppHostBase
    {
        
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
                case "s3":
                case "s3virtualfiles":
                    var s3Config = config.FromJsv<S3Config>();
                    var region = RegionEndpoint.GetBySystemName(s3Config.Region);
                    var awsClient = new AmazonS3Client(s3Config.AccessKey, s3Config.SecretKey, region);
                    return new S3VirtualFiles(awsClient, s3Config.Bucket);
                case "mapping":
                case "filesystemmapping":
                    var fsConfig = config.FromJsv<FileSystemMappingConfig>();
                    return new FileSystemMapping(fsConfig.Alias, fsConfig.Path);
            }

            throw new NotSupportedException($"Unknown VirtualFiles Provider '{provider}'");
        }

        IVirtualPathProvider vfs;

        public override List<IVirtualPathProvider> GetVirtualFileSources()
        {
            if (vfs == null)
                return base.GetVirtualFileSources();

            var fileSources = base.GetVirtualFileSources();
            fileSources.Insert(0, vfs);
            return fileSources;
        }

        public override void Configure(Container container)
        {
            SetConfig(new HostConfig {
                DebugMode = AppSettings.Get("debug", true),
            });

            var dbFactory = GetDbFactory(AppSettings.GetString("db"), AppSettings.GetString("db.connection"));
            if (dbFactory != null)
                container.Register<IDbConnectionFactory>(dbFactory);

            var redisConnString = AppSettings.GetString("redis.connection");
            if (redisConnString != null)
                container.Register<IRedisClientsManager>(c => new RedisManagerPool(redisConnString));

            vfs = GetVirtualFiles(AppSettings.GetString("files"), AppSettings.GetString("files.config"));

            Plugins.Add(new TemplatePagesFeature {
                PageFormats = { new MarkdownPageFormat() }
            });
        }
    }
}
