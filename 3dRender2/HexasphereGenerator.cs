using System;
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

            Parallel.For(0, normals.Count, v =>
            {
                var hex = CalculateHex(triIndexes, v, normals, triangles);

                lock (hexes)
                {
                    hexes.Add(hex);
                }

            });

            sw.Stop();
        }

        public Hex CalculateHex(List<int> triIndexes, int v, List<Vector3D> normals, List<int> triangles)
        {
            List<int> hexTri = new List<int>();
            List<int> triInd = new List<int>();
            List<Vector3D> hexNorm = new List<Vector3D>();

            triInd = triIndexes.FindAll(i => v == triangles[i * 3] || v == triangles[i * 3 + 1] || v == triangles[i * 3 + 2]).Select(a => a * 3).ToList();

            foreach (var i in triInd)
            {
                var vert = GetCentroid(normals[triangles[i]], normals[triangles[i + 1]], normals[triangles[i + 2]]);

                hexNorm.Add(vert);
            }

            hexTri = Enumerable.Range(0, hexNorm.Count).ToList();

            if (hexTri.Count >= 5)
            {
                var tris = CreateHexTriangles(hexTri, hexNorm);

                Hex hex = new Hex(hexNorm, tris, normals[v], 1);
                return hex;
            }

            return null;
        }

        public Vector3D GetCentroid(Vector3D v0, Vector3D v1, Vector3D v2)
        {
            return (v0 + v1 + v2) / 2;
        }

        public List<int> CreateHexTriangles(List<int> triangles, List<Vector3D> hexNorm)
        {
            Vector3D side1;
            Vector3D side2;
            Vector3D perp;

            List<int> tris = new List<int>();

            int triCount = triangles.Count;

            int first = triangles[2];

            triangles.RemoveAt(2);

            int min1 = 1000000;
            int min2 = 1000000;

            double minDist1 = 1000000;
            double minDist2 = 1000000;

            foreach (var tri in triangles)
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

            side1 = hexNorm[min1] - hexNorm[first];
            side2 = hexNorm[min2] - hexNorm[first];

            perp = Vector3D.CrossProduct(side1, side2);
            perp = Normalize(perp);

            if (Vector3D.Subtract((hexNorm[first] + hexNorm[min1] + hexNorm[min2]) / 3, perp).Length > 1)
            {
                tris.Add(first);
                tris.Add(min2);
                tris.Add(min1);
            }
            else
            {
                tris.Add(first);
                tris.Add(min1);
                tris.Add(min2);
            }

            triangles.Remove(min1);
            triangles.Remove(min2);

            int min3 = 1000000;

            double minDist3 = 1000000;

            foreach (var tri in triangles)
            {
                var dist = Vector3D.Subtract(hexNorm[min1], hexNorm[tri]).Length;
                if (dist < minDist3)
                {
                    minDist3 = dist;
                    min3 = tri;
                }
            }

            side1 = hexNorm[min1] - hexNorm[min2];
            side2 = hexNorm[min1] - hexNorm[min3];
            perp = Vector3D.CrossProduct(side1, side2);
            perp.Normalize();

            if (Vector3D.Subtract((hexNorm[min3] + hexNorm[min1] + hexNorm[min2]) / 3, perp).Length > 1)
            {
                tris.Add(min1);
                tris.Add(min3);
                tris.Add(min2);
            }
            else
            {
                tris.Add(min1);
                tris.Add(min2);
                tris.Add(min3);
            }

            triangles.Remove(min3);

            if (triCount == 6)
            {
                int min4 = 1000000;

                double minDist4 = 1000000;

                foreach (var tri in triangles)
                {
                    var dist = Vector3D.Subtract(hexNorm[min2], hexNorm[tri]).Length;
                    if (dist < minDist4)
                    {
                        minDist4 = dist;
                        min4 = tri;
                    }
                }

                side1 = hexNorm[min2] - hexNorm[min4];
                side2 = hexNorm[min2] - hexNorm[min3];
                perp = Vector3D.CrossProduct(side1, side2);
                perp.Normalize();

                if (Vector3D.Subtract((hexNorm[min4] + hexNorm[min3] + hexNorm[min2]) / 3, perp).Length > 1)
                {
                    tris.Add(min2);
                    tris.Add(min3);
                    tris.Add(min4);

                }
                else
                {
                    tris.Add(min2);
                    tris.Add(min4);
                    tris.Add(min3);
                }

                triangles.Remove(min4);


                int last = triangles[0];

                side1 = hexNorm[last] - hexNorm[min4];
                side2 = hexNorm[last] - hexNorm[min3];
                perp = Vector3D.CrossProduct(side1, side2);
                perp.Normalize();

                if (Vector3D.Subtract((hexNorm[min4] + hexNorm[min3] + hexNorm[min3]) / 3, perp).Length > 1)
                {
                    tris.Add(last);
                    tris.Add(min3);
                    tris.Add(min4);

                }
                else
                {
                    tris.Add(last);
                    tris.Add(min4);
                    tris.Add(min3);
                }
            }
            else
            {
                int last = triangles[0];

                side1 = hexNorm[last] - hexNorm[min2];
                side2 = hexNorm[last] - hexNorm[min3];
                perp = Vector3D.CrossProduct(side1, side2);
                perp.Normalize();

                if (Vector3D.Subtract((hexNorm[min2] + hexNorm[min3] + hexNorm[last]) / 3, perp).Length > 1)
                {
                    tris.Add(last);
                    tris.Add(min3);
                    tris.Add(min2);
                }
                else
                {
                    tris.Add(last);
                    tris.Add(min2);
                    tris.Add(min3);
                }
            }

            tris.AddRange(tris);

            return tris;
        }

        public static Vector3D Normalize(Vector3D v1)
        {
            double x = double.Parse(Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y + v1.Z * v1.Z).ToString());
            return new Vector3D(v1.X / x, v1.Y / x, v1.Z / x);
        }
    }

    public class Hex
    {
        public Vector3D mainPoint;

        public List<Vector3D> Normals { get; set; }
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
        }
    }
}
