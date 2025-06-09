using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace Estate.Models
{
    public class District
    {
        [Key]
        public int District_ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string Coordinates { get; set; }

        public ICollection<RealEstate> RealEstates { get; set; }
    }
}