using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace QuizR.Models
{
    public class QuizRoom
    {
        [Key]
        public string ID { get; set; }
        public ApplicationUser Owner { get; set; }
        public QuestionSet Set { get; set; }
        public virtual List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}