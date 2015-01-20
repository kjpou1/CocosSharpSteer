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

namespace SteeringDemo.PlugIns.Wanderer
{
	public class Wanderer : WandererBase
	{

		// constructor
		public Wanderer(IAnnotationService annotations = null)
            :base(annotations)
		{
			Reset();
		}

		// reset state
		public override void Reset()
		{
			base.Reset();
			BodyColor = new CCColor4B((byte)(255.0f * 0.4f), (byte)(255.0f * 0.6f), (byte)(255.0f * 0.4f)); // greenish
            Speed = Globals.DEFAULT_SPEED;

            Position = Globals.HomeBaseCenter;
            // randomize 2D heading
            RandomizeHeadingOnXYPlane();
		}

		// one simulation step
		public void Update(float currentTime, float elapsedTime)
		{
			CCVector2 wander2D = SteerForWander(elapsedTime, WanderJitter, WanderRadius, WanderDistance, annotation);

            CCVector2 steer = Forward + (wander2D * 1.0f);

            ApplySteeringForce(steer, elapsedTime);
            annotation.VelocityAcceleration(this);

            Position = Position.WrapAround(Globals.WORLD_WIDTH,Globals.WORLD_HEIGHT);
           
            //Trail.Record(currentTime, Position);
		}
	}
}
