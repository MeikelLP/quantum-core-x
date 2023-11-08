using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuantumCore.Core.Cache;
using QuantumCore.Database.Repositories;

namespace QuantumCore.Database
{
    [System.ComponentModel.DataAnnotations.Schema.Table("empires")]
    public class Empires : BaseModel, ICache
    {
        public Guid AccountId { get; set; }
        public byte Empire { get; set; }    
    }
}
