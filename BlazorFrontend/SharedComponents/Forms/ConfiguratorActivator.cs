using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using BlazorComponents.Shared;
using BlazorFrontend.SharedComponents.PageDesigner;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.JSInterop;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.Kernel;

namespace BlazorComponents.Client
{
    public class ConfiguratorActivator : ComponentBase
    {
        [Parameter]
        public StructureNode DesignNode { get; set; } // Demonstrates how a parent component can supply parameters

        //[Parameter]
        //FormEventDispatcher Dispatcher { get; set; }

        //[Parameter]
        //ConfiguratorRegistry ConfiguratorRegistry { get; set; }



        private static Dictionary<string, Type> widgetTypeCodeLookup;
        private static Dictionary<Type, Type> componentLookupByInterface;



        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);

            EnsureTypeCacheBuilt();

            if (!widgetTypeCodeLookup.ContainsKey(DesignNode.WidgetTypeCode))
            {
                Console.WriteLine("[Forms] Unknown toolbox item: " + DesignNode.WidgetTypeCode);
                return;
            }


            var linkedConfigType = widgetTypeCodeLookup[DesignNode.WidgetTypeCode];
            var activated = Activator.CreateInstance(Type.GetType(linkedConfigType.AssemblyQualifiedName));
            activated.SetPropertyValues(DesignNode.Parameters,x => true);
            

            var useComponentType = componentLookupByInterface[linkedConfigType];
            Console.WriteLine("[Forms] Activating "+ useComponentType.FullName);


            var localSeq = 0;
            builder.OpenComponent(localSeq++, useComponentType);
            builder.AddAttribute(localSeq++, nameof(IConfiguratorComponent<TextboxToolboxItem>.Config), activated);

            if (activated is ISupportsSubContent)
            {
                var pageStructure = new PageStructure();
                pageStructure.ChildNodes = DesignNode.ChildNodes;

                var formDescription = new ConfiguratorFormDescription
                {
                    Layout = pageStructure
                };
                builder.AddAttribute(localSeq++, nameof(IRendersSubLayout.DesignNode), formDescription);
            }

            builder.CloseComponent();

            Console.WriteLine("[Forms] Activated " + useComponentType.FullName);

        }

        private static void EnsureTypeCacheBuilt()
        {
            if (widgetTypeCodeLookup == null)
            {
                RebuildWidgetTypeCodeLookup();
                Console.WriteLine("[Forms] Discovered toolbox");

                foreach (var kvp in widgetTypeCodeLookup)
                    Console.WriteLine($"[Forms] Widget Item={kvp.Key} Value={kvp.Value.FullName}");
            }


            if (componentLookupByInterface == null)
            {
                RebuildComponentLookup();

                foreach (var kvp in componentLookupByInterface)
                    Console.WriteLine($"[Forms] Component Item={kvp.Key.FullName} Value={kvp.Value.FullName}");
            }


        }

        private static void RebuildComponentLookup()
        {
            componentLookupByInterface = new Dictionary<Type, Type>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var allTypes = assemblies.SelectMany(x => x.GetTypes()).ToList();
            var allComponents = allTypes
                .Where(x => typeof(IConfiguratorComponent)
                    .IsAssignableFrom(x))
                .ToList();

            var needsComponent = widgetTypeCodeLookup.Values;
            foreach (var type in needsComponent)
            {
                var componentInterfaceBlank = typeof(IConfiguratorComponent<>);
                var generic = componentInterfaceBlank.MakeGenericType(type);
                var canFulfull = allComponents.FirstOrDefault(x => generic.IsAssignableFrom(x));
                if (canFulfull == null)
                {
                    var loaded = string.Join(",", allComponents.Select(x => x.FullName));
                    throw new Exception("Could not find blazor component matching " + generic.FullName + ". Available components are: "+loaded);
                }
                componentLookupByInterface.Add(type, canFulfull);
            }
        }

        private static void RebuildWidgetTypeCodeLookup()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var allTypes = assemblies.SelectMany(x => x.GetTypes()).ToList();
            var toolboxItemTypes = allTypes.Where(x => x.GetCustomAttribute<ToolboxItemAttribute>() != null).ToList();
            widgetTypeCodeLookup =
                toolboxItemTypes.ToDictionary(x => x.GetCustomAttribute<ToolboxItemAttribute>().WidgetUid, x => x);
        }
    }
}