using C3.ModKit;
using HarmonyLib;
using Unfoundry;
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
            VERSION = "0.2.0";

        public static LogSource log;

        public static TypedConfigEntry<float> chanceMultiplier;
        public static TypedConfigEntry<float> yieldMultiplier;

        public Plugin()
        {
            log = new LogSource(MODNAME);

            new Config(GUID)
                .Group("Ore Multiplication")
                    .Entry(out chanceMultiplier, "chanceMultiplier", 2, "Ore patch chance multiplication factor.")
                    .Entry(out yieldMultiplier, "yieldMultiplier", 8, "Ore patch yield multiplication factor.")
                .EndGroup()
                .Load()
                .Save();
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
                    var chanceFactor = chanceMultiplier.Get();
                    if (chanceFactor > 0.0f && chanceFactor != 1.0f)
                    {
                        log.Log($"Multiplying {__instance.identifier} chance x{chanceFactor}");
                        __instance.oreSpawn_chancePerChunk_ground = (uint)Mathf.Ceil(__instance.oreSpawn_chancePerChunk_ground / (float)chanceFactor);
                        __instance.oreSpawn_chancePerChunk_surface = (uint)Mathf.Ceil(__instance.oreSpawn_chancePerChunk_surface / (float)chanceFactor);
                    }

                    var yieldFactor = yieldMultiplier.Get();
                    if (yieldFactor > 0.0f && yieldFactor != 1.0f)
                    {
                        log.Log($"Multiplying {__instance.identifier} yield x{yieldFactor}");
                        __instance.averageYield = (int)Mathf.Ceil(__instance.averageYield * yieldFactor);
                    }
                }
            }

            [HarmonyPatch(typeof(ChunkManager), nameof(ChunkManager.fixMissingOrePatches))]
            [HarmonyPrefix]
            public static void ChunkManager_fixMissingOrePatches(CubeSavegame saveGame)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    log.Log($"Triggering missing ore patch fixup routine");
                    saveGame.hasOrePatchFixupBeenApplied = false;
                    saveGame.doesSaveRequireOrePatchFixup = true;
                }
            }
        }
    }
}
