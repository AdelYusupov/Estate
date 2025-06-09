using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estate.Models
{
    public class Agent
    {
        [Key]
        public int Agent_ID { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string MiddleName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        public int? DealShare { get; set; }

        public ICollection<Supply> Supplies { get; set; }
        public ICollection<Demand> Demands { get; set; }
    }
}