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

namespace SteeringDemo.PlugIns.LowSpeedTurn
{
	public class LowSpeedTurn : SimpleVehicle
	{
		//Trail _trail;

        internal readonly CtfNode lowSpeedTurnNode;

        public override float MaxForce { get { return Globals.DEFAULT_MAX_FORCE / 2; } }
        public override float MaxSpeed { get { return Globals.DEFAULT_MAX_SPEED; } }

        static CCVector2 HeadingForward;
		// constructor
        public LowSpeedTurn(IAnnotationService annotations = null)
            :base(annotations)
		{
            lowSpeedTurnNode = new CtfNode(Radius);
			Reset();
		}

		// reset state
		public override void Reset()
		{
			// reset vehicle state
			base.Reset();

 			// speed along Forward direction.
			Speed = _startSpeed;

			// initial position along X axis
			Position = Globals.HomeBaseCenter + new CCVector2(_startX, 0);

            Forward = HeadingForward;
            Side = CCVector2.PerpendicularCCW(Forward);

			// for next instance: step starting location
			_startX += 40;

			// for next instance: step speed
			_startSpeed += 1.15f;

			// 15 seconds and 150 points along the trail
			//_trail = new Trail(15, 150);
		}

		// draw into the scene
		public void Draw()
		{

            lowSpeedTurnNode.Position = Position;

            var angle = CCMathHelper.ToDegrees(-Forward.Angle);
            lowSpeedTurnNode.Rotation = angle;


			//Drawing.DrawBasic2dCircularVehicle(this, CCColor4B.Gray);
            //_trail.Draw(annotation);
		}

		// per frame simulation update
		public void Update(float currentTime, float elapsedTime)
		{
			ApplySteeringForce(Steering, elapsedTime);

            Position = Position.WrapAround(Globals.WORLD_WIDTH, Globals.WORLD_HEIGHT);
			// annotation
			annotation.VelocityAcceleration(this);
			//_trail.Record(currentTime, Position);
		}

		// reset starting positions
		public static void ResetStarts()
		{
            HeadingForward = CCVector2Helpers.RandomUnitVectorOnXYPlane();

			_startX = 0;
			_startSpeed = 10;
		}

		// constant steering force
	    private static CCVector2 Steering
		{
			get { return new CCVector2(1, -1); }
		}

		// for stepping the starting conditions for next vehicle
		static float _startX;
		static float _startSpeed;
	}
}
