using System;
using System.Collections.Generic;
using System.Text;

namespace WarpCore.DbEngine
{
    public interface IRow
    {
        DbTableSchema Schema { get; set; }
        object this[string columnName] { get; }
    }

    public interface ITable
    {
        ICollection<IRow> Rows { get; }
    }
}
