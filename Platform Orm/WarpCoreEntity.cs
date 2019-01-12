using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.Kernel;
using WarpCore.Platform.Kernel.Extensions;

namespace WarpCore.Platform.Orm
{

    [DebuggerDisplay("Title = {" + nameof(Title) + "}")]
    public abstract class WarpCoreEntity
    {
        
        private readonly string _titleProperty;


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
            {
                InitializeCustomFields(def);
                _titleProperty = def.TitleProperty;
            }
        }


        [JsonIgnore]
        public bool IsNew => RowKey == null;

        [SerializedComplexObject]
        public Dictionary<string, string> CustomFieldData { get; set; } = new Dictionary<string, string>();

        public void SetCustomField<T>(string fieldName, T value)
        {
            if (!CustomFieldData.ContainsKey(fieldName))
                throw new Exception(fieldName + " is not a registered field.");

            CustomFieldData[fieldName] = (string)ExtensibleTypeConverter.ChangeType(value, typeof(string));
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

        public string RowKey { get; set; }
        public string PartitionKey { get; set; }


        /// <summary>
        /// An id unique to this content, but shared between master and published copies.
        /// </summary>
        public Guid ContentId { get; set; }

        private string ChangeTracking { get; set; }

        public void InitializeChangeTracking()
        {
            ChangeTracking = JsonConvert.SerializeObject(this);
        }

        [JsonIgnore]
        public bool IsDirty => InternalId == null || !string.Equals(ChangeTracking, JsonConvert.SerializeObject(this));


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

        [IgnoreProperty]
        public virtual string Title
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_titleProperty))
                    return ContentId.ToString();

                var val = this.GetType().GetProperty(_titleProperty).GetValue(this)?.ToString();
                return val;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(_titleProperty))
                    throw new Exception("A title property is not defined for type "+GetType().FullName);

                this.GetType().GetProperty(_titleProperty).SetValue(this, value);
            }
        }
    }
}