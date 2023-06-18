using System;
using Dapper.Contrib.Extensions;

namespace QuantumCore.Database
{
    [System.ComponentModel.DataAnnotations.Schema.Table("affects")]
    public class Affect
    {
        [ExplicitKey]
        public Guid PlayerId { get; set; }
        [ExplicitKey]
        public long Type { get; set; }
        [ExplicitKey]
        public byte ApplyOn { get; set; }
        [ExplicitKey]
        public int ApplyValue { get; set; }
        public int Flag { get; set; }
        public DateTime Duration { get; set; }
        public int SpCost { get; set; }
    }
}