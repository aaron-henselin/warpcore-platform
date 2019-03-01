using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace WarpCore.Platform.DataAnnotations
{

    public class Slug : IPrimitiveBacked
    {
        public Slug()
        {
        }

        public Slug(string slug)
        {
            _rawSlug = slug;
        }

        public static Slug FromPageName(string text)
        {
            var cleaned = text;
            string regex = " ~`!@#$%^&*()_+{}[]:;?/\\|,.<>'\"";
            foreach (var c in regex)
                cleaned = cleaned.Replace(c, '-');

            return new Slug(cleaned.ToLower());
        }

        string IPrimitiveBacked.BackingPrimitive {
            get => _rawSlug;
            set => _rawSlug = value;
        }

        private string _rawSlug;

        public override string ToString()
        {
            return _rawSlug;
        }
    }


    public interface IRequiresDataSource
    {
        Guid RepositoryApiId { get; set; }
        string DataSourceType { get; set; }

        DataSourceItemCollection Items { get; set; }

     
    }
    public interface ISupportsSubContent
    {
    }
    public interface ISupportsJavaScriptSerializer
    {

    }

    public interface IPrimitiveBacked
    {
        string BackingPrimitive { get; set; }
    }

    [DebuggerDisplay("Item Count={Items.Count}")]
    public class DataSourceItemCollection : ISupportsJavaScriptSerializer
    {
        public static DataSourceItemCollection FromKeyValuePairs(IEnumerable<KeyValuePair<string,string>> items)
        {
            var list = new List<DataSourceItem>();
            foreach (var item in items)
                list.Add(new DataSourceItem{Name=item.Key,Value=item.Value});

            return new DataSourceItemCollection{Items = list};
        }

        public IEnumerable<KeyValuePair<string, string>> ToKeyValuePairs()
        {
            return Items.Select(x => new KeyValuePair<string, string>(x.Name, x.Value));
        }

        public List<DataSourceItem> Items { get; set; } = new List<DataSourceItem>();
    }


    public class FixedOptionsDataSourceAttribute : Attribute
    {
        public string[] Options { get; }

        public FixedOptionsDataSourceAttribute(params string[] options)
        {
            Options = options;
        }

        public FixedOptionsDataSourceAttribute(params int[] options)
        {
            Options = options.Select(x => x.ToString()).ToArray();
        }
    }

    public static class DataSourceTypes
    {
        public const string Repository = nameof(Repository);
        public const string FixedItems = nameof(FixedItems);

    }

    [DebuggerDisplay("Name = {" + nameof(DataSourceItem.Name) + "}, Value = {" + nameof(DataSourceItem.Value) + "}")]
    public class DataSourceItem
    {
        public DataSourceItem()
        {
        }

        public DataSourceItem(string value)
        {
            Name = value;
            Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class SerializedComplexObjectAttribute : Attribute
    {
    }

}
