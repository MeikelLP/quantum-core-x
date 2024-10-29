using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.Core.Types;

namespace QuantumCore.Game
{
    public class AnimationManager : IAnimationManager
    {
        private readonly Dictionary<uint, Dictionary<AnimationType, Dictionary<AnimationSubType, Animation>>>
            _animations = new Dictionary<uint, Dictionary<AnimationType, Dictionary<AnimationSubType, Animation>>>();

        private readonly IMonsterManager _monsterManager;
        private readonly ILogger<AnimationManager> _logger;

        public AnimationManager(IMonsterManager monsterManager, ILogger<AnimationManager> logger)
        {
            _monsterManager = monsterManager;
            _logger = logger;
        }

        /// <summary>
        /// Load all the animation data for all characters and monsters.
        /// So the server can calculate movement duration.
        /// </summary>
        public async Task LoadAsync(CancellationToken token = default)
        {
            // Load character animations first
            var characterClasses = new[]
            {
                "warrior",
                "assassin",
                "sura",
                "shaman"
            };
            var pc1Path = Path.Combine("data", "pc");
            if (Directory.Exists(pc1Path))
            {
                _logger.LogInformation("Loading animations from {Path}", pc1Path);
                for (uint i = 0; i < characterClasses.Length; i++)
                {
                    var characterClass = characterClasses[i];
                    var pc1 = Path.Join(pc1Path, characterClass);

                    await LoadAnimation(i, AnimationType.Walk, AnimationSubType.General,
                        Path.Join(pc1, "general", "walk.msa"));
                    await LoadAnimation(i, AnimationType.Run, AnimationSubType.General,
                        Path.Join(pc1, "general", "run.msa"));
                }
            }
            else
            {
                _logger.LogWarning("{Path} does not exist, pc animations not loaded", pc1Path);
            }

            var pc2Path = Path.Combine("data", "pc2");
            if (Directory.Exists(pc1Path))
            {
                for (uint i = 0; i < characterClasses.Length; i++)
                {
                    var characterClass = characterClasses[i];
                    var pc2 = Path.Join(pc2Path, characterClass);

                    await LoadAnimation(i + 4, AnimationType.Walk, AnimationSubType.General,
                        Path.Join(pc2, "general", "walk.msa"));
                    await LoadAnimation(i + 4, AnimationType.Run, AnimationSubType.General,
                        Path.Join(pc2, "general", "run.msa"));
                }
            }
            else
            {
                _logger.LogWarning("{Path} does not exist, pc animations not loaded", pc2Path);
            }

            var monsters1Path = Path.Combine("data", "monster");
            var monsters2Path = Path.Combine("data", "monster2");
            if (Directory.Exists(monsters1Path))
            {
                // Load monster animations
                foreach (var monster in _monsterManager.GetMonsters())
                {
                    // The animation could be in monster or monster2
                    var folder = monster.Folder.Trim('\0');
                    if (folder.Length == 0) continue;

                    var monster1 = Path.Join(monsters1Path, folder);

                    if (Directory.Exists(monster1))
                    {
                        await LoadMonsterAnimation(monster, monster1);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Failed to find animation folder of monster {Name}({Id}) {Folder}", monster.TranslatedName,
                            monster.Id, monster.Folder);
                    }
                }
            }
            else if (Directory.Exists(monsters2Path))
            {
                // Load monster animations
                foreach (var monster in _monsterManager.GetMonsters())
                {
                    // The animation could be in monster or monster2
                    var folder = monster.Folder.Trim('\0');
                    if (folder.Length == 0) continue;

                    var monster2 = Path.Join(monsters2Path, folder);

                    if (Directory.Exists(monster2))
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

            if (!Directory.Exists(monsters1Path))
            {
                _logger.LogWarning("{Path} does not exist, mob animations not loaded", monsters1Path);
            }

            if (!Directory.Exists(monsters2Path))
            {
                _logger.LogWarning("{Path} does not exist, mob animations not loaded", monsters2Path);
            }
        }

        private async Task LoadMonsterAnimation(MonsterData monster, string folder)
        {
            var motlist = Path.Join(folder, "motlist.txt");
            if (!File.Exists(motlist))
            {
                _logger.LogWarning("No motlist.txt in monster folder {Folder}", folder);
                return;
            }

            var lines = await File.ReadAllLinesAsync(motlist);
            foreach (var line in lines)
            {
                var parts = line.Split(new char[] {'\t', ' '});
                if (parts.Length != 4) continue;
                if (parts[0].ToLower() != "general") continue;

                if (parts[1].ToLower() == "run")
                {
                    await LoadAnimation(monster.Id, AnimationType.Run, AnimationSubType.General,
                        Path.Join(folder, parts[2]));
                }
                else if (parts[1].ToLower() == "walk")
                {
                    await LoadAnimation(monster.Id, AnimationType.Walk, AnimationSubType.General,
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
            if (!_animations.ContainsKey(id)) return null;
            if (!_animations[id].ContainsKey(type)) return null;
            if (!_animations[id][type].ContainsKey(subType)) return null;

            return _animations[id][type][subType];
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
            if (!File.Exists(path)) return false;

            _logger.LogDebug("Loading animation file {Path} ({Id} {Type} {SubType})", path, id, type, subType);

            var msa = new StructuredFile();
            await msa.ReadAsync(path);

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
}