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

namespace CocosSharpSteer.Helpers
{
    public static class CCVector2Helpers
    {
        /// <summary>
        /// return component of vector parallel to a unit basis vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="unitBasis">A unit length basis vector</param>
        /// <returns></returns>
        public static CCVector2 ParallelComponent(CCVector2 vector, CCVector2 unitBasis)
        {
            float projection = CCVector2.Dot(vector, unitBasis);
            return unitBasis * projection;
        }

        /// <summary>
        /// return component of vector perpendicular to a unit basis vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="unitBasis">A unit length basis vector</param>
        /// <returns></returns>
        public static CCVector2 PerpendicularComponent(CCVector2 vector, CCVector2 unitBasis)
        {
            return (vector - ParallelComponent(vector, unitBasis));
        }

        /// <summary>
        /// clamps the length of a given vector to maxLength.  If the vector is
        /// shorter its value is returned unaltered, if the vector is longer
        /// the value returned has length of maxLength and is parallel to the
        /// original input.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static CCVector2 TruncateLength(this CCVector2 vector, float maxLength)
        {
            float maxLengthSquared = maxLength * maxLength;
            float vecLengthSquared = vector.LengthSquared();
            if (vecLengthSquared <= maxLengthSquared)
                return vector;

            return (vector * (maxLength / (float)Math.Sqrt(vecLengthSquared)));
        }

        /// <summary>
        /// rotate this vector about the global Y (up) axis by the given angle
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="radians"></param>
        /// <returns></returns>
//        public static CCVector2 RotateAboutGlobalY(this CCVector2 vector, float radians)
//        {
//            float s = 0;
//            float c = 0;
//            return RotateAboutGlobalY(vector, radians, ref s, ref c);
//        }
//
//        /// <summary>
//        /// Rotate this vector about the global Y (up) axis by the given angle
//        /// </summary>
//        /// <param name="vector"></param>
//        /// <param name="radians"></param>
//        /// <param name="sin">Either Sin(radians) or default(float), if default(float) this value will be initialized with Sin(radians)</param>
//        /// <param name="cos">Either Cos(radians) or default(float), if default(float) this value will be initialized with Cos(radians)</param>
//        /// <returns></returns>
//        public static CCVector2 RotateAboutGlobalY(this CCVector2 vector, float radians, ref float sin, ref float cos)
//        {
//            // if both are default, they have not been initialized yet
//// ReSharper disable CompareOfFloatsByEqualityOperator
//            if (sin == default(float) && cos == default(float))
//// ReSharper restore CompareOfFloatsByEqualityOperator
//            {
//                sin = (float)Math.Sin(radians);
//                cos = (float)Math.Cos(radians);
//            }
//            return new CCVector2((vector.X * cos) + (vector.Z * sin), vector.Y);//, (vector.Z * cos) - (vector.X * sin));
//        }
//
        ///////////////////////////////////////////////////////////////////////////////
        //treats a window as a toroid
        public static CCVector2 WrapAround(this CCVector2 pos, int MaxX, int MaxY) {
            if (pos.X > MaxX) {
                pos.X = 0.0f;
            }

            if (pos.X < 0) {
                pos.X = MaxX;
            }

            if (pos.Y < 0) {
                pos.Y = MaxY;
            }

            if (pos.Y > MaxY) {
                pos.Y = 0.0f;
            }
            return pos;
        }

        /// <summary>
        /// Wrap a position around so it is always within 1 radius of the sphere (keeps repeating wrapping until position is within sphere)
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static CCVector2 SphericalWrapAround(this CCVector2 vector, CCVector2 center, float radius)
        {
            float r;
            do
            {
                CCVector2 offset = vector - center;
                r = offset.Length();

                if (r > radius)
                    vector = vector + ((offset / r) * radius * -2);

            } while (r > radius);

            return vector;
        }

        /// <summary>
        /// Returns a position randomly distributed on a disk of unit radius
        /// on the XZ (Y=0) plane, centered at the origin.  Orientation will be
        /// random and length will range between 0 and 1
        /// </summary>
        /// <returns></returns>
//        public static CCVector2 RandomVectorOnUnitRadiusXZDisk()
//        {
//            CCVector2 v;
//            do
//            {
//                v.X = (RandomHelpers.Random() * 2) - 1;
//                v.Y = 0;
//                v.Z = (RandomHelpers.Random() * 2) - 1;
//            }
//            while (v.Length() >= 1);
//
//            return v;
//        }

