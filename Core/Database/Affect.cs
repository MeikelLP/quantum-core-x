using System;
<<<<<<< HEAD
using Dapper.Contrib.Extensions;
=======
>>>>>>> a2b5a04 (affect table->db, affect packets add/remove)

namespace QuantumCore.Database
{
    [System.ComponentModel.DataAnnotations.Schema.Table("affects")]
    public class Affect
    {
<<<<<<< HEAD
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
=======
        public Guid PlayerId { get; private set; }
        public long Type { get; set; }
        public byte ApplyOn { get; private set; }
        public int ApplyValue { get; private set; }
        public int Flag { get; set; }
        public DateTime Duration { get; set; }
>>>>>>> a2b5a04 (affect table->db, affect packets add/remove)
    }
}