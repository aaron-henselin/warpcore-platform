using System;
using System.Threading.Tasks;
using WarpCore.DbEngine;

namespace WarpCore.Framework.Orm
{
    public interface IDataAccessLayer
    {
        void Insert(DbTableSchema dbTableSchema, IRow entityValueSet);
        void Update(DbTableSchema dbTableSchema, IRow entityValueSet);

        Task<ITable> Retrieve(DbTableSchema dbTableSchema, Guid id);
    }
}
