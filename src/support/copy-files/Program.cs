using System;
using System.IO;
using Funq;
using ServiceStack;
using ServiceStack.IO;
using ServiceStack.Aws;
using Amazon.S3;
using Amazon;

namespace CopyFiles
{
    public class Program
    {
        const string SourcePath = "~/../../apps/fs-xplat";

        public static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: ");
                Console.WriteLine("copy-files [provider] [config]");

                Console.WriteLine("\nVirtual File System Providers:");
                Console.WriteLine(" - files [destination]");
                Console.WriteLine(" - s3 {AccessKey:key,SecretKey:secretKey,Region:us-east-1,Bucket:s3Bucket}");
                return;
            }

            var destFs = GetVirtualFiles(args[0], args[1]);
            if (destFs == null)
            {
                Console.WriteLine("Unknown Provider: " + args[0]);
                return;
            }

            var sourcePath = SourcePath.MapProjectPath();
            if (!Directory.Exists(sourcePath))
            {
                Console.WriteLine("Source Directory does not exist: " + sourcePath);
                return;
            }

            var sourceFs = new FileSystemVirtualFiles(sourcePath);

            foreach (var file in sourceFs.GetAllMatchingFiles("*"))
            {
                Console.WriteLine("Copying: " + file.VirtualPath);
                destFs.WriteFile(file);
            }
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

        public static IVirtualPathProvider GetVirtualFiles(string provider, string config)
        {
            if (provider != null)
            {
                switch (provider.ToLower())
                {
                    case "fs":
                    case "files":
                        return new FileSystemVirtualFiles(config.MapProjectPath());
                    case "s3":
                    case "s3virtualfiles":
                        var s3Config = config.FromJsv<S3Config>();
                        var region = RegionEndpoint.GetBySystemName(s3Config.Region);
                        var awsClient = new AmazonS3Client(s3Config.AccessKey, s3Config.SecretKey, region);
                        return new S3VirtualFiles(awsClient, s3Config.Bucket);
                }
            }

            return null;
        }
    }
}
