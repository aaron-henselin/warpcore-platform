using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using BlazorComponents.Client.Shared;
using BlazorComponents.Client.Shared.Forms;
using BlazorComponents.Client.Shared.PageDesigner;
using BlazorComponents.Shared;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.RenderTree;
using Microsoft.JSInterop;
using WarpCore.Platform.DataAnnotations;

namespace BlazorComponents.Client
{
    public interface IConfiguratorComponent
    {
        [Parameter]
        ConfiguratorSetup Setup { get; set; } // Demonstrates how a parent component can supply parameters

        [Parameter]
        FormEventDispatcher Dispatcher { get; set; }

        [Parameter]
        ConfiguratorRegistry ConfiguratorRegistry { get; set; }

        string Value { get; set; }

        bool IsValid { get; }

    }


    public static class DragDropContext
    {
        public static PreviewNode Dragging { get; set; }
    }

    public class ConfiguratorActivator : BlazorComponent
    {
        [Parameter]
        StructureNode DesignNode { get; set; } // Demonstrates how a parent component can supply parameters

        [Parameter]
        FormEventDispatcher Dispatcher { get; set; }

        [Parameter]
        ConfiguratorRegistry ConfiguratorRegistry { get; set; }

        private Type TypeLookup(ConfiguratorSetup setup)
        {
            return typeof(FormTextBox);
        }



        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);

            var toJson = Json.Serialize(DesignNode.Parameters);
            var configuration = Json.Deserialize<ConfiguratorSetup>(toJson);

            var t = TypeLookup(configuration);
            if (!typeof(IConfiguratorComponent).IsAssignableFrom(t))
                throw new Exception("Not an Iconfiguratorcomponent.");

            Console.WriteLine("[Forms] Activating "+ t.FullName);

