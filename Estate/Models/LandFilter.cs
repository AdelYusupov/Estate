using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estate.Models
{
    public class LandFilter
    {
        [Key]
        public int LandFilter_ID { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MinArea { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MaxArea { get; set; }

        public ICollection<Demand> Demands { get; set; }
    }
}