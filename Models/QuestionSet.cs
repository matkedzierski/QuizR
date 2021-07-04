using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizR.Models
{
    public class QuestionSet
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Display(Name = "Nazwa zestawu")]
        
        [Required(ErrorMessage = "Nie podano nazwy zestawu")]
        public string Name { get; set; }
        
        public ApplicationUser Owner { get; set; }

        public virtual List<Question> Questions { get; set; } = new List<Question>();
    }
}