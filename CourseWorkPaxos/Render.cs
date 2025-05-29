using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CourseWorkPaxos
{
    class Render
    {
        static private System.Windows.Shapes.Path GetArrow(Point p1, Point p2, byte[] color, int thinckness)
        {
            if (thinckness <= 0)
            {
                return null;
            }
            GeometryGroup lineGroup = new GeometryGroup();
            double theta = Math.Atan2((p2.Y - p1.Y), (p2.X - p1.X)) * 180 / Math.PI;

            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            Point p = new Point(p1.X + ((p2.X - p1.X) / 1.35), p1.Y + ((p2.Y - p1.Y) / 1.35));
            pathFigure.StartPoint = p;

            Point lpoint = new Point(p.X + 6, p.Y + 15);
            Point rpoint = new Point(p.X - 6, p.Y + 15);
            LineSegment seg1 = new LineSegment();
            seg1.Point = lpoint;
            pathFigure.Segments.Add(seg1);

            LineSegment seg2 = new LineSegment();
            seg2.Point = rpoint;
            pathFigure.Segments.Add(seg2);

            LineSegment seg3 = new LineSegment();
            seg3.Point = p;
            pathFigure.Segments.Add(seg3);

            pathGeometry.Figures.Add(pathFigure);
            RotateTransform transform = new RotateTransform();
            transform.Angle = theta + 90;
            transform.CenterX = p.X;
            transform.CenterY = p.Y;
            pathGeometry.Transform = transform;
            lineGroup.Children.Add(pathGeometry);

            LineGeometry connectorGeometry = new LineGeometry();
            connectorGeometry.StartPoint = p1;
            connectorGeometry.EndPoint = p2;
            lineGroup.Children.Add(connectorGeometry);
            System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
            path.Data = lineGroup;
            path.StrokeThickness = thinckness * 2;
            path.Stroke = path.Fill = new SolidColorBrush(Color.FromRgb(color[0], color[1], color[2]));
            return (path);
        }

        public static void render(UIElementCollection uis, Canvas canvas)
        {
            List<UIElement> filter = new List<UIElement>();
            foreach (UIElement it in uis)
            {
                if (it is System.Windows.Shapes.Path)
                {
                    filter.Add(it);
                }
            }
            foreach (UIElement it in filter)
            {
                uis.Remove(it);
            }
            List<System.Windows.Shapes.Path> paths = new List<System.Windows.Shapes.Path>();
            int curOdd = 1;
            foreach (UIElement it in uis)
            {
                if (it is Ellipse)
                {
                    var cur = (it as Ellipse);
                    if (cur.Name == "Example1" || cur.Name == "Example2")
                    {
                        continue;
                    }
                    curOdd = 0 - curOdd;
                    int curOffset = curOdd * 10;
                    var rm = (RM)cur.Tag;
                    cur.Stroke = new SolidColorBrush(Color.FromRgb(rm.rgb[0], rm.rgb[1], rm.rgb[2])); ;
                    cur.SetValue(Canvas.LeftProperty, (double)rm.x);
                    cur.SetValue(Canvas.TopProperty, (double)rm.y);
                    foreach (var i in rm.connectedRms)
                    {
                        var arrow = GetArrow(new Point(i.Key.x + curOffset, i.Key.y + curOffset),
                            new Point(rm.x + curOffset, rm.y + curOffset), new byte[] { 0, 0, 100 },
                             Paxos.ArrowMaxThiness() - (int)(Paxos.GetTime() - (int)i.Value) / Paxos.ArrowThinessTimeSlide());
                        if (arrow != null)
                        {
                            paths.Add(arrow);
                        }
                    }
                }
                else if (it is Rectangle)
                {

                    var cur = (it as Rectangle);
                    if (cur.Name == "Example1" || cur.Name == "Example2")
                    {
                        continue;
                    }
                    curOdd = 0 - curOdd;
                    int curOffset = curOdd * 10;
                    var rm = (RM)cur.Tag;
                    cur.Stroke = new SolidColorBrush(Color.FromRgb(rm.rgb[0], rm.rgb[1], rm.rgb[2])); ;
                    cur.SetValue(Canvas.LeftProperty, (double)rm.x);
                    cur.SetValue(Canvas.TopProperty, (double)rm.y);
                    foreach (var i in rm.connectedRms)
                    {
                        var arrow = GetArrow(new Point(i.Key.x + curOffset, i.Key.y + curOffset),
                            new Point(rm.x + curOffset, rm.y + curOffset), new byte[] { 100, 0, 0 },
                            Paxos.ArrowMaxThiness() - (int)(Paxos.GetTime() - (int)i.Value) / Paxos.ArrowThinessTimeSlide());
                        if (arrow != null)
                        {
                            paths.Add(arrow);
                        }
                    }
                }
            }
            foreach (System.Windows.Shapes.Path it in paths)
            {
                canvas.Children.Add(it);
            }
        }
    }

}
