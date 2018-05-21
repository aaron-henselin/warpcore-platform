using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using WarpCore.Data.Schema;
using WarpCore.DbEngine;
using WarpCore.Kernel;

namespace WarpCore.Framework.Orm
{
    public interface IDataAccessLayer
    {
        void Insert(DbTableSchema dbTableSchema, IRow entityValueSet);
        void Update(DbTableSchema dbTableSchema, IRow entityValueSet);

        Task<ITable> Retrieve(DbTableSchema dbTableSchema, Guid id);
    }




}
