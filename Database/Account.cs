using System;
using System.ComponentModel.DataAnnotations;

namespace QuantumCore.Database
{
    public class Account : BaseModel
    {
        [Required]
        [StringLength(30)]
        public string Username { get; set; }
        
        [Required]
        [StringLength(60)]
        public string Password { get; set; }
        
        [Required]
        public string Email { get; set; }

        [Required] public EAccountStatus Status { get; set; } = EAccountStatus.OK;
        
        public DateTime LastLogin { get; set; }
    }
}