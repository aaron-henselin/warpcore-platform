using System;
using System.Collections.Generic;
using System.Text;

namespace WarpCore.DbEngine
{
    public interface IRow
    {
        object this[string columnName] { get; }
    }

    public interface ITable
    {
        ICollection<IRow> Rows { get; }
    }
}
