using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace _3dRender2
{
    class GeosphereGenerator
    {
        private int detailsLevel = 0;
        public readonly List<Vector3D> baseNormals = new List<Vector3D>() { new Vector3D(-0.525731f, 0.000000f, 0.850651f), new Vector3D(0.525731f, 0.000000f, 0.850651f), new Vector3D(-0.525731f, 0.000000f, -0.850651f), new Vector3D(0.525731f, 0.000000f, -0.850651f), new Vector3D(0.000000f, 0.850651f, 0.525731f), new Vector3D(0.000000f, 0.850651f, -0.525731f), new Vector3D(0.000000f, -0.850651f, 0.525731f), new Vector3D(0.000000f, -0.850651f, -0.525731f), new Vector3D(0.850651f, 0.525731f, 0.000000f), new Vector3D(-0.850651f, 0.525731f, 0.000000f), new Vector3D(0.850651f, -0.525731f, 0.000000f), new Vector3D(-0.850651f, -0.525731f, 0.000000f) };
        public readonly static List<int> baseTriangles = new List<int>() { 0, 1, 4, 0, 4, 9, 9, 4, 5, 4, 8, 5, 4, 1, 8, 8, 1, 10, 8, 10, 3, 5, 8, 3, 5, 3, 2, 2, 3, 7, 7, 3, 10, 7, 10, 6, 7, 6, 11, 11, 6, 0, 0, 6, 1, 6, 10, 1, 9, 11, 0, 9, 2, 11, 9, 5, 2, 7, 11, 2 };

        public List<Vector3D> normals = new List<Vector3D>();
        public List<int> triangles = new List<int>();

        public GeosphereGenerator(int detailsLvl, bool normalize = true)
        {
            detailsLevel = detailsLvl;

            normals = baseNormals;
            triangles = baseTriangles;

            SubDivide(normalize);
        }


        public void SubDivide(bool normalize)
        {
            List<int> newTriangles = new List<int>();
            Dictionary<string, int> midPoints = new Dictionary<string, int>();

            for (int d = 0; d < detailsLevel; d++)
            {
                for (int i = 0; i < triangles.Count - 2; i = i + 3)
                {
                    int i0 = triangles[i];
                    int i1 = triangles[i + 1];
                    int i2 = triangles[i + 2];

                    int m01 = GetMidpoints(i0, i1, normalize, midPoints);
                    int m12 = GetMidpoints(i1, i2, normalize, midPoints);
                    int m02 = GetMidpoints(i2, i0, normalize, midPoints);

                    newTriangles.Add(i0);
                    newTriangles.Add(m01);
                    newTriangles.Add(m02);

                    newTriangles.Add(i1);
                    newTriangles.Add(m12);
                    newTriangles.Add(m01);

                    newTriangles.Add(i2);
                    newTriangles.Add(m02);
                    newTriangles.Add(m12);

                    newTriangles.Add(m02);
                    newTriangles.Add(m01);
                    newTriangles.Add(m12);
                }

                triangles = new List<int>(newTriangles);
                newTriangles.Clear();
            }
        }

        public int GetMidpoints(int x, int y, bool normalize, Dictionary<string, int> mids)
        {
            int midpointIndex = -1;

            int min = x < y ? x : y;
            int max = x > y ? x : y;

            string edgeKey = min.ToString() + "_" + max.ToString();

            var bMid = mids.TryGetValue(edgeKey, out midpointIndex);

            if (!bMid)
            {
                Vector3D v0 = normals[x];
                Vector3D v1 = normals[y];

                Vector3D midpoint = Vector3D.Divide(Vector3D.Add(v0, v1), 2);

                if (normalize)
                {
                    midpoint.Normalize(); 
                }


                midpointIndex = normals.Count;
                normals.Add(midpoint);
                mids.Add(edgeKey, midpointIndex);

            }

            return midpointIndex;
        }
    }
}
