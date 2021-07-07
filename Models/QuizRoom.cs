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
        [Display(Name = "Unikalna nazwa pokoju")]
        public string ID { get; set; }

        [Required(ErrorMessage = "Nie wybrano zestawu pytań!")]
        [Display(Name = "Zestaw pytań")]
        public QuestionSet Set { get; set; }

        [Required]
        public ApplicationUser Owner { get; set; }

        public virtual List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}