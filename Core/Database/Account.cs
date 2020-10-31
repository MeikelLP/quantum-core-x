using System;
using System.ComponentModel.DataAnnotations;

namespace QuantumCore.Database
{
    public class Account : BaseModel
    {
        public string Username { get; set; }
        
        public string Password { get; set; }
        
        public string Email { get; set; }

        public EAccountStatus Status { get; set; } = EAccountStatus.OK;
        
        public DateTime LastLogin { get; set; }
    }
}