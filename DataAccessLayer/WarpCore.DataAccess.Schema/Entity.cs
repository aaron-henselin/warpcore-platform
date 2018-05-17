using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarpCore.Data.Schema
{
    public class Entity
    {
        [Column]
        public Guid? Id { get; set; }

        public bool IsNew { get; set; } = true;
    }



}