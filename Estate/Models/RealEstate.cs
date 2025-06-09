using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estate.Models
{
    public class RealEstate
    {
        [Key]
        public int RealEstate_ID { get; set; }

        [ForeignKey("District")]
        public int District_ID { get; set; }
        public District District { get; set; }

        [ForeignKey("RealEstateType")]
        public int Type_ID { get; set; }
        public RealEstateType RealEstateType { get; set; }

        [MaxLength(255)]
        public string Address { get; set; }

        [MaxLength(20)]
        public string ApartmentNumber { get; set; }

        [MaxLength(20)]
        public string HouseNumber { get; set; }

        [MaxLength(20)]
        public string LandNumber { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? TotalArea { get; set; }

        public int? Rooms { get; set; }

        public int? Floor { get; set; }

        public int? TotalFloors { get; set; }

        public ICollection<Supply> Supplies { get; set; }
    }
}