// Copyright (c) 2002-2003, Sony Computer Entertainment America
// Copyright (c) 2002-2003, Craig Reynolds <craig_reynolds@playstation.sony.com>
// Copyright (C) 2007 Bjoern Graf <bjoern.graf@gmx.net>
// Copyright (C) 2007 Michael Coles <michael@digini.com>
// All rights reserved.
//
// This software is licensed as described in the file license.txt, which
// you should have received as part of this distribution. The terms
// are also available at http://www.codeplex.com/SharpSteer/Project/License.aspx.

using System;
using System.Collections.Generic;
using CocosSharp;
using CocosSharpSteer;
using CocosSharpSteer.Helpers;
using CocosSharpSteer.Obstacles;

namespace SteeringDemo.PlugIns.Ctf
{ // spherical obstacle group

	public class CtfBase : SimpleVehicle
	{
	    protected readonly CtfPlugIn Plugin;
	    private readonly float _baseRadius;

        internal readonly CtfNode ctfNode;

	    //protected Trail Trail;

        public override float MaxForce { get { return Globals.DEFAULT_MAX_FORCE; } }
        public override float MaxSpeed { get { return Globals.DEFAULT_MAX_SPEED; } }

		// constructor
	    protected CtfBase(CtfPlugIn plugin, IAnnotationService annotations = null, float baseRadius = Globals.DEFAULT_RADIUS)
            :base(annotations)
	    {
	        Plugin = plugin;
	        _baseRadius = baseRadius;

            ctfNode = new CtfNode(baseRadius);
            ctfNode.BodyColor = BodyColor;

            Plugin.CocosSharpNode.AddChild(ctfNode);

	        Reset();
	    }

	    // reset state
		public override void Reset()
		{
			base.Reset();  // reset the vehicle 

			Speed = Globals.DEFAULT_SPEED;             // speed along Forward direction.

			Avoiding = false;         // not actively avoiding

			RandomizeStartingPositionAndHeading();  // new starting position

//			Trail = new Trail();
//			Trail.Clear();     // prevent long streaks due to teleportation
		}

		// draw this character/vehicle into the scene
		public virtual void Draw()
		{
            ctfNode.Position = Position;

            var angle = CCMathHelper.ToDegrees(-Forward.Angle);
            ctfNode.Rotation = angle;

            //CCAffineTransform m = CCAffineTransform.Identity;
            //m.Rotation = Forward.Angle;
            //wandererNode.AdditionalTransform = m;

			//Drawing.DrawBasic2dCircularVehicle(this, BodyColor);
//			Trail.Draw(annotation);
		}

		// annotate when actively avoiding obstacles
		// xxx perhaps this should be a call to a general purpose annotation
		// xxx for "local xxx axis aligned box in XZ plane" -- same code in in
		// xxx Pedestrian.cpp
		public void AnnotateAvoidObstacle(float minDistanceToCollision)
		{
			CCVector2 boxSide = Side * Radius;
			CCVector2 boxFront = Forward * minDistanceToCollision;
			CCVector2 fr = Position + boxFront - boxSide;
			CCVector2 fl = Position + boxFront + boxSide;
			CCVector2 br = Position - boxSide;
			CCVector2 bl = Position + boxSide;
			annotation.Line(fr, fl, CCColor4B.White);
            annotation.Line(fl, bl, CCColor4B.White);
            annotation.Line(bl, br, CCColor4B.White);
            annotation.Line(br, fr, CCColor4B.White);
		}

		public void DrawHomeBase()
		{
			CCVector2 up = new CCVector2(0, 0.01f);
            CCColor4B atColor = new CCColor4B((byte)(255.0f * 0.3f), (byte)(255.0f * 0.3f), (byte)(255.0f * 0.5f), (byte)(255.0f * 0.5f));
            CCColor4B noColor = CCColor4B.Gray;
			bool reached = Plugin.CtfSeeker.State == SeekerState.AtGoal;
            CCColor4B baseColor = (reached ? atColor : noColor);
            Drawing.DrawXZDisk(_baseRadius, Globals.HomeBaseCenter, baseColor, 40);
            Drawing.DrawXZDisk(_baseRadius / 15, Globals.HomeBaseCenter + up, CCColor4B.Black, 20);
		}

