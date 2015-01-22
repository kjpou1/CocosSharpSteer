using CocosSharp;
using CocosSharpSteer;
using SteeringDemo.PlugIns.Ctf;

namespace SteeringDemo.PlugIns.Arrival
{
    public class ArrivalPlugIn
        : CtfPlugIn
    {
        public override string Name
        {
            get { return "Arrival"; }
        }

        public ArrivalPlugIn(CCNode node, IAnnotationService annotations)
            :base(node, annotations, 0, true, Globals.DEFAULT_RADIUS, 25)
        {
        }
    }
}
