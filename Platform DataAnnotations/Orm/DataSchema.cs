using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarpCore.Platform.DataAnnotations.Orm
{
    public class TableAttribute : Attribute
    {
        public string TableName { get; }

        public TableAttribute(string tableName)
        {
            TableName = tableName;
        }
    }

    public class ColumnAttribute : Attribute { }

}
