using System.Reflection;
using System.Text;
using CommandLine;
using QuantumCore.API.Game;
using QuantumCore.Game.Commands;

namespace DocsGenerator;

public static class CommandDocsGenerator
{
    public static async Task GenerateAsync(string targetDir)
    {
        var types = new[] {typeof(GotoCommand).Assembly} // ensure assembly is loaded into AppDomain
            .Concat(AppDomain.CurrentDomain.GetAssemblies())
            .SelectMany(x => x.GetTypes())
            .Distinct()
            .Where(x => x is {IsAbstract: false, IsClass: true} &&
                        x.GetCustomAttributes<CommandAttribute>().Any() &&
                        x.GetInterfaces()
                            .Any(i => (i.IsGenericType &&
                                       i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)) ||
                                      i == typeof(ICommandHandler)))
            .Select(x => new
            {
                x.Name,
                Commands = x.GetCustomAttributes<CommandAttribute>().Select(c => (c.Name, c.Description)).ToArray(),
                ArgumentType = x.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<>))
                    ?.GenericTypeArguments[0],
                Arguments = new List<(string Name, string Description, string Type, object? Default)>()
            })
            .ToArray();
        foreach (var type in types)
        {
            if (type.ArgumentType is null) continue;
            var props = type.ArgumentType
                .GetProperties()
                .Select(x => (x, Attribute: x.GetCustomAttribute<ValueAttribute>()))
                .Where(x => x.Attribute is not null)
                .Cast<(PropertyInfo Property, ValueAttribute Attribute)>()
                .OrderBy(x => x.Attribute.Index)
                .Select(x => (x.Property.Name, x.Attribute.HelpText, x.Property.PropertyType.Name, x.Attribute.Default))
                .ToArray();
            foreach (var prop in props)
            {
                type.Arguments.Add(prop);
            }
        }

        var commandsDir = Path.Combine(targetDir, "Commands");
        if (!Directory.Exists(commandsDir)) Directory.CreateDirectory(commandsDir);

        foreach (var type in types.AsParallel())
        {
            var path = Path.GetFullPath(Path.Combine(commandsDir, $"{type.Name}.md"));
            var content = new StringBuilder();
            content.AppendLine($"""
                                ---
                                title: /{type.Commands.First().Name}
                                ---
                                # {type.Name}

                                ## Command

                                ```sh
                                """);

            var arguments = string.Join(" ", type.Arguments.Select(x => $"[{x.Name}]"));
            foreach (var command in type.Commands)
            {
                content.AppendLine($"/{command.Name} {arguments}");
            }

            content.AppendLine("```");

            if (type.Arguments.Count > 0)
            {
                content.AppendLine("""

                                   ## Arguments

                                   |Argument|Type|Description|Default|
                                   |---|---|---|---|
                                   """);

                foreach (var argument in type.Arguments)
                {
                    content.AppendLine(
                        $"|`{argument.Name}`|{argument.Type}|{argument.Description}|{argument.Default ?? "`null`"}|");
                }
            }

            content.AppendLine($"""

                                ## Description

                                {type.Commands[0].Description}
                                """);

            await File.WriteAllTextAsync(path, content.ToString());
            Console.WriteLine(path);
        }
    }
}
