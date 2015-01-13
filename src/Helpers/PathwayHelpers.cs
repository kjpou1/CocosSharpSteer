using CocosSharp;
using CocosSharpSteer.Pathway;

namespace CocosSharpSteer.Helpers
{
    public static class PathwayHelpers
    {
        /// <summary>
        /// is the given point inside the path tube?
        /// </summary>
        /// <param name="pathway"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool IsInsidePath(this IPathway pathway, CCVector2 point)
		{
			float outside;
			CCVector2 tangent;
            pathway.MapPointToPath(point, out tangent, out outside);
			return outside < 0;
		}

        /// <summary>
        /// how far outside path tube is the given point?  (negative is inside)
        /// </summary>
        /// <param name="pathway"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static float HowFarOutsidePath(this IPathway pathway, CCVector2 point)
		{
			float outside;
			CCVector2 tangent;
            pathway.MapPointToPath(point, out tangent, out outside);
			return outside;
		}
    }
}