        /// <summary>
        /// Returns a position randomly distributed on a disk of unit radius
        /// on the XZ (Y=0) plane, centered at the origin.  Orientation will be
        /// random and length will range between 0 and 1
        /// </summary>
        /// <returns></returns>
        public static CCVector2 RandomVectorOnUnitRadiusXYDisk()
        {
            CCVector2 v;
            do
            {
                v.X = (RandomHelpers.Random() * 2) - 1;
                v.Y = (RandomHelpers.Random() * 2) - 1;
            }
            while (v.Length() >= 1);

            return v;
        }

        /// <summary>
        /// Returns a position randomly distributed inside a sphere of unit radius
        /// centered at the origin.  Orientation will be random and length will range
        /// between 0 and 1
        /// </summary>
        /// <returns></returns>
//        public static CCVector2 RandomVectorInUnitRadiusSphere()
//        {
//            CCVector2 v = new CCVector2();
//            do
//            {
//                v.X = (RandomHelpers.Random() * 2) - 1;
//                v.Y = (RandomHelpers.Random() * 2) - 1;
//                v.Z = (RandomHelpers.Random() * 2) - 1;
//            }
//            while (v.Length() >= 1);
//
//            return v;
//        }
        /// <summary>
        /// Returns a position randomly distributed inside a Circle of unit radius
        /// centered at the origin.  Orientation will be random and length will range
        /// between 0 and 1
        /// </summary>
        /// <returns></returns>
        public static CCVector2 RandomVectorInUnitRadiusCircle()
        {
            CCVector2 v = new CCVector2();
            do
            {
                v.X = (RandomHelpers.Random() * 20) - 1;
                v.Y = (RandomHelpers.Random() * 30) - 1;
            }
            while (v.Length() >= 1);

            return v;
        }

        /// <summary>
        /// Returns a position randomly distributed on the surface of a sphere
        /// of unit radius centered at the origin.  Orientation will be random
        /// and length will be 1
        /// </summary>
        /// <returns></returns>
        public static CCVector2 RandomUnitVector()
        {
            CCVector2 temp = RandomVectorInUnitRadiusCircle();
            temp.Normalize();

            return temp;
        }

        /// <summary>
        /// Returns a position randomly distributed on a circle of unit radius
        /// on the XZ (Y=0) plane, centered at the origin.  Orientation will be
        /// random and length will be 1
        /// </summary>
        /// <returns></returns>
//        public static CCVector2 RandomUnitVectorOnXZPlane()
//        {
//            CCVector2 temp = RandomVectorInUnitRadiusSphere();
//
//            temp.Y = 0;
//            temp.Normalize();
//
//            return temp;
//        }
        /// <summary>
        /// Returns a position randomly distributed on a circle of unit radius
        /// on the XZ (Y=0) plane, centered at the origin.  Orientation will be
        /// random and length will be 1
        /// </summary>
        /// <returns></returns>
        public static CCVector2 RandomUnitVectorOnXYPlane()
        {
            CCVector2 temp = RandomVectorInUnitRadiusCircle();

            //temp.Y = 0;
            temp.Normalize();

            return temp;
        }

        /// <summary>
        /// Clip a vector to be within the given cone
        /// </summary>
        /// <param name="source">A vector to clip</param>
        /// <param name="cosineOfConeAngle">The cosine of the cone angle</param>
        /// <param name="basis">The vector along the middle of the cone</param>
        /// <returns></returns>
        public static CCVector2 LimitMaxDeviationAngle(this CCVector2 source, float cosineOfConeAngle, CCVector2 basis)
        {
            return LimitDeviationAngleUtility(true, // force source INSIDE cone
                source, cosineOfConeAngle, basis);
        }

        /// <summary>
        /// Clip a vector to be outside the given cone
        /// </summary>
        /// <param name="source">A vector to clip</param>
        /// <param name="cosineOfConeAngle">The cosine of the cone angle</param>
        /// <param name="basis">The vector along the middle of the cone</param>
        /// <returns></returns>
        public static CCVector2 LimitMinDeviationAngle(this CCVector2 source, float cosineOfConeAngle, CCVector2 basis)
        {
            return LimitDeviationAngleUtility(false, // force source OUTSIDE cone
                source, cosineOfConeAngle, basis);
        }

