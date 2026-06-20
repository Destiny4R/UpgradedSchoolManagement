using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpgradedSchoolManagementModels.Models
{
    public class PaymentItem
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(300)]
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(CategoryId))]
        public PaymentCategory PaymentCategory { get; set; }
        public ICollection<PaymentSetup> PaymentSetups { get; set; } = new List<PaymentSetup>();
    }
}