            var localSeq = 0;
            builder.OpenComponent(localSeq++,t);
            builder.AddAttribute(localSeq++, nameof(IConfiguratorComponent.Setup), Microsoft.AspNetCore.Blazor.Components.RuntimeHelpers.TypeCheck<BlazorComponents.Shared.ConfiguratorSetup>(configuration));
            builder.AddAttribute(localSeq++, nameof(IConfiguratorComponent.Dispatcher), Microsoft.AspNetCore.Blazor.Components.RuntimeHelpers.TypeCheck<BlazorComponents.Client.FormEventDispatcher>(Dispatcher));
            builder.AddAttribute(localSeq++, nameof(IConfiguratorComponent.ConfiguratorRegistry), Microsoft.AspNetCore.Blazor.Components.RuntimeHelpers.TypeCheck<ConfiguratorRegistry>(ConfiguratorRegistry));
            builder.CloseComponent();
        }
    }

    public class PageDesignerChildren : BlazorComponent
    {
        [Parameter]
        List<PreviewNode> DesignNodeCollection { get; set; } // Demonstrates how a parent component can supply parameters

        [CascadingParameter]
        protected PageDesignerPagePreview Dispatcher { get; set; }

        private string CreateLayoutHtml()
        {
            string htmlRaw = string.Empty;
            int i = 0;
            foreach (var child in DesignNodeCollection)
            {
                if (child.Type == NodeType.Html)
                    htmlRaw += child.Html;
                else
                    htmlRaw += "<wc-child-" + i + " />";

                i++;
            }

            return htmlRaw;
        }

        private static Dictionary<string, XNode> _cache = new Dictionary<string, XNode>();

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);

            if (!DesignNodeCollection.Any())
                return;

            var nonHtmlCount = DesignNodeCollection.Any(x => x.Type != NodeType.Html);
            var htmlCount = DesignNodeCollection.Count(x => x.Type == NodeType.Html);
            if (!nonHtmlCount && htmlCount == 1)
            {
                builder.AddMarkupContent(0, DesignNodeCollection[0].Html);
                return;
            }

            XNode layoutXml;
            int seq = 0;
            var key = Json.Serialize(DesignNodeCollection);
            if (!_cache.ContainsKey(key))
            {
                var layoutHtml = CreateLayoutHtml();
                try
                {
                    layoutXml = ParseLayoutXml(layoutHtml);
                    _cache.Add(key,layoutXml);
                }
                catch (Exception exception)
                {
                    builder.AddContent(seq++, exception.Message);
                    return;
                }
            }
            else
            {
                layoutXml = _cache[key];
            }

            //var sw = new Stopwatch();
            //sw.Start();
            //Console.WriteLine("[Page Preview] Rendering Sublayout");

            var reader = layoutXml.CreateReader();
            reader.Read();

            bool hasMore = true;
            while (hasMore)
            {
                seq = WriteLayout(builder, reader,seq);

                if (reader.EOF)
                    hasMore = false;
                else
                {
                    reader.Read();
                    hasMore = reader.NodeType != XmlNodeType.None;
                }
            }

            //sw.Stop();
            //Console.WriteLine("[Page Preview] Finished Sublayout. "+ seq + " elements, delivered in " + sw.Elapsed.TotalSeconds);
            

  
        }

        private XNode ParseLayoutXml(string layoutHtml)
        {

            XNode htmlDoc;
            try
            {

                return XDocument.Parse(layoutHtml);
            }
            catch (Exception)
            {
                try
                {
                    return XElement.Parse(layoutHtml);
                }
                catch (Exception)
                {
                    layoutHtml = "<wc-render-wrapper>" + layoutHtml + "</wc-render-wrapper>";

                    return XDocument.Parse(layoutHtml);

                }
            }

        }

        private int WriteLayout(RenderTreeBuilder builder, XmlReader reader, int globalSeq)
        {
            

            int localSeq = globalSeq;

            if (reader.NodeType == XmlNodeType.Element)
            {
                //Console.WriteLine(globalSeq + " element " + reader.Name);

                if (reader.LocalName.StartsWith("wc-child-"))
                {
                    var position = Convert.ToInt32(reader.LocalName.Substring("wc-child-".Length));
                    var toRender = DesignNodeCollection[position];

                    builder.OpenComponent<PageDesignerChild>(localSeq++);
                    builder.AddAttribute(localSeq++, nameof(PageDesignerChild.DesignNode), Microsoft.AspNetCore.Blazor.Components.RuntimeHelpers.TypeCheck<BlazorComponents.Shared.PreviewNode>(toRender));
                    builder.AddAttribute(localSeq++, nameof(PageDesignerChild.Dispatcher), Microsoft.AspNetCore.Blazor.Components.RuntimeHelpers.TypeCheck<PageDesignerPagePreview>(Dispatcher));

                    var justElements = DesignNodeCollection.Where(x => x.Type != NodeType.Html).ToList();

                    var childPosition = ChildPosition.Middle;
                    if (justElements.First() == toRender)
                        childPosition = ChildPosition.First;

                    if (justElements.Last() == toRender)
                        childPosition = ChildPosition.Last;
 
                    builder.AddAttribute(localSeq++, nameof(PageDesignerChild.Position), Microsoft.AspNetCore.Blazor.Components.RuntimeHelpers.TypeCheck<ChildPosition>(childPosition));

                    builder.CloseComponent();
                    return localSeq;
                }
                else
                {
                    //Console.WriteLine(globalSeq + " BLAZOR OPEN EL");
                    builder.OpenElement(localSeq++, reader.Name.ToString());


                    if (reader.HasAttributes)
                    {
                        //Console.WriteLine(reader.Name + " Attribute");
                        for (int i = 0; i < reader.AttributeCount; i++)
                        {
                            reader.MoveToAttribute(i);
                            //Console.WriteLine(localSeq + " attr " + reader.Name + ", " + reader.Value);
                            //Console.WriteLine(localSeq + " BLAZOR ADD ATTR");
                            builder.AddAttribute(localSeq++, reader.Name, reader.Value);
                        }
                        reader.MoveToElement();
                    }


                    if (reader.IsEmptyElement)
                    {
                        //Console.WriteLine(localSeq + " BLAZOR CLOSE EL *EMPTY*");
                        builder.CloseElement();
                    }

                    return localSeq;
                }
                
            }

            if (reader.NodeType == XmlNodeType.Text)
            {
               // Console.WriteLine(globalSeq + " text " + reader.Value);
                //Console.WriteLine(globalSeq + " BLAZOR ADD MARKUP");
                builder.AddMarkupContent(localSeq++, reader.Value?.ToString());
                return localSeq;
            }

            if (reader.NodeType == XmlNodeType.EndElement)
            {
                //Console.WriteLine(globalSeq + " end-element " + reader.Name);
                //Console.WriteLine(globalSeq + " BLAZOR CLOSE EL");
                builder.CloseElement();
                return localSeq;
            }
            

            return localSeq;
        }

    }

    public enum ChildPosition { First,Middle,Last}
}
