using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;

namespace WarpCore.Data.Schema
{
    public class EntityMetadataReader
    {
        public EntityMetadata ReadMetadata(Type type)
        {
            var metadata = new EntityMetadata();

            if (type.BaseType != typeof(object))
                metadata.BaseMetadata = ReadMetadata(type.BaseType);

            var tableAttribute = type.GetCustomAttribute<TableAttribute>();
            metadata.TableName = tableAttribute?.Name;
            
            var allProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach (var propertyInfo in allProperties)
            {
                var columnInfo = propertyInfo.GetCustomAttribute<ColumnAttribute>();
                if (columnInfo == null)
                    continue;

                metadata.Properties.Add(new PropertyMetadata
                {
                    PropertyName = propertyInfo.Name,
                    PropertyType = propertyInfo.PropertyType,
                    ColumnName = columnInfo.Name ?? propertyInfo.Name,
                    ColumnOrder = columnInfo.Order,
                });
            }

            return metadata;
        }
    }
}
