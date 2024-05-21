using C3.ModKit;
using HarmonyLib;
using System.Linq;
using Unfoundry;
using UnityEngine;

namespace OreMultiplier
{
    [UnfoundryMod(GUID)]
    public class Plugin : UnfoundryPlugin
    {
        public const string
            MODNAME = "OreMultiplier",
            AUTHOR = "erkle64",
            GUID = AUTHOR + "." + MODNAME,
            VERSION = "0.2.6";

        public static LogSource log;

        public static TypedConfigEntry<float> chanceMultiplier;
        public static TypedConfigEntry<float> yieldMultiplier;
        public static TypedConfigEntry<float> reservoirChanceMultiplierOverride;
        public static TypedConfigEntry<float> reservoirYieldMultiplierOverride;
        public static TypedConfigEntry<float> veinChanceMultiplierOverride;
        public static TypedConfigEntry<float> veinYieldMultiplierOverride;

        public Plugin()
        {
            log = new LogSource(MODNAME);

            new Config(GUID)
                .Group("Ore Multiplication")
                    .Entry(out chanceMultiplier, "chanceMultiplier", 2.0f, true,
                        "Ore patch chance multiplication factor.",
                        "Increase this to spawn more ore patches, reservoirs and veins.",
                        "Large numbers will cause olumite reservoirs to fill all available positions, leaving no space for ore patches.",
                        "Use reservoirChanceMultiplierOverride to prevent that from happening.")
                    .Entry(out yieldMultiplier, "yieldMultiplier", 8.0f, true,
                        "Ore patch yield multiplication factor.",
                        "Increase this to make ore patches and veins contain more ore per block and reservoirs contain more olumite.")
                .EndGroup()
                .Group("Reservoir Overrides")
                    .Entry(out reservoirChanceMultiplierOverride, "reservoirChanceMultiplierOverride", 0.0f, true,
                        "Override chance multiplier for olumite reservoirs.",
                        "0 or negative = use chanceMultiplier.",
                        "1 = disable reservoir chance multiplication.",
                        "Numbers larger than 1 increase the number of olumite reservoirs on the map.",
                        "Recommended to keep this below 10.")
                    .Entry(out reservoirYieldMultiplierOverride, "reservoirYieldMultiplierOverride", 0.0f, true,
                        "Override yield multiplier for olumite reservoirs.",
                        "0 = use yieldMultiplier.",
                        "1 = disable reservoir yield multiplication.",
                        "Numbers larger than 1 increase the amount of olumite in each reservoir.")
                .EndGroup()
                .Group("Vein Overrides")
                    .Entry(out veinChanceMultiplierOverride, "veinChanceMultiplierOverride", 1.0f, true,
                        "Override chance multiplier for ore veins.",
                        "Ore veins are the large patches of underground ore unlocked later in the game.",
                        "0 = use chanceMultiplier.",
                        "1 = disable ore vein chance multiplication.",
                        "Numbers larger than 1 increase the number of ore veins on the map.",
                        "Very large numbers will cause ore veins to take all the available spaces, leaving no room for ore patches or olumite reservoirs.",
                        "Recommended to keep this below 5.")
                    .Entry(out veinYieldMultiplierOverride, "veinYieldMultiplierOverride", 1.0f, true,
                        "Override yield multiplier for ore veins.",
                        "Ore veins are the large patches of underground ore unlocked later in the game.",
                        "0 = use yieldMultiplier.",
                        "1 = disable ore vein yield multiplication.",
                        "Numbers larger than 1 increase the amount of ore in each block for ore veins.")
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
                if (__instance.flags.HasFlagNonAlloc(TerrainBlockType.TerrainTypeFlags.Ore))
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
                else if (__instance.flags.HasFlagNonAlloc(TerrainBlockType.TerrainTypeFlags.OreVeinMineable))
                {
                    var yieldFactor = yieldMultiplier.Get();
                    if (veinYieldMultiplierOverride.Get() > 0.0f) yieldFactor = veinYieldMultiplierOverride.Get();
                    if (yieldFactor > 0.0f && yieldFactor != 1.0f)
                    {
                        log.Log($"Multiplying {__instance.identifier} yield x{yieldFactor}");
                        __instance.oreVeinMineable_averageYield = (int)Mathf.Ceil(__instance.oreVeinMineable_averageYield * yieldFactor);
                    }
                }
            }

            [HarmonyPatch(typeof(ReservoirTemplate), nameof(ReservoirTemplate.onLoad))]
            [HarmonyPrefix]
            public static void ReservoirTemplate_onLoad(ReservoirTemplate __instance)
            {
                if (__instance.chancePerChunk_ground > 0)
                {
                    var chanceFactor = chanceMultiplier.Get();
                    if (reservoirChanceMultiplierOverride.Get() > 0.0f) chanceFactor = reservoirChanceMultiplierOverride.Get();
                    if (chanceFactor > 0.0f && chanceFactor != 1.0f)
                    {
                        log.Log($"Multiplying {__instance.identifier} chance x{chanceFactor}");
                        __instance.chancePerChunk_ground = (uint)Mathf.Ceil(__instance.chancePerChunk_ground / (float)chanceFactor);
                        __instance.chancePerChunk_surface = (uint)Mathf.Ceil(__instance.chancePerChunk_surface / (float)chanceFactor);
                    }

                    var yieldFactor = yieldMultiplier.Get();
                    if (reservoirYieldMultiplierOverride.Get() > 0.0f) yieldFactor = reservoirYieldMultiplierOverride.Get();
                    if (yieldFactor > 0.0f && yieldFactor != 1.0f)
                    {
                        log.Log($"Multiplying {__instance.identifier} yield x{yieldFactor}");
                        __instance.minContent_str = (System.Convert.ToDouble(__instance.minContent_str) * yieldFactor).ToString(System.Globalization.CultureInfo.InvariantCulture);
                        __instance.maxContent_str = (System.Convert.ToDouble(__instance.maxContent_str) * yieldFactor).ToString(System.Globalization.CultureInfo.InvariantCulture);
                        log.Log($"Result: {__instance.minContent_str} -> {__instance.maxContent_str}");
                    }
                }
            }

            [HarmonyPatch(typeof(OreVeinTemplate), nameof(OreVeinTemplate.onLoad))]
            [HarmonyPrefix]
            public static void OreVeinTemplate_onLoad(OreVeinTemplate __instance)
            {
                if (__instance.spawnChancePerOreChunk > 0)
                {
                    var chanceFactor = chanceMultiplier.Get();
                    if (veinChanceMultiplierOverride.Get() > 0.0f) chanceFactor = veinChanceMultiplierOverride.Get();
                    if (chanceFactor > 0.0f && chanceFactor != 1.0f)
                    {
                        log.Log($"Multiplying {__instance.identifier} chance x{chanceFactor}");
                        __instance.spawnChancePerOreChunk = (uint)Mathf.Ceil(__instance.spawnChancePerOreChunk / (float)chanceFactor);
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
