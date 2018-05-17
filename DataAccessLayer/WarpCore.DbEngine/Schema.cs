using System;
using System.Collections.Generic;
using WarpCore.Data.Schema;

namespace WarpCore.DbEngine
{

    public class DbTableSchemaFactory
    {
        private string MakeColumnName(string propertyName)
        {
            return propertyName.ToLower();
        }

        private string MakeTableName(string entityName)
        {
            return entityName.ToLower();
        }

        public DbTableSchema CreateTableSchemaFromEntity(EntityMetadata entityMetadata)
        {
            var tableSchema = new DbTableSchema {TableName = MakeTableName(entityMetadata.TableName)};
            foreach (var property in entityMetadata.Properties)
            {
                var schema = new ColumnSchema
                {
                    ColumnName = MakeColumnName(property.PropertyName),
                    OriginatingPropertyName = property.PropertyName,
                };

                if (property.PropertyType == typeof(string))
                    schema.DbColumnType = DbColumnType.NVarChar;

                if (property.PropertyType == typeof(int))
                    schema.DbColumnType = DbColumnType.Integer;

                if (property.PropertyType == typeof(bool))
                    schema.DbColumnType = DbColumnType.Bit;

                if (property.PropertyType == typeof(DateTimeOffset))
                    schema.DbColumnType = DbColumnType.DateTimeOffset;

                if (property.PropertyType == typeof(Guid))
                    schema.DbColumnType = DbColumnType.Guid;

                if (property.PrimaryKey)
                    tableSchema.PrimaryKeyColumnName = schema.ColumnName;

                tableSchema.Columns.Add(schema);
            }

            return tableSchema;
        }
    }

    public class DbTableSchema
    {
        public string PrimaryKeyColumnName { get; set; }

        public string TableName { get; set; }

        public List<ColumnSchema> Columns { get; set; } = new List<ColumnSchema>();
    }

    public class ColumnSchema
    {
        public string OriginatingPropertyName { get; set; }
        public string ColumnName { get; set; }
        public DbColumnType DbColumnType { get; set; }
    }

    public enum DbColumnType
    {
        NVarChar,NText,Decimal,Integer,Bit, DateTimeOffset, Guid
    }
}
