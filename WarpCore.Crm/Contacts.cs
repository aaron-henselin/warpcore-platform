using System.ComponentModel.DataAnnotations.Schema;
using WarpCore.DbEngines.AzureStorage;

namespace WarpCore.Crm
{
    [Table("crm_individual")]
    public class Individual :Contact
    {
        [Column]
        public string FirstName { get; set; }

        [Column]
        public string LastName { get; set; }
    }

    [Table("crm_organization")]
    public class Organization : Contact
    {
        [Column]
        public string Name { get; set; }
    }


    [Table("crm_contact")]
    public class Contact : UnversionedContentEntity
    {

    }

}
