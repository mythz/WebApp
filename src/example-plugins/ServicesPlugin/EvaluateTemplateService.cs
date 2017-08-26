using System.Collections.Generic;
using ServiceStack;
using ServiceStack.Templates;
using ServiceStack.IO;
using System.Threading.Tasks;
using System;
using ServiceStack.Web;
using ServiceStack.OrmLite;
using ServiceStack.Data;

namespace TemplatePages
{
    [Route("/template/eval")]
    public class EvaluateTemplate
    {
        public string Template { get; set; }
    }

    public class ReturnExceptionsInJsonAttribute : ResponseFilterAttribute
    {
        public override void Execute(IRequest req, IResponse res, object responseDto)
        {
            if (responseDto is Exception || responseDto is IHttpError)
                req.ResponseContentType = MimeTypes.Json;
        }
    }

    [ReturnExceptionsInJson]
    public class TemplateServices : Service
    {
        public ITemplatePages Pages { get; set; }

        public async Task<string> Any(EvaluateTemplate request)
        {
            var pageResult = new PageResult(Pages.OneTimePage(request.Template, "html"))
            {
                Args = base.Request.GetTemplateRequestParams()
            };
            return await pageResult.RenderToStringAsync();
        }
    }
}