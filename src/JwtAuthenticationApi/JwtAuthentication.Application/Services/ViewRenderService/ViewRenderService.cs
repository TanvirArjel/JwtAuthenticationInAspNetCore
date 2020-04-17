using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace JwtAuthentication.Application.Services.ViewRenderService
{
    internal class ViewRenderService : IViewRenderService
    {
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public ViewRenderService(IRazorViewEngine razorViewEngine, ITempDataProvider tempDataProvider, IServiceProvider serviceProvider)
        {
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> RenderViewToStringAsync(string viewNameOrPath, object model)
        {
            try
            {
                DefaultHttpContext httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
                ActionContext actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

                ViewEngineResult viewEngineResult = _razorViewEngine.FindView(actionContext, viewNameOrPath, false);

                if (!viewEngineResult.Success)
                {
                    viewEngineResult = _razorViewEngine.GetView(executingFilePath: viewNameOrPath, viewPath: viewNameOrPath, isMainPage: false);
                }

                if (viewEngineResult.View == null || (!viewEngineResult.Success))
                {
                    throw new ArgumentNullException($"Unable to find view '{viewNameOrPath}'");
                }

                IView view = viewEngineResult.View;

                using (StringWriter stringWriter = new StringWriter())
                {
                    ViewDataDictionary viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                    {
                        Model = model
                    };

                    TempDataDictionary tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);

                    ViewContext viewContext = new ViewContext(actionContext, view, viewDictionary, tempData, stringWriter, new HtmlHelperOptions());

                    await view.RenderAsync(viewContext);

                    return stringWriter.ToString();
                }
            }
            catch (Exception exception)
            {
                var methodParameterObj = new { ViewName = viewNameOrPath, Model = model };

                // Handle exception here.
                throw;
            }
        }
    }
}
