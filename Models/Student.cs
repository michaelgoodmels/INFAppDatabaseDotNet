using System;
using System.ComponentModel.DataAnnotations;

namespace IMS25T_Core.Models
{
    public class Student
    {
        public int StudentId { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        public char Gender { get; set; } // 'm' oder 'w'

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(100)]
        public string Street { get; set; }

        [Required]
        [StringLength(10)]
        public string PostalCode { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [Required]
        [StringLength(100)]
        public string Canton { get; set; }

        [StringLength(20)]
        [Phone]
        public string PhoneMobile { get; set; }

        [StringLength(20)]
        [Phone]
        public string PhoneHome { get; set; }

        [Required]
        [StringLength(100)]
        public string TeacherName { get; set; }

        public DateTime CreatedAt { get; set; }

        public string FullName
        {
            get { return FirstName + " " + LastName; }
        }

        public int Age
        {
            get
            {
                DateTime today = DateTime.Today;
                int age = today.Year - DateOfBirth.Year;
                if (DateOfBirth.Date > today.AddYears(-age)) age--;
                return age;
            }
        }
    }
}
