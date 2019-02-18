using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.DataAnnotations.UserInteraceHints;
namespace BlazorComponents.Shared
{
    public class ContentBrowserConfiguration
    {
        [UserInterfaceHint(Editor = Editor.OptionList)]
        [DataRelation("3a9a6f79-9564-4b51-af1c-9d926fddbc35")]
        [DisplayName("Repository")]
        public Guid RepositoryApiId { get; set; }

        public Guid? ListId { get; set; }
        public Guid? EditFormId { get; set; }
        public Guid? AddFormId { get; set; }

        public List<ContentBrowserLink> Links { get; set; } = new List<ContentBrowserLink>();
    }

    public class ContentBrowserLink
    {
        public string LinkTemplate { get; set; }

    }

    
}
