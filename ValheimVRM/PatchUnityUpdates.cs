using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Debug = UnityEngine.Debug;

namespace ValheimVRM
{
    public static class PatchAllUpdateMethods
    {
        public static void ApplyPatches(Harmony harmony)
        {
 
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assemblyFilesInPath = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.AllDirectories);
            var assemblyFiles = loadedAssemblies.Where(a => IsAssemblyInDirectory(a, assemblyFilesInPath)).ToList();

            foreach (var assembly in assemblyFiles)
            {
                try
                {
                    if (assembly.FullName.StartsWith("UnityEngine") ||
                        assembly.FullName.StartsWith("System") ||
                        assembly.FullName.StartsWith("mscorlib") ||
                        assembly.FullName.StartsWith("netstandard") ||
                        assembly.FullName.StartsWith("Microsoft") ||
                        assembly.FullName.StartsWith("Editor") ||
                        assembly.FullName.StartsWith("LuxParticles") ||
                        assembly.FullName.StartsWith("DemoScript"))
                    {
                        continue;
                    }

                    foreach (var type in assembly.GetTypes())
                    {
                        try
                        {
                            PatchMethod(harmony, type, "Update");
                            PatchMethod(harmony, type, "FixedUpdate");
                            PatchMethod(harmony, type, "LateUpdate");
                        }
                        catch (Exception ex)
                        {
                            Debug.Log($"Error patching type {type.FullName}: {ex.Message}");
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Debug.Log($"Error loading types from {assembly.FullName}: {ex.LoaderExceptions[0].Message}");
                }
            }
        }

        private static bool IsAssemblyInDirectory(Assembly assembly, string[] assemblyFiles)
        {
            string assemblyLocation = assembly.Location;
            return assemblyFiles.Any(file => file.Equals(assemblyLocation, StringComparison.OrdinalIgnoreCase));
        }

        private static void PatchMethod(Harmony harmony, Type type, string methodName)
        {
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null)
            {
                try
                {
                    harmony.Patch(method,
                        prefix: new HarmonyMethod(typeof(PatchAllUpdateMethods), nameof(GenericPrefix)),
                        postfix: new HarmonyMethod(typeof(PatchAllUpdateMethods), nameof(GenericPostfix)));
                    //Debug.Log($"Patched {methodName} in {type.FullName}");
                }
                catch (Exception ex)
                {
                    Debug.Log($"Failed to patch method {methodName} in {type.FullName}: {ex.Message}");
                }
            }
        }

        public static void GenericPrefix(out Stopwatch __state)
        {
            __state = new Stopwatch(); 
            __state.Start();
            // var stackTrace = new StackTrace();
            // var frame = stackTrace.GetFrame(1); 
            // var method = frame.GetMethod();
            // Debug.Log($"Before {method.DeclaringType.FullName}.{method.Name}");
        }

        public static void GenericPostfix(Stopwatch __state)
        {
            __state.Stop();

            int elapsedMilliseconds = (int)__state.Elapsed.TotalMilliseconds;
            
            if (elapsedMilliseconds > Settings.globalSettings.ProfileLogThresholdMs)
            {
                var stackTrace = new StackTrace();
                var frame = stackTrace.GetFrame(1);
                var method = frame.GetMethod();
                Debug.Log($"{method.DeclaringType.FullName}.{method.Name} | Runtime -> {elapsedMilliseconds} ms");

            }
        }
    }
}
