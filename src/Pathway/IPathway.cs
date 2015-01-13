using CocosSharp;

namespace CocosSharpSteer.Pathway
{
    public interface IPathway
    {
        /// <summary>
        /// Given an arbitrary point ("A"), returns the nearest point ("P") on
        /// this path.  Also returns, via output arguments, the path tangent at
        /// P and a measure of how far A is outside the Pathway's "tube".  Note
        /// that a negative distance indicates A is inside the Pathway.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="tangent"></param>
        /// <param name="outside"></param>
        /// <returns></returns>
        CCVector2 MapPointToPath(CCVector2 point, out CCVector2 tangent, out float outside);

        /// <summary>
        /// given a distance along the path, convert it to a point on the path
        /// </summary>
        /// <param name="pathDistance"></param>
        /// <returns></returns>
        CCVector2 MapPathDistanceToPoint(float pathDistance);

        /// <summary>
        /// Given an arbitrary point, convert it to a distance along the path.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        float MapPointToPathDistance(CCVector2 point);
    }
}
