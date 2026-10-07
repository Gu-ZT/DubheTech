using System.Collections.Generic;
using DubheTech.Comps;
using DubheTech.Geometry;
using UnityEngine;
using Verse;

namespace DubheTech;

/// <summary>
/// 输电杆网络：对全图输电杆的位置做 Delaunay 三角化并按互联距离过滤，得到的连线集合
/// 既是无线输电补丁的链路依据，也直接绘制为两根杆头部之间的电线。
/// 输电杆生成或拆除时标记重建，绘制与电网泛洪读取的是同一份缓存。
/// </summary>
public class PoleNetwork : MapComponent
{
    private const float WireWidth = 0.1f;
    /// <summary>电线端点在杆贴图高度上的占比：杆头蓝色部分位于贴图顶端附近。</summary>
    private const float HeadHeightFraction = 0.9f;

    private static readonly Material WireMaterial =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.13f, 0.13f, 0.16f), false);
    private static readonly List<Building> NoLinks = new();

    private readonly List<(Building a, Building b)> links = new();
    private readonly Dictionary<Building, List<Building>> adjacency = new();
    private bool dirty = true;

    public PoleNetwork(Map map) : base(map)
    {
    }

    /// <summary>输电杆生成或拆除后调用，连线与输电链路在下次读取时重建。</summary>
    public static void MarkDirty(Map map) => map.GetComponent<PoleNetwork>().dirty = true;

    /// <summary>与指定输电杆有连线（即可无线输电）的其他输电杆。</summary>
    public List<Building> LinkedPoles(Building pole)
    {
        EnsureLinks();
        return adjacency.TryGetValue(pole, out List<Building> linked) ? linked : NoLinks;
    }

    public override void MapComponentUpdate()
    {
        EnsureLinks();
        float altitude = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
        foreach ((Building a, Building b) in links)
        {
            GenDraw.DrawLineBetween(HeadPosition(a, altitude), HeadPosition(b, altitude), WireMaterial, WireWidth);
        }
    }

    private void EnsureLinks()
    {
        if (!dirty)
        {
            return;
        }
        dirty = false;
        links.Clear();
        adjacency.Clear();
        List<Building> poles = CompWirelessTransmitter.FindPolesOnMap(map);
        if (poles.Count < 2)
        {
            return;
        }
        List<DelaunayTriangulator.Point> points = new(poles.Count);
        for (int i = 0; i < poles.Count; i++)
        {
            points.Add(new DelaunayTriangulator.Point(i, poles[i].Position.x, poles[i].Position.z));
            adjacency[poles[i]] = new List<Building>();
        }
        foreach (DelaunayTriangulator.Edge edge in DelaunayTriangulator.Triangulate(points))
        {
            Building a = poles[edge.A];
            Building b = poles[edge.B];
            if (!a.GetComp<CompWirelessTransmitter>().LinksTo(b.GetComp<CompWirelessTransmitter>()))
            {
                continue;
            }
            links.Add((a, b));
            adjacency[a].Add(b);
            adjacency[b].Add(a);
        }
    }

    /// <summary>
    /// 杆头蓝色部分的世界坐标：贴图底部与放置格对齐，图形中心为 DrawPos + drawOffset，
    /// 头部约在贴图高度的九成处。
    /// </summary>
    private static Vector3 HeadPosition(Building pole, float altitude)
    {
        GraphicData data = pole.def.graphicData;
        Vector3 center = pole.DrawPos;
        float headZ = center.z + data.drawOffset.z + data.drawSize.y * (HeadHeightFraction - 0.5f);
        return new Vector3(center.x, altitude, headZ);
    }
}
