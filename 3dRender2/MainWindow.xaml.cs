using _3DTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _3dRender2
{
    public partial class MainWindow : Window
    {
        ScreenSpaceLines3D wireframeSphere = new ScreenSpaceLines3D();
        Viewport3D myViewport3D = new Viewport3D();
        Model3DGroup myModel3DGroup = new Model3DGroup();
        GeometryModel3D myGeometryModel = new GeometryModel3D();
        ModelVisual3D myModelVisual3D = new ModelVisual3D();

        PerspectiveCamera myPCamera = new PerspectiveCamera();

        AxisAngleRotation3D ax3d;

        bool wireframe = false;


        public MainWindow()
        {
            InitializeComponent();
            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);

            ax3d = new AxisAngleRotation3D(new Vector3D(0, 2, 0), 0);
            RotateTransform3D myRotateTransform = new RotateTransform3D(ax3d);
            myModelVisual3D.Transform = myRotateTransform;
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W)
            {
                myPCamera.Position = new Point3D(myPCamera.Position.X, myPCamera.Position.Y, myPCamera.Position.Z - .1);
            }

            if (e.Key == Key.S)
            {
                myPCamera.Position = new Point3D(myPCamera.Position.X, myPCamera.Position.Y, myPCamera.Position.Z + .1);
            }

            if (e.Key == Key.A)
            {
                ax3d.Axis = new Vector3D(0, 2, 0);

                ax3d.Angle += 1;

                wireframeSphere.Transform = myModelVisual3D.Transform;
            }

            if (e.Key == Key.D)
            {
                ax3d.Axis = new Vector3D(0, 2, 0);

                ax3d.Angle -= 1;

                wireframeSphere.Transform = myModelVisual3D.Transform;
            }

            if (e.Key == Key.F)
            {
                if (wireframe)
                {
                    wireframeSphere.Thickness = 0;
                }
                else
                {
                    wireframeSphere.Thickness = 1;
                }

                wireframe = !wireframe;
            }

            myViewport3D.Camera = myPCamera;
        }
       
        private void CreateSphere(object sender, RoutedEventArgs e)
        {
            GeosphereGenerator sphere = new GeosphereGenerator(8);

            myPCamera.Position = new Point3D(0, 0, 1.5);

            myPCamera.LookDirection = new Vector3D(0, 0, -1);

            myPCamera.FieldOfView = 90;

            myViewport3D.Camera = myPCamera;

            DirectionalLight myDirectionalLight = new DirectionalLight();
            myDirectionalLight.Color = Colors.White;
            myDirectionalLight.Direction = new Vector3D(-0.61, -0.5, -0.61);

            myModel3DGroup.Children.Add(myDirectionalLight);

            MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();

            Vector3DCollection myNormalCollection = new Vector3DCollection();

            foreach (var verts in sphere.normals)
            {
                myNormalCollection.Add(verts);
            }

            myMeshGeometry3D.Normals = myNormalCollection;

            Point3DCollection myPositionCollection = new Point3DCollection();

            foreach (var verts in sphere.normals)
            {
                myPositionCollection.Add(new Point3D(verts.X, verts.Y, verts.Z));
            }

            myMeshGeometry3D.Positions = myPositionCollection;

            Int32Collection myTriangleIndicesCollection = new Int32Collection();

            foreach (var triangle in sphere.triangles)
            {
                myTriangleIndicesCollection.Add(triangle);
            }

            myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;

            myGeometryModel.Geometry = myMeshGeometry3D;


            Material material = new DiffuseMaterial(
                new SolidColorBrush(Colors.BlueViolet));

            GeometryModel3D model = new GeometryModel3D(
                myMeshGeometry3D, material);

            myModel3DGroup.Children.Add(model);

            myModel3DGroup.Children.Add(myGeometryModel);

            Color c = Colors.Yellow;
            double width = 1;

            wireframeSphere.Thickness = width;
            wireframeSphere.Color = c;

            for (int i = 0; i < sphere.triangles.Count - 1; i += 3)
            {
                wireframeSphere.Points.Add(new Point3D(sphere.normals[sphere.triangles[i]].X, sphere.normals[sphere.triangles[i]].Y, sphere.normals[sphere.triangles[i]].Z));
                wireframeSphere.Points.Add(new Point3D(sphere.normals[sphere.triangles[i + 1]].X, sphere.normals[sphere.triangles[i + 1]].Y, sphere.normals[sphere.triangles[i + 1]].Z));

                wireframeSphere.Points.Add(new Point3D(sphere.normals[sphere.triangles[i]].X, sphere.normals[sphere.triangles[i]].Y, sphere.normals[sphere.triangles[i]].Z));
                wireframeSphere.Points.Add(new Point3D(sphere.normals[sphere.triangles[i + 2]].X, sphere.normals[sphere.triangles[i + 2]].Y, sphere.normals[sphere.triangles[i + 2]].Z));

                wireframeSphere.Points.Add(new Point3D(sphere.normals[sphere.triangles[i + 1]].X, sphere.normals[sphere.triangles[i + 1]].Y, sphere.normals[sphere.triangles[i + 1]].Z));
                wireframeSphere.Points.Add(new Point3D(sphere.normals[sphere.triangles[i + 2]].X, sphere.normals[sphere.triangles[i + 2]].Y, sphere.normals[sphere.triangles[i + 2]].Z));
            }



            myViewport3D.Children.Add(wireframeSphere);

            myModelVisual3D.Content = myModel3DGroup;

            myViewport3D.Children.Add(myModelVisual3D);

            this.Content = myViewport3D;
        }

        private void CreateHexaSphere(object sender, RoutedEventArgs e)
        {
            GeosphereGenerator sphereX = new GeosphereGenerator(6);

            HexasphereGenerator sphere = new HexasphereGenerator();
            sphere.CreateGoldberg(sphereX.normals, sphereX.triangles);

            myPCamera.Position = new Point3D(0, 0, 2.5);

            myPCamera.LookDirection = new Vector3D(0, 0, -1);

            myPCamera.FieldOfView = 90;

            myViewport3D.Camera = myPCamera;

            DirectionalLight myDirectionalLight = new DirectionalLight();
            myDirectionalLight.Color = Colors.White;
            myDirectionalLight.Direction = new Vector3D(-0.61, -0.5, -0.61);

            myModel3DGroup.Children.Add(myDirectionalLight);

            bool switcher = true;

            Stopwatch sw = new Stopwatch();

            var hexes = sphere.hexes.ToArray();

            sw.Start();

            for (int j = 0; j < hexes.Length; j++)
            {
                MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();

                Vector3DCollection myNormalCollection = new Vector3DCollection();

                foreach (var verts in hexes[j].Normals)
                {
                    myNormalCollection.Add(verts);
                }

                myMeshGeometry3D.Normals = myNormalCollection;

                Point3DCollection myPositionCollection = new Point3DCollection();

                foreach (var verts in hexes[j].Normals)
                {
                    myPositionCollection.Add(new Point3D(verts.X, verts.Y, verts.Z));
                }

                myMeshGeometry3D.Positions = myPositionCollection;

                Int32Collection myTriangleIndicesCollection = new Int32Collection();

                foreach (var triangle in hexes[j].Tris)
                {
                    myTriangleIndicesCollection.Add(triangle);
                }

                myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;                

                Material material = new DiffuseMaterial(
                                new SolidColorBrush(Colors.Black)); ;

                if (switcher)
                {
                    material = new DiffuseMaterial(
                                new SolidColorBrush(Colors.BlueViolet)); 
                }

                switcher = !switcher;

                GeometryModel3D model = new GeometryModel3D(
                    myMeshGeometry3D, material);

                myGeometryModel.Geometry = myMeshGeometry3D;

                myModel3DGroup.Children.Add(model);

                myModel3DGroup.Children.Add(myGeometryModel);

                //Color c = Colors.Yellow;
                //double width = .5;

                //wireframeSphere.Thickness = width;
                //wireframeSphere.Color = c;

                //for (int i = 0; i < hex.Tris.Count - 1; i += 3)
                //{
                //    wireframeSphere.Points.Add(new Point3D(hex.Normals[hex.Tris[i]].X, hex.Normals[hex.Tris[i]].Y, hex.Normals[hex.Tris[i]].Z));
                //    wireframeSphere.Points.Add(new Point3D(hex.Normals[hex.Tris[i + 1]].X, hex.Normals[hex.Tris[i + 1]].Y, hex.Normals[hex.Tris[i + 1]].Z));

                //    wireframeSphere.Points.Add(new Point3D(hex.Normals[hex.Tris[i]].X, hex.Normals[hex.Tris[i]].Y, hex.Normals[hex.Tris[i]].Z));
                //    wireframeSphere.Points.Add(new Point3D(hex.Normals[hex.Tris[i + 2]].X, hex.Normals[hex.Tris[i + 2]].Y, hex.Normals[hex.Tris[i + 2]].Z));

                //    wireframeSphere.Points.Add(new Point3D(hex.Normals[hex.Tris[i + 1]].X, hex.Normals[hex.Tris[i + 1]].Y, hex.Normals[hex.Tris[i + 1]].Z));
                //    wireframeSphere.Points.Add(new Point3D(hex.Normals[hex.Tris[i + 2]].X, hex.Normals[hex.Tris[i + 2]].Y, hex.Normals[hex.Tris[i + 2]].Z));
                //}

            }

            sw.Stop();

            //myViewport3D.Children.Add(wireframeSphere);

            myModelVisual3D.Content = myModel3DGroup;

            myViewport3D.Children.Add(myModelVisual3D);

            this.Content = myViewport3D;
        }
    }
}
