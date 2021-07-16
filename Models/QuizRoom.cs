using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace QuizR.Models
{
    public class QuizRoom
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Display(Name = "Nazwa pokoju")]
        [Required(ErrorMessage = "Nie podano nazwy pokoju!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Nie wybrano zestawu pytań!")]
        [Display(Name = "Zestaw pytań")]
        public QuestionSet Set { get; set; }

        [Required]
        public ApplicationUser Owner { get; set; }

        public virtual List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}