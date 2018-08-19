using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Funq;
using ServiceStack;
using ServiceStack.IO;
using ServiceStack.Auth;
using ServiceStack.Text;
using ServiceStack.Data;
using ServiceStack.Redis;
using ServiceStack.Aws.S3;
using ServiceStack.OrmLite;
using ServiceStack.Templates;
using ServiceStack.VirtualPath;
using ServiceStack.Configuration;
using ServiceStack.Azure.Storage;

namespace WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var dotnetArgs = args;
            var webSettings = "web.settings";
            if (args.Length == 1)
            {
                webSettings = args[0];
                dotnetArgs = dotnetArgs.Skip(1).ToArray();
            }

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

            var port = "port".GetAppSetting(defaultValue:"5000");
            var contentRoot = "contentRoot".GetAppSetting(defaultValue:Directory.GetCurrentDirectory());
            if (contentRoot.StartsWith("~/"))
                contentRoot = contentRoot.MapAbsolutePath();

            var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var webRoot = Directory.Exists(wwwrootPath)
                ? wwwrootPath
                : contentRoot;

            var useWebRoot = "webRoot".GetAppSetting(webRoot);
            if (useWebRoot.StartsWith("~/"))
                useWebRoot = useWebRoot.MapAbsolutePath();

            var bind = "bind".GetAppSetting("localhost");
            var builder = WebHost.CreateDefaultBuilder(dotnetArgs)
                .UseContentRoot(contentRoot)
                .UseWebRoot(useWebRoot)
                .UseStartup<Startup>();

            if (Environment.GetEnvironmentVariable("ASPNETCORE_URLS") == null)
                builder.UseUrls($"http://{bind}:{port}/");

            var host = builder.Build();
            host.Run();
        }
    }

    public class Startup
    {
        IHostingEnvironment env;
        public Startup(IHostingEnvironment env) => this.env = env;

        IPlugin[] plugins;
        IPlugin[] Plugins 
        {
            get
            {
                if (plugins != null)
                    return plugins;

                var features = "features".GetAppSetting();
                if (features != null)
                {
                    var featureTypes = features.Split(',').Map(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                    featureTypes.Remove(nameof(TemplatePagesFeature)); //already added
                    var featureIndex = featureTypes.ToArray();
                    var registerPlugins = new IPlugin[featureTypes.Count];

                    foreach (var type in appHost.ScanAllTypes())
                    {
                        if (featureTypes.Count == 0)
                            break;

                        if (featureTypes.Contains(type.Name))
                        {
                            registerPlugins[Array.IndexOf(featureIndex, type.Name)] = type.CreatePlugin();
                            featureTypes.Remove(type.Name);
                        }
                    }

                    //Register any wildcard plugins at end
                    const string AllRemainingPlugins = "plugins/*";
                    if (featureTypes.Count == 1 && featureTypes[0] == AllRemainingPlugins)
                    {
                        var remainingPlugins = new List<IPlugin>();
                        foreach (var type in typeof(ServiceStackHost).Assembly.GetTypes())
                        {
                            if (type.HasInterface(typeof(IPlugin)) && !registerPlugins.Any(x => x?.GetType() == type))
                            {
                                var plugin = type.CreatePlugin();
                                remainingPlugins.Add(plugin);
                            }
                        }
                        $"Registering wildcard plugins: {remainingPlugins.Map(x => x.GetType().Name).Join(", ")}".Print(); 
                        featureTypes.Remove(AllRemainingPlugins);
                        if (remainingPlugins.Count > 0)
                        {
                            var mergedPlugins = new List<IPlugin>(registerPlugins.Where(x => x != null));
                            mergedPlugins.AddRange(remainingPlugins);
                            registerPlugins = mergedPlugins.ToArray();
                        }
                    }

                    if (featureTypes.Count > 0)
                    {
                        var plural = featureTypes.Count > 1 ? "s" : "";
                        throw new NotSupportedException($"Unable to locate plugin{plural}: " + string.Join(", ", featureTypes));
                    }

                    return plugins = registerPlugins;
                }

                return null;
            }
        }

        AppHostBase appHost;
        AppHostBase AppHost
        {
            get
            {
                if (appHost != null)
                    return appHost;

                WebTemplateUtils.VirtualFiles = new FileSystemVirtualFiles(env.ContentRootPath);

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
                                if (typeof(AppHostBase).IsAssignableFrom(type))
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

                WebTemplateUtils.AppHost = appHost;

                if (assemblies.Count > 0)
                    assemblies.Each(x => appHost.ServiceAssemblies.AddIfNotExists(x));

                if (vfs != null)
                    appHost.AddVirtualFileSources.Add(vfs);

                if (vfs is IVirtualFiles writableFs)
                    appHost.VirtualFiles = writableFs;
                    
                return appHost;
            }
        }

        public void ConfigureServices(IServiceCollection services) 
        {
            var appHost = AppHost;
            var plugins = Plugins;
            plugins?.Each(x => services.AddSingleton(x.GetType(), x));

            services.AddSingleton<ServiceStackHost>(appHost);

            plugins?.OfType<IStartup>().Each(x => x.ConfigureServices(services));

            (appHost as IStartup)?.ConfigureServices(services);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            plugins?.OfType<IStartup>().Each(x => x.Configure(app));

            appHost.BeforeConfigure.Add(ConfigureAppHost);

            app.UseServiceStack(appHost);
        }

        public void ConfigureAppHost(ServiceStackHost appHost)
        {
            appHost.Config.DebugMode = "debug".GetAppSetting(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production");
            appHost.Config.ForbiddenPaths.Add("/plugins");

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

            var markdownProvider = "markdownProvider".GetAppSetting();
            var useMarkdownDeep = markdownProvider?.EqualsIgnoreCase("MarkdownDeep") == true;
            MarkdownConfig.Transformer = useMarkdownDeep
                ? new MarkdownDeep.MarkdownDeepTransformer()
                : (IMarkdownTransformer) new MarkdigTransformer();
            if (markdownProvider != null)
                ("Using markdown provider " + (useMarkdownDeep ? "MarkdownDeep" : "Markdig")).Print();

            var contextArgKeys = WebTemplateUtils.AppSettings.GetAllKeys().Where(x => x.StartsWith("args."));
            foreach (var key in contextArgKeys)
            {
                var name = key.RightPart('.');
                var value = key.GetAppSetting();

                feature.Args[name] = value.StartsWith("{") || value.StartsWith("[")
                    ? JS.eval(value)
                    : value;
            }

            appHost.Plugins.Add(feature);

            IPlugin[] registerPlugins = Plugins;
            if (registerPlugins != null)
            {
                foreach (var plugin in registerPlugins)
                {
                    appHost.Plugins.RemoveAll(x => x.GetType() == plugin.GetType());
                    appHost.Plugins.Add(plugin);
                }
            }
        }
    }

    public class AppHost : AppHostBase
    {
        public AppHost()
            : base("name".GetAppSetting("ServiceStack Web App"), typeof(AppHost).Assembly) {}

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

    public class AzureConfig
    {
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
    }

    public class MarkdigTransformer : IMarkdownTransformer
    {
        private Markdig.MarkdownPipeline Pipeline { get; } = 
            Markdig.MarkdownExtensions.UseAdvancedExtensions(new Markdig.MarkdownPipelineBuilder()).Build();
        public string Transform(string markdown) => Markdig.Markdown.ToHtml(markdown, Pipeline);
    }

    public static class WebTemplateUtils
    {
        public static AppHostBase AppHost;
        public static IAppSettings AppSettings;
        public static IVirtualFiles VirtualFiles;

        public static string ResolveValue(this string value)
        {
            if (value?.StartsWith("$") == true)
            {
                var envValue = Environment.GetEnvironmentVariable(value.Substring(1));
                if (!string.IsNullOrEmpty(envValue)) return envValue;
            }
            return value;
        }

        public static string GetAppSetting(this string name) => ResolveValue(AppSettings.GetString(name));

        public static T GetAppSetting<T>(this string name, T defaultValue)
        {
            var value = AppSettings.GetString(name);
            if (value == null) return defaultValue;

            var resolvedValue = ResolveValue(value);
            return resolvedValue.FromJsv<T>();
        }

        public static IVirtualPathProvider GetVirtualFiles(this string provider, string config)
        {
            if (provider == null) return null;
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
                    var region = Amazon.RegionEndpoint.GetBySystemName(s3Config.Region.ResolveValue());
                    s3Config.AccessKey = s3Config.AccessKey.ResolveValue();
                    s3Config.SecretKey = s3Config.SecretKey.ResolveValue();
                    var awsClient = new Amazon.S3.AmazonS3Client(s3Config.AccessKey, s3Config.SecretKey, region);
                    return new S3VirtualFiles(awsClient, s3Config.Bucket.ResolveValue());
                case "azure":
                case "azureblob":
                case "azureblobvirtualfiles":
                    var azureConfig = config.FromJsv<AzureConfig>();
                    var storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(ResolveValue(azureConfig.ConnectionString));
                    var container = storageAccount.CreateCloudBlobClient().GetContainerReference(ResolveValue(azureConfig.ContainerName));
                    container.CreateIfNotExists();
                    return new AzureBlobVirtualFiles(container);
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
                        {
                            connectionString = file.RealPath;
                        }
                        else
                        {
                            connectionString = AppHost.MapProjectPath(connectionString);
                            if (!File.Exists(connectionString))
                            {
                                var fs = File.Create(connectionString);
                                fs.Close();
                            }
                        }
                    }
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

        public static IEnumerable<Type> ScanAllTypes(this ServiceStackHost appHost)
        {
            var externalPlugins = new[] {
                typeof(ServiceStack.Api.OpenApi.OpenApiFeature),
                typeof(ServiceStack.AutoQueryFeature), 
            };

            foreach (var type in externalPlugins)
                yield return type;
            foreach (var type in typeof(ServiceStackHost).Assembly.GetTypes())
                yield return type;
            foreach (var type in appHost.ServiceAssemblies.SelectMany(x => x.GetTypes()))
                yield return type;
        }

        public static IPlugin CreatePlugin(this Type type)
        {
            if (!type.HasInterface(typeof(IPlugin)))
                throw new NotSupportedException($"'{type.Name}' is not a ServiceStack IPlugin");
            
            IPlugin plugin = null;
            var pluginConfig = type.Name.GetAppSetting();

            if (type.Name == nameof(AuthFeature))
            {
                var authProviders = new List<IAuthProvider>();
                var authProviderNames = "AuthFeature.AuthProviders".GetAppSetting();
                var authProviderTypes = authProviderNames != null
                    ? authProviderNames.Split(',').Map(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList()
                    : new List<string>();

                foreach (var t in AppHost.ScanAllTypes())
                {
                    if (authProviderTypes.Count == 0)
                        break;

                    if (authProviderTypes.Contains(t.Name))
                    {
                        authProviders.Add(t.CreateAuthProvider());
                        authProviderTypes.Remove(t.Name);
                    }
                }

                if (authProviderTypes.Count > 0)
                {
                    var plural = authProviderTypes.Count > 1 ? "s" : "";
                    throw new NotSupportedException($"Unable to locate AuthProvider{plural}: " + string.Join(", ", authProviderTypes));
                }

                $"Creating AuthFeature".Print();
                if (authProviders.Count == 0)
                    throw new NotSupportedException($"List of 'AuthFeature.AuthProviders' required for feature 'AuthFeature', e.g: AuthFeature.AuthProviders TwitterAuthProvider, FacebookAuthProvider");

                plugin = new AuthFeature(() => new AuthUserSession(), authProviders.ToArray());
            }
            else
            {
                $"Creating plugin '{type.Name}'".Print();
                plugin = type.CreateInstance<IPlugin>();
            }

            if (pluginConfig != null)
            {
                var value = JS.eval(pluginConfig);
                if (value is Dictionary<string, object> objDictionary)
                {
                    $"Populating '{type.Name}' with: {pluginConfig}".Print();
                    objDictionary.PopulateInstance(plugin);
                }
                else throw new NotSupportedException($"'{pluginConfig}' is not an Object Dictionary");
            }

            return plugin;
        }

        public static IAuthProvider CreateAuthProvider(this Type type)
        {
            var ctorWithAppSettings = type.GetConstructors()
                .FirstOrDefault(x => x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof(IAppSettings));

            var ctorDefault = type.GetConstructors().FirstOrDefault(x => x.GetParameters().Length == 0);

            if (ctorWithAppSettings == null && ctorDefault == null)
                throw new NotSupportedException($"No IAppSettings or default Constructor found for Type '{type.Name}'");

            $"Creating Auth Provider '{type.Name}'".Print();
            var authProvider = ctorWithAppSettings != null
                ? (IAuthProvider)ctorWithAppSettings.Invoke(new object[]{ WebTemplateUtils.AppSettings })
                : (IAuthProvider)ctorDefault.Invoke(new object[]{ WebTemplateUtils.AppSettings });

            var authProviderConfig = type.Name.GetAppSetting();
            if (authProviderConfig != null)
            {
                var value = JS.eval(authProviderConfig);
                if (value is Dictionary<string, object> objDictionary)
                {
                    objDictionary.PopulateInstance(authProvider);
                }
            }

            return authProvider;
        }
    }
}
