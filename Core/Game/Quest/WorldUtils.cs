using QuantumCore.API.Game.World;
using QuantumCore.Game.World;
using Tomlyn.Model;

namespace QuantumCore.Game.Quest;

public static class WorldUtils
{

    public static SpawnGroup GroupFromToml(TomlTable toml)
    {
        var sg = new SpawnGroup();
        if (toml["member"] is TomlTableArray members)
        {
            foreach (var member in members)
            {
                sg.Members.Add(MemberFromToml(member));
            }
        }

        sg.Id = (int) (toml["id"] as long? ?? 0);
        sg.Name = toml.Keys.Contains("name") ? toml["name"] as string : "";

        return sg;
    }

    public static SpawnMember MemberFromToml(TomlTable toml)
    {
        return new SpawnMember {
            Id = (uint)(toml["id"] as long? ?? 0)
        };
    }
}