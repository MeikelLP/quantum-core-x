using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types.Entities;

namespace QuantumCore.Game;

public class AnimationManager : IAnimationManager, ILoadable
{
    private readonly Dictionary<uint, Dictionary<AnimationType, Dictionary<AnimationSubType, Animation>>>
        _animations = new();

    private readonly IMonsterManager _monsterManager;
    private readonly ILogger<AnimationManager> _logger;
    private readonly IStructuredFileProvider _structuredFileProvider;
    private readonly IFileProvider _fileProvider;

    public AnimationManager(IMonsterManager monsterManager, ILogger<AnimationManager> logger,
        IStructuredFileProvider structuredFileProvider, IFileProvider fileProvider)
    {
        _monsterManager = monsterManager;
        _logger = logger;
        _structuredFileProvider = structuredFileProvider;
        _fileProvider = fileProvider;
    }

    /// <summary>
    /// Load all the animation data for all characters and monsters.
    /// So the server can calculate movement duration.
    /// </summary>
    public async Task LoadAsync(CancellationToken token = default)
    {
        _logger.LogInformation("Loading animation data");

        // Load character animations first
        var characterClasses = new[]
        {
            "warrior",
            "assassin",
            "sura",
            "shaman"
        };
        for (uint i = 0; i < characterClasses.Length; i++)
        {
            var characterClass = characterClasses[i];
            var pc1 = Path.Join("pc", characterClass);
            var pc2 = Path.Join("pc2", characterClass);

            await LoadAnimation(i, AnimationType.WALK, AnimationSubType.GENERAL,
                Path.Join(pc1, "general", "walk.msa"));
            await LoadAnimation(i, AnimationType.RUN, AnimationSubType.GENERAL,
                Path.Join(pc1, "general", "run.msa"));
            await LoadAnimation(i + 4, AnimationType.WALK, AnimationSubType.GENERAL,
                Path.Join(pc2, "general", "walk.msa"));
            await LoadAnimation(i + 4, AnimationType.RUN, AnimationSubType.GENERAL,
                Path.Join(pc2, "general", "run.msa"));
        }

        // Make sure mob data was loaded before proceeding
        await _monsterManager.LoadAsync(token);
        // Load monster animations
        foreach (var monster in _monsterManager.GetMonsters())
        {
            // The animation could be in monster or monster2
            var folder = monster.Folder.Trim('\0');
            if (folder.Length == 0) continue;

            var monster1 = Path.Join("monster", folder);
            var monster2 = Path.Join("monster2", folder);

            if (_fileProvider.GetDirectoryContents(monster1).Exists)
            {
                await LoadMonsterAnimation(monster, monster1);
            }
            else if (_fileProvider.GetDirectoryContents(monster2).Exists)
            {
                await LoadMonsterAnimation(monster, monster2);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to find animation folder of monster {Name}({Id}) {Folder}", monster.TranslatedName,
                    monster.Id, monster.Folder);
            }
        }
    }

    private async Task LoadMonsterAnimation(MonsterData monster, string folder)
    {
        var file = _fileProvider.GetFileInfo(Path.Combine(folder, "motlist.txt"));
        if (!file.Exists)
        {
            _logger.LogWarning("No motlist.txt in monster folder {Folder}", folder);
            return;
        }

        await using var fs = file.CreateReadStream();
        using var sr = new StreamReader(fs);
        while (!sr.EndOfStream)
        {
            var line = await sr.ReadLineAsync();
            var parts = line!.Split('\t', ' ');
            if (parts.Length != 4) continue;
            if (!parts[0].Equals("general", StringComparison.InvariantCultureIgnoreCase)) continue;

            if (parts[1].Equals("run", StringComparison.InvariantCultureIgnoreCase))
            {
                await LoadAnimation(monster.Id, AnimationType.RUN, AnimationSubType.GENERAL,
                    Path.Join(folder, parts[2]));
            }
            else if (parts[1].Equals("walk", StringComparison.InvariantCultureIgnoreCase))
            {
                await LoadAnimation(monster.Id, AnimationType.WALK, AnimationSubType.GENERAL,
                    Path.Join(folder, parts[2]));
            }
        }
    }

    /// <summary>
    /// Get animation for specific entity type id
    /// </summary>
    /// <param name="id">Player Class or Monster ID</param>
    /// <param name="type">The main animation type</param>
    /// <param name="subType">The sub animation type</param>
    /// <returns>The animation or null if the animation doesn't exists</returns>
    public Animation? GetAnimation(uint id, AnimationType type, AnimationSubType subType)
    {
        if (_animations.TryGetValue(id, out var value) &&
            value.TryGetValue(type, out var val) &&
            val.TryGetValue(subType, out var output))
        {
            return output;
        }

        return null;
    }

    /// <summary>
    /// Load and parse the given animation
    /// </summary>
    /// <param name="id">The entity id (character type or monster id)</param>
    /// <param name="type">The main animation type</param>
    /// <param name="subType">The sub animation type</param>
    /// <param name="path">The path to the msa file</param>
    /// <returns>True if the animation was loaded successfully</returns>
    private async Task<bool> LoadAnimation(uint id, AnimationType type, AnimationSubType subType, string path)
    {
        var file = _fileProvider.GetFileInfo(path);
        if (!file.Exists) return false;

        _logger.LogDebug("Loading animation file {Path} ({Id} {Type} {SubType})", path, id, type, subType);

        var msa = await _structuredFileProvider.GetAsync(path);

        var scriptType = msa.GetValue("ScriptType");
        if (scriptType != "MotionData")
        {
            _logger.LogWarning(
                "Invalid msa file found under {Path}. Expected ScriptType 'MotionData' but got '{ScriptType}'",
                path, scriptType);
            return false;
        }

        var duration = msa.GetFloatValue("MotionDuration");
        if (duration == null)
        {
            _logger.LogWarning("Missing MotionDuration in msa file {Path}", path);
            return false;
        }

        var accuX = msa.GetFloatValue("Accumulation", 0);
        var accuY = msa.GetFloatValue("Accumulation", 1);
        var accuZ = msa.GetFloatValue("Accumulation", 2);
        if (accuX == null || accuY == null || accuZ == null)
        {
            _logger.LogWarning("Invalid Accumulation found in msa file {Path}", path);
            return false;
        }

        var animation = new Animation(duration.Value, accuX.Value, accuY.Value, accuZ.Value);
        PutAnimation(id, type, subType, animation);

        return true;
    }

    private void PutAnimation(uint id, AnimationType type, AnimationSubType subType, Animation animation)
    {
        if (!_animations.ContainsKey(id))
            _animations[id] = new Dictionary<AnimationType, Dictionary<AnimationSubType, Animation>>();
        if (!_animations[id].ContainsKey(type))
            _animations[id][type] = new Dictionary<AnimationSubType, Animation>();

        _animations[id][type][subType] = animation;
    }
}