	    private void RandomizeStartingPositionAndHeading()
		{
			// randomize position on a ring between inner and outer radii
			// centered around the home base
			float rRadius = RandomHelpers.Random(Globals.MIN_START_RADIUS, Globals.MAX_START_RADIUS);
            CCVector2 randomOnRing = CCVector2Helpers.RandomUnitVectorOnXYPlane() * rRadius;
			Position = (Globals.HomeBaseCenter + randomOnRing);

			// are we are too close to an obstacle?
			if (MinDistanceToObstacle(Position) < Radius * 5)
			{
				// if so, retry the randomization (this recursive call may not return
				// if there is too little free space)
				RandomizeStartingPositionAndHeading();
			}
			else
			{
				// otherwise, if the position is OK, randomize 2D heading
				RandomizeHeadingOnXYPlane();
			}
		}

		public enum SeekerState
		{
			Running,
			Tagged,
			AtGoal
		}

		// for draw method
        protected CCColor4B BodyColor;

		// xxx store steer sub-state for anotation
	    protected bool Avoiding;

		// dynamic obstacle registry
		public static void InitializeObstacles(float radius, int obstacles)
		{
			// start with 40% of possible obstacles
			if (ObstacleCount == -1)
			{
				ObstacleCount = 0;
                for (int i = 0; i < obstacles; i++)
                    AddOneObstacle(radius);
			}
		}

        public static void AddOneObstacle(float radius)
		{
			// pick a random center and radius,
			// loop until no overlap with other obstacles and the home base
			float r;
			CCVector2 c;
			float minClearance;
			float requiredClearance = Globals.Seeker.Radius * 2; // 2 x diameter
			do
			{
				r = RandomHelpers.Random(7.5f, 20);
                r = RandomHelpers.Random(20, 30);
				c = Globals.HomeBaseCenter + CCVector2Helpers.RandomVectorOnUnitRadiusXYDisk() * Globals.MAX_START_RADIUS * 1.1f;
				minClearance = float.MaxValue;
				System.Diagnostics.Debug.WriteLine(String.Format("[{0}, {1}]", c.X, c.Y));
				for (int so = 0; so < AllObstacles.Count; so++)
				{
					minClearance = TestOneObstacleOverlap(minClearance, r, AllObstacles[so].Radius, c, AllObstacles[so].Center);
				}

                minClearance = TestOneObstacleOverlap(minClearance, r, radius - requiredClearance, c, Globals.HomeBaseCenter);
			}
			while (minClearance < requiredClearance);

			// add new non-overlapping obstacle to registry
			AllObstacles.Add(new CircleObstacle(r, c));
			ObstacleCount++;
		}

		public static void RemoveOneObstacle()
		{
		    if (ObstacleCount <= 0)
		        return;

		    ObstacleCount--;
		    AllObstacles.RemoveAt(ObstacleCount);
		}

	    private static float MinDistanceToObstacle(CCVector2 point)
		{
			const float r = 0;
			CCVector2 c = point;
			float minClearance = float.MaxValue;
			for (int so = 0; so < AllObstacles.Count; so++)
			{
				minClearance = TestOneObstacleOverlap(minClearance, r, AllObstacles[so].Radius, c, AllObstacles[so].Center);
			}
			return minClearance;
		}

		static float TestOneObstacleOverlap(float minClearance, float r, float radius, CCVector2 c, CCVector2 center)
		{
			float d = CCVector2.Distance(c, center);
			float clearance = d - (r + radius);
			if (minClearance > clearance) minClearance = clearance;
			return minClearance;
		}

		protected static int ObstacleCount = -1;
		public static readonly List<CircleObstacle> AllObstacles = new List<CircleObstacle>();
	}
}
