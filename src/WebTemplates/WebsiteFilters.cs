using ServiceStack.OrmLite;
using System;
using ServiceStack.Data;
using System.Collections.Generic;
using System.Data;
using ServiceStack.Templates;
using ServiceStack;

namespace TemplateWebsites
{
    public class WebsiteTemplateFilters : TemplateFilter
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
}
