using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantumCore.API.Game
{
    public record PermissionGroup
    {
        public Guid Id;
        public string Name;
        public IList<Guid> Users;
        public IList<string> Permissions;
    }
}
