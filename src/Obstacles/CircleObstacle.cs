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
using CocosSharp;
using CocosSharpSteer.Helpers;

namespace CocosSharpSteer.Obstacles
{
    /// <summary>
    /// CircleObstacle a simple concrete type of obstacle.
    /// </summary>
    public class CircleObstacle : IObstacle
    {
        public float Radius;
        public CCVector2 Center;

        public CircleObstacle()
            : this(1, CCVector2.Zero)
        {
        }

        public CircleObstacle(float r, CCVector2 c)
        {
            Radius = r;
            Center = c;
        }

        // xxx couldn't this be made more compact using localizePosition?
        /// <summary>
        /// Checks for intersection of the given spherical obstacle with a
        /// volume of "likely future vehicle positions": a cylinder along the
        /// current path, extending minTimeToCollision seconds along the
        /// forward axis from current position.
        ///
        /// If they intersect, a collision is imminent and this function returns
        /// a steering force pointing laterally away from the obstacle's center.
        ///
        /// Returns a zero vector if the obstacle is outside the cylinder
        /// </summary>
        /// <param name="v"></param>
        /// <param name="minTimeToCollision"></param>
        /// <returns></returns>
        public CCVector2 SteerToAvoid(IVehicle v, float minTimeToCollision)
        {
            // Capsule x Sphere collision detection
            //http://www.altdev.co/2011/04/26/more-collision-detection-for-dummies/

            var capStart = v.Position;
            var capEnd = v.PredictFuturePosition(minTimeToCollision);

            var alongCap = capEnd - capStart;
            var capLength = alongCap.Length();

            //If the vehicle is going very slowly then simply test vehicle sphere against obstacle sphere
            if (capLength <= 0.05)
            {
                var distance = CCVector2.Distance(Center, v.Position);
                if (distance < Radius + v.Radius)
                    return (v.Position - Center);
                return CCVector2.Zero;
            }

            var capAxis = alongCap / capLength;

            //Project vector onto capsule axis
            var b = CCMathHelper.Clamp(CCVector2.Dot(Center - capStart, capAxis), 0, capLength);

            //Get point on axis (closest point to sphere center)
            var r = capStart + capAxis * b;

            //Get distance from circle center to closest point
            var dist = CCVector2.Distance(Center, r);

            //Basic sphere sphere collision about the closest point
            var inCircle = dist < Radius + v.Radius;
            if (!inCircle)
                return CCVector2.Zero;

            //avoidance vector calculation
            CCVector2 avoidance = CCVector2Helpers.PerpendicularComponent(v.Position - Center, v.Forward);
            //if no avoidance was calculated this is because the vehicle is moving exactly forward towards the sphere, add in some random sideways deflection
            if (avoidance == CCVector2.Zero)
                avoidance = -v.Forward + v.Side * 3.01f * RandomHelpers.Random();

            avoidance.Normalize();
            avoidance *= v.MaxForce;
            avoidance += v.Forward * v.MaxForce * 0.75f;
            return avoidance;
        }

        public float? NextIntersection(IVehicle vehicle)
        {
            // This routine is based on Mat Buckland's book:
            // Programming Game AI by Example
            // Chapter 3 - Obstacle Avoidance - Finding the closest intersection point.

            // find "local center" (lc) of sphere in boid's coordinate space
            CCVector2 lc = vehicle.LocalizePosition(Center);

            //if the local position has a negative x value then it must lay
            //behind the agent. (in which case it can be ignored)
            if (lc.X >= 0)
            {
                //if the distance from the x axis to the object's position is less
                //than its radius + half the width of the detection box then there
                //is a potential intersection.
                var ExpandedRadius = Radius + vehicle.Radius;

                if ((float)Math.Abs(lc.Y) < ExpandedRadius)
                {
                    //now to do a line/circle intersection test. The center of the
                    //circle is represented by (cX, cY). The intersection points are
                    //given by the formula x = cX +/-sqrt(r^2-cY^2) for y=0.
                    //We only need to look at the smallest positive value of x because
                    //that will be the closest point of intersection.
                    var cX = lc.X;
                    var cY = lc.Y;

                    //we only need to calculate the sqrt part of the above equation once
                    var SqrtPart = (float)Math.Sqrt(ExpandedRadius * ExpandedRadius - cY * cY);
                    var ip = cX - SqrtPart;
                    if (ip <= 0)
                    {
                        ip = cX + SqrtPart;
                    }

                    return ip;
                }

            }
            return null;

        }

    }
}
