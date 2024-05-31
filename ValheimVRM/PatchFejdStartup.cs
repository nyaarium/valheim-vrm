using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ValheimVRM
{
    internal static class PatchFejdStartup
    {
        public static void Apply(Harmony harmony)
        {
            var originalMethod = typeof(FejdStartup).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            if (originalMethod == null)
            {
                Debug.LogError("Failed to find Awake method in FejdStartup.");
                return;
            }

            var postfix = new HarmonyMethod(typeof(PatchFejdStartup), nameof(Postfix));

            harmony.Patch(originalMethod, null, postfix);
        }

        private static void Postfix()
        {
            MainPlugin.PatchAll();
        }
    }
}