using System;
using System.Collections.Generic;
using System.Text;

namespace WarpCore.Cms.Content
{
    public interface ICmsContent
    {
        string Name { get; set; }
        Guid Id { get; set; }
    }

    public class CmsContentManager
    {
        public IEnumerable<ICmsContent> GetAll(string contentTypeCode)
        {
            return new List<ICmsContent>();
        }
    }

}
