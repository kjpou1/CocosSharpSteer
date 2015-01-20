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

namespace SteeringDemo.PlugIns.MultiplePursuit
{
	public class MpPursuer : MpBase
	{
		// constructor
		public MpPursuer(MpWanderer w, IAnnotationService annotations = null)
            :base(annotations)
		{
			_wanderer = w;
			Reset();
		}

        public override float MaxSpeed
        {
            get
            {
                return Globals.DEFAULT_MAX_SPEED * 3;
            }
        }

		// reset state
		public override void Reset()
		{
			base.Reset();
			BodyColor = new CCColor4B((byte)(255.0f * 0.6f), (byte)(255.0f * 0.4f), (byte)(255.0f * 0.4f)); // redish
			if(_wanderer != null) 
                RandomizeStartingPositionAndHeading();
		}

		// one simulation step
		public void Update(float currentTime, float elapsedTime)
		{
			// when pursuer touches quarry ("wanderer"), reset its position
			float d = CCVector2.Distance(Position, _wanderer.Position);
			float r = Radius + _wanderer.Radius;
			if (d < r) Reset();

			const float maxTime = 20; // xxx hard-to-justify value
			ApplySteeringForce(SteerForPursuit(_wanderer, maxTime), elapsedTime);

            // Make sure we stay within the world bounds
            Position = Position.WrapAround(Globals.WORLD_WIDTH,Globals.WORLD_HEIGHT);
			// for annotation
			//Trail.Record(currentTime, Position);
		}

		// reset position
	    private void RandomizeStartingPositionAndHeading()
		{
			// randomize position on a ring between inner and outer radii
			// centered around the home base
			const float inner = Globals.MIN_START_RADIUS;
			const float outer = Globals.MIN_START_RADIUS;
			float radius = RandomHelpers.Random(inner, outer);
            CCVector2 randomOnRing = CCVector2Helpers.RandomUnitVectorOnXYPlane() * radius;
            Position = (_wanderer.Position + randomOnRing);

			// randomize 2D heading
			RandomizeHeadingOnXYPlane();
		}

	    readonly MpWanderer _wanderer;
	}
}
