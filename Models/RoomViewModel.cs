using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace WATHoot2.Models
{
    public class RoomViewModel
    {
        [Key]
        public string ID { get; set; }
        public ApplicationUser Owner { get; set; }

        public virtual List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();        
    }
}