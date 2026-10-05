using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace DubheTech.Patches;

/// <summary>
/// 雷击重定向补丁：将落点 110 格（切比雪夫距离）内存在接闪杆的雷击，改落到最近的接闪杆所在格。
/// 雷击事件在 FireEvent 中依据 strikeLoc 同时生成闪电网格与结算伤害，因此在 FireEvent 前置阶段
/// 改写 strikeLoc 即可同步移动视觉效果与伤害位置。
/// </summary>
[HarmonyPatch(typeof(WeatherEvent_LightningStrike), nameof(WeatherEvent_LightningStrike.FireEvent))]
public static class LightningStrikeRedirect
{
    /// <summary>接闪杆引雷半径（切比雪夫距离，格）。</summary>
    public const int RedirectRadius = 110;

    /// <summary>
    /// 在雷击事件触发前重定向落点。若落点尚未确定，先按原版逻辑选取一个可站立格，
    /// 再在半径内存在接闪杆时将落点替换为最近接闪杆的位置。
    /// </summary>
    /// <param name="___map">事件所在地图（Harmony 注入基类 WeatherEvent 的 map 字段）。</param>
    /// <param name="___strikeLoc">原版雷击落点字段（Harmony 注入，写回以生效）。</param>
    private static void Prefix(Map ___map, ref IntVec3 ___strikeLoc)
    {
        IntVec3 target = ___strikeLoc.IsValid
            ? ___strikeLoc
            : CellFinderLoose.RandomCellWith(c => c.Standable(___map), ___map);
        Building rod = FindNearestRod(___map, target);
        ___strikeLoc = rod is null ? target : rod.Position;
    }

    /// <summary>
    /// 查找距给定落点切比雪夫距离不超过 <see cref="RedirectRadius"/> 的最近接闪杆；不存在时返回 null。
    /// </summary>
    private static Building FindNearestRod(Map map, IntVec3 target)
    {
        Building best = null;
        int bestDist = int.MaxValue;
        foreach (Thing thing in map.listerThings.ThingsOfDef(DubheTechDefOf.DubheTech_LightingRod))
        {
            if (thing is not Building rod)
            {
                continue;
            }
            int dist = Math.Max(Math.Abs(rod.Position.x - target.x), Math.Abs(rod.Position.z - target.z));
            if (dist <= RedirectRadius && dist < bestDist)
            {
                bestDist = dist;
                best = rod;
            }
        }
        return best;
    }
}
