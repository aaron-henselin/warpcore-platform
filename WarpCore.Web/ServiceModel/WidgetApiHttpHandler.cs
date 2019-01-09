using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.ModelBinding;
using System.Web.Script.Serialization;
using Cms;
using Modules.Cms.Features.Context;
using Modules.Cms.Features.Presentation.PageComposition;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;
using WarpCore.Web.EmbeddedResourceVirtualPathProvider;
using WarpCore.Web.Widgets;

namespace WarpCore.Web.ServiceModel
{
    public abstract class EditingCommand
    {
        public EditingContext EditingContext { get; set; }
    }

    public class ConfigureCommand : EditingCommand
    {
        public string ParameterName { get; set; }
        public string NewValue { get; set; }
    }

    public class DeleteCommand : EditingCommand
    {
        public Guid PageContentId { get; set; }

    }

    public class AddCommand : EditingCommand
    {
        public string WidgetType { get; set; }
        public string ToContentPlaceHolderId { get; set; }
        public Guid? ToLayoutBuilderId { get; set; }
        public Guid? BeforePageContentId { get; set; }
    }

    public class MoveCommand : EditingCommand
    {
        public Guid PageContentId { get; set; }
        public string ToContentPlaceHolderId { get; set; }
        public Guid? ToLayoutBuilderId { get; set; }
        public Guid? BeforePageContentId { get; set; }
    }


    
    public class ApplyConfigurationCommand : EditingCommand
    {
        public Guid PageContentId { get; set; }

        public Dictionary<string,string> NewConfiguration { get; set; }
    }


    public class PageDesignerApi
    {
        public Dictionary<string, string> ApplyConfiguration(ApplyConfigurationCommand command)
        {
            var contentToConfigure = command.EditingContext.FindSubContentReursive(x => x.Id == command.PageContentId).SingleOrDefault();
            return contentToConfigure.LocatedContent.Parameters = command.NewConfiguration;
        }


        public EditingContext Add(AddCommand addCommand)
        {
            ProcessAddCommand(addCommand.EditingContext,addCommand);
            return addCommand.EditingContext;
        }

        public EditingContext Delete(DeleteCommand deleteCommand)
        {
            ProcessDeleteCommand(deleteCommand.EditingContext, deleteCommand);
            return deleteCommand.EditingContext;
        }

        public EditingContext Move(MoveCommand moveCommand)
        {
            ProcessMoveCommand(moveCommand.EditingContext,moveCommand);
            return moveCommand.EditingContext;
        }

        private void ProcessDeleteCommand(EditingContext editingContext, DeleteCommand deleteCommand)
        {
            var contentToMoveSearch = editingContext.FindSubContentReursive(x => x.Id == deleteCommand.PageContentId).SingleOrDefault();
            if (contentToMoveSearch == null)
                throw new Exception("component not found.");

            contentToMoveSearch.ParentContent.AllContent.Remove(contentToMoveSearch.LocatedContent);
        }

        private void ProcessAddCommand(EditingContext editingContext, AddCommand addCommand)
        {
            var toolboxItem = new ToolboxManager().GetToolboxItemByCode(addCommand.WidgetType);
            var defaults = new CmsPageContentActivator().GetDefaultContentParameterValues(toolboxItem);


            var newContent = new CmsPageContent
            {
                Id = Guid.NewGuid(),
                PlacementContentPlaceHolderId = addCommand.ToContentPlaceHolderId,
                PlacementLayoutBuilderId = addCommand.ToLayoutBuilderId,
                WidgetTypeCode = addCommand.WidgetType,
                Parameters = defaults.ToDictionary(x => x.Key,x => x.Value)
            };



            if (addCommand.ToLayoutBuilderId != null)
            {
                var newParentSearch = editingContext.FindSubContentReursive(x => x.Id == addCommand.ToLayoutBuilderId).Single();


                //var newParentSearch =
                //    editingContext.FindSubContentReursive(x =>
                //        x.Parameters.ContainsKey(nameof(LayoutControl.LayoutBuilderId)) &&
                //        new Guid(x.Parameters[nameof(LayoutControl.LayoutBuilderId)]) ==
                //        addCommand.ToLayoutBuilderId.Value).SingleOrDefault();

                if (newParentSearch == null)
                    throw new Exception("Invalid content location -- there is no layoutbuilder with id: " +
                                        addCommand.ToLayoutBuilderId);

                //todo: ordering.

                newParentSearch.LocatedContent.AllContent.Add(newContent);
            }
            else
            {
                //todo: ordering.

                editingContext.AllContent.Add(newContent);
            }
        }

