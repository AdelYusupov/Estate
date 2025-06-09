using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace Estate.Models
{
    public class RealEstateType
    {
        [Key]
        public int Type_ID { get; set; }

        [Required]
        [MaxLength(50)]
        public string TypeName { get; set; }

        public ICollection<RealEstate> RealEstates { get; set; }
    }
}