using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizR.Models
{

    public class Question
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public ApplicationUser Owner { get; set; }
        [Required]
        public QuestionSet Set { get; set; }

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