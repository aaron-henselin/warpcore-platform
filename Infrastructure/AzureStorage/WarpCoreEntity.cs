using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Framework;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace WarpCore.DbEngines.AzureStorage
{
    public abstract class DynamicEntityProxy : TableEntity
    {
        public Dictionary<string, string> CustomFieldData { get; set; } = new Dictionary<string, string>();

    }

    public abstract class WarpCoreEntity : TableEntity
    {
        private IDynamicTypeDefinitionResolver _dynamicTypeDefinitionResolver;

        public WarpCoreEntity(DynamicTypeDefinition definition)
        {
            InitializeCustomFields(definition);
        }

        private void InitializeCustomFields(DynamicTypeDefinition definition)
        {
            foreach (var field in definition.DynamicProperties)
            {
                var t = Type.GetType(field.PropertyTypeName);
                CustomFieldData[field.PropertyName] = t.GetDefault()?.ToString();
            }
        }

        protected WarpCoreEntity(IDynamicTypeDefinitionResolver dynamicTypeDefinitionResolver)
        {
            var def =dynamicTypeDefinitionResolver.Resolve(this.GetType());
            if (def != null)
                InitializeCustomFields(def);
        }

        [JsonIgnore]
        public bool IsNew => RowKey == null;

        [SerializedComplexObject]
        public Dictionary<string, string> CustomFieldData { get; set; } = new Dictionary<string, string>();

        public void SetCustomField<T>(string fieldName, T value)
        {
            if (!CustomFieldData.ContainsKey(fieldName))
                throw new Exception(fieldName + " is not a registered field.");

            CustomFieldData[fieldName] = (string)DesignerTypeConverter.ChangeType(value, typeof(string));
        }

        /// <summary>
        /// An id unique to each environment, not shared between master and published copies
        /// </summary>
        [IgnoreProperty]
        public Guid? InternalId
        {
            get
            {
                if (RowKey == null)
                    return null;

                return new Guid(this.RowKey);
            }
            set { this.RowKey = value?.ToString(); }
        }

        /// <summary>
        /// An id unique to this content, but shared between master and published copies.
        /// </summary>
        public Guid ContentId { get; set; }

        private string ChangeTracking { get; set; }

        internal void InitializeChangeTracking()
        {
            ChangeTracking = JsonConvert.SerializeObject(this);
        }

        internal bool IsDirty => InternalId == null || !string.Equals(ChangeTracking, JsonConvert.SerializeObject(this));



        public string ComplexData
        {
            get
            {
                var propsToSerialize = this.GetType()
                    .GetProperties()
                    .Where(x => CustomAttributeExtensions.GetCustomAttribute<SerializedComplexObjectAttribute>((MemberInfo) x) != null
                                || CustomAttributeExtensions.GetCustomAttribute<SerializedComplexObjectAttribute>((MemberInfo) x.PropertyType) != null  
                    );
                  

                var dataDict = new Dictionary<string, object>();
                foreach (var prop in propsToSerialize)
                {
                    dataDict.Add(prop.Name, prop.GetValue(this));
                }

                return JsonConvert.SerializeObject(dataDict);
            }
            set
            {
                var dataDict = JsonConvert.DeserializeObject(value, this.GetType());
                var propsToSerialize = this.GetType()
                    .GetProperties()
                    .Where(x => x.GetCustomAttribute<SerializedComplexObjectAttribute>() != null
                                || x.PropertyType.GetCustomAttribute<SerializedComplexObjectAttribute>() != null
                    );

                foreach (var prop in propsToSerialize)
                {
                    prop.SetValue(this, prop.GetValue(dataDict));
                }
            }
        }
    }
}