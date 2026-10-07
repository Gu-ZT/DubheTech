using System;
using System.Collections.Generic;

namespace DubheTech.Geometry;

/// <summary>
/// Delaunay 三角化（Bowyer-Watson 逐点插入），移植自 AnvilCraft 的同名实现并保留其边界行为：
/// 输入点坐标量化去重；全部共线时退化为按投影排序的链式边；
/// 含超三角形顶点的三角形仍贡献两个输入点之间的凸包边，部分输入点共线时凸包边不会丢失。
/// </summary>
public static class DelaunayTriangulator
{
    private const double Epsilon = 1.0E-9;

    /// <summary>输入点：Id 由调用方指定并随边原样返回，X/Y 为平面坐标。</summary>
    public readonly struct Point
    {
        public readonly int Id;
        public readonly double X;
        public readonly double Y;

        public Point(int id, double x, double y)
        {
            Id = id;
            X = x;
            Y = y;
        }
    }

    /// <summary>三角剖分的边：两端为输入点的 Id，小号在前以便去重。</summary>
    public readonly struct Edge : IEquatable<Edge>
    {
        public readonly int A;
        public readonly int B;

        public Edge(int a, int b)
        {
            A = Math.Min(a, b);
            B = Math.Max(a, b);
        }

        public bool Equals(Edge other) => A == other.A && B == other.B;

        public override bool Equals(object obj) => obj is Edge other && Equals(other);

        public override int GetHashCode() => A * 397 ^ B;
    }

    /// <summary>
    /// 对输入点集做三角剖分，返回剖分结果的边集；少于两个点或点重合时返回空集。
    /// </summary>
    public static HashSet<Edge> Triangulate(IReadOnlyList<Point> input)
    {
        List<Point> points = Deduplicate(input);
        if (points.Count < 2)
        {
            return new HashSet<Edge>();
        }
        if (points.Count == 2)
        {
            return new HashSet<Edge> { new Edge(points[0].Id, points[1].Id) };
        }
        if (IsCollinear(points))
        {
            return CollinearEdges(points);
        }

        int inputSize = points.Count;
        SuperTriangle super = CreateSuperTriangle(points);
        points.Add(new Point(inputSize, super.AX, super.AY));
        points.Add(new Point(inputSize + 1, super.BX, super.BY));
        points.Add(new Point(inputSize + 2, super.CX, super.CY));

        List<Triangle> triangles = new() { Triangle.Create(inputSize, inputSize + 1, inputSize + 2, points) };
        Dictionary<long, (int count, int a, int b)> edgeUse = new();
        HashSet<Triangle> bad = new();
        for (int index = 0; index < inputSize; index++)
        {
            Point point = points[index];
            bad.Clear();
            for (int i = 0; i < triangles.Count; i++)
            {
                if (triangles[i].ContainsInCircumcircle(point))
                {
                    bad.Add(triangles[i]);
                }
            }
            if (bad.Count == 0)
            {
                continue;
            }
            // 空洞重剖：只被一个坏三角形使用的边构成空洞边界，逐条与新点连成新三角形
            edgeUse.Clear();
            foreach (Triangle triangle in bad)
            {
                CountEdge(edgeUse, triangle.A, triangle.B);
                CountEdge(edgeUse, triangle.B, triangle.C);
                CountEdge(edgeUse, triangle.C, triangle.A);
            }
            triangles.RemoveAll(bad.Contains);
            foreach ((int count, int a, int b) in edgeUse.Values)
            {
                if (count != 1)
                {
                    continue;
                }
                Triangle fresh = Triangle.Create(a, b, index, points);
                if (fresh != null)
                {
                    triangles.Add(fresh);
                }
            }
        }

        HashSet<Edge> edges = new();
        foreach (Triangle triangle in triangles)
        {
            Collect(edges, triangle.A, triangle.B);
            Collect(edges, triangle.B, triangle.C);
            Collect(edges, triangle.C, triangle.A);
        }
        return edges;

        void Collect(HashSet<Edge> set, int a, int b)
        {
            if (a < inputSize && b < inputSize)
            {
                set.Add(new Edge(points[a].Id, points[b].Id));
            }
        }
    }

    private static void CountEdge(Dictionary<long, (int count, int a, int b)> edgeUse, int a, int b)
    {
        long key = (long)Math.Min(a, b) << 32 | (uint)Math.Max(a, b);
        if (edgeUse.TryGetValue(key, out (int count, int a, int b) entry))
        {
            edgeUse[key] = (entry.count + 1, entry.a, entry.b);
        }
        else
        {
            edgeUse[key] = (1, a, b);
        }
    }

    private static bool IsCollinear(List<Point> points)
    {
        Point origin = points[0];
        double dirX = points[1].X - origin.X;
        double dirY = points[1].Y - origin.Y;
        double dirLength = Math.Sqrt(dirX * dirX + dirY * dirY);
        for (int i = 2; i < points.Count; i++)
        {
            Point point = points[i];
            double cross = dirX * (point.Y - origin.Y) - dirY * (point.X - origin.X);
            if (Math.Abs(cross) > Epsilon * dirLength)
            {
                return false;
            }
        }
        return true;
    }

