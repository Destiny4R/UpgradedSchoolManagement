using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpgradedSchoolManagementModels.Models
{
    public class PschoItems
    {
        public int Id { get; set; }
        [StringLength(50)]
        public string Name { get; set; }
        public int PschoCateId { get; set; }
        [ForeignKey(nameof(PschoCateId))]
        public PschoCategory PschoCategory { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
