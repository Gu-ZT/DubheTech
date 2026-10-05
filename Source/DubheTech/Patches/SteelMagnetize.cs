using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace DubheTech.Patches;

/// <summary>
/// 钢铁磁化补丁：雷击结算后，将落点附近切比雪夫距离 2 格以内的钢铁堆转化为磁铁，
/// 并按 12% 的比例损失数量；只有 1 个钢铁时以 12% 概率整个消失。
/// </summary>
[HarmonyPatch(typeof(WeatherEvent_LightningStrike), nameof(WeatherEvent_LightningStrike.FireEvent))]
public static class SteelMagnetize
{
    /// <summary>磁化范围（切比雪夫距离，格；对应需求中的「<3」）。</summary>
    public const int ConvertRadius = 2;

    /// <summary>磁化时的数量损失比例。</summary>
    public const float LossRatio = 0.12f;

    /// <summary>
    /// 在原版雷击结算（伤害与火焰）之后执行磁化转换。
    /// FireEvent 结束时 strikeLoc 必然有效：原版会在落点无效时先随机选取落点。
    /// </summary>
    /// <param name="___map">事件所在地图（Harmony 注入基类 WeatherEvent 的 map 字段）。</param>
    /// <param name="___strikeLoc">原版雷击落点字段（Harmony 注入）。</param>
    private static void Postfix(Map ___map, IntVec3 ___strikeLoc)
    {
        foreach (IntVec3 cell in GenRadial.RadialCellsAround(___strikeLoc, ConvertRadius, true))
        {
            if (cell.InBounds(___map))
            {
                ConvertSteelAt(___map, cell);
            }
        }
    }

    private static void ConvertSteelAt(Map map, IntVec3 cell)
    {
        List<Thing> things = new List<Thing>(cell.GetThingList(map));
        foreach (Thing thing in things)
        {
            if (thing.def != ThingDefOf.Steel)
            {
                continue;
            }
            int count = thing.stackCount;
            int magnets = count == 1
                ? (Rand.Chance(LossRatio) ? 0 : 1)
                : Mathf.FloorToInt(count * (1f - LossRatio));
            thing.Destroy();
            if (magnets > 0)
            {
                Thing magnet = ThingMaker.MakeThing(DubheTechDefOf.DubheTech_Magnet);
                magnet.stackCount = magnets;
                GenSpawn.Spawn(magnet, cell, map);
            }
        }
    }
}
