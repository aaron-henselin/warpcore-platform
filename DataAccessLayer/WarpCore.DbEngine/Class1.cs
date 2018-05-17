using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WarpCore.DbEngine
{
    public interface IDbAdapter
    {
        void Insert(DbTableSchema dbTableSchema, IRow entityValueSet);
        void Update(DbTableSchema dbTableSchema, IRow entityValueSet);

        Task<ITable> Retrieve(DbTableSchema dbTableSchema, Guid id);
    }
}
