using RimWorld;

namespace DubheTech.Comps;

/// <summary>
/// 无线输电 Comp 的属性：在原版输电属性的基础上声明无线互联距离。
/// </summary>
public class CompProperties_WirelessTransmitter : CompProperties_Power
{
    /// <summary>无线互联距离（切比雪夫距离，格）。</summary>
    public int radius;
}
