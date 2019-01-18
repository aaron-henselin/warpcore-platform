using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using BlazorComponents.Client.Shared;
using BlazorComponents.Client.Shared.PageDesigner;
using BlazorComponents.Shared;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.RenderTree;

namespace BlazorComponents.Client
{
    public class PageDesignerChildren : BlazorComponent
    {
        [Parameter]
        List<Node> DesignNodeCollection { get; set; } // Demonstrates how a parent component can supply parameters

        [Parameter]
        PageDesignEventsDispatcher Dispatcher { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);


            var sw = new Stopwatch();
            sw.Start();
            Console.WriteLine("starting layout");

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
            Console.WriteLine(seq + " elements, delivered in " + sw.Elapsed.TotalSeconds);

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
                    builder.AddAttribute(localSeq++, nameof(PageDesignerChild.DesignNode), Microsoft.AspNetCore.Blazor.Components.RuntimeHelpers.TypeCheck<BlazorComponents.Shared.Node>(toRender));
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
