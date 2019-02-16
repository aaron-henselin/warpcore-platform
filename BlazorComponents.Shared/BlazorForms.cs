using System;
using System.Collections.Generic;
using System.Text;
using WarpCore.Platform.DataAnnotations;

namespace BlazorComponents.Shared
{




    public class ConfiguratorRow
    {
        public int NumColumns { get; set; }
    }

    public class ContentListData
    {
        public List<IDictionary<string,string>> Items { get; set; } = new List<IDictionary<string, string>>();

    }

    public class ListField
    {
        public Guid FieldId { get; set; }
        public string DisplayName { get; set; }
        public string PropertyName { get; set; }
        public string DataType { get; set; }
        public string Format { get; set; }
    }

    public class ContentListDescription
    {
        public List<ListField> Fields { get; set; } = new List<ListField>();
   

    }

    public struct ContentBrowserApiRoutes
    {
        public const string ListDescription = "api/content/lists/{listId}/description";
        public const string ListDataFetch = "api/content/{repositoryApiId}/lists/{listId}/fetch";
        public const string ListFetch = "api/content/lists/by-repository/{repositoryApiId}";

    }

}
