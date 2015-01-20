using CocosSharp;

using CocosSharpSteer;
using CocosSharpSteer.Pathway;

using SteeringDemo.PlugIns.Ctf;

namespace SteeringDemo.PlugIns.MeshPathFollowing
{
    public class PathWalker
        :SimpleVehicle
    {
        private readonly IPathway _path;

        internal CtfNode pathWalkerNode;

        public override float MaxForce { get { return Globals.DEFAULT_MAX_FORCE * 10; } }
        public override float MaxSpeed { get { return Globals.DEFAULT_MAX_SPEED; } }

        public PathWalker(IPathway path, IAnnotationService annotation)
            :base(annotation)
        {
            _path = path;

            pathWalkerNode = new CtfNode(Radius);

            Position = Globals.HomeBaseCenter;

            RandomizeHeadingOnXYPlane();
        }

        public void Update(float dt)
        {
            ApplySteeringForce(SteerToFollowPath(true, Globals.PATHWALKER_PREDICTION_TIME, _path), dt);

            annotation.VelocityAcceleration(this);
        }

        internal void Draw()
        {
            //Drawing.DrawBasic2dCircularVehicle(this, CCColor4B.Gray);
            pathWalkerNode.Position = Position;

            var angle = CCMathHelper.ToDegrees(-Forward.Angle);
            pathWalkerNode.Rotation = angle;

        }
    }
}
