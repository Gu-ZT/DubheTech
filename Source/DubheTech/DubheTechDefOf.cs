using RimWorld;
using Verse;

namespace DubheTech;

/// <summary>
/// 天枢科技全部 Def 的强类型引用，游戏加载 Def 后自动填充。
/// </summary>
[DefOf]
public static class DubheTechDefOf
{
    /// <summary>
    /// 接闪杆建筑：将周围 110 格（切比雪夫距离）内的雷击落点引向自身。
    /// </summary>
    public static ThingDef DubheTech_LightingRod;

    static DubheTechDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(DubheTechDefOf));
}
