﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JsonSubTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.DataAnnotations.Expressions;
using WarpCore.Platform.DataAnnotations.Orm;
using WarpCore.Platform.Kernel;

namespace WarpCore.Platform.Orm
{


    public interface ICosmosOrm
    {
        void Save(WarpCoreEntity item);

        Task<IReadOnlyCollection<T>> FindContentVersions<T>(SqlFilter condition,
            ContentEnvironment version = ContentEnvironment.Live)
            where T : VersionedContentEntity, new();

        Task<IReadOnlyCollection<T>> FindUnversionedContent<T>(SqlFilter condition)
            where T : UnversionedContentEntity, new();

        void Delete(WarpCoreEntity copy);
    }

    public class CosmosDbConfiguration
    {

        //public static string ConnectionString => "DefaultEndpointsProtocol=https;AccountName=starfleetops;AccountKey=sLUhmnUL5siz2GfUQ981nBBfHyCvocdp6cK5bQUJaxlmfvhuABnBwJrMOpzpdtIXLizEFm3lovy5VfipTFOlKg==;TableEndpoint=https://starfleetops.table.cosmosdb.azure.com:443/;";



        public string ConnectionString { get; set; }

    }





    public enum ContentEnvironment
    {
        Any = -1,
        Draft = 0,
        Live = 1,
        Archive = 2
    }


    [Table("cms_content_checksum")]
    public class ContentChecksum : UnversionedContentEntity
    {
        public string ContentType { get; set; }
        public string Draft { get; set; }
        public string Live { get; set; }
    }
    

    interface IHasRuntimeTypeExtensionDefinition
    {
        Guid TypeExtensionUid { get; }
    }

    //public class DynamicVersionedContentBase : VersionedContentEntity
    //{
    //    public DynamicVersionedContentBase()
    //    {
    //    }
    //}
    
    [Table("orm_dynamic_object")]
    public class DynamicVersionedContent : VersionedContentEntity, IHasRuntimeTypeExtensionDefinition
    {
        private readonly Guid _uid;

        public DynamicVersionedContent(Guid uid)
        {
            _uid = uid;
        }

        Guid IHasRuntimeTypeExtensionDefinition.TypeExtensionUid => _uid;
    }

    public class DynamicUnversionedContent : UnversionedContentEntity, IHasRuntimeTypeExtensionDefinition
    {
        private readonly Guid _uid;

        public DynamicUnversionedContent(Guid uid)
        {
            _uid = uid;
        }

        Guid IHasRuntimeTypeExtensionDefinition.TypeExtensionUid => _uid;
    }

    public abstract class VersionedContentEntity : WarpCoreEntity
    {

        public VersionedContentEntity() : base(Dependency.Resolve<IDynamicTypeDefinitionResolver>())
        {
            this.ContentEnvironment = ContentEnvironment.Draft;
        }
        

        public string GetContentChecksum()
        {
            var obj = JsonConvert.SerializeObject(this);
            var dict = JObject.Parse(obj);
            dict.Remove("RowKey");
            dict.Remove("ETag");
            dict.Remove("PartitionKey");
            dict.Remove("Timestamp");

            return dict.ToString();
        }


        [JsonIgnore] public decimal ContentVersion { get; set; }

        [JsonIgnore]
        public ContentEnvironment ContentEnvironment
        {
            get => (ContentEnvironment) Enum.Parse(typeof(ContentEnvironment), PartitionKey);
            set => PartitionKey = value.ToString();
        }

    }

    public abstract class UnversionedContentEntity : WarpCoreEntity
    {
        public UnversionedContentEntity() : base(Dependency.Resolve<IDynamicTypeDefinitionResolver>())
        {
            this.PartitionKey = ContentEnvironment.Any.ToString();
        }
    }

    public class KnownChoiceTypes
    {
        public string TrueFalse = "TrueFalse";
        public string OptionList = "OptionList";
    }

