using System.Collections.Generic;
using System.Linq;
using DubheTech.Comps;
using HarmonyLib;
using RimWorld;
using Verse;

namespace DubheTech.Patches;

/// <summary>
/// 无线输电链路补丁：在原版电网泛洪（仅沿贴邻的输电建筑扩散）的结果之上，
/// 把与网内输电杆无线互联的杆、以及从这些杆出发按原版贴邻规则可达的输电建筑一并并入电网。
/// 必须补丁私有方法的原因：电网拓扑的唯一收口是 PowerNetMaker.ContiguousPowerBuildings，
/// 调用链上的 PowerNetManager.TryCreateNetAt / TryDestroyNetAt 等同样均为私有，
/// 经反编译确认不存在任何可供扩展电网拓扑的公共入口点。
/// </summary>
[HarmonyPatch(typeof(PowerNetMaker), "ContiguousPowerBuildings")]
public static class WirelessPowerLink
{
    private static void Postfix(Building root, ref IEnumerable<CompPower> __result)
    {
        Map map = root.Map;
        List<Building> poles = CompWirelessTransmitter.FindPolesOnMap(map);
        if (poles.Count == 0)
        {
            return;
        }
        HashSet<Building> inNet = new HashSet<Building>(__result.Select(c => (Building)c.parent));
        HashSet<Building> poleSet = new HashSet<Building>(poles);
        // 只有输电杆能产生无线链路，主队列只需携带杆；普通输电建筑无需参与链路发现
        Queue<Building> queue = new Queue<Building>(poles.Where(inNet.Contains));
        bool expanded = false;
        while (queue.Count > 0)
        {
            CompWirelessTransmitter wireless = queue.Dequeue().GetComp<CompWirelessTransmitter>();
            foreach (Building pole in poles)
            {
                if (!inNet.Contains(pole) && wireless.LinksTo(pole.GetComp<CompWirelessTransmitter>()))
                {
                    Absorb(pole, map, inNet, queue, poleSet);
                    expanded = true;
                }
            }
        }
        if (expanded)
        {
            __result = inNet.Select(b => b.PowerComp).ToList();
        }
    }

    /// <summary>
    /// 将新互联的输电杆及其沿原版贴邻规则（四方向相邻格上的首个输电建筑）可达的
    /// 全部输电建筑并入网络；新并入的杆同时进入主队列，以继续发现下一跳无线链路。
    /// </summary>
    private static void Absorb(Building start, Map map, HashSet<Building> inNet, Queue<Building> queue, HashSet<Building> poleSet)
    {
        inNet.Add(start);
        queue.Enqueue(start);
        Queue<Building> local = new Queue<Building>();
        local.Enqueue(start);
        while (local.Count > 0)
        {
            Building current = local.Dequeue();
            foreach (IntVec3 cell in GenAdj.CellsAdjacentCardinal(current))
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }
                List<Thing> things = cell.GetThingList(map);
                for (int i = 0; i < things.Count; i++)
                {
                    if (things[i] is Building { TransmitsPowerNow: true } building && inNet.Add(building))
                    {
                        local.Enqueue(building);
                        if (poleSet.Contains(building))
                        {
                            queue.Enqueue(building);
                        }
                        break;
                    }
                }
            }
        }
    }
}
