using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MobileDebug_WPF
{
    class MeshShapes
    {

        // Make a sphere.
        public void MakeSphere(Model3DGroup model_group,
            ref MeshGeometry3D sphere_mesh, Material sphere_material,
            double radius, double cx, double cy, double cz, int num_phi,
            int num_theta)
        {
            // Make the mesh if we must.
            if (sphere_mesh == null)
            {
                sphere_mesh = new MeshGeometry3D();
                GeometryModel3D new_model = new GeometryModel3D(sphere_mesh, sphere_material);
                model_group.Children.Add(new_model);
            }

            double dphi = Math.PI / num_phi;
            double dtheta = 2 * Math.PI / num_theta;

            // Remember the first point.
            int pt0 = sphere_mesh.Positions.Count;

            // Make the points.
            double phi1 = Math.PI / 2;
            for (int p = 0; p <= num_phi; p++)
            {
                double r1 = radius * Math.Cos(phi1);
                double y1 = radius * Math.Sin(phi1);

                double theta = 0;
                for (int t = 0; t <= num_theta; t++)
                {
                    sphere_mesh.Positions.Add(new Point3D(
                        cx + r1 * Math.Cos(theta),
                        cy + y1,
                        cz + -r1 * Math.Sin(theta)));
                    sphere_mesh.TextureCoordinates.Add(new Point(
                        (double)t / num_theta, (double)p / num_phi));
                    theta += dtheta;
                }
                phi1 -= dphi;
            }

            // Make the triangles.
            int i1, i2, i3, i4;
            for (int p = 0; p <= num_phi - 1; p++)
            {
                i1 = p * (num_theta + 1);
                i2 = i1 + (num_theta + 1);
                for (int t = 0; t <= num_theta - 1; t++)
                {
                    i3 = i1 + 1;
                    i4 = i2 + 1;
                    sphere_mesh.TriangleIndices.Add(pt0 + i1);
                    sphere_mesh.TriangleIndices.Add(pt0 + i2);
                    sphere_mesh.TriangleIndices.Add(pt0 + i4);

                    sphere_mesh.TriangleIndices.Add(pt0 + i1);
                    sphere_mesh.TriangleIndices.Add(pt0 + i4);
                    sphere_mesh.TriangleIndices.Add(pt0 + i3);
                    i1 += 1;
                    i2 += 1;
                }
            }
        }


        public void MakeCube(Model3DGroup model_group,
            ref MeshGeometry3D sphere_mesh, Material sphere_material,
            double x, double y, double z)
        {
            // Make the mesh if we must.
            if (sphere_mesh == null)
            {
                sphere_mesh = new MeshGeometry3D();
                GeometryModel3D new_model =
                    new GeometryModel3D(sphere_mesh, sphere_material);
                model_group.Children.Add(new_model);
            }



            //sphere_mesh.Positions = GetCube(x, y, z);
            //sphere_mesh.TextureCoordinates = GetRectangle(x, y, z);
            //sphere_mesh.TriangleIndices = GetCubeIndices();
        }

        public void MakeXYPlane(Model3DGroup model_group, ref MeshGeometry3D mesh, Material material,
                                double x, double y)
        {
            // Make the mesh if we must.
            if (mesh == null)
            {
                mesh = new MeshGeometry3D();
                GeometryModel3D new_model = new GeometryModel3D(mesh, material);
                new_model.BackMaterial = material;
                model_group.Children.Add(new_model);
            }

            // Remember the first point.
            int pt0 = mesh.Positions.Count;

            mesh.Positions.Add(new Point3D(-x, -y, 0));
            mesh.Positions.Add(new Point3D(x, -y, 0));
            mesh.Positions.Add(new Point3D(-x, y, 0));
            mesh.Positions.Add(new Point3D(x, y, 0));

            mesh.TextureCoordinates.Add(new Point(0, 0));
            mesh.TextureCoordinates.Add(new Point(1, 0));
            mesh.TextureCoordinates.Add(new Point(1, 1));
            mesh.TextureCoordinates.Add(new Point(0, 1));

            mesh.TriangleIndices.Add(pt0 + 0);
            mesh.TriangleIndices.Add(pt0 + 1);
            mesh.TriangleIndices.Add(pt0 + 2);
            mesh.TriangleIndices.Add(pt0 + 1);
            mesh.TriangleIndices.Add(pt0 + 3);
            mesh.TriangleIndices.Add(pt0 + 2);
        }

        public void MakeXZPlane(Model3DGroup model_group, ref MeshGeometry3D mesh, Material material,
                        double x, double z)
        {
            // Make the mesh if we must.
            if (mesh == null)
            {
                mesh = new MeshGeometry3D();
                GeometryModel3D new_model = new GeometryModel3D(mesh, material);
                new_model.BackMaterial = material;
                model_group.Children.Add(new_model);
            }

            // Remember the first point.
            int pt0 = mesh.Positions.Count;

            mesh.Positions.Add(new Point3D(-x, 0, -z));
            mesh.Positions.Add(new Point3D(x, 0, -z));
            mesh.Positions.Add(new Point3D(-x, 0, z));
            mesh.Positions.Add(new Point3D(x, 0, z));

            mesh.TextureCoordinates.Add(new Point(0, 0));
            mesh.TextureCoordinates.Add(new Point(1, 0));
            mesh.TextureCoordinates.Add(new Point(1, 1));
            mesh.TextureCoordinates.Add(new Point(0, 1));

            mesh.TriangleIndices.Add(pt0 + 0);
            mesh.TriangleIndices.Add(pt0 + 1);
            mesh.TriangleIndices.Add(pt0 + 2);
            mesh.TriangleIndices.Add(pt0 + 1);
            mesh.TriangleIndices.Add(pt0 + 3);
            mesh.TriangleIndices.Add(pt0 + 2);
        }

        public void AddTriangle(MeshGeometry3D mesh, Point3D[] pts)
        {
            if (pts.Count() != 3) return;

            //use the three point of the triangle to calculate the normal (angle of the surface)
            Vector3D normal = CalculateNormal(pts[0], pts[1], pts[2]);
            normal.Normalize();

            //calculate the uv products
            Vector3D u;
            if (normal.X == 0 && normal.Z == 0) u = new Vector3D(normal.Y, -normal.X, 0);
            else u = new Vector3D(normal.X, -normal.Z, 0);

            u.Normalize();
            Vector3D n = new Vector3D(normal.Z, normal.X, normal.Y);
            Vector3D v = Vector3D.CrossProduct(n, u);

            int index = mesh.Positions.Count;
            foreach (Point3D pt in pts)
            {
                //add the points to create the triangle
                mesh.Positions.Add(pt);
                mesh.TriangleIndices.Add(index++);
                

                //apply the uv texture positions
                double u_coor = Vector3D.DotProduct(u, new Vector3D(pt.Z, pt.X, pt.Y));
                double v_coor = Vector3D.DotProduct(v, new Vector3D(pt.Z, pt.X, pt.Y));
                //mesh.TextureCoordinates.Add(new Point(u_coor, v_coor));
            }
        }

        private Vector3D CalculateNormal(Point3D firstPoint, Point3D secondPoint, Point3D thirdPoint)
        {
            var u = new Point3D(firstPoint.X - secondPoint.X,
                firstPoint.Y - secondPoint.Y,
                firstPoint.Z - secondPoint.Z);

            var v = new Point3D(secondPoint.X - thirdPoint.X,
                secondPoint.Y - thirdPoint.Y,
                secondPoint.Z - thirdPoint.Z);

            return new Vector3D(u.Y * v.Z - u.Z * v.Y, u.Z * v.X - u.X * v.Z, u.X * v.Y - u.Y * v.X);
        }

        public Point3DCollection GetCube(double x, double y, double z)
        {
            System.Windows.Media.Media3D.Point3DCollection points = new System.Windows.Media.Media3D.Point3DCollection(20);
            System.Windows.Media.Media3D.Point3D point;
            //top of the floor
            point = new System.Windows.Media.Media3D.Point3D(-x, 0, z);// Floor Index - 0
            points.Add(point);
            point = new System.Windows.Media.Media3D.Point3D(x, 0, z);// Floor Index - 1
            points.Add(point);
            point = new System.Windows.Media.Media3D.Point3D(x, 0, -z);// Floor Index - 2
            points.Add(point);
            point = new System.Windows.Media.Media3D.Point3D(-x, 0, -z);// Floor Index - 3
            points.Add(point);
            //front side
            point = new System.Windows.Media.Media3D.Point3D(-x, 0, z);// Floor Index - 4
            points.Add(point);
            point = new System.Windows.Media.Media3D.Point3D(-x, y, z);// Floor Index - 5
            points.Add(point);
            point = new System.Windows.Media.Media3D.Point3D(x, y, z);// Floor Index - 6
            points.Add(point);
            point = new System.Windows.Media.Media3D.Point3D(x, 0, z);// Floor Index - 7
            points.Add(point);
            //right side
            point = new System.Windows.Media.Media3D.Point3D(x, 0, z);// Floor Index - 8
            points.Add(point);
            point = new System.Windows.Media.Media3D.Point3D(x, y, z);// Floor Index - 9
            points.Add(point);
            point = new System.Windows.Media.Media3D.Point3D(x, y, -z);// Floor Index - 10
            points.Add(point);
            point = new System.Windows.Media.Media3D.Point3D(x, 0, -z);// Floor Index - 11
            points.Add(point);
            //back side
            point = new System.Windows.Media.Media3D.Point3D(x, 0, -z);// Floor Index - 12
            points.Add(point);
            point = new System.Windows.Media.Media3D.Point3D(x, y, -z);// Floor Index - 13
            points.Add(point);
            point = new System.Windows.Media.Media3D.Point3D(-x, y, -z);// Floor Index - 14
            points.Add(point);
            point = new System.Windows.Media.Media3D.Point3D(-x, 0, -z);// Floor Index - 15
            points.Add(point);
            //left side
            point = new System.Windows.Media.Media3D.Point3D(-x, 0, -z);// Floor Index - 16
            points.Add(point);
            point = new System.Windows.Media.Media3D.Point3D(-x, y, -z);// Floor Index - 17
            points.Add(point);
            point = new System.Windows.Media.Media3D.Point3D(-x, y, z);// Floor Index - 18
            points.Add(point);
            point = new System.Windows.Media.Media3D.Point3D(-x, 0, z);// Floor Index - 19
            points.Add(point);
            return points;

        }
        public Int32Collection GetCubeIndices()
        {
            int[] indices = new int[] { 0, 1, 2, 0, 2, 3, 4, 5, 7, 5, 6, 7, 8, 9, 11, 9, 10, 11, 12, 13, 15, 13,
       14, 15, 16, 17, 19, 17, 18, 19 };

            return new Int32Collection(indices);
        }
        //class BigPlanet : SphereGeometry3D
        //{
        //    BigPlanet()
        //    {
        //        Radius = 30;
        //        Separators = 5;
        //    }
        //}

        //class SmallPlanet : SphereGeometry3D
        //{
        //    SmallPlanet()
        //    {
        //        Radius = 5;
        //        Separators = 5;
        //    }
        //}

        //abstract class RoundMesh3D
        //{
        //    protected int n = 10;
        //    protected int r = 20;
        //    protected Point3DCollection points;
        //    protected Int32Collection triangleIndices;

        //    public virtual int Radius
        //    {
        //        get { return r; }
        //        set { r = value; CalculateGeometry(); }
        //    }

        //    public virtual int Separators
        //    {
        //        get { return n; }
        //        set { n = value; CalculateGeometry(); }
        //    }

        //    public Point3DCollection Points
        //    {
        //        get { return points; }
        //    }

        //    public Int32Collection TriangleIndices
        //    {
        //        get { return triangleIndices; }
        //    }

        //    protected abstract void CalculateGeometry();
        //}

        //class SphereGeometry3D : RoundMesh3D
        //{
        //    protected override void CalculateGeometry()
        //    {
        //        int e;
        //        double segmentRad = Math.PI / 2 / (n + 1);
        //        int numberOfSeparators = 4 * n + 4;

        //        points = new Point3DCollection();
        //        triangleIndices = new Int32Collection();

        //        for (e = -n; e <= n; e++)
        //        {
        //            double r_e = r * Math.Cos(segmentRad * e);
        //            double y_e = r * Math.Sin(segmentRad * e);

        //            for (int s = 0; s <= (numberOfSeparators - 1); s++)
        //            {
        //                double z_s = r_e * Math.Sin(segmentRad * s) * (-1);
        //                double x_s = r_e * Math.Cos(segmentRad * s);
        //                points.Add(new Point3D(x_s, y_e, z_s));
        //            }
        //        }
        //        points.Add(new Point3D(0, r, 0));
        //        points.Add(new Point3D(0, -1 * r, 0));

        //        for (e = 0; e < 2 * n; e++)
        //        {
        //            for (int i = 0; i < numberOfSeparators; i++)
        //            {
        //                triangleIndices.Add(e * numberOfSeparators + i);
        //                triangleIndices.Add(e * numberOfSeparators + i +
        //                                    numberOfSeparators);
        //                triangleIndices.Add(e * numberOfSeparators + (i + 1) %
        //                                    numberOfSeparators + numberOfSeparators);

        //                triangleIndices.Add(e * numberOfSeparators + (i + 1) %
        //                                    numberOfSeparators + numberOfSeparators);
        //                triangleIndices.Add(e * numberOfSeparators +
        //                                   (i + 1) % numberOfSeparators);
        //                triangleIndices.Add(e * numberOfSeparators + i);
        //            }
        //        }

        //        for (int i = 0; i < numberOfSeparators; i++)
        //        {
        //            triangleIndices.Add(e * numberOfSeparators + i);
        //            triangleIndices.Add(e * numberOfSeparators + (i + 1) %
        //                                numberOfSeparators);
        //            triangleIndices.Add(numberOfSeparators * (2 * n + 1));
        //        }

        //        for (int i = 0; i < numberOfSeparators; i++)
        //        {
        //            triangleIndices.Add(i);
        //            triangleIndices.Add((i + 1) % numberOfSeparators);
        //            triangleIndices.Add(numberOfSeparators * (2 * n + 1) + 1);
        //        }
        //    }

        //    public SphereGeometry3D()
        //    { }
        //}

    }
}
