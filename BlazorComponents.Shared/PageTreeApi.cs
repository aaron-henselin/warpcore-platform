﻿using System;
using System.Collections.Generic;

namespace BlazorComponents.Shared
{

    public class PageTreeModel
    {
        public string SiteName;
        public Guid SiteId { get; set; }
        public List<PageTreeItem> Items { get; set; } = new List<PageTreeItem>();
        
    }
    
    public class PageTreeItem
    {
        public string Name { get; set; }
        public string VirtualPath { get; set; }
        public int Depth { get; set; }
        public string ParentPath { get; set; }
        public bool IsHomePage { get;  set; }
        public bool IsPublished { get; set; }
        public string DesignUrl { get; set; }

        public string SettingsUrl { get; set; }
        public bool IsExpanded { get; set; }
        public bool Visible { get; set; }
        public bool HasChildItems { get; set; }
        public Guid PageId { get; set; }
    }
}
