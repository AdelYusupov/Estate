using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estate.Models
{
    public class HouseFilter
    {
        [Key]
        public int HouseFilter_ID { get; set; }

        public int? MinFloor { get; set; }

        public int? MaxFloor { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MinArea { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MaxArea { get; set; }

        public int? MinRooms { get; set; }

        public int? MaxRooms { get; set; }

        public ICollection<Demand> Demands { get; set; }
    }
}