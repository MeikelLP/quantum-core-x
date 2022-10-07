using System;

namespace QuantumCore.Database
{
    [System.ComponentModel.DataAnnotations.Schema.Table("items")]
    public class Item : BaseModel
    {
        public Guid PlayerId { get; private set; }
        public uint ItemId { get; set; }
        public byte Window { get; private set; }
        public uint Position { get; private set; }
        public byte Count { get; set; }
    }
}