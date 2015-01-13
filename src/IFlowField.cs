using CocosSharp;

namespace CocosSharpSteer
{
    /// <summary>
    /// A flow field which can be sampled at arbitrary locations
    /// </summary>
    public interface IFlowField
    {
        /// <summary>
        /// Sample the flow field at the given location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        CCVector2 Sample(CCVector2 location);
    }
}
