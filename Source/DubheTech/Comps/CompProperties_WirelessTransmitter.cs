using RimWorld;

namespace DubheTech.Comps;

/// <summary>
/// 无线输电 Comp 的属性：在原版输电属性的基础上声明无线互联距离与电线连接点高度。
/// </summary>
public class CompProperties_WirelessTransmitter : CompProperties_Power
{
    /// <summary>无线互联距离（切比雪夫距离，格）。</summary>
    public int radius;

    /// <summary>电线连接点高出杆放置格中心的高度（格），随杆贴图头部位置逐杆配置。</summary>
    public float wireConnectHeight = 1.3f;
}
