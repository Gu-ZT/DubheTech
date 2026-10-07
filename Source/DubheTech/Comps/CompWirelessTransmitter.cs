using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace DubheTech.Comps;

/// <summary>
/// 无线输电 Comp：输电杆除按原版规则贴邻输电外，还与互联距离内的其他输电杆无线连通。
/// 杆与杆的互联会改变双方所在电网的拓扑，因此自身生成或消失时必须通知所有互联杆重建电网。
/// </summary>
public class CompWirelessTransmitter : CompPowerTransmitter
{
    /// <summary>本杆的无线互联距离（切比雪夫距离，格）。</summary>
    public int Radius => ((CompProperties_WirelessTransmitter)props).radius;

    /// <summary>
    /// 判断本杆能否与另一根输电杆无线互联：两杆位置的切比雪夫距离不超过两者互联距离的较大值。
    /// </summary>
    public bool LinksTo(CompWirelessTransmitter other)
    {
        int reach = Math.Max(Radius, other.Radius);
        int dist = Math.Max(
            Math.Abs(parent.Position.x - other.parent.Position.x),
            Math.Abs(parent.Position.z - other.parent.Position.z));
        return dist <= reach;
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        // 读档时全部电网都会在建筑生成完毕后统一重建，无需逐杆通知
        if (!respawningAfterLoad)
        {
            PoleNetwork.MarkDirty(parent.Map);
            RefreshLinkedPoles(parent.Map);
        }
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        base.PostDeSpawn(map, mode);
        // 与原版 PostDeSpawn 的跳过条件保持一致：被新蓝图原位替换时电网本就无需重建
        if (mode != DestroyMode.WillReplace || parent.BeingTransportedOnGravship)
        {
            PoleNetwork.MarkDirty(map);
            RefreshLinkedPoles(map);
        }
    }

    public override string CompInspectStringExtra()
    {
        string text = "DubheTech_WirelessTransmitterStats".Translate(Radius, CountLinkedPoles());
        string baseText = base.CompInspectStringExtra();
        return baseText.NullOrEmpty() ? text : baseText + "\n" + text;
    }

    private int CountLinkedPoles() => LinkedPoles(parent.Map).Count();

    /// <summary>
    /// 通知所有与本杆互联的输电杆重建电网：本杆生成或消失都会改变对端所在电网的拓扑，
    /// 需触发对端的注销与重新注册，使其所在电网经由补丁后的泛洪重建。
    /// </summary>
    private void RefreshLinkedPoles(Map map)
    {
        foreach (CompWirelessTransmitter other in LinkedPoles(map))
        {
            map.powerNetManager.Notfiy_TransmitterTransmitsPowerNowChanged(other);
        }
    }

    /// <summary>
    /// 枚举地图上与本杆无线互联的其他所有输电杆。
    /// </summary>
    private IEnumerable<CompWirelessTransmitter> LinkedPoles(Map map)
    {
        foreach (Building pole in FindPolesOnMap(map))
        {
            if (pole != parent && pole.GetComp<CompWirelessTransmitter>() is CompWirelessTransmitter other && LinksTo(other))
            {
                yield return other;
            }
        }
    }

    /// <summary>
    /// 枚举地图上全部已生成的无线输电杆；地图上没有输电杆时返回空列表。
    /// </summary>
    public static List<Building> FindPolesOnMap(Map map)
    {
        List<Building> poles = new List<Building>();
        AddPolesOfDef(map, DubheTechDefOf.DubheTech_TransmissionPole, poles);
        AddPolesOfDef(map, DubheTechDefOf.DubheTech_RemoteTransmissionPole, poles);
        return poles;
    }

    private static void AddPolesOfDef(Map map, ThingDef def, List<Building> poles)
    {
        List<Thing> things = map.listerThings.ThingsOfDef(def);
        for (int i = 0; i < things.Count; i++)
        {
            poles.Add((Building)things[i]);
        }
    }
}
