using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using WarpCore.DbEngines.AzureStorage;

namespace WarpCore.Cms
{
    public interface ISitemapNode
    {
        Guid NodeId { get;  }
        List<SitemapNode> ChildNodes { get; set; }
    }

    public class Sitemap: ISitemapNode
    {
        public Guid NodeId { get; set; }
        public List<SitemapNode> ChildNodes { get; set; } = new List<SitemapNode>();
    }

    [Unversioned]
    [Table("cms_site_tree")]
    public class SitemapNode : CosmosEntity, ISitemapNode
    {
        [Column]
        public Guid SiteId { get; set; }

        [Column]
        public Guid PageId { get; set; }

        [Column]
        public Guid ParentNodeId { get; set; }


        public Guid NodeId { get => this.ContentId.Value; }
        [JsonIgnore]
        public List<SitemapNode> ChildNodes { get; set; } = new List<SitemapNode>();
    }



}