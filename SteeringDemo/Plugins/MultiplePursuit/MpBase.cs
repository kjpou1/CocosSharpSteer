// Copyright (c) 2002-2003, Sony Computer Entertainment America
// Copyright (c) 2002-2003, Craig Reynolds <craig_reynolds@playstation.sony.com>
// Copyright (C) 2007 Bjoern Graf <bjoern.graf@gmx.net>
// All rights reserved.
//
// This software is licensed as described in the file license.txt, which
// you should have received as part of this distribution. The terms
// are also available at http://www.codeplex.com/SharpSteer/Project/License.aspx.

using CocosSharp;
using CocosSharpSteer;
using SteeringDemo.PlugIns.Ctf;

namespace SteeringDemo.PlugIns.MultiplePursuit
{
	public class MpBase : SimpleVehicle
	{
		//protected Trail Trail;

        internal readonly CtfNode mpBaseNode;

        public override float MaxForce { get { return Globals.DEFAULT_MAX_FORCE * 0.75f; } }
        public override float MaxSpeed { get { return Globals.DEFAULT_MAX_SPEED; } }

		// constructor
	    protected MpBase(IAnnotationService annotations = null)
            :base(annotations)
		{
            mpBaseNode = new CtfNode(Radius);
            mpBaseNode.BodyColor = BodyColor;

			Reset();
		}

		// reset state
		public override void Reset()
		{
			base.Reset();			// reset the vehicle 

			Speed = 0;            // speed along Forward direction.
			//Trail = new Trail();
			//Trail.Clear();    // prevent long streaks due to teleportation 
		}

		// draw into the scene
		public void Draw()
		{
            mpBaseNode.Position = Position;

            var angle = CCMathHelper.ToDegrees(-Forward.Angle);
            mpBaseNode.Rotation = angle;

			//Drawing.DrawBasic2dCircularVehicle(this, BodyColor);
            //Trail.Draw(annotation);
		}

		// for draw method
		protected CCColor4B BodyColor;
	}
}
