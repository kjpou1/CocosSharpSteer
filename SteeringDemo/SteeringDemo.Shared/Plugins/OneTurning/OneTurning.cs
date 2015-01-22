// Copyright (c) 2002-2003, Sony Computer Entertainment America
// Copyright (c) 2002-2003, Craig Reynolds <craig_reynolds@playstation.sony.com>
// Copyright (C) 2007 Bjoern Graf <bjoern.graf@gmx.net>
// Copyright (C) 2007 Michael Coles <michael@digini.com>
// All rights reserved.
//
// This software is licensed as described in the file license.txt, which
// you should have received as part of this distribution. The terms
// are also available at http://www.codeplex.com/SharpSteer/Project/License.aspx.

using CocosSharp;
using CocosSharpSteer;
using CocosSharpSteer.Helpers;

using SteeringDemo.PlugIns.Ctf;

namespace SteeringDemo.PlugIns.OneTurning
{
	public class OneTurning : SimpleVehicle
	{
		//Trail _trail;

        internal readonly CtfNode oneTurningNode;

        public override float MaxForce { get { return Globals.DEFAULT_MAX_FORCE / 10.0f; } }
        public override float MaxSpeed { get { return Globals.DEFAULT_MAX_SPEED; } }

		// constructor
		public OneTurning(IAnnotationService annotations = null)
            :base(annotations)
        {
            oneTurningNode = new CtfNode(Radius);
            oneTurningNode.BodyColor = CCColor4B.Gray;
            
			Reset();
		}

		// reset state
		public override void Reset()
		{
			base.Reset(); // reset the vehicle 
            Speed = Globals.DEFAULT_SPEED / 2;         // speed along Forward direction.
            Position = Globals.HomeBaseCenter;
            // randomize 2D heading
            RandomizeHeadingOnXYPlane();
			//_trail = new Trail();
			//_trail.Clear();    // prevent long streaks due to teleportation 
		}

		// per frame simulation update
		public void Update(float currentTime, float elapsedTime)
		{

            ApplySteeringForce(new CCVector2(-2, -3) * 10, elapsedTime);
            annotation.VelocityAcceleration(this, MaxForce * 5, MaxSpeed);

            Position = Position.WrapAround(Globals.WORLD_WIDTH, Globals.WORLD_HEIGHT);
			
			//_trail.Record(currentTime, Position);
		}

		// draw this character/vehicle into the scene
		public void Draw()
		{
            oneTurningNode.Position = Position;
            var angle = CCMathHelper.ToDegrees(-Forward.Angle);
            oneTurningNode.Rotation = angle;

			//Drawing.DrawBasic2dCircularVehicle(this, CCColor4B.Gray);
            //_trail.Draw(annotation);
		}
	}
}
