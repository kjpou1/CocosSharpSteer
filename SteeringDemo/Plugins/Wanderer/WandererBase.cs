// Copyright (c) 2002-2003, Sony Computer Entertainment America
// Copyright (c) 2002-2003, Craig Reynolds <craig_reynolds@playstation.sony.com>
// Copyright (C) 2007 Bjoern Graf <bjoern.graf@gmx.net>
// All rights reserved.
//
// This software is licensed as described in the file license.txt, which
// you should have received as part of this distribution. The terms
// are also available at http://www.codeplex.com/SharpSteer/Project/License.aspx.

using System;
using System.Text;

using CocosSharp;
using CocosSharpSteer;
using SteeringDemo.PlugIns.Ctf;
using CocosSharpSteer.Helpers;

namespace SteeringDemo.PlugIns.Wanderer
{
	public class WandererBase : SimpleVehicle
	{
		//protected Trail Trail;
        public readonly CtfNode wandererNode;

        public float WanderDistance { get; set; }
        public float WanderRadius { get; set; }
        public float WanderJitter { get; set; }

        public override float MaxForce { get { return Globals.DEFAULT_MAX_FORCE * 5.75f; } }
        public override float MaxSpeed { get { return Globals.DEFAULT_MAX_SPEED * 2; } }

		// constructor
	    protected WandererBase(IAnnotationService annotations = null)
            :base(annotations)
		{
            wandererNode = new CtfNode(Radius);
            wandererNode.BodyColor = BodyColor;

			Reset();
		}

		// reset state
		public override void Reset()
		{
			base.Reset();			// reset the vehicle 

			Speed = 0;            // speed along Forward direction.

            WanderDistance = Globals.WANDER_DISTANCE;
            WanderRadius = Globals.WANDER_RADIUS;
            WanderJitter = Globals.WANDER_SPEED;

            //Trail = new Trail();
			//Trail.Clear();    // prevent long streaks due to teleportation 
		}

		// draw into the scene
		public void Draw()
		{
			//Drawing.DrawBasic2dCircularVehicle(this, BodyColor);
            wandererNode.Position = Position;

            var angle = CCMathHelper.ToDegrees(-Forward.Angle);
            wandererNode.Rotation = angle;

            //CCAffineTransform m = CCAffineTransform.Identity;
            //m.Rotation = Forward.Angle;
            //wandererNode.AdditionalTransform = m;

            StringBuilder status = new StringBuilder();

            status.AppendFormat("Distance {0} - (Up + / Down -)", WanderDistance);
            status.AppendFormat("\nRadius {0} - (Left + / Right -)", WanderRadius);
            status.AppendFormat("\nJitter {0} - (D + / S-)", WanderJitter);
            
            if (status.Length > 0)
            {
                Drawing.UpdateStatus(status.ToString(), CCColor4B.LightGray);
            }
            //Trail.Draw(annotation);
		}

		// for draw method
		protected CCColor4B BodyColor;
	}
}
