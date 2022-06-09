using Common;
using Common.Model;
using System;
using System.Collections.Generic;
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
using Point = System.Windows.Point;

namespace Grafika_Projekat_2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point start = new Point();
        private int zoomMax = 20;
        private int zoomCurent = 1;
        private MapHandler mapHandler = new MapHandler();
        List<GeometryModel3D> entities = new List<GeometryModel3D>();
        List<EntityWrapper> entityWrappers = new List<EntityWrapper>();
        List<GeometryModel3D> connectors = new List<GeometryModel3D>();
        List<LineEntity> lines = new List<LineEntity>();
        Viewport2DVisual3D viewport;
        RotateTransform3D rotateTransform = new RotateTransform3D();
        AxisAngleRotation3D axisAngleRotation = new AxisAngleRotation3D();
        GeometryModel3D lit1, lit2;
        DiffuseMaterial oldmat1,oldmat2;
        List<GeometryModel3D> hidden = new List<GeometryModel3D>();
        List<GeometryModel3D> hidden2 = new List<GeometryModel3D>();
        short filter = 0; 
        // button1, button2, button3, button4
        //
        public MainWindow()
        {
            InitializeComponent();
            mapHandler.CalculateEntities();
            SetEntities();
        }

        private void SetEntities()
        {
            int k;
            for (int i = 0; i < MapHandler.width; i++)
            {
                for (int j = 0; j < MapHandler.height; j++)
                {
                    k = 0;
                    EntityWrapper temp = MapHandler.BigArray[i, j];
                    while (temp != null)
                    {
                        DrawCube(temp, i, j, k);
                        k++;
                        temp = temp.nextEntity;
                    }

                }
            }
            foreach (var item in Loader.lineEntities)
            {
                DrawLine(item);
            }
        }

        private void DrawCube(EntityWrapper entity, int x, int y, int z)
        {
            double x1 = x * 0.659;
            double y1 = y * 1.516;
            x1 *= 5;
            y1 *= 5;
            y1 -= 588;
            x1 -= 388;
            double temp = x1;
            x1 = y1;
            y1 = temp;
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(new Point3D(x1 - 1.25, y1 + 1.25, z * 2.5 + 0.1));
            mesh.Positions.Add(new Point3D(x1 + 1.25, y1 + 1.25, z * 2.5 + 0.1));
            mesh.Positions.Add(new Point3D(x1 - 1.25, y1 - 1.25, z * 2.5 + 0.1));
            mesh.Positions.Add(new Point3D(x1 + 1.25, y1 - 1.25, z * 2.5 + 0.1));
            mesh.Positions.Add(new Point3D(x1 - 1.25, y1 + 1.25, z * 2.5 + 2.5));
            mesh.Positions.Add(new Point3D(x1 + 1.25, y1 + 1.25, z * 2.5 + 2.5));
            mesh.Positions.Add(new Point3D(x1 - 1.25, y1 - 1.25, z * 2.5 + 2.5));
            mesh.Positions.Add(new Point3D(x1 + 1.25, y1 - 1.25, z * 2.5 + 2.5));

            #region Triangles
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(1);
            
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);

            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(6);

            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(6);

            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(5);

            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(5);

            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(2);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(2);

            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(4);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(4);

            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(6);

            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(3);
            #endregion

            Material material = new DiffuseMaterial((entity.entityType == EntityType.Substation) ? Brushes.Brown : ((entity.entityType == EntityType.Switch) ? Brushes.Violet : Brushes.Blue));
            GeometryModel3D model = new GeometryModel3D(mesh, material);
            model.Transform = assign;
            ModelVisual3D modelVisual3D = new ModelVisual3D();
            modelVisual3D.Content = model;
            entities.Add(model);
            entityWrappers.Add(entity);
            viewport1.Children.Add(modelVisual3D);
        }

        private void DrawLine(LineEntity line)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            int pointsUsed = 0;
            Tuple<int, int> point;
            for (int i = 0; i < line.Vertices.Count; i++)
            {
                if(i == 0)
                {
                    point = MapHandler.GetTuple(line.FirstEnd);
                    pointsUsed = Connect2(mesh,
                        new Point(point.Item1, point.Item2), pointsUsed, true);
                    continue;
                }
                if(i == line.Vertices.Count - 1)
                {
                    point = MapHandler.GetTuple(line.SecondEnd);
                    Connect2(mesh, new Point(point.Item1, point.Item2), pointsUsed, false);
                    break;
                }
                pointsUsed = Connect2(mesh, 
                    new Point((line.Vertices[i].X - 45.2325) / ((45.277031 - 45.2325) / MapHandler.width), (line.Vertices[i].Y - 19.793909) / ((19.894459 - 19.793909) / MapHandler.height)), pointsUsed, false);
            }

            Material material = new DiffuseMaterial((line.ConductorMaterial.ToLower() == "steel") ? Brushes.DarkGray : ((line.ConductorMaterial.ToLower() == "copper") ? Brushes.Brown : ((line.ConductorMaterial.ToLower() == "acsr") ? Brushes.Blue : Brushes.Violet)));
            GeometryModel3D model = new GeometryModel3D(mesh, material);
            model.Transform = assign;
            ModelVisual3D modelVisual3D = new ModelVisual3D();
            modelVisual3D.Content = model;
            connectors.Add(model);
            lines.Add(line);
            viewport1.Children.Add(modelVisual3D);
        }

        private int Connect2(MeshGeometry3D mesh, Point first, int pointsUsed, bool starting)
        {
            double x1 = first.X * 0.659;
            double y1 = first.Y * 1.516;
            x1 *= 5;
            y1 *= 5;
            y1 -= 588;
            x1 -= 388;
            double temp = x1;
            x1 = y1;
            y1 = temp;

            mesh.Positions.Add(new Point3D(x1 - 0.3, y1, 0.1));
            mesh.Positions.Add(new Point3D(x1 + 0.3, y1, 0.1));
            mesh.Positions.Add(new Point3D(x1 - 0.3, y1, 3));
            mesh.Positions.Add(new Point3D(x1 + 0.3, y1, 3));
            if (starting)
                return 0;

            #region Triangles
            mesh.TriangleIndices.Add(1+pointsUsed);
            mesh.TriangleIndices.Add(5+pointsUsed);
            mesh.TriangleIndices.Add(7+pointsUsed);

            mesh.TriangleIndices.Add(1+pointsUsed);
            mesh.TriangleIndices.Add(7+pointsUsed);
            mesh.TriangleIndices.Add(3+pointsUsed);

            mesh.TriangleIndices.Add(0+pointsUsed);
            mesh.TriangleIndices.Add(2+pointsUsed);
            mesh.TriangleIndices.Add(6+pointsUsed);

            mesh.TriangleIndices.Add(0+pointsUsed);
            mesh.TriangleIndices.Add(6+pointsUsed);
            mesh.TriangleIndices.Add(4+pointsUsed);

            mesh.TriangleIndices.Add(3+pointsUsed);
            mesh.TriangleIndices.Add(7+pointsUsed);
            mesh.TriangleIndices.Add(6+pointsUsed);

            mesh.TriangleIndices.Add(2+pointsUsed);
            mesh.TriangleIndices.Add(3+pointsUsed);
            mesh.TriangleIndices.Add(6+pointsUsed);

            mesh.TriangleIndices.Add(0+pointsUsed);
            mesh.TriangleIndices.Add(4+pointsUsed);
            mesh.TriangleIndices.Add(1+pointsUsed);

            mesh.TriangleIndices.Add(1+pointsUsed);
            mesh.TriangleIndices.Add(4+pointsUsed);
            mesh.TriangleIndices.Add(5+pointsUsed);
            #endregion
            return pointsUsed + 4;
        }

        private void viewport1_MouseMove(object sender, MouseEventArgs e)
        {
            if (viewport1.IsMouseCaptured )
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point end = e.GetPosition(this);
                    double offsetX = end.X - start.X;
                    double offsetY = end.Y - start.Y;
                    Point3D position = camera.Position;
                    position.X += offsetX * (1-(zoomCurent/20));
                    position.Y -= offsetY * (1-(zoomCurent/20));
                    camera.Position = position;
                    start = end;
                }
                if(e.MiddleButton == MouseButtonState.Pressed)
                {
                    Point end = e.GetPosition(this);
                    double offsetX = end.X - start.X;
                    rotate.Angle += offsetX;
                    axisAngleRotation.Angle += offsetX;
                    start = end;
                }
            }
        }

        private void viewport1_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0 && zoomCurent < zoomMax)
            {
                Point3D position = camera.Position;
                zoomCurent++;
                position.Y *= 0.85;
                position.Z *= 0.85;
                camera.Position = position;
            }
            else if (e.Delta <= 0 && zoomCurent > -zoomMax)
            {
                Point3D position = camera.Position;
                zoomCurent--;
                position.Y *= 1.176;
                position.Z *= 1.176;
                camera.Position = position;
            }
        }

        private void viewport1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            start = e.GetPosition(this);
            if(oldmat1 != null)
            {
                lit1.Material = oldmat1;
                lit2.Material = oldmat2;
                oldmat1 = null;
                oldmat2 = null;
            }
            if(viewport != null)
            {
                viewport1.Children.Remove(viewport);
                viewport = null;
            }
            Point hitTestStart = new Point(start.X, start.Y-50);

            Point3D testpoint3D = new Point3D(start.X, start.Y - 50, 0);
            Vector3D testdirection = new Vector3D(start.X, start.Y - 50, 10);

            PointHitTestParameters pointparams =
                     new PointHitTestParameters(hitTestStart);
            RayHitTestParameters rayparams =
                     new RayHitTestParameters(testpoint3D, testdirection);

            VisualTreeHelper.HitTest(viewport1, null, HTResult, pointparams);

            viewport1.CaptureMouse();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if((filter & 0b100) > 0)
            {
                for (int i = 0; i < entityWrappers.Count; i++)
                {
                    if (entityWrappers[i].entityType == EntityType.Switch)
                    {
                        entities[i].Material = new DiffuseMaterial(Brushes.Violet);
                    }
                }
                filter -= 0b100;
                return;
            }
            for (int i = 0; i < entityWrappers.Count; i++)
            {
                if(entityWrappers[i].entityType == EntityType.Switch)
                {
                    if(((SwitchEntity)entityWrappers[i].powerEntity).Status.ToLower() == "open")
                        entities[i].Material = new DiffuseMaterial(Brushes.Red);
                    else
                        entities[i].Material = new DiffuseMaterial(Brushes.Green);
                }
            }
            filter += 0b100;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if ((filter & 0b10) > 0)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    connectors[i].Material = new DiffuseMaterial((lines[i].ConductorMaterial.ToLower() == "steel") ? Brushes.DarkGray : ((lines[i].ConductorMaterial.ToLower() == "copper") ? Brushes.Brown : ((lines[i].ConductorMaterial.ToLower() == "acsr") ? Brushes.Blue : Brushes.Violet)));
                }
                filter -= 0b10;
                return;
            }
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].R > 2)
                    connectors[i].Material = new DiffuseMaterial(Brushes.Yellow);
                else if (lines[i].R > 1)
                    connectors[i].Material = new DiffuseMaterial(Brushes.Orange);
                else
                    connectors[i].Material = new DiffuseMaterial(Brushes.Red);
            }
            filter += 0b10;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if ((filter & 0b1000) > 0)
            {
                foreach (GeometryModel3D item in hidden)
                {
                    for (int i = 0; i < ((MeshGeometry3D)item.Geometry).Positions.Count; i++)
                    {
                        Point3D temp = ((MeshGeometry3D)item.Geometry).Positions[i];
                        temp.Z += 30;
                        ((MeshGeometry3D)item.Geometry).Positions[i] = temp;
                    }
                }
                hidden.Clear();
                filter -= 0b1000;
                return;
            }
            for (int i = 0; i < entityWrappers.Count; i++)
            {
                if(entityWrappers[i].entityType == EntityType.Switch && ((SwitchEntity)entityWrappers[i].powerEntity).Status.ToLower() == "open")
                {
                    int temp = lines.FindIndex(t => t.FirstEnd == entityWrappers[i].powerEntity.Id);
                    if (temp < 0)
                        continue;
                    hidden.Add(connectors[temp]);
                    hidden.Add(entities[entityWrappers.FindIndex(t => t.powerEntity.Id == lines[temp].SecondEnd)]);
                }
            }
            foreach (GeometryModel3D item in hidden)
            {
                for (int i = 0; i < ((MeshGeometry3D)item.Geometry).Positions.Count; i++)
                {
                    Point3D temp = ((MeshGeometry3D)item.Geometry).Positions[i];
                    temp.Z -= 30;
                    ((MeshGeometry3D)item.Geometry).Positions[i] = temp;
                }
            }
            filter += 0b1000;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Filter4(0, 3);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Filter4(3, 5);
        }
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Filter4(5, 30);
        }
        private void Filter4(int min, int max)
        {
            if ((filter & 0b1) > 0)
            {
                foreach (GeometryModel3D item in hidden2)
                {
                    for (int i = 0; i < ((MeshGeometry3D)item.Geometry).Positions.Count; i++)
                    {
                        Point3D temp = ((MeshGeometry3D)item.Geometry).Positions[i];
                        temp.Z += 30;
                        ((MeshGeometry3D)item.Geometry).Positions[i] = temp;
                    }
                }
                hidden2.Clear();
                filter -= 0b1;
                return;
            }
            for (int i = 0; i < entityWrappers.Count; i++)
            {
                int num = lines.FindAll(t=>t.FirstEnd == entityWrappers[i].powerEntity.Id || t.SecondEnd == entityWrappers[i].powerEntity.Id).Count;
                if (num >= min && num <= max)
                    hidden2.Add(entities[i]);
            }
            foreach (GeometryModel3D item in hidden2)
            {
                for (int i = 0; i < ((MeshGeometry3D)item.Geometry).Positions.Count; i++)
                {
                    Point3D temp = ((MeshGeometry3D)item.Geometry).Positions[i];
                    temp.Z -= 30;
                    ((MeshGeometry3D)item.Geometry).Positions[i] = temp;
                }
            }
            filter += 0b1;
        }

        private HitTestResultBehavior HTResult(System.Windows.Media.HitTestResult rawresult)
        {

            RayHitTestResult rayResult = rawresult as RayHitTestResult;

            if (rayResult != null)
            {
                for (int i = 0; i < entities.Count; i++)
                {
                    if (entities[i] == rayResult.ModelHit)
                    {
                        viewport = new Viewport2DVisual3D();
                        Point3D showLocation = rayResult.PointHit;
                        MeshGeometry3D mesh = new MeshGeometry3D();
                        mesh.Positions = new Point3DCollection {
                            new Point3D(showLocation.X + 7*6, showLocation.Y + 4*6, 13),
                            new Point3D(showLocation.X + 1, showLocation.Y + 4*6, 13),
                            new Point3D(showLocation.X + 7*6, showLocation.Y + 1, 10),
                            new Point3D(showLocation.X + 1, showLocation.Y + 1, 10)
                        };
                        mesh.TriangleIndices.Add(0);
                        mesh.TriangleIndices.Add(1);
                        mesh.TriangleIndices.Add(3);
                        mesh.TriangleIndices.Add(0);
                        mesh.TriangleIndices.Add(3);
                        mesh.TriangleIndices.Add(2);
                        mesh.TextureCoordinates = new PointCollection { 
                            new Point(1, 0),
                            new Point(0, 0), 
                            new Point(1, 1), 
                            new Point(0, 1) 
                        };
                        viewport.Geometry = mesh;
                        DiffuseMaterial material = new DiffuseMaterial(Brushes.LightGray);
                        Viewport2DVisual3D.SetIsVisualHostMaterial(material, true);
                        viewport.Material = material;
                        Label textBlock = new Label();
                        textBlock.Content = "ID:" + entityWrappers[i].powerEntity.Id + "\nIme:" + entityWrappers[i].powerEntity.Name + "\nTip:" + entityWrappers[i].entityType.ToString();
                        textBlock.FontSize = 30;
                        textBlock.Foreground = Brushes.White;
                        textBlock.Background = Brushes.Gray;
                        viewport.Visual = textBlock;
                        rotateTransform.Rotation = axisAngleRotation;
                        axisAngleRotation.Axis = rotate.Axis;
                        axisAngleRotation.Angle = rotate.Angle;
                        viewport.Transform = rotateTransform;
                        if (!viewport1.Children.Contains(viewport)) 
                            viewport1.Children.Add(viewport);

                        return HitTestResultBehavior.Stop;
                    }
                }
                for (int i = 0; i < connectors.Count; i++)
                {
                    if (connectors[i] == rayResult.ModelHit)
                    {
                        for(int j = 0; j < entities.Count; j++)
                        {
                            if (entityWrappers[j].powerEntity.Id == lines[i].FirstEnd)
                            {
                                lit1 = entities[j];
                            }

                            if (entityWrappers[j].powerEntity.Id == lines[i].SecondEnd)
                            {
                                lit2 = entities[j];
                            }
                        }

                        oldmat1 = lit1.Material as DiffuseMaterial;
                        oldmat2 = lit2.Material as DiffuseMaterial;

                        lit1.Material = new DiffuseMaterial(Brushes.Green);
                        lit2.Material = new DiffuseMaterial(Brushes.Green);

                        return HitTestResultBehavior.Stop;
                    }
                }
            }

            return HitTestResultBehavior.Stop;
        }

        private void viewport1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            viewport1.ReleaseMouseCapture();
        }
    }
}
