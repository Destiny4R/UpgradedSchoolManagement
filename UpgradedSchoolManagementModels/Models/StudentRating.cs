using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpgradedSchoolManagementModels.Models
{
    public class StudentRating
    {
        public int Id { get; set; }
        public byte? Attentiveness { get; set; }//Yes
        public byte? Attendance { get; set; }//Yes
        public byte? Reliability { get; set; }//Yes
        public byte Punctuality { get; set; }//Yes
        public byte? Perseverance { get; set; }//Yes
        public byte? Neatness { get; set; }//yes
        public byte? Sense_of_Responsibility { get; set; }//Yes
        public byte? Politeness { get; set; }//yes
        public byte? Spirit_of_Cooperation { get; set; }//Yes
        public byte? SelfControl { get; set; }
        public byte? Relationship_With_Student { get; set; }//Yes
        public byte? Relation_With_Staff { get; set; }//Yes
        public byte? Curiosity { get; set; }//yes
        public byte? Initiative { get; set; }//yes
        public byte? Honesty{ get; set; }//yes
        public byte? Industry { get; set; }//Yes
        public byte? Humility{ get; set; }//yes
        public byte? Organisational_Ability { get; set; }//Yes
        public byte? Tolanrance{ get; set; }//yes
        public byte? Leadership { get; set; }//Yes
        public byte? Respect_For_Other { get; set; }//Yes
        public byte? Courage{ get; set; }//yes
        /// <summary>
        /// /PSYCHOMOTOR SKILLS
        /// </summary>
        public byte? Handwriting { get; set; }//yes
        public byte? Fluecy { get; set; }//yes
        public byte? Drawing_Painting { get; set; }//Yes
        public byte? Handing_WShop_Tool { get; set; }
        public byte? Games_Sport { get; set; }
        public byte? Musical_Skill { get; set; }
        public byte? Constrution { get; set; }
        public long TermRegId { get; set; }
        [ForeignKey(nameof(TermRegId))]
        public TermRegistration TermRegistration { get; set; }

    }
}
