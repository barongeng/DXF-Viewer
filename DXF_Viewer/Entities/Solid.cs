﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using DXF.Extensions;
using DXF.Viewer.Model;
using DXF.Viewer.Util;

namespace DXF.Viewer.Entities
{
    class Solid : Entity
    {
        Point[] vertices = new Point[4];

        public Solid(Schematic drawing, Viewer viewer)
            : base(drawing, viewer)
        {
        }

        public override Entity parse(List<string> section)
        {
            gatherCodes(section);
            getCommonCodes();

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].X = getCode(" 1" + i, vertices[i].X);
                vertices[i].Y = getCode(" 2" + i, vertices[i].Y);
            }
           
            return this;
        }

        public override Path draw()
        {
            Path path = new Path();
            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure();
            PathSegmentCollection group = new PathSegmentCollection();

            //reverse 3, 2 because ordering of vertices is different in WPF
            group.Add(new LineSegment(ViewerHelper.mapToWPF(vertices[1], parent), true));
            group.Add(new LineSegment(ViewerHelper.mapToWPF(vertices[3], parent), true));
            group.Add(new LineSegment(ViewerHelper.mapToWPF(vertices[2], parent), true));
            group.Add(new LineSegment(ViewerHelper.mapToWPF(vertices[0], parent), true));

            figure.IsFilled = true;
            figure.StartPoint = ViewerHelper.mapToWPF(vertices[0], parent);
            figure.Segments = group;

            geometry.Figures.Add(figure);

            path.Data = geometry;
            path.Stroke = new SolidColorBrush(ViewerHelper.getColor(layer.lineColor));
            path.Fill = new SolidColorBrush(ViewerHelper.getColor(layer.lineColor));
            path.StrokeThickness = 0;

            return path;
        }

        public override Path draw(Insert insert)
        {
            //Add the offset from the composite entity
            for(int i = 0; i < vertices.Length; i++)
            {
                vertices[i].X += insert.anchor.X;
                vertices[i].Y += insert.anchor.Y;
            }
            //Draw the offset entity
            Path path = this.draw();
            //Revert the offset from the composite entity
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].X -= insert.anchor.X;
                vertices[i].Y -= insert.anchor.Y;
            }

            //Apply block level scale and rotation transforms
            path.RenderTransform = insert.getTransforms(path.RenderTransform);

            //return the offset entity leaving the object unaltered
            return path;
        }
    }
}