    public class ChoiceFieldOption
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }

    public class ChoiceInterfaceFieldDataSource
    {
        public Guid FormInteropUid { get; set; }
        public string Filter { get; set; }

        public List<ChoiceFieldOption> Options { get; set; } = new List<ChoiceFieldOption>();
    }

    public class ChoiceInterfaceField : InterfaceField
    {
        public const string ContentFieldTypeUid = "Choice";
        public string ChoiceType { get; set; }
        public bool AllowMultipleSelections { get; set; }
        public ChoiceInterfaceFieldDataSource DataSource { get; set; }
        public ChoiceInterfaceField() : base(ContentFieldTypeUid)
        {
        }
    }

    public class TextInterfaceField : InterfaceField
    {
        public const string ContentFieldTypeUid = "Text";
        public bool IsHtml { get; set; }
        public int MaxLength { get; set; }
        public TextInterfaceField() : base(ContentFieldTypeUid)
        {
        }
    }
    
    internal interface IContentFieldTypeResolverMetadata
    {
        string ContentFieldType { get; }
    }
    
    [JsonConverter(typeof(JsonSubtypes), nameof(ContentFieldType))]
    [JsonSubtypes.KnownSubType(typeof(TextInterfaceField), TextInterfaceField.ContentFieldTypeUid)]
    [JsonSubtypes.KnownSubType(typeof(ChoiceInterfaceField), ChoiceInterfaceField.ContentFieldTypeUid)]
    public abstract class InterfaceField
    {
        protected InterfaceField(string contentFieldType)
        {
            ContentFieldType = contentFieldType;
        }
        public Guid PropertyId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyTypeName { get; set; }
        public string ContentFieldType { get; }
    }

    public class DynamicTypeDefinition
    {
        public List<InterfaceField> DynamicProperties { get; set; } =
            new List<InterfaceField>();

        public string TitleProperty { get; set; }
    }


    public interface IDynamicTypeDefinitionResolver
    {
        DynamicTypeDefinition Resolve(Type type);
    }


    public static class By
    {
        public static BooleanExpression ContentId(Guid contentId)
        {
            var text = nameof(WarpCoreEntity.ContentId)+" == {"+ contentId + "}";
            return By.Condition(text);
        }

        //public static BooleanExpression BooleanExpression(BooleanExpression booleanExpression, Type type)
        //{
        //    return BooleanExpressionEntityFilter.Build(booleanExpression,type);            
        //}

        public static BooleanExpression Condition(string booleanExpressionText)
        {
            var tokens =
                new BooleanExpressionParser().ReadBooleanExpressionTokens(booleanExpressionText);

            return new BooleanExpressionTokenReader().CreateBooleanExpressionFromTokens(tokens);
        }
    }



    public class SqlFilter
    {
        public static SqlFilter Empty => new SqlFilter();

        public string Filter { get; private set; }
        public IDictionary<string, object> Parameters { get; }= new Dictionary<string, object>();

        private int _autoGeneratedParameterCount;

        public void Append(string text)
        {
            Filter += text;

        }

        public string AddParameter(object val)
        {
            var parameterCode = $"p{_autoGeneratedParameterCount++}";
            Parameters.Add(parameterCode, val);
            return parameterCode;
        }

        public static SqlFilter FromEntityFilter(string entityFilter, Type type)
        {
            return SqlTranslator.Build(By.Condition(entityFilter),type);
        }
    }

    public class EntityFilter
    {
        public Type Type { get; set; }
        public string Filter { get; set; }
    }

    public class SqlTranslator 
    {
        private readonly Type _type;


        public SqlTranslator(Type type)
        {
            _type = type;
        }

        public static SqlFilter Build(BooleanExpression booleanExpression, Type type)
        {
            if (null == booleanExpression)
                return SqlFilter.Empty;

            var translator = new SqlTranslator(type);
            var filter = new SqlFilter();
            translator.Translate(booleanExpression,filter);
            return filter;
        }

        private void Translate(BooleanExpression booleanExpression, SqlFilter appendToFilter)
        {
            if (booleanExpression is BooleanBinaryExpression booleanBinaryExpression)
            {
                TranslateBooleanBinaryExpression(booleanBinaryExpression, appendToFilter);
                return;
            }

            if (booleanExpression is ComparisonExpression comparisonExpression)
            {
                TranslateBooleanComparisonExpression(comparisonExpression, appendToFilter);
                return;
            }

            if (booleanExpression is ScalarValueExpression scalarValueExpression)
            {
                TranslateScalarValue(scalarValueExpression, appendToFilter);
                return;
            }

            throw new ArgumentException();

        }

        private void TranslateScalarValue(ScalarValueExpression scalarValueExpression, SqlFilter appendToFilter)
        {

            

            if (scalarValueExpression is LiteralStringExpression @string)
            {
                var parameter = appendToFilter.AddParameter(@string.Value);
                appendToFilter.Append("@"+parameter);
                return;
            }

            if (scalarValueExpression is LiteralDecimalExpression @decimal)
            {
                var parameter = appendToFilter.AddParameter(@decimal.Value);
                appendToFilter.Append("@" + parameter);
                return;
            }

            if (scalarValueExpression is LiteralIntExpression @int)
            {
                var parameter = appendToFilter.AddParameter(@int.Value);
                appendToFilter.Append("@" + parameter);
                return;
            }

            if (scalarValueExpression is LiteralGuidExpression @guid)
            {
                var parameter = appendToFilter.AddParameter(@guid.Value);
                appendToFilter.Append("@" + parameter);
                return;
            }

            if (scalarValueExpression is IdentifierExpression @identifier)
            {
                var settingProperty = ToolboxMetadataReader.GetPropertyMetadata(_type,@identifier.Name);
                var settingName = settingProperty.PropertyInfo.Name;
                appendToFilter.Append(settingName);

                return;
            }


            throw new ArgumentException();
        }

        private void TranslateBooleanComparisonExpression(ComparisonExpression comparisonExpression, SqlFilter filter)
        {
            Translate(comparisonExpression.Left,filter);
  
            string word;
            switch (comparisonExpression.ComparisonType)
            {
                case ComparisonType.NotEqual:
                    word = "<>";
                    break;
                case ComparisonType.Equals:
                    word = "=";
                    break;
                case ComparisonType.GreaterThan:
                    word = ">";
                    break;
                case ComparisonType.GreaterThanOrEqualTo:
                    word = ">=";
                    break;
                case ComparisonType.LessThan:
                    word = "<";
                    break;
                case ComparisonType.LessThanOrEqualTo:
                    word = "<=";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            filter.Append($" {word} ");
            Translate(comparisonExpression.Right, filter);
        }

        private void TranslateBooleanBinaryExpression(BooleanBinaryExpression booleanBinaryExpression, SqlFilter filter)
        {
            Translate(booleanBinaryExpression.Left,filter);

            var word = (booleanBinaryExpression.GateType == LogicGateType.And ? " AND " : " OR ");
            filter.Append($"{word}");

            Translate(booleanBinaryExpression.Right,filter);

        }
    }

    //public class CosmosOrm : ICosmosOrm
    //{
    //    private bool _offline = true;
    //    private CloudTableClient _client;

    //    private static Dictionary<Type, Uri> _collectionUris = new Dictionary<Type, Uri>();

    //    public CosmosOrm(CosmosDbConfiguration options)
    //    {
    //        var account = CloudStorageAccount.Parse(options.ConnectionString);
    //        _client = account.CreateCloudTableClient();
    //        _offline = false;
    //    }

    //    private void AssertIsOnline()
    //    {
    //        if (_offline)
    //            throw new Exception("Application is not connected to storage account.");
    //    }


    //    public async void Delete(WarpCoreEntity item)
    //    {
    //        AssertIsOnline();

    //        var table = GetOrCreateTable(item.GetType()).Result;
    //        await table.ExecuteAsync(TableOperation.Delete(item));
    //    }

    //    public async void Save(WarpCoreEntity item)
    //    {
    //        AssertIsOnline();

    //        if (!item.IsDirty)
    //            return;

    //        if (item.InternalId == null)
    //            item.InternalId = Guid.NewGuid();

    //        var table = GetOrCreateTable(item.GetType()).Result;
    //        await table.ExecuteAsync(TableOperation.InsertOrReplace(item));
    //    }

    //    public async Task<IReadOnlyCollection<T>> FindContentVersions<T>(string filter, ContentEnvironment version)
    //        where T : VersionedContentEntity, new()
    //    {

    //        string partitionCondition = null;
    //        if (version != ContentEnvironment.Any)
    //            partitionCondition = $"PartitionKey == '{version}'";

    //        var allConditions = new[] {filter, partitionCondition}.Where(x => !string.IsNullOrWhiteSpace(x));
    //        var joinedCondition = string.Join(" and ", allConditions);

    //        return await FindContentImpl<T>(joinedCondition);
    //    }

    //    public async Task<IReadOnlyCollection<T>> FindUnversionedContent<T>(string condition) where T : UnversionedContentEntity, new()
    //    {
    //        return await FindContentImpl<T>(condition);
    //    }

   
        

    //    private async Task<IReadOnlyCollection<T>> FindContentImpl<T>(string condition = null) where T : WarpCoreEntity, new()
    //    {
    //        AssertIsOnline();

    //        var cloudTable = GetOrCreateTable(typeof(T)).Result;
    //        var query = new TableQuery<T> { FilterString = condition };
    //        var items = await cloudTable.ExecuteQuerySegmentedAsync(query, new TableContinuationToken());

    //        var result = items.ToList();
    //        foreach (var item in result)
    //            item.InitializeChangeTracking();

    //        return result;
    //    }

    //    private async Task<CloudTable> GetOrCreateTable(Type type) 
    //    {
    //        var table = type.GetCustomAttribute<TableAttribute>();
    //        var tableRef = _client.GetTableReference(table.Name);
    //        if (!_collectionUris.ContainsKey(type))
    //        {
    //            await tableRef.CreateIfNotExistsAsync();
    //            _collectionUris.Add(type, tableRef.Uri);
    //        }

    //        return tableRef;
    //    }
        



    //}

}
