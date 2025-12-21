using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class User : BaseEntity
    {
        [Required]
        public string Login { get; set; } = null!;
        [Required]
        public string PasswordHash { get; set; } = null!;
        
        public string? FullName { get; set; }
        public DateTime? BirthDay { get; set; }
    }
}
