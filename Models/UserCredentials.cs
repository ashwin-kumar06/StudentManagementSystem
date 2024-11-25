using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    public class UserCredential
    {
        [Key]
        public int UserCredentialId { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address")] 
        public string? Email { get; set; }
        public string? Password { get; set; }
        public DateTime LastLoginDate { get; set; }


        public User? User { get; set; }
    }
}