        /// <summary>
        /// used by limitMaxDeviationAngle / limitMinDeviationAngle
        /// </summary>
        /// <param name="insideOrOutside"></param>
        /// <param name="source"></param>
        /// <param name="cosineOfConeAngle"></param>
        /// <param name="basis"></param>
        /// <returns></returns>
        private static CCVector2 LimitDeviationAngleUtility(bool insideOrOutside, CCVector2 source, float cosineOfConeAngle, CCVector2 basis)
        {
            // immediately return zero length input vectors
            float sourceLength = source.Length();
            if (sourceLength < float.Epsilon)
                return source;

            // measure the angular diviation of "source" from "basis"
            CCVector2 direction = source / sourceLength;

            float cosineOfSourceAngle = CCVector2.Dot(direction, basis);

            // Simply return "source" if it already meets the angle criteria.
            // (note: we hope this top "if" gets compiled out since the flag
            // is a constant when the function is inlined into its caller)
            if (insideOrOutside)
            {
                // source vector is already inside the cone, just return it
                if (cosineOfSourceAngle >= cosineOfConeAngle)
                    return source;
            }
            else if (cosineOfSourceAngle <= cosineOfConeAngle)
                return source;

            // find the portion of "source" that is perpendicular to "basis"
            CCVector2 perp = PerpendicularComponent(source, basis);
            if (perp == CCVector2.Zero)
                return CCVector2.Zero;

            // normalize that perpendicular
            CCVector2 unitPerp = perp;
            unitPerp.Normalize();

            // construct a new vector whose length equals the source vector,
            // and lies on the intersection of a plane (formed the source and
            // basis vectors) and a cone (whose axis is "basis" and whose
            // angle corresponds to cosineOfConeAngle)
            float perpDist = (float)Math.Sqrt(1 - (cosineOfConeAngle * cosineOfConeAngle));
            CCVector2 c0 = basis * cosineOfConeAngle;
            CCVector2 c1 = unitPerp * perpDist;
            return (c0 + c1) * sourceLength;
        }

        /// <summary>
        /// Returns the distance between a point and a line.
        /// </summary>
        /// <param name="point">The point to measure distance to</param>
        /// <param name="lineOrigin">A point on the line</param>
        /// <param name="lineUnitTangent">A UNIT vector parallel to the line</param>
        /// <returns></returns>
        public static float DistanceFromLine(this CCVector2 point, CCVector2 lineOrigin, CCVector2 lineUnitTangent)
        {
            CCVector2 offset = point - lineOrigin;
            CCVector2 perp = PerpendicularComponent(offset, lineUnitTangent);
            return perp.Length();
        }

        /// <summary>
        /// Calculates the perpendicular line of v1 and v2 offset by offset
        /// </summary>
        /// <param name="v1">Start vector</param>
        /// <param name="v2">End vector</param>
        /// <param name="offset">Offset amount</param>
        /// <param name="p1">Perpendicular offset of Start vector</param>
        /// <param name="p2">Perpendicular offset of End vector</param>
        public static void CalculatePerpLine (CCVector2 v1, CCVector2 v2,float offset, ref CCVector2 p1, ref CCVector2 p2)
        {
            var x1 = v1.X;
            var x2 = v2.X;
            var y1 = v1.Y;
            var y2 = v2.Y;
            var length = (float)Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));

            // This is the second line
            p1.X = x1 + offset * (y2-y1) / length;
            p2.X = x2 + offset * (y2-y1) / length;
            p1.Y = y1 + offset * (x1-x2) / length;
            p2.Y = y2 + offset * (x1-x2) / length;
        }


        /// <summary>
        /// Find any arbitrary vector which is perpendicular to the given vector
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
//        public static CCVector2 FindPerpendicularIn3d(this CCVector2 direction)
//        {
//            // to be filled in:
//            CCVector2 quasiPerp;  // a direction which is "almost perpendicular"
//            CCVector2 result;     // the computed perpendicular to be returned
//
//            // three mutually perpendicular basis vectors
//            CCVector2 i = CCVector2.Right;
//            CCVector2 j = CCVector2.Up;
//            CCVector2 k = CCVector2.Backward;
//
//            // measure the projection of "direction" onto each of the axes
//            float id = CCVector2.Dot(i, direction);
//            float jd = CCVector2.Dot(j, direction);
//            float kd = CCVector2.Dot(k, direction);
//
//            // set quasiPerp to the basis which is least parallel to "direction"
//            if ((id <= jd) && (id <= kd))
//                quasiPerp = i;           // projection onto i was the smallest
//            else if ((jd <= id) && (jd <= kd))
//                quasiPerp = j;           // projection onto j was the smallest
//            else
//                quasiPerp = k;           // projection onto k was the smallest
//
//            // return the cross product (direction x quasiPerp)
//            // which is guaranteed to be perpendicular to both of them
//            CCVector2.Cross(ref direction, ref quasiPerp, out result);
//
//            return result;
//        }
    }
}
