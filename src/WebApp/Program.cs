﻿using System;
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

namespace WebApp
{
    public class Program
    {
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

            WebTemplateUtils.AppSettings = new MultiAppSettings(usingWebSettings
                    ? new TextFileSettings(appSettingsPath)
                    : new DictionarySettings(),
                new EnvironmentVariableSettings());

            var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var webRoot = Directory.Exists(wwwrootPath)
                ? wwwrootPath
                : Directory.GetCurrentDirectory();

            var port = "port".GetAppSetting(defaultValue:"5000");
            var contentRoot = "contentRoot".GetAppSetting(defaultValue:Directory.GetCurrentDirectory());
            if (contentRoot.StartsWith("~/"))
                contentRoot = contentRoot.MapAbsolutePath();

            var useWebRoot = "webRoot".GetAppSetting(webRoot);
            if (useWebRoot.StartsWith("~/"))
                useWebRoot = useWebRoot.MapAbsolutePath();

            var bind = "bind".GetAppSetting("localhost");
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(contentRoot)
                .UseWebRoot(useWebRoot)
                .UseStartup<Startup>()
                .UseUrls($"http://{bind}:{port}/")
                .Build();

            host.Run();
        }
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services) {}

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            WebTemplateUtils.VirtualFiles = new FileSystemVirtualFiles(env.ContentRootPath);
            
            AppHostBase appHost = null;

            var assemblies = new List<Assembly>();
            var vfs = "files".GetAppSetting().GetVirtualFiles(config:"files.config".GetAppSetting());
            var pluginsDir = (vfs ?? WebTemplateUtils.VirtualFiles).GetDirectory("plugins");
            if (pluginsDir != null)
            {
                var plugins = pluginsDir.GetFiles();
                foreach (var plugin in plugins)
                {
                    if (plugin.Extension != "dll" && plugin.Extension != "exe")
                        continue;

                    var dllBytes = plugin.ReadAllBytes();
                    $"Attempting to load plugin '{plugin.VirtualPath}', size: {dllBytes.Length} bytes".Print();
                    var asm = Assembly.Load(dllBytes);
                    assemblies.Add(asm);

                    if (appHost == null)
                    {
                        foreach (var type in asm.GetTypes())
                        {
                            if (typeof(AppHostBase).IsAssignableFromType(type))
                            {
                                $"Using AppHost from Plugin '{plugin.VirtualPath}'".Print();
                                appHost = type.CreateInstance<AppHostBase>();
                                appHost.AppSettings = WebTemplateUtils.AppSettings;
                                break;
                            }
                        }
                    }
                }
            }

            if (appHost == null)
                appHost = new AppHost();

            if (assemblies.Count > 0)
                assemblies.Each(x => appHost.ServiceAssemblies.AddIfNotExists(x));

            if (vfs != null)
                appHost.AddVirtualFileSources.Add(vfs);

            if (vfs is IVirtualFiles writableFs)
                appHost.VirtualFiles = writableFs;

            appHost.BeforeConfigure.Add(ConfigureAppHost);

            app.UseServiceStack(appHost);
        }

        public void ConfigureAppHost(ServiceStackHost appHost)
        {
            appHost.Config.DebugMode = "debug".GetAppSetting(true);

            var feature = appHost.GetPlugin<TemplatePagesFeature>();
            if (feature != null)
                "Using existing TemplatePagesFeature from appHost".Print();

            if (feature == null)
            {
                feature = (nameof(TemplatePagesFeature).GetAppSetting() != null
                    ? (TemplatePagesFeature)typeof(TemplatePagesFeature).CreatePlugin()
                    : new TemplatePagesFeature { ApiPath = "apiPath".GetAppSetting() ?? "/api" });
            }

            var dbFactory = "db".GetAppSetting().GetDbFactory(connectionString:"db.connection".GetAppSetting());
            if (dbFactory != null)
            {
                appHost.Container.Register<IDbConnectionFactory>(dbFactory);
                feature.TemplateFilters.Add(new TemplateDbFiltersAsync());
            }

            var redisConnString = "redis.connection".GetAppSetting();
            if (redisConnString != null)
            {
                appHost.Container.Register<IRedisClientsManager>(c => new RedisManagerPool(redisConnString));
                feature.TemplateFilters.Add(new TemplateRedisFilters { 
                    RedisManager = appHost.Container.Resolve<IRedisClientsManager>()
                });
            }

           var checkForModifiedPagesAfterSecs = "checkForModifiedPagesAfterSecs".GetAppSetting();
            if (checkForModifiedPagesAfterSecs != null)
                feature.CheckForModifiedPagesAfter = TimeSpan.FromSeconds(checkForModifiedPagesAfterSecs.ConvertTo<int>());

            var defaultFileCacheExpirySecs = "defaultFileCacheExpirySecs".GetAppSetting();
            if (defaultFileCacheExpirySecs != null)
                feature.Args[TemplateConstants.DefaultFileCacheExpiry] = TimeSpan.FromSeconds(defaultFileCacheExpirySecs.ConvertTo<int>());

            var defaultUrlCacheExpirySecs = "defaultUrlCacheExpirySecs".GetAppSetting();
            if (defaultUrlCacheExpirySecs != null)
                feature.Args[TemplateConstants.DefaultUrlCacheExpiry] = TimeSpan.FromSeconds(defaultUrlCacheExpirySecs.ConvertTo<int>());

            var contextArgKeys = WebTemplateUtils.AppSettings.GetAllKeys().Where(x => x.StartsWith("args."));
            foreach (var key in contextArgKeys)
            {
                var name = key.RightPart('.');
                var value = key.GetAppSetting();
                feature.Args[name] = value;
            }

            appHost.Plugins.Add(feature);

            var features = "features".GetAppSetting();
            if (features != null)
            {
                var featureTypes = features.Split(',').Map(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                featureTypes.Remove(nameof(TemplatePagesFeature)); //already added

                var externalPlugins = new[] {
                    typeof(ServiceStack.Api.OpenApi.OpenApiFeature),
                    typeof(ServiceStack.AutoQueryFeature), 
                };

                foreach (var type in externalPlugins)
                {
                    if (featureTypes.Contains(type.Name))
                    {
                        var plugin = type.CreatePlugin();
                        appHost.Plugins.RemoveAll(x => x.GetType() == type);
                        appHost.Plugins.Add(plugin);
                        featureTypes.Remove(type.Name);
                    }
                }

                foreach (var type in typeof(ServiceStackHost).GetAssembly().GetTypes())
                {
                    if (featureTypes.Count == 0)
                        break;

                    if (featureTypes.Contains(type.Name))
                    {
                        var plugin = type.CreatePlugin();
                        appHost.Plugins.RemoveAll(x => x.GetType() == type);
                        appHost.Plugins.Add(plugin);
                        featureTypes.Remove(type.Name);
                    }
                }

                foreach (var type in appHost.ServiceAssemblies.SelectMany(x => x.GetTypes()))
                {
                    if (featureTypes.Count == 0)
                        break;

                    if (featureTypes.Contains(type.Name))
                    {
                        var plugin = type.CreatePlugin();
                        appHost.Plugins.RemoveAll(x => x.GetType() == type);
                        appHost.Plugins.Add(plugin);
                        featureTypes.Remove(type.Name);
                    }
                }

                if (featureTypes.Count > 0)
                {
                    var plural = featureTypes.Count > 1 ? "s" : "";
                    throw new NotSupportedException($"Unable to locate plugin{plural}: " + string.Join(", ", featureTypes));
                }
            }
        }
    }

    public class MyServices : Service {}

    public class AppHost : AppHostBase
    {
        public AppHost()
            : base("name".GetAppSetting("ServiceStack WebTemplate"), typeof(MyServices).GetAssembly()) {}

        public override void Configure(Container container) {}
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

    public class WebAppFilters : TemplateFilter
    {
        public string dirPath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || filePath[filePath.Length - 1] == '/')
                return null;

            var lastDirPos = filePath.LastIndexOf('/');
            return lastDirPos >= 0
                ? filePath.Substring(0, lastDirPos)
                : null;
        }

        public string resolveAsset(TemplateScopeContext scope, string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
                return string.Empty;

            if (!scope.Context.Args.TryGetValue("assetsBase", out object assetsBase))
                return virtualPath;

            return virtualPath[0] == '/'
                ? assetsBase.ToString().CombineWith(virtualPath).ResolvePaths()
                : assetsBase.ToString().CombineWith(dirPath(scope.Page.VirtualPath), virtualPath).ResolvePaths();
        }
    }

    public static class WebTemplateUtils
    {
        public static IAppSettings AppSettings;
        public static IVirtualFiles VirtualFiles;

        public static string ResolveValue(this string value)
        {
            if (value?.StartsWith("$") == true)
            {
                var envValue = Environment.GetEnvironmentVariable(value.Substring(1));
                if (!string.IsNullOrEmpty(envValue))
                    return envValue;
            }
            return value;
        }

        public static string GetAppSetting(this string name) => ResolveValue(AppSettings.GetString(name));

        public static T GetAppSetting<T>(this string name, T defaultValue)
        {
            var value = AppSettings.GetString(name);
            if (value == null)
                return defaultValue;

            var resolvedValue = ResolveValue(value);
            return resolvedValue.FromJsv<T>();
        }

        public static IVirtualPathProvider GetVirtualFiles(this string provider, string config)
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
                    var region = RegionEndpoint.GetBySystemName(s3Config.Region.ResolveValue());
                    s3Config.AccessKey = s3Config.AccessKey.ResolveValue();
                    s3Config.SecretKey = s3Config.SecretKey.ResolveValue();
                    var awsClient = new AmazonS3Client(s3Config.AccessKey, s3Config.SecretKey, region);
                    return new S3VirtualFiles(awsClient, s3Config.Bucket.ResolveValue());
                case "mapping":
                case "filesystemmapping":
                    var fsConfig = config.FromJsv<FileSystemMappingConfig>();
                    return new FileSystemMapping(fsConfig.Alias.ResolveValue(), fsConfig.Path.ResolveValue());
            }

            throw new NotSupportedException($"Unknown VirtualFiles Provider '{provider}'");
        }

        public static OrmLiteConnectionFactory GetDbFactory(this string dbProvider, string connectionString)
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

        public static IPlugin CreatePlugin(this Type type)
        {
            if (!type.HasInterface(typeof(IPlugin)))
                throw new NotSupportedException($"'{type.Name}' is not a ServiceStack IPlugin");
            
            var pluginConfig = type.Name.GetAppSetting();
            if (pluginConfig != null)
            {
                pluginConfig.ToStringSegment().ParseNextToken(out object value, out _);
                if (value is Dictionary<string, object> objDictionary)
                {
                    $"Creating '{type.Name}' with: {pluginConfig}".Print();
                    var plugin = objDictionary.FromObjectDictionary(type);
                    return (IPlugin)plugin;
                }
                else throw new NotSupportedException($"'{pluginConfig}' is not an Object Dictionary");
            }
            else
            {
                $"Registering Plugin '{type.Name}'".Print();
                var plugin = type.CreateInstance<IPlugin>();
                return plugin;
            }
        }
    }
}