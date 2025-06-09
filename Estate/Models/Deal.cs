using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estate.Models
{
    public class Deal
    {
        [ForeignKey("Demand")]
        public int Demand_ID { get; set; }
        public Demand Demand { get; set; }

        [ForeignKey("Supply")]
        public int Supply_ID { get; set; }
        public Supply Supply { get; set; }
    }
}