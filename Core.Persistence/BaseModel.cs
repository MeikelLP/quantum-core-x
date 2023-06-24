using Dapper.Contrib.Extensions;

namespace QuantumCore.Database
{
    public class BaseModel
    {
        [ExplicitKey]
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}