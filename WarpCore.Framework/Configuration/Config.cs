using System;
using System.Collections.Generic;
using System.Text;

namespace WarpCore.Kernel
{
    public class WarpCoreDataAccessConfig
    {
        public string StorageConnectionString { get; set; }

        public static WarpCoreDataAccessConfig Current { get; set; } = new WarpCoreDataAccessConfig();
    }
}
