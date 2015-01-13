using System;
using CocosSharp;

namespace CocosSharpSteer.Helpers
{
    public static class LocalSpaceBasisHelpers
    {
        /// <summary>
        /// Transforms a direction in global space to its equivalent in local space.
        /// </summary>
        /// <param name="basis">The basis which this should operate on</param>
        /// <param name="globalDirection">The global space direction to transform.</param>
        /// <returns>The global space direction transformed to local space .</returns>
        public static CCVector2 LocalizeDirection(this ILocalSpaceBasis basis, CCVector2 globalDirection)
        {
            // dot offset with local basis vectors to obtain local coordiantes
            return new CCVector2(CCVector2.Dot(globalDirection, basis.Forward), 
                CCVector2.Dot(globalDirection, basis.Side));//, CCVector2.Dot(globalDirection, basis.Forward));
        }

        /// <summary>
        /// Transforms a point in global space to its equivalent in local space.
        /// </summary>
        /// <param name="basis">The basis which this should operate on</param>
        /// <param name="globalPosition">The global space position to transform.</param>
        /// <returns>The global space position transformed to local space.</returns>
        public static CCVector2 LocalizePosition(this ILocalSpaceBasis basis, CCVector2 globalPosition)
        {
            // global offset from local origin
            CCVector2 globalOffset = globalPosition - basis.Position;

            // dot offset with local basis vectors to obtain local coordiantes
            return LocalizeDirection(basis, globalOffset);
        }

        /// <summary>
        /// Transforms a point in local space to its equivalent in global space.
        /// </summary>
        /// <param name="basis">The basis which this should operate on</param>
        /// <param name="localPosition">The local space position to tranform.</param>
        /// <returns>The local space position transformed to global space.</returns>
        public static CCVector2 GlobalizePosition(this ILocalSpaceBasis basis, CCVector2 localPosition)
        {
            return basis.Position + GlobalizeDirection(basis, localPosition);
        }

        /// <summary>
        /// Transforms a direction in local space to its equivalent in global space.
        /// </summary>
        /// <param name="basis">The basis which this should operate on</param>
        /// <param name="localDirection">The local space direction to tranform.</param>
        /// <returns>The local space direction transformed to global space</returns>
        public static CCVector2 GlobalizeDirection(this ILocalSpaceBasis basis, CCVector2 localDirection)
        {
            return ((basis.Forward * localDirection.X) +
            (basis.Side * localDirection.Y));
                    //(basis.Forward * localDirection.Z));
        }

        /// <summary>
        /// Rotates, in the canonical direction, a vector pointing in the
        /// "forward" (+Z) direction to the "side" (+/-X) direction as implied
        /// by IsRightHanded.
        /// </summary>
        /// <param name="basis">The basis which this should operate on</param>
        /// <param name="value">The local space vector.</param>
        /// <returns>The rotated vector.</returns>
        public static CCVector2 LocalRotateForwardToSide(this ILocalSpaceBasis basis, CCVector2 value)
        {
            return CCVector2.PerpendicularCCW(value);//CCVector2.Zero;//   new CCVector2(-value.Z, value.Y, value.X);
        }

        public static void ResetLocalSpace(out CCVector2 forward, out CCVector2 side, out CCVector2 up, out CCVector2 position)
        {
            forward = CCVector2.Zero;//CCVector2.Forward;
            side = new CCVector2(-1, 0);
            up = CCVector2.UnitY;
            position = CCVector2.Zero;
        }

        /// <summary>
        /// set "side" basis vector to normalized cross product of forward and up
        /// </summary>
        /// <param name="forward"></param>
        /// <param name="side"></param>
        /// <param name="up"></param>
        public static void SetUnitSideFromForward(ref CCVector2 forward, out CCVector2 side, ref CCVector2 up)
        {
            // derive new unit side basis vector from forward and up
            side = CCVector2.Normalize(CCVector2.PerpendicularCCW(forward));//CCVector2.Normalize(CCVector2.CrossProduct(forward, up));

        }

        /// <summary>
        /// regenerate the orthonormal basis vectors given a new forward
        /// (which is expected to have unit length)
        /// </summary>
        /// <param name="newUnitForward"></param>
        /// <param name="forward"></param>
        /// <param name="side"></param>
        /// <param name="up"></param>
        public static void RegenerateOrthonormalBasisUF(CCVector2 newUnitForward, out CCVector2 forward, out CCVector2 side, ref CCVector2 up)
        {
            forward = newUnitForward;

            // derive new side basis vector from NEW forward and OLD up
            SetUnitSideFromForward(ref forward, out side, ref up);

            // derive new Up basis vector from new Side and new Forward
            //(should have unit length since Side and Forward are
            // perpendicular and unit length)
            //up = CCVector2.Cross(side, forward);
        }

        /// <summary>
        /// for when the new forward is NOT know to have unit length
        /// </summary>
        /// <param name="newForward"></param>
        /// <param name="forward"></param>
        /// <param name="side"></param>
        /// <param name="up"></param>
        public static void RegenerateOrthonormalBasis(CCVector2 newForward, out CCVector2 forward, out CCVector2 side, ref CCVector2 up)
        {
            RegenerateOrthonormalBasisUF(CCVector2.Normalize(newForward), out forward, out side, ref up);
        }

        /// <summary>
        /// for supplying both a new forward and and new up
        /// </summary>
        /// <param name="newForward"></param>
        /// <param name="newUp"></param>
        /// <param name="forward"></param>
        /// <param name="side"></param>
        /// <param name="up"></param>
        public static void RegenerateOrthonormalBasis(CCVector2 newForward, CCVector2 newUp, out CCVector2 forward, out CCVector2 side, out CCVector2 up)
        {
            up = newUp;
            RegenerateOrthonormalBasis(CCVector2.Normalize(newForward), out forward, out side, ref up);
        }

        public static CCAffineTransform ToMatrix(this ILocalSpaceBasis basis)
        {
            return ToMatrix(basis.Forward, basis.Side, basis.Up, basis.Position);
        }

        public static CCAffineTransform ToMatrix(CCVector2 forward, CCVector2 side, CCVector2 up, CCVector2 position)
        {

            CCAffineTransform m = CCAffineTransform.Identity;

            float rotation = side.Angle;
            m.Rotation = rotation;
            m.Tx = position.X;
            m.Ty = position.Y;

            return m;
        }

        public static void FromMatrix(CCAffineTransform transformation, out CCVector2 forward, out CCVector2 side, out CCVector2 up, out CCVector2 position)
        {
            position = new CCVector2(transformation.Tx, transformation.Ty);
            side = new CCVector2(transformation.A, transformation.B);
            up = CCVector2.UnitY;
            forward = new CCVector2(transformation.C, transformation.D);
        }
    }
}
