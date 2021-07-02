using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Diagnostics;

namespace QuizR.Models
{
    public class QuestionSet
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public ApplicationUser Owner { get; set; }

        public virtual List<Question> Questions { get; set; } = new List<Question>();
    }

    public class Question
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [Required(ErrorMessage = "Treść pytania jest wymagana!")]
        [DisplayName("Treść pytania")]
        public string Content { get; set; }
        
        
        [Required(ErrorMessage = "Należy uzupełnić wszystkie odpowiedzi!")]
        [DisplayName("Odpowiedź A")]
        public string Aans { get; set; }

        [Required(ErrorMessage = "Należy uzupełnić wszystkie odpowiedzi!")]
        [DisplayName("Odpowiedź B")]
        public string Bans { get; set; }

        [Required(ErrorMessage = "Należy uzupełnić wszystkie odpowiedzi!")]
        [DisplayName("Odpowiedź C")]
        public string Cans { get; set; }

        [Required(ErrorMessage = "Należy uzupełnić wszystkie odpowiedzi!")]
        [DisplayName("Odpowiedź D")]
        public string Dans { get; set; }

        [Required(ErrorMessage = "Wskaż prawidłową odpowiedź")]
        [DisplayName("Poprawna odpowiedź")]
        public string Correct { get; set; }
    }
}