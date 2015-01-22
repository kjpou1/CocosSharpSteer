using System;
using System.Collections.Generic;

using CocosSharp;
using CocosSharpSteer;
using CocosSharpSteer.Pathway;

using SteeringDemo.PlugIns.Ctf;

namespace SteeringDemo.PlugIns.MeshPathFollowing
{
    public class MeshPathFollowingPlugin
        :PlugIn
    {
        private PolylinePathway _path;
        private PathWalker _walker;

        public override bool RequestInitialSelection
        {
            get
            {
                return true;
            }
        }

        public MeshPathFollowingPlugin(CCNode node, IAnnotationService annotations)
            :base(node, annotations)
        {
        }

        public override void Open()
        {
            GeneratePath();
            _walker = new PathWalker(_path, Annotations);
            CocosSharpNode.AddChild(_walker.pathWalkerNode);
        }

        private void GeneratePath()
        {
            Random rand = new Random();

            List<CCVector2> points = new List<CCVector2>();
            for (int i = 0; i < 30; i++)
            {
                float f = CCMathHelper.TwoPi / 30f * i;
                float r = (float)rand.NextDouble() * 150 + 150;
                points.Add(Globals.HomeBaseCenter + new CCVector2((float)Math.Sin(f) * r, (float)Math.Cos(f) * r));
            }

            _path = new PolylinePathway(
                points.ToArray(), 0.5f, true
            );
        }

        public override void Update(float currentTime, float elapsedTime)
        {
            _walker.Update(elapsedTime);
        }

        public override void Redraw(float currentTime, float elapsedTime)
        {
            //Demo.UpdateCamera(elapsedTime, _walker);
            _walker.Draw();

            for (int i = 0; i < _path.PointCount; i++)
                if (i > 0)
                    Drawing.DrawLine(_path.Points[i], _path.Points[i - 1], CCColor4B.Red);
        }

        public override void Close()
        {
            _walker.pathWalkerNode.RemoveFromParent();    
        }

        public override string Name
        {
            get { return "Nav Mesh Path Following"; }
        }

        public override IEnumerable<IVehicle> Vehicles
        {
            get { yield return _walker; }
        }
    }
}