    private static HashSet<Edge> CollinearEdges(List<Point> points)
    {
        Point origin = points[0];
        double dirX = points[1].X - origin.X;
        double dirY = points[1].Y - origin.Y;
        List<Point> sorted = new(points);
        sorted.Sort((left, right) => (dirX * left.X + dirY * left.Y).CompareTo(dirX * right.X + dirY * right.Y));
        HashSet<Edge> edges = new();
        for (int i = 0; i + 1 < sorted.Count; i++)
        {
            edges.Add(new Edge(sorted[i].Id, sorted[i + 1].Id));
        }
        return edges;
    }

    private static List<Point> Deduplicate(IReadOnlyList<Point> input)
    {
        List<Point> points = new(input.Count);
        HashSet<long> seen = new();
        for (int i = 0; i < input.Count; i++)
        {
            Point point = input[i];
            long key = QuantizedKey(point.X, point.Y);
            if (seen.Add(key))
            {
                points.Add(point);
            }
        }
        return points;
    }

    private static long QuantizedKey(double x, double y)
    {
        long qx = (long)Math.Round(x / Epsilon);
        long qy = (long)Math.Round(y / Epsilon);
        return qx * 31 ^ qy;
    }

    private static SuperTriangle CreateSuperTriangle(List<Point> points)
    {
        double minX = double.PositiveInfinity;
        double minY = double.PositiveInfinity;
        double maxX = double.NegativeInfinity;
        double maxY = double.NegativeInfinity;
        foreach (Point point in points)
        {
            if (point.X < minX) minX = point.X;
            if (point.Y < minY) minY = point.Y;
            if (point.X > maxX) maxX = point.X;
            if (point.Y > maxY) maxY = point.Y;
        }
        double delta = Math.Max(maxX - minX, maxY - minY);
        if (delta < Epsilon) delta = 1.0;
        double midX = (minX + maxX) * 0.5;
        double midY = (minY + maxY) * 0.5;
        return new SuperTriangle(
            midX - 20 * delta, midY - delta,
            midX, midY + 20 * delta,
            midX + 20 * delta, midY - delta);
    }

    private readonly struct SuperTriangle
    {
        public readonly double AX;
        public readonly double AY;
        public readonly double BX;
        public readonly double BY;
        public readonly double CX;
        public readonly double CY;

        public SuperTriangle(double ax, double ay, double bx, double by, double cx, double cy)
        {
            AX = ax;
            AY = ay;
            BX = bx;
            BY = by;
            CX = cx;
            CY = cy;
        }
    }

    private sealed class Triangle
    {
        public readonly int A;
        public readonly int B;
        public readonly int C;
        private readonly double centerX;
        private readonly double centerY;
        private readonly double radiusSquared;

        private Triangle(int a, int b, int c, double centerX, double centerY, double radiusSquared)
        {
            A = a;
            B = b;
            C = c;
            this.centerX = centerX;
            this.centerY = centerY;
            this.radiusSquared = radiusSquared;
        }

        /// <summary>三点退化（共线）时返回 null，对应 Java 版 TriangleBuffer.add 的 -1 返回。</summary>
        public static Triangle Create(int a, int b, int c, IReadOnlyList<Point> points)
        {
            Point p1 = points[a];
            Point p2 = points[b];
            Point p3 = points[c];
            double determinant = 2.0 * (p1.X * (p2.Y - p3.Y) + p2.X * (p3.Y - p1.Y) + p3.X * (p1.Y - p2.Y));
            if (Math.Abs(determinant) <= Epsilon)
            {
                return null;
            }
            double centerX = (
                (Square(p1.X) + Square(p1.Y)) * (p2.Y - p3.Y)
                + (Square(p2.X) + Square(p2.Y)) * (p3.Y - p1.Y)
                + (Square(p3.X) + Square(p3.Y)) * (p1.Y - p2.Y)
            ) / determinant;
            double centerY = (
                (Square(p1.X) + Square(p1.Y)) * (p3.X - p2.X)
                + (Square(p2.X) + Square(p2.Y)) * (p1.X - p3.X)
                + (Square(p3.X) + Square(p3.Y)) * (p2.X - p1.X)
            ) / determinant;
            double dx = p1.X - centerX;
            double dy = p1.Y - centerY;
            return new Triangle(a, b, c, centerX, centerY, dx * dx + dy * dy);
        }

        public bool ContainsInCircumcircle(Point point)
        {
            double dx = point.X - centerX;
            double dy = point.Y - centerY;
            return dx * dx + dy * dy <= radiusSquared + Epsilon;
        }

        private static double Square(double value) => value * value;
    }
}
