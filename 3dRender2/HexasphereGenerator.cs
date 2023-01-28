using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace _3dRender2
{
    public class HexasphereGenerator
    {
        public List<Hex> hexes = new List<Hex>();

        public void CreateGoldberg(List<Vector3D> normals, List<int> triangles)
        {
            var triIndexes = Enumerable.Range(0, triangles.Count / 3).ToList();

            Stopwatch sw = new Stopwatch();

            sw.Start();

            ConcurrentBag<Hex> hexBag = new ConcurrentBag<Hex>();

            Parallel.For(0, normals.Count, v =>
            {
                var hex = CalculateHex(triIndexes, v, normals, triangles);

                if (hex != null)
                {
                    hexBag.Add(hex);
                }
            });

            hexes = hexBag.ToList();
            sw.Stop();
        }

        public Hex CalculateHex(List<int> triIndexes, int v, List<Vector3D> normals, List<int> triangles)
        {
            List<int> triInd = new List<int>();
            List<Vector3D> hexNorm = new List<Vector3D>();

            triInd = triIndexes.FindAll(i => v == triangles[i * 3] || v == triangles[i * 3 + 1] || v == triangles[i * 3 + 2]).Select(a => a * 3).ToList();

            foreach (var i in triInd)
            {
                var vert = GetCentroid(normals[triangles[i]], normals[triangles[i + 1]], normals[triangles[i + 2]]);

                hexNorm.Add(vert);
            }

            if (hexNorm.Count < 5)
            {
                return null;
            }

            var tris = CreateHexTriangles(hexNorm);

            Hex hex = new Hex(hexNorm, tris, normals[v], 1);
            return hex;
        }

        public Vector3D GetCentroid(Vector3D v0, Vector3D v1, Vector3D v2)
        {
            return (v0 + v1 + v2) / 2;
        }

        public List<int> CreateHexTriangles(List<Vector3D> hexNorm)
        {
            List<int> normalIndexes = new List<int>();

            List<int> tris = new List<int>();

            int normCount = hexNorm.Count;

            if (normCount == 5)
            {
                normalIndexes = new List<int>() { 0, 1, 2, 3, 4 };
            }
            else
            {
                normalIndexes = new List<int>() { 0, 1, 2, 3, 4, 5 };
            }

            int first = normalIndexes[2];

            normalIndexes.RemoveAt(2);

            int min1 = 1000000;
            int min2 = 1000000;

            double minDist1 = 1000000;
            double minDist2 = 1000000;

            foreach (var tri in normalIndexes)
            {
                var dist = Vector3D.Subtract(hexNorm[first], hexNorm[tri]).Length;
                if (dist < minDist1 || dist < minDist2)
                {
                    if (minDist1 > minDist2)
                    {
                        minDist1 = dist;
                        min1 = tri;
                    }
                    else
                    {
                        minDist2 = dist;
                        min2 = tri;
                    }
                }
            }

            tris.AddRange(GetTriangleIndices(hexNorm, first, min1, min2));

            normalIndexes.Remove(min1);
            normalIndexes.Remove(min2);

            int min3 = GetMinDistance(hexNorm, normalIndexes, min1);
            tris.AddRange(GetTriangleIndices(hexNorm, min1, min2, min3));

            normalIndexes.Remove(min3);

            if (normCount == 6)
            {
                int min4 = GetMinDistance(hexNorm, normalIndexes, min2);
                tris.AddRange(GetTriangleIndices(hexNorm, min2, min3, min4));

                normalIndexes.Remove(min4);

                int last = normalIndexes[0];
                tris.AddRange(GetTriangleIndices(hexNorm, last, min3, min4));
            }
            else
            {
                int last = normalIndexes[0];
                tris.AddRange(GetTriangleIndices(hexNorm, last, min2, min3));
            }

            return tris;
        }

        private List<int> GetTriangleIndices(List<Vector3D> hexNorm, int first, int second, int third)
        {
            Vector3D side1 = hexNorm[second] - hexNorm[first];
            Vector3D side2 = hexNorm[third] - hexNorm[first];
            Vector3D perp = Vector3D.CrossProduct(side1, side2);
            perp = Normalize(perp);

            if (Vector3D.Subtract((hexNorm[first] + hexNorm[second] + hexNorm[third]) / 3, perp).Length > 1)
            {
                return new List<int> { first, third, second };
            }
            else
            {
                return new List<int> { first, second, third };
            }
        }

        public int GetMinDistance(List<Vector3D> hexNorm, List<int> triangles, int triangleIndex)
        {
            int min = 1000000;

            double minDist = 1000000;

            foreach (var tri in triangles)
            {
                var dist = Vector3D.Subtract(hexNorm[triangleIndex], hexNorm[tri]).Length;
                if (dist < minDist)
                {
                    minDist = dist;
                    min = tri;
                }
            }

            return min;
        }

        public Vector3D Normalize(Vector3D v1)
        {
            double x = double.Parse(Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y + v1.Z * v1.Z).ToString());
            return new Vector3D(v1.X / x, v1.Y / x, v1.Z / x);
        }
    }

    public class Hex
    {
        public Vector3D mainPoint;

        public List<Vector3D> Normals { get; set; }

        public List<Point3D> Vericies { get; set; }
        public List<int> Tris { get; set; }

        public float multipliyer { get; set; }

        List<Vector3D> verts { get; set; }

        List<Vector3D> norms { get; set; }

        public Hex(List<Vector3D> normals, List<int> tris, Vector3D point, float mult)
        {
            Normals = normals;
            Tris = tris;
            multipliyer = mult;
            mainPoint = point;

            Vericies = new List<Point3D>();

            foreach (var norm in Normals)
            {
                Vericies.Add(new Point3D(norm.X, norm.Y, norm.Z));
            }
        }
    }
}