        private void ProcessMoveCommand(EditingContext editingContext, MoveCommand moveCommand)
        {
            var contentToMoveSearch = editingContext.FindSubContentReursive(x => x.Id == moveCommand.PageContentId).SingleOrDefault();
            var contentToMove = contentToMoveSearch.LocatedContent;
            var previousParentCollection = contentToMoveSearch.ParentContent.AllContent;

            previousParentCollection.Remove(contentToMove);

            List<CmsPageContent> insertInCollection;
            if (moveCommand.ToLayoutBuilderId != null)
            {
                var newParentSearch = editingContext.FindSubContentReursive(x => x.Id == moveCommand.ToLayoutBuilderId).Single();
                insertInCollection = newParentSearch.LocatedContent.AllContent;
            }
            else
            {
                insertInCollection = editingContext.AllContent;
            }

            int insertLocation = 0;
            if (moveCommand.BeforePageContentId != null)
            {
                var before = insertInCollection.Single(x => x.Id == moveCommand.PageContentId);
                insertLocation = insertInCollection.IndexOf(before);
            }

            insertInCollection.Insert(insertLocation, contentToMove);

            contentToMove.PlacementContentPlaceHolderId = moveCommand.ToContentPlaceHolderId;
            contentToMove.PlacementLayoutBuilderId = moveCommand.ToLayoutBuilderId;

            for (int i = 0; i < insertInCollection.Count; i++)
            {
                insertInCollection[i].Order = i;
            }

        }

    }

    public class BlazorHostHttpHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            var tk = new BlazorToolkit();
            var withoutRoutePrefix = "blazorcomponents.client/dist"+context.Request.RawUrl;
            try
            {
                var hostedPath = tk.GetHostedPath(withoutRoutePrefix);

                if (withoutRoutePrefix.EndsWith(".dll"))
                    context.Response.ContentType = "application/octet-stream";

                if (withoutRoutePrefix.EndsWith(".wasm"))
                    context.Response.ContentType = "application/wasm";

                if (withoutRoutePrefix.EndsWith(".json"))
                    context.Response.ContentType = "application/json";

                context.Server.Transfer(hostedPath);
            }
            catch (ThreadAbortException)
            {
                return;
            }
            catch (Exception e)
            {
                context.Response.StatusCode = 404;
            }



        }

        public bool IsReusable { get; }
    }

    public class ConfiguratorHttpHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Server.Transfer("/App_Data/Configurator.aspx",true);
        }

        public bool IsReusable { get; }
    }

    public class PageDesignerApiHttpHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            //var methodName = context.Request.PathInfo.Substring(1);
            //no idea why pathinfo isn't coming accross.

            var methodName = context.Request.Path.Substring(context.Request.Path.LastIndexOf("/")+1);
            Type type = typeof(PageDesignerApi);
            var ws = Activator.CreateInstance(type);
            var method = type.GetMethod(methodName);

            var inputs = new List<object>();
            var parameters = method.GetParameters();
            foreach (var parameter in parameters)
            {
                if (parameter.ParameterType.IsPrimitive || parameter.ParameterType == typeof(string))
                    inputs.Add(context.Request[parameter.Name]);
                else
                {
                    var contentTypeHints = HttpContext.Current.Request.ContentType.Split(new[] {';',' '});

                    var body = GetDocumentContents(context.Request);
                    if (contentTypeHints.Contains("application/json"))
                    {
                        var obj = new JavaScriptSerializer().Deserialize(body, parameter.ParameterType);
                        inputs.Add(obj);
                    }

                    if (contentTypeHints.Contains("application/x-www-form-urlencoded"))
                    {
                        var executionContext = new ModelBindingExecutionContext(new HttpContextWrapper(HttpContext.Current),new ModelStateDictionary());
                        var model = Activator.CreateInstance(parameter.ParameterType);
                        var provider = new NameValueCollectionValueProvider(HttpUtility.ParseQueryString(body),
                            CultureInfo.CurrentCulture);
                        var modelBindingContext = new ModelBindingContext();
                        modelBindingContext.ValueProvider = provider;
                        modelBindingContext.ModelMetadata =
                            ModelMetadataProviders.Current.GetMetadataForType(() => model, parameter.ParameterType);
                        modelBindingContext.Model = model;
                        var binder = ModelBinderProviders.Providers.GetBinder(executionContext, modelBindingContext);
                        binder.BindModel(executionContext, modelBindingContext);
                        inputs.Add(modelBindingContext.Model);
                    }

                }
            }
            var output = method.Invoke(ws, inputs.ToArray());
            context.Response.ContentType = "application\\json";
            context.Response.Write(new JavaScriptSerializer().Serialize(output));
            
        }

        private string GetDocumentContents(HttpRequest Request)
        {
            MemoryStream memstream = new MemoryStream();
            Request.InputStream.CopyTo(memstream);
            memstream.Position = 0;
            using (StreamReader reader = new StreamReader(memstream))
            {
                return reader.ReadToEnd();
            }
            
        }

        public bool IsReusable { get; }
    }
}