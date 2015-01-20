// Copyright (c) 2002-2003, Sony Computer Entertainment America
// Copyright (c) 2002-2003, Craig Reynolds <craig_reynolds@playstation.sony.com>
// Copyright (C) 2007 Bjoern Graf <bjoern.graf@gmx.net>
// Copyright (C) 2007 Michael Coles <michael@digini.com>
// All rights reserved.
//
// This software is licensed as described in the file license.txt, which
// you should have received as part of this distribution. The terms
// are also available at http://www.codeplex.com/SharpSteer/Project/License.aspx.

using System;
using System.Collections.Generic;
using CocosSharp;
using CocosSharpSteer;
using CocosSharpSteer.Helpers;

namespace SteeringDemo
{
    public struct TextEntry
    {
        public CCColor4B CCColor4B;
        public CCVector2 Position;
        public String Text;
    }

    public class Drawing
    {
        static CCColor4B _curColor;
        static readonly LocalSpace _localSpace = new LocalSpace();

        static void SetColor(CCColor4B color)
        {
            _curColor = color;
            CCDrawingPrimitives.DrawColor = color;
        }

        static void BeginDoubleSidedDrawing()
        {
            //HACK
            //cullMode = game.graphics.GraphicsDevice.RasterizerState.CullMode;
            //game.graphics.GraphicsDevice.RasterizerState.CullMode = CullMode.None;
        }

        static void EndDoubleSidedDrawing()
        {
            //game.graphics.GraphicsDevice.RasterizerState.CullMode = cullMode;
        }

        public static void iDrawLine(CCVector2 startPoint, CCVector2 endPoint, CCColor4B color)
        {

            CCDrawingPrimitives.Begin();
            SetColor(color);
            CCDrawingPrimitives.LineWidth = 2;
            CCDrawingPrimitives.DrawLine(startPoint, endPoint);
            CCDrawingPrimitives.End();
        }

        static void iDrawTriangle(CCVector2 a, CCVector2 b, CCVector2 c, CCColor4B color)
        {
            SetColor(color);
            CCDrawingPrimitives.Begin();
            CCDrawingPrimitives.DrawSolidPoly(new CCPoint[] { a, b, c }, color);
            CCDrawingPrimitives.End();
        }

        // Draw a single OpenGL quadrangle given four CCVector2 vertices, and color.
        static void iDrawQuadrangle(CCVector2 a, CCVector2 b, CCVector2 c, CCVector2 d, CCColor4B color)
        {
            SetColor(color);
            CCDrawingPrimitives.Begin();
            CCDrawingPrimitives.DrawSolidPoly(new CCPoint[] { a, b, c, d }, color);
            CCDrawingPrimitives.End();

        }

        // draw a line with alpha blending
        public void Line(CCVector2 startPoint, CCVector2 endPoint, CCColor4B color, float alpha = 1)
        {
            DrawLineAlpha(startPoint, endPoint, color, alpha);
        }

        public void CircleOrDisk(float radius, CCVector2 axis, CCVector2 center, CCColor4B color, int segments, bool filled, bool in3D)
        {
            DrawCircleOrDisk(radius, axis, center, color, segments, filled, in3D);
        }

        public static void DrawLine(CCVector2 startPoint, CCVector2 endPoint, CCColor4B color)
        {
            if (GameLayer.IsDrawPhase)
            {
                iDrawLine(startPoint, endPoint, color);
            }
            else
            {
                DeferredLine.AddToBuffer(startPoint, endPoint, color);
            }
        }

        // draw a line with alpha blending
        public static void DrawLineAlpha(CCVector2 startPoint, CCVector2 endPoint, CCColor4B color, float alpha)
        {
            CCColor4B c = new CCColor4B(color.R, color.G, color.B, (byte)(255.0f * alpha));
            iDrawLine(startPoint, endPoint, c);
            if (GameLayer.IsDrawPhase)
            {
                iDrawLine(startPoint, endPoint, c);
            }
            else
            {
                DeferredLine.AddToBuffer(startPoint, endPoint, c);
            }
        }

        // draw 2d lines in screen space: x and y are the relevant coordinates
        public static void Draw2dLine(CCVector2 startPoint, CCVector2 endPoint, CCColor4B color)
        {
            iDrawLine(startPoint, endPoint, color);
        }

        // draws a "wide line segment": a rectangle of the given width and color
        // whose mid-line connects two given endpoints
        public static void DrawXZWideLine(CCVector2 startPoint, CCVector2 endPoint, CCColor4B color, float width)
        {
            CCVector2 offset = endPoint - startPoint;
            offset.Normalize();
            CCVector2 perp = _localSpace.LocalRotateForwardToSide(offset);
            CCVector2 radius = perp * width / 2;

            CCVector2 a = startPoint + radius;
            CCVector2 b = endPoint + radius;
            CCVector2 c = endPoint - radius;
            CCVector2 d = startPoint - radius;

            iDrawQuadrangle(a, b, c, d, color);
        }

        // draw a (filled-in, polygon-based) square checkerboard grid on the XZ
        // (horizontal) plane.
        //
        // ("size" is the length of a side of the overall grid, "subsquares" is the
        // number of subsquares along each edge (for example a standard checkboard
        // has eight), "center" is the 3d position of the center of the grid,
        // color1 and color2 are used for alternating subsquares.)
        public static void DrawXYCheckerboardGrid(float size, int subsquares, CCVector2 center, CCColor4B color1, CCColor4B color2)
        {
            
            float half = size / 2;
            float spacing = size / subsquares;

            BeginDoubleSidedDrawing();
            {
                bool flag1 = false;
                float p = -half;
                CCVector2 corner = new CCVector2();
                for (int i = 0; i < subsquares; i++)
                {
                    bool flag2 = flag1;
                    float q = -half;
                    for (int j = 0; j < subsquares; j++)
                    {
                        corner.X = p;
                        corner.Y = q;

                        corner += center;

                        if (GameLayer.IsDrawPhase)
                        {
                            iDrawQuadrangle(corner,
                                             corner + new CCVector2(spacing, 0),
                                             corner + new CCVector2(spacing, spacing),
                                             corner + new CCVector2(0, spacing),
                                             flag2 ? color1 : color2);
                        }
                        else
                        {
                            //DeferredLine.AddToBuffer(corner, corner + new CCVector2(spacing, 0), flag2 ? color1 : color2);
                        }
 
                        flag2 = !flag2;
                        q += spacing;
                    }
                    flag1 = !flag1;
                    p += spacing;
                }
            }
            EndDoubleSidedDrawing();
        }

        // draw a square grid of lines on the XZ (horizontal) plane.
        //
        // ("size" is the length of a side of the overall grid, "subsquares" is the
        // number of subsquares along each edge (for example a standard checkboard
        // has eight), "center" is the 3d position of the center of the grid, lines
        // are drawn in the specified "color".)
        public static void DrawXYLineGrid(float size, int subsquares, CCVector2 center, CCColor4B color)
        {
            float half = size / 2;
            float spacing = size / subsquares;

            // set grid drawing color
            SetColor(color);

            // draw a square XZ grid with the given size and line count
            SetColor(color);
            CCDrawingPrimitives.LineWidth = 1;
            CCDrawingPrimitives.Begin();
            float q = -half;
            for (int i = 0; i < (subsquares + 1); i++)
            {
                CCVector2 x1 = new CCVector2(q, +half); // along X parallel to Z
                CCVector2 x2 = new CCVector2(q, -half);
                CCVector2 z1 = new CCVector2(+half, q); // along Z parallel to X
                CCVector2 z2 = new CCVector2(-half, q);

                q += spacing;
            }
            CCDrawingPrimitives.End();

        }

        // draw the three axes of a LocalSpace: three lines parallel to the
        // basis vectors of the space, centered at its origin, of lengths
        // given by the coordinates of "size".
        public static void DrawAxes(ILocalSpaceBasis ls, CCVector2 size, CCColor4B color)
        {
//            CCVector2 x = new CCVector2(size.X / 2, 0, 0);
//            CCVector2 y = new CCVector2(0, size.Y / 2, 0);
//            CCVector2 z = new CCVector2(0, 0, size.Z / 2);
//
//            iDrawLine(ls.GlobalizePosition(x), ls.GlobalizePosition(x * -1), color);
//            iDrawLine(ls.GlobalizePosition(y), ls.GlobalizePosition(y * -1), color);
//            iDrawLine(ls.GlobalizePosition(z), ls.GlobalizePosition(z * -1), color);
        }

        public static void DrawQuadrangle(CCVector2 a, CCVector2 b, CCVector2 c, CCVector2 d, CCColor4B color)
        {
            iDrawQuadrangle(a, b, c, d, color);
        }

        // draw the edges of a box with a given position, orientation, size
        // and color.  The box edges are aligned with the axes of the given
        // LocalSpace, and it is centered at the origin of that LocalSpace.
        // "size" is the main diagonal of the box.
        //
        // use gGlobalSpace to draw a box aligned with global space
        public static void DrawBoxOutline(ILocalSpaceBasis localSpace, CCVector2 size, CCColor4B color)
        {
            CCVector2 s = size / 2.0f;  // half of main diagonal

            CCVector2 a = localSpace.GlobalizePosition(new CCVector2(+s.X, +s.Y));//, +s.Z));
            CCVector2 b = localSpace.GlobalizePosition(new CCVector2(+s.X, -s.Y));//, +s.Z));
            CCVector2 c = localSpace.GlobalizePosition(new CCVector2(-s.X, -s.Y));//, +s.Z));
            CCVector2 d = localSpace.GlobalizePosition(new CCVector2(-s.X, +s.Y));//, +s.Z));

            CCVector2 e = localSpace.GlobalizePosition(new CCVector2(+s.X, +s.Y));//, -s.Z));
            CCVector2 f = localSpace.GlobalizePosition(new CCVector2(+s.X, -s.Y));//, -s.Z));
            CCVector2 g = localSpace.GlobalizePosition(new CCVector2(-s.X, -s.Y));//, -s.Z));
            CCVector2 h = localSpace.GlobalizePosition(new CCVector2(-s.X, +s.Y));//, -s.Z));

            iDrawLine(a, b, color);
            iDrawLine(b, c, color);
            iDrawLine(c, d, color);
            iDrawLine(d, a, color);

            iDrawLine(a, e, color);
            iDrawLine(b, f, color);
            iDrawLine(c, g, color);
            iDrawLine(d, h, color);

            iDrawLine(e, f, color);
            iDrawLine(f, g, color);
            iDrawLine(g, h, color);
            iDrawLine(h, e, color);
        }

        public static void DrawXZCircle(float radius, CCVector2 center, CCColor4B color, int segments)
        {
            DrawXZCircleOrDisk(radius, center, color, segments, false);
        }

        public static void DrawXZDisk(float radius, CCVector2 center, CCColor4B color, int segments)
        {
            DrawXZCircleOrDisk(radius, center, color, segments, true);
        }

        // drawing utility used by both drawXZCircle and drawXZDisk
        private static void DrawXZCircleOrDisk(float radius, CCVector2 center, CCColor4B color, int segments, bool filled)
        {
            // draw a circle-or-disk on the XZ plane
            DrawCircleOrDisk(radius, CCVector2.Zero, center, color, segments, filled, false);
        }

        // a simple 2d vehicle on the XZ plane
//        public static void DrawBasic2dCircularVehicle(IVehicle vehicle, CCColor4B color)
//        {
//            // "aspect ratio" of body (as seen from above)
//            const float x = 0.5f;
//            float y = (float)Math.Sqrt(1 - (x * x));

//            // radius and position of vehicle
//            float r = vehicle.Radius;
//            CCVector2 p = vehicle.Position;

//            // shape of triangular body
//            CCVector2 u = new CCVector2(0, 1) * r * 0.05f; // slightly up
//            CCVector2 f = new CCVector2(0, -1) * r;
//            CCVector2 s = new CCVector2(1, 0) * x * r;
//            CCVector2 b = new CCVector2(0, -1) * -y * r;

//            var matrix = vehicle.ToMatrix();

////            // draw double-sided triangle (that is: no (back) face culling)
////            BeginDoubleSidedDrawing();
//            iDrawTriangle(CCVector2.Transform(f + u, matrix),
//                          CCVector2.Transform(b - s + u, matrix),
//                          CCVector2.Transform(b + s + u, matrix),
//                          color);
////            EndDoubleSidedDrawing();
////

//            // draw the circular collision boundary
//            DrawXZCircle(r, p, CCColor4B.White, 20);

//        }

        // a simple 3d vehicle
        public static void DrawBasic3dSphericalVehicle(IVehicle vehicle, CCColor4B color)
        {
//            CCVector2 vCCColor4B = new CCVector2(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
//
//            // "aspect ratio" of body (as seen from above)
//            const float x = 0.5f;
//            float y = (float)Math.Sqrt(1 - (x * x));
//
//            // radius and position of vehicle
//            float r = vehicle.Radius;
//            CCVector2 p = vehicle.Position;
//
//            // body shape parameters
//            CCVector2 f = vehicle.Forward * r;
//            CCVector2 s = vehicle.Side * r * x;
//            CCVector2 u = vehicle.Up * r * x * 0.5f;
//            CCVector2 b = vehicle.Forward * r * -y;
//
//            // vertex positions
//            CCVector2 nose = p + f;
//            CCVector2 side1 = p + b - s;
//            CCVector2 side2 = p + b + s;
//            CCVector2 top = p + b + u;
//            CCVector2 bottom = p + b - u;

            // colors
            const float j = +0.05f;
            const float k = -0.05f;
//            CCColor4B color1 = new CCColor4B(vCCColor4B + new CCVector2(j, j, k));
//            CCColor4B color2 = new CCColor4B(vCCColor4B + new CCVector2(j, k, j));
//            CCColor4B color3 = new CCColor4B(vCCColor4B + new CCVector2(k, j, j));
//            CCColor4B color4 = new CCColor4B(vCCColor4B + new CCVector2(k, j, k));
//            CCColor4B color5 = new CCColor4B(vCCColor4B + new CCVector2(k, k, j));
//
//            // draw body
//            iDrawTriangle(nose, side1, top, color1);  // top, side 1
//            iDrawTriangle(nose, top, side2, color2);  // top, side 2
//            iDrawTriangle(nose, bottom, side1, color3);  // bottom, side 1
//            iDrawTriangle(nose, side2, bottom, color4);  // bottom, side 2
//            iDrawTriangle(side1, side2, top, color5);  // top back
//            iDrawTriangle(side2, side1, bottom, color5);  // bottom back
        }

        // a simple sphere
        public static void DrawBasic3dSphere(CCVector2 position, float radius, CCColor4B color)
        {
            //CCVector2 vCCColor4B = new CCVector2(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);

            // "aspect ratio" of body (as seen from above)
            const float x = 0.5f;
            float y = (float)Math.Sqrt(1 - (x * x));

            // radius and position of vehicle
            float r = radius;
            CCVector2 p = position;

            // body shape parameters
//            CCVector2 f = CCVector2.Forward * r;
//            CCVector2 s = CCVector2.Left * r * x;
//            CCVector2 u = CCVector2.Up * r * x;
//            CCVector2 b = CCVector2.Forward * r * -y;
//
//            // vertex positions
//            CCVector2 nose = p + f;
//            CCVector2 side1 = p + b - s;
//            CCVector2 side2 = p + b + s;
//            CCVector2 top = p + b + u;
//            CCVector2 bottom = p + b - u;
//
//            // colors
//            const float j = +0.05f;
//            const float k = -0.05f;
//            CCColor4B color1 = new CCColor4B(vCCColor4B + new CCVector2(j, j, k));
//            CCColor4B color2 = new CCColor4B(vCCColor4B + new CCVector2(j, k, j));
//            CCColor4B color3 = new CCColor4B(vCCColor4B + new CCVector2(k, j, j));
//            CCColor4B color4 = new CCColor4B(vCCColor4B + new CCVector2(k, j, k));
//            CCColor4B color5 = new CCColor4B(vCCColor4B + new CCVector2(k, k, j));
//
//            // draw body
//            iDrawTriangle(nose, side1, top, color1);  // top, side 1
//            iDrawTriangle(nose, top, side2, color2);  // top, side 2
//            iDrawTriangle(nose, bottom, side1, color3);  // bottom, side 1
//            iDrawTriangle(nose, side2, bottom, color4);  // bottom, side 2
//            iDrawTriangle(side1, side2, top, color5);  // top back
//            iDrawTriangle(side2, side1, bottom, color5);  // bottom back
        }

        // General purpose circle/disk drawing routine.  Draws circles or disks (as
        // specified by "filled" argument) and handles both special case 2d circles
        // on the XZ plane or arbitrary circles in 3d space (as specified by "in3d"
        // argument)
        public static void DrawCircleOrDisk(float radius, CCVector2 axis, CCVector2 center, CCColor4B color, int segments, bool filled, bool in3D)
        {
            if (GameLayer.IsDrawPhase)
            {

                // set drawing color
                SetColor(color);
                CCDrawingPrimitives.LineWidth = 1;
                CCDrawingPrimitives.Begin();

                if (filled)
                    CCDrawingPrimitives.DrawSolidCircle(center, radius, 0, segments);
                else
                    CCDrawingPrimitives.DrawCircle(center, radius, 0, segments, false);

                CCDrawingPrimitives.End();

            }
            else
            {
                DeferredCircle.AddToBuffer(radius, axis, center, color, segments, filled, in3D);
            }
        }

        public static void Draw3dCircleOrDisk(float radius, CCVector2 center, CCVector2 axis, CCColor4B color, int segments, bool filled)
        {
            // draw a circle-or-disk in the given local space
            DrawCircleOrDisk(radius, axis, center, color, segments, filled, true);
        }

        public static void Draw3dCircle(float radius, CCVector2 center, CCVector2 axis, CCColor4B color, int segments)
        {
            Draw3dCircleOrDisk(radius, center, axis, color, segments, false);
        }

        public static void AllDeferredLines()
        {
            DeferredLine.DrawAll();
        }

        public static void AllDeferredCirclesOrDisks()
        {
            DeferredCircle.DrawAll();
        }

        public static void Draw2dTextAt3dLocation(String text, CCVector2 location, CCColor4B color)
        {
            // XXX NOTE: "it would be nice if" this had a 2d screenspace offset for
            // the origin of the text relative to the screen space projection of
            // the 3d point.

            // set text color and raster position
            //CCVector2 p = Game.Graphics.GraphicsDevice.Viewport.Project(location, Game.ProjectionMatrix, Game.ViewMatrix, Game.WorldMatrix);
            //TextEntry textEntry = new TextEntry { CCColor4B = color, Position = new CCVector2(p.X, p.Y), Text = text };
            //Game.AddText(textEntry);
        }

        public static void Draw2dTextAt2dLocation(String text, CCVector2 location, CCColor4B color)
        {
            // set text color and raster position
            TextEntry textEntry = new TextEntry { CCColor4B = color, Position = new CCVector2(location.X, location.Y), Text = text };
            Console.WriteLine(textEntry.Text);
            //Game.AddText(textEntry);
        }

        public static void UpdateStatus (string text, CCColor4B color)
        {
            GameLayer.statusLabel.Text = text;
            GameLayer.statusLabel.Color = new CCColor3B(color);
        }


        public static float GetWindowWidth()
        {
            return 1024;
        }

        public static float GetWindowHeight()
        {
            return 640;
        }
    }
}
