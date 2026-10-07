using Verse;

namespace DubheTech.Tesla;

/// <summary>
/// 特斯拉塔的数值配置：攻击节奏、充能协作与雷电特效参数，随 ThingDef 的 modExtensions 注入。
/// </summary>
public class TeslaTowerSettings : DefModExtension
{
    /// <summary>索敌与攻击的最大距离（格，欧氏距离）。</summary>
    public float attackRange = 20f;

    /// <summary>特斯拉塔之间互相充能的最大距离（格，切比雪夫距离）。</summary>
    public int chargeRadius = 16;

    /// <summary>两次攻击之间的最短间隔（tick）。</summary>
    public int cooldownTicks = 150;

    /// <summary>冷却就绪后重新扫描目标的间隔（tick）。</summary>
    public int scanIntervalTicks = 25;

    /// <summary>无充能时单次闪电的基础伤害。</summary>
    public float baseDamage = 15f;

    /// <summary>每级充能额外增加的伤害。</summary>
    public float damagePerCharge = 5f;

    /// <summary>塔头发球点高出放置格中心的高度（格）。</summary>
    public float headHeight = 1.25f;

    /// <summary>闪电特效的基础线宽。</summary>
    public float boltWidthBase = 0.06f;

    /// <summary>闪电特效每级充能增加的线宽。</summary>
    public float boltWidthPerCharge = 0.02f;

    /// <summary>闪电特效存在的时长（tick）。</summary>
    public int boltDurationTicks = 12;
}
