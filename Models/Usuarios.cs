using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NgCapitalApi.Models
{
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        [StringLength(45)]
        public required string Nombre { get; set; }
        
        [Required]
        [StringLength(45)]
        public required string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string ? Password { get; set; }
    }
}