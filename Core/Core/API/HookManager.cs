using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using QuantumCore.API;
using Serilog;

namespace QuantumCore.Core.API
{
    /// <summary>
    /// Manage and maintain all hooks into the main core for plugins.
    /// </summary>
    public class HookManager : IHookManager
    {
        private readonly Dictionary<Type, List<object>> _registeredHooks = new Dictionary<Type, List<object>>();

        private readonly Dictionary<Type, MethodInfo> _functionCache = new Dictionary<Type, MethodInfo>();
        
        public static HookManager Instance { get; private set; }

        public HookManager()
        {
            Debug.Assert(Instance == null);
            Instance = this;
        }
        
        /// <summary>
        /// Call all registered hooks in order of registrations.
        /// If a hook returns false further execution of hooks is aborted.
        /// The hooked method should than immediately stop any further execution. 
        /// </summary>
        /// <param name="args">Arguments needed for the hook</param>
        /// <returns>if this method returns false further execution should be stopped</returns>
        /// <typeparam name="T">Hook type</typeparam>
        public bool CallHook<T>(params object[] args) where T : IHook
        {
            var type = typeof(T);
            if (!_registeredHooks.ContainsKey(type))
            {
                // We have no registered hooks for this type
                return true;
            }

            if (!_functionCache.ContainsKey(type))
            {
                // Generate all required reflection cache
                // todo: maybe move to hook registration to safe performance for the first call
                GenerateHookCache<T>();
            }

            foreach (var hook in _registeredHooks[type])
            {
                var ret = _functionCache[type].Invoke(hook, args);
                
                if (ret is bool b && b == false)
                {
                    // Hook returned false, abort further execution
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Generates all required reflection cache for the given hook type
        /// </summary>
        /// <typeparam name="T">Hook type</typeparam>
        private void GenerateHookCache<T>() where T : IHook
        {
            var type = typeof(T);
            var methods = type.GetMethods();
            Debug.Assert(methods.Length == 1);

            _functionCache[type] = methods[0];
        }
        
        /// <summary>
        /// Register the hook
        /// </summary>
        /// <param name="hook"></param>
        /// <typeparam name="T"></typeparam>
        public void RegisterHook<T>(T hook) where T : IHook
        {
            if (!_registeredHooks.ContainsKey(typeof(T)))
            {
                _registeredHooks[typeof(T)] = new List<object>();
            }
            
            Log.Information($"Register Hook {typeof(T).Name}");
            _registeredHooks[typeof(T)].Add(hook);
        }

        /// <summary>
        /// Unregister the given hook
        /// </summary>
        /// <param name="hook">Instance of the hook class which is already registered</param>
        /// <typeparam name="T">Hook type</typeparam>
        public void UnregisterHook<T>(T hook) where T : IHook
        {
            if (!_registeredHooks.ContainsKey(typeof(T)))
            {
                Log.Warning($"Failed to unregister hook {typeof(T).Name} because it's not registered");
                return;
            }

            if (!_registeredHooks[typeof(T)].Remove(hook))
            {
                Log.Warning($"Failed to unregister hook {typeof(T).Name} because it's not registered");   
            }
            
            Log.Information($"Unregistered hook {typeof(T).Name}");
        }

        /// <summary>
        /// Unregister all hooks defined in the given assembly
        /// </summary>
        /// <param name="assembly"></param>
        public void UnregisterAssembly(Assembly assembly)
        {
            foreach (var (type, hooks) in _registeredHooks)
            {
                var hooksToUnregister = hooks.Where(hook => hook.GetType().Assembly == assembly).ToList();
                foreach (var hook in hooksToUnregister)
                {
                    _registeredHooks[type].Remove(hook);
                        
                    Log.Information($"Unregistered hook {type.Name}");
                }
            }
        }
    }
}