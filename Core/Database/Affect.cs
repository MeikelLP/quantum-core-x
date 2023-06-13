using System;

namespace QuantumCore.Database
{
    [System.ComponentModel.DataAnnotations.Schema.Table("affects")]
    public class Affect
    {
        public Guid PlayerId { get; private set; }
        public long Type { get; set; }
        public byte ApplyOn { get; private set; }
        public int ApplyValue { get; private set; }
        public int Flag { get; set; }
        public DateTime Duration { get; set; }
    }
}