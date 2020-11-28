using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using QuantumCore.Core.Types;
using Serilog;

namespace QuantumCore.Game
{
    public enum AnimationType
    {
        Run,
        Walk
    }

    public enum AnimationSubType
    {
        General
    }

    public class Animation
    {
        public float MotionDuration { get; private set; }
        public float AccumulationX { get; private set; }
        public float AccumulationY { get; private set; }
        public float AccumulationZ { get; private set; }

        public Animation(float motionDuration, float accumulationX, float accumulationY, float accumulationZ)
        {
            MotionDuration = motionDuration;
            AccumulationX = accumulationX;
            AccumulationY = accumulationY;
            AccumulationZ = accumulationZ;
        }
    }
    
    public static class AnimationManager
    {
        private static readonly Dictionary<uint, Dictionary<AnimationType, Dictionary<AnimationSubType, Animation>>>
            _animations = new Dictionary<uint, Dictionary<AnimationType, Dictionary<AnimationSubType, Animation>>>();
        
        /// <summary>
        /// Load all the animation data for all characters and monsters.
        /// So the server can calculate movement duration.
        /// </summary>
        public static void Load()
        {
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
                var pc1 = Path.Join("data", "pc", characterClass);
                var pc2 = Path.Join("data", "pc2", characterClass);

                LoadAnimation(i, AnimationType.Walk, AnimationSubType.General, Path.Join(pc1, "general", "walk.msa"));
                LoadAnimation(i, AnimationType.Run, AnimationSubType.General, Path.Join(pc1, "general", "run.msa"));
                
                LoadAnimation(i + 4, AnimationType.Walk, AnimationSubType.General, Path.Join(pc2, "general", "walk.msa"));
                LoadAnimation(i + 4, AnimationType.Run, AnimationSubType.General, Path.Join(pc2, "general", "run.msa"));
            }
            
            // Load monster animations
            // todo implement after mob proto load is implemented
        }

        /// <summary>
        /// Get animation for specific entity type id
        /// </summary>
        /// <param name="id">Player Class or Monster ID</param>
        /// <param name="type">The main animation type</param>
        /// <param name="subType">The sub animation type</param>
        /// <returns>The animation or null if the animation doesn't exists</returns>
        public static Animation GetAnimation(uint id, AnimationType type, AnimationSubType subType)
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
        private static bool LoadAnimation(uint id, AnimationType type, AnimationSubType subType, string path)
        {
            if (!File.Exists(path)) return false;
            
            Log.Debug($"Loading animation file {path} ({id} {type} {subType})");
            
            var msa = new StructuredFile();
            msa.Read(path);

            var scriptType = msa.GetValue("ScriptType");
            if (scriptType != "MotionData")
            {
                Log.Warning($"Invalid msa file found under {path}. Expected ScriptType 'MotionData' but got '{scriptType}'.");
                return false;
            }

            var duration = msa.GetFloatValue("MotionDuration");
            if (duration == null)
            {
                Log.Warning($"Missing MotionDuration in msa file {path}");
                return false;
            }

            var accuX = msa.GetFloatValue("Accumulation", 0);
            var accuY = msa.GetFloatValue("Accumulation", 1);
            var accuZ = msa.GetFloatValue("Accumulation", 2);
            if (accuX == null || accuY == null || accuZ == null)
            {
                Log.Warning($"Invalid Accumulation found in msa file {path}");
                return false;
            }
            
            var animation = new Animation(duration??0, accuX??0, accuY??0, accuZ??0);
            PutAnimation(id, type, subType, animation);
            
            return true;
        }

        private static void PutAnimation(uint id, AnimationType type, AnimationSubType subType, Animation animation)
        {
            if(!_animations.ContainsKey(id)) _animations[id] = new Dictionary<AnimationType, Dictionary<AnimationSubType, Animation>>();
            if(!_animations[id].ContainsKey(type)) _animations[id][type] = new Dictionary<AnimationSubType, Animation>();

            _animations[id][type][subType] = animation;
        }
    }
}