using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    public class StudentDetails
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(User))]
        public int StudentId { get; set; }

        [Required]
        public string? Course { get; set; }

        [Required]
        [Range(1, 8)]
        public int Semester { get; set; }

        public User? User { get; set; }

    }
}