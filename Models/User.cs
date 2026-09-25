using System;
using System.ComponentModel.DataAnnotations;

namespace IMS25T_Core.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Benutzername ist erforderlich")]
        [StringLength(100)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Passwort ist erforderlich")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [StringLength(255)]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Rolle ist erforderlich")]
        public string Role { get; set; } // "Student" oder "Admin"

        [Required(ErrorMessage = "Vorname ist erforderlich")]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Nachname ist erforderlich")]
        [StringLength(100)]
        public string LastName { get; set; }

        public DateTime CreatedAt { get; set; }

        public string FullName
        {
            get { return FirstName + " " + LastName; }
        }
    }
}
