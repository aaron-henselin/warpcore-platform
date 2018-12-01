using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarpCore.Platform.Kernel
{
    public interface IHasPerItemSecurity
    {
    }

    public enum SecurityAction
    {
        Unknown, Create, Read, Update, Delete
    }

    public static class SecuredOperation
    {
        public static void Demand(SecurityAction securityAction, IHasPerItemSecurity securedResource)
        {
            
        }
        
    }



}
