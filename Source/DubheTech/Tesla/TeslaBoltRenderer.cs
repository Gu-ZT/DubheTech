using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace DubheTech.Tesla;

/// <summary>
/// 特斯拉塔的雷电特效绘制。闪电是一段只存在数 tick 的锯齿折线，宽度随充能等级增加，
/// 由特斯拉塔在开火时注册；每几 tick 重新抖动一次形状以产生闪烁感。
/// </summary>
[StaticConstructorOnStartup]
public class TeslaBoltRenderer : MapComponent
{
    private const int BoltSegments = 8;
    private const int FlickerIntervalTicks = 3;

    // 复用原版雷击的材质（游戏资源包内的 Weather/LightningBolt），让闪电呈现与落雷一致的纹理
    private static readonly Material BoltMaterial = MatLoader.LoadMat("Weather/LightningBolt");

    private readonly List<Bolt> bolts = new();

    private sealed class Bolt
    {
        public Vector3 from;
        public Vector3 to;
        public float width;
        public int startTick;
        public int expireTick;
        public int seed;
    }

    public TeslaBoltRenderer(Map map) : base(map)
    {
    }

    /// <summary>
    /// 注册一条闪电：from 与 to 为世界坐标（y 无效，绘制高度由本类统一），充能等级决定线宽。
    /// </summary>
    public void AddBolt(Vector3 from, Vector3 to, int chargeLevel)
    {
        TeslaTowerSettings settings = DubheTechDefOf.DubheTech_TeslaTower.GetModExtension<TeslaTowerSettings>();
        bolts.Add(new Bolt
        {
            from = from,
            to = to,
            width = settings.boltWidthBase + settings.boltWidthPerCharge * chargeLevel,
            startTick = Find.TickManager.TicksGame,
            expireTick = Find.TickManager.TicksGame + settings.boltDurationTicks,
            seed = Rand.Int
        });
    }

    public override void MapComponentUpdate()
    {
        if (bolts.Count == 0)
        {
            return;
        }
        int now = Find.TickManager.TicksGame;
        float altitude = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
        for (int i = bolts.Count - 1; i >= 0; i--)
        {
            Bolt bolt = bolts[i];
            if (now >= bolt.expireTick)
            {
                bolts.RemoveAt(i);
                continue;
            }
            DrawBolt(bolt, altitude, now);
        }
    }

    private static void DrawBolt(Bolt bolt, float altitude, int now)
    {
        Vector3 from = new(bolt.from.x, altitude, bolt.from.z);
        Vector3 to = new(bolt.to.x, altitude, bolt.to.z);
        float dx = to.x - from.x;
        float dz = to.z - from.z;
        float length = Mathf.Sqrt(dx * dx + dz * dz);
        if (length < 0.01f)
        {
            return;
        }
        // 垂直于闪电方向的横向偏移轴，锯齿只在这个方向上展开
        Vector2 perpendicular = new(-dz / length, dx / length);
        // 亮度随存活时间衰减，模拟落雷的闪光淡出
        float brightness = (float)(bolt.expireTick - now) / (bolt.expireTick - bolt.startTick);
        Material material = FadedMaterialPool.FadedVersionOf(BoltMaterial, brightness);
        Rand.PushState(bolt.seed + now / FlickerIntervalTicks);
        try
        {
            Vector3 previous = from;
            for (int s = 1; s <= BoltSegments; s++)
            {
                float t = (float)s / BoltSegments;
                Vector3 point = Vector3.Lerp(from, to, t);
                if (s < BoltSegments)
                {
                    // 偏移幅度随离两端距离增大，保证折线两端精确落在塔头与目标上
                    float spread = length * 0.08f * 4f * t * (1f - t);
                    float offset = Rand.Range(-spread, spread);
                    point.x += perpendicular.x * offset;
                    point.z += perpendicular.y * offset;
                }
                GenDraw.DrawLineBetween(previous, point, material, bolt.width);
                previous = point;
            }
        }
        finally
        {
            Rand.PopState();
        }
    }
}
