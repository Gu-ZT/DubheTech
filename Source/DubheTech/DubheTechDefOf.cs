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

    /// <summary>
    /// 磁铁物品：雷击附近的钢铁会磁化而成，也可开采磁铁矿获得。
    /// </summary>
    public static ThingDef DubheTech_Magnet;

    /// <summary>
    /// 输电杆：与切比雪夫距离 24 格内的其他输电杆无线互联，组成跨区域的统一电网。
    /// </summary>
    public static ThingDef DubheTech_TransmissionPole;

    /// <summary>
    /// 远程输电杆：与切比雪夫距离 48 格内的其他输电杆无线互联，组成跨区域的统一电网。
    /// </summary>
    public static ThingDef DubheTech_RemoteTransmissionPole;

    static DubheTechDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(DubheTechDefOf));
}
