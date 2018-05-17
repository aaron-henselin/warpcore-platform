﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarpCore.DbEngine;

namespace WarpCore.Data.Schema
{
    public class DbEngineAdapter
    {
        private readonly IDbAdapter _dbAdapter;

        public DbEngineAdapter(IDbAdapter dbAdapter)
        {
            _dbAdapter = dbAdapter;
        }

        private List<IRow> BuildTableRowsFromEntity(Entity entity)
        {
            var topLevelType = entity.GetType();
            var topLevelMetadata = new OrmMetadataReader().ReadMetadata(topLevelType);
            return BuildTableRowsFromEntity(entity, topLevelMetadata);
        }

        private List<IRow> BuildTableRowsFromEntity(Entity entity,OrmMetadata metadata)
        {
            var schema = new DbTableSchemaFactory().CreateTableSchemaFromEntity(metadata);
            var values = metadata.Properties.ToDictionary(x => x.ColumnName, x => x.PropertyInfo.GetValue(entity));
            var thisRow = DbRow.Create(schema, values);

            if (metadata.BaseMetadata != null)
            {
                var baseRows = BuildTableRowsFromEntity(entity,metadata.BaseMetadata);
                baseRows.Add(thisRow);
                return baseRows;
            }
            else
            {
                return new List<IRow>{thisRow};
            }
        }

        public void Save(Entity entity)
        {
            if (entity.IsNew && entity.Id == null)
                entity.Id = new Guid();

            var rows = BuildTableRowsFromEntity(entity);
            if (entity.IsNew)
            {
                foreach (var row in rows)
                    _dbAdapter.Insert(row.Schema,row);
            }
            if (entity.IsNew)
            {
                foreach (var row in rows)
                    _dbAdapter.Update(row.Schema, row);
            }
        }

        public void Get<TEntity>(Guid id) where TEntity:Entity
        {
            var entityType = typeof(TEntity);
            var metadata = new OrmMetadataReader().ReadMetadata(entityType);
            List<IRow> dataDependencies = GetDataDependencies(id, metadata);
            new EntityFactory().BuildEntity<TEntity>(dataDependencies);
        }

        private List<IRow> GetDataDependencies(Guid id, OrmMetadata metadata)
        {
            var schema = new DbTableSchemaFactory().CreateTableSchemaFromEntity(metadata);
            var baseData = _dbAdapter.Retrieve(schema, id).Result.Rows.Single();

            if (metadata.BaseMetadata != null)
            {
                var dependencies = GetDataDependencies(id, metadata);
                dependencies.Add(baseData);
                return dependencies;
            }
            else
                return new List<IRow>{baseData};
        }
    }


    public class EntityFactory
    {
        public Entity BuildEntity<TEntity>(IReadOnlyCollection<IRow> dataDependencies) where TEntity:Entity
        {
            return BuildEntity(typeof(TEntity), dataDependencies);
        }

        private Entity BuildEntity(Type entityType, IReadOnlyCollection<IRow> dataDependencies)
        {
            var entity = (Entity)Activator.CreateInstance(entityType);
            var metadata = new OrmMetadataReader().ReadMetadata(entityType);

            var schema = new DbTableSchemaFactory().CreateTableSchemaFromEntity(metadata);
            var matchingRow = dataDependencies.Single(x => x.Schema.TableName == schema.TableName);
            foreach (var property in metadata.Properties)
                property.PropertyInfo.SetValue(entity,matchingRow[property.ColumnName]);

            return entity;
        }
    }
}
