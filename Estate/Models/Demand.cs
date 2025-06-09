using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estate.Models
{
    public class Demand
    {
        [Key]
        public int Demand_ID { get; set; }

        [MaxLength(255)]
        public string Address { get; set; }

        public int? MinPrice { get; set; }

        public int? MaxPrice { get; set; }

        [ForeignKey("Agent")]
        public int Agent_ID { get; set; }
        public Agent Agent { get; set; }

        [ForeignKey("Client")]
        public int Client_ID { get; set; }
        public Client Client { get; set; }

        [ForeignKey("ApartmentFilter")]
        public int? ApartmentFilter_ID { get; set; }
        public ApartmentFilter ApartmentFilter { get; set; }

        [ForeignKey("HouseFilter")]
        public int? HouseFilter_ID { get; set; }
        public HouseFilter HouseFilter { get; set; }

        [ForeignKey("LandFilter")]
        public int? LandFilter_ID { get; set; }
        public LandFilter LandFilter { get; set; }

        public ICollection<Deal> Deals { get; set; }
    }
}