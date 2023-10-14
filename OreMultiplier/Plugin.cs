using Channel3.ModKit;
using HarmonyLib;
using System.Reflection;
using System;
using Unfoundry;
using Channel3;
using UnityEngine;

namespace OreMultiplier
{
    [UnfoundryMod(Plugin.GUID)]
    public class Plugin : UnfoundryPlugin
    {
        public const string
            MODNAME = "OreMultiplier",
            AUTHOR = "erkle64",
            GUID = AUTHOR + "." + MODNAME,
            VERSION = "0.1.0";

        public static LogSource log;

        public Plugin()
        {
            log = new LogSource(MODNAME);
        }

        public override void Load(Mod mod)
        {
            log.Log($"Loading {MODNAME}");
        }

        [HarmonyPatch]
        public static class Patch
        {
            [HarmonyPatch(typeof(TerrainBlockType), nameof(TerrainBlockType.onLoad))]
            [HarmonyPrefix]
            public static void TerrainBlockType_onLoad(TerrainBlockType __instance)
            {
                if (__instance.oreSpawn_chancePerChunk_ground > 0)
                {
                    log.Log($"Multiplying {__instance.identifier} chance x6");
                    __instance.oreSpawn_chancePerChunk_ground /= 6;
                    __instance.oreSpawn_chancePerChunk_surface /= 6;
                }
            }

            [HarmonyPatch(typeof(ChunkManager), nameof(ChunkManager.fixMissingOrePatches))]
            [HarmonyPrefix]
            public static void ChunkManager_fixMissingOrePatches(CubeSavegame saveGame)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    saveGame.hasOrePatchFixupBeenApplied = false;
                    saveGame.doesSaveRequireOrePatchFixup = true;
                }
            }
        }
    }
}
