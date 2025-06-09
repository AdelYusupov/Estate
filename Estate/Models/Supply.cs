using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estate.Models
{
    public class Supply
    {
        [Key]
        public int Supply_ID { get; set; }

        [Required]
        public int Price { get; set; }

        [ForeignKey("RealEstate")]
        public int RealEstate_ID { get; set; }
        public RealEstate RealEstate { get; set; }

        [ForeignKey("Agent")]
        public int Agent_ID { get; set; }
        public Agent Agent { get; set; }

        [ForeignKey("Client")]
        public int Client_ID { get; set; }
        public Client Client { get; set; }

        public ICollection<Deal> Deals { get; set; }
    }
}