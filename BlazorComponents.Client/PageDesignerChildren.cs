﻿using System;
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

    }

    public class ConfiguratorActivator : BlazorComponent
    {
        [Parameter]
        public StructureNode DesignNode { get; set; } // Demonstrates how a parent component can supply parameters

        [Parameter]
        public FormEventDispatcher Dispatcher { get; set; }

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
            builder.CloseComponent();
        }
    }

    public class PageDesignerChildren : BlazorComponent
    {
        [Parameter]
        List<PreviewNode> DesignNodeCollection { get; set; } // Demonstrates how a parent component can supply parameters

        [Parameter]
        PageDesignEventsDispatcher Dispatcher { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);

            var nonHtmlCount = DesignNodeCollection.Any(x => x.Type != NodeType.Html);
            var htmlCount = DesignNodeCollection.Count(x => x.Type == NodeType.Html);
            if (!nonHtmlCount && htmlCount == 1)
            {
                builder.AddMarkupContent(0,DesignNodeCollection[0].Html);
                return;
            }

            var sw = new Stopwatch();
            sw.Start();
            Console.WriteLine("[Page Preview] Rendering Sublayout");

            int seq = 0;
            string htmlRaw = string.Empty;
            int i = 0;
            foreach (var child in DesignNodeCollection)
            {
                if (child.Type == NodeType.Html)
                    htmlRaw += child.Html;
                else
                    htmlRaw += "<wc-child-"+i+" />";

                i++;
            }
            //htmlRaw += "</wc-render-tree>";


            XNode htmlDoc;
            try
            {
                
                htmlDoc = XDocument.Parse(htmlRaw);
            }
            catch (Exception)
            {
                try
                {
                    htmlDoc = XElement.Parse(htmlRaw);
                }
                catch (Exception)
                {
                    htmlRaw = "<wc-render-wrapper>" + htmlRaw + "</wc-render-wrapper>";
                    try
                    {
                        htmlDoc = XDocument.Parse(htmlRaw);
                    }
                    catch (Exception exception)
                    {
                        builder.AddContent(seq++, exception.Message);
                        return;
                    }
                }
            }


            var reader = htmlDoc.CreateReader();
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

            sw.Stop();
            Console.WriteLine("[Page Preview] Finished Sublayout. "+ seq + " elements, delivered in " + sw.Elapsed.TotalSeconds);
            

            //Console.WriteLine("preparing to transform: " + htmlDoc.Root?.Name);
            //WriteLayout(builder,htmlDoc);
            //int seq = 0;
            //builder.OpenElement(seq++, "wc-node-list");
            //foreach(var child in DesignNodeCollection)
            //{
            //    if (child.Type == NodeType.Html)
            //        builder.AddMarkupContent(seq++,child.Html);

            //}
            //builder.CloseElement();
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


                    //Console.WriteLine(globalSeq + " BLAZOR OPEN CO");
                    builder.OpenComponent<PageDesignerChild>(localSeq++);
                    builder.AddAttribute(localSeq++, nameof(PageDesignerChild.DesignNode), Microsoft.AspNetCore.Blazor.Components.RuntimeHelpers.TypeCheck<BlazorComponents.Shared.PreviewNode>(toRender));
                    builder.AddAttribute(localSeq++, nameof(PageDesignerChild.Dispatcher), Microsoft.AspNetCore.Blazor.Components.RuntimeHelpers.TypeCheck<BlazorComponents.Client.PageDesignEventsDispatcher>(Dispatcher));
                    builder.CloseComponent();
                    //Console.WriteLine(globalSeq + " BLAZOR CLOSE CO");
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

            //Console.WriteLine(globalSeq + " unexpected node type: "+ reader.NodeType);

            return localSeq;
        }

    }
}
