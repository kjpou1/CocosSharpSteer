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

	// Capture the Flag   (a portion of the traditional game)
	//
	// The "Capture the Flag" sample steering problem, proposed by Marcin
	// Chady of the Working Group on Steering of the IGDA's AI Interface
	// Standards Committee (http://www.igda.org/Committees/ai.htm) in this
	// message (http://sourceforge.net/forum/message.php?msg_id=1642243):
	//
	//     "An agent is trying to reach a physical location while trying
	//     to stay clear of a group of enemies who are actively seeking
	//     him. The environment is littered with obstacles, so collision
	//     avoidance is also necessary."
	//
	// Note that the enemies do not make use of their knowledge of the 
	// seeker's goal by "guarding" it.  
	//
	// XXX hmm, rename them "attacker" and "defender"?
	//
	// 08-12-02 cwr: created 
	
	public class CtfPlugIn : PlugIn
	{
	    private readonly bool _arrive;
	    public readonly float BaseRadius;
	    private readonly int _obstacles;

	    public CtfSeeker CtfSeeker = null;
        public readonly CtfEnemy[] CtfEnemies;

		public CtfPlugIn(CCNode node, IAnnotationService annotations, int enemyCount = 3, bool arrive = false, float baseRadius = Globals.DEFAULT_RADIUS, int obstacles = 50)
            :base(node, annotations)
		{
            CocosSharpNode = node;
		    _arrive = arrive;
		    BaseRadius = baseRadius;
		    _obstacles = obstacles;
		    CtfEnemies = new CtfEnemy[enemyCount];

			_all = new List<CtfBase>();
		}
        
		public override String Name { get { return "Capture the Flag"; } }

		public override float SelectionOrderSortKey { get { return 0.01f; } }

		public override void Open()
		{
			// create the seeker ("hero"/"attacker")
            CtfSeeker = new CtfSeeker(this, Annotations, _arrive);
			_all.Add(CtfSeeker);

			// create the specified number of enemies, 
			// storing pointers to them in an array.
			for (int i = 0; i < CtfEnemies.Length; i++)
			{
                CtfEnemies[i] = new CtfEnemy(this, Annotations);
				_all.Add(CtfEnemies[i]);
			}

			// initialize camera
//			Demo.Init2dCamera(CtfSeeker);
//			Demo.Camera.Mode = Camera.CameraMode.FixedDistanceOffset;
//			Demo.Camera.FixedTarget = CCVector2.Zero;
//            Demo.Camera.FixedTarget.X = 15;
//			Demo.Camera.FixedPosition.X = 80;
//            Demo.Camera.FixedPosition.Y = 60;
//            Demo.Camera.FixedPosition.Z = 0;

            CtfBase.InitializeObstacles(BaseRadius, _obstacles);
		}

		public override void Update(float currentTime, float elapsedTime)
		{
			// update the seeker
			CtfSeeker.Update(currentTime, elapsedTime);

			// update each enemy
			for (int i = 0; i < CtfEnemies.Length; i++)
			{
				CtfEnemies[i].Update(currentTime, elapsedTime);
			}
		}

		public override void Redraw(float currentTime, float elapsedTime)
		{
			// selected vehicle (user can mouse click to select another)
            IVehicle selected = GameLayer.SelectedVehicle;

			// vehicle nearest mouse (to be highlighted)
            IVehicle nearMouse = GameLayer.VehicleNearestToMouse ();

			// update camera
			//Demo.UpdateCamera(elapsedTime, selected);

			// draw "ground plane" centered between base and selected vehicle
            CCVector2 goalOffset = Globals.HomeBaseCenter;// - Demo.Camera.Position;
			CCVector2 goalDirection = goalOffset;
            goalDirection.Normalize();
            CCVector2 cameraForward = CCVector2.Zero;//Demo.Camera.xxxls().Forward;
            float goalDot = CCVector2.Dot(cameraForward, goalDirection);
			float blend = Utilities.RemapIntervalClip(goalDot, 1, 0, 0.5f, 0);
            CCVector2 gridCenter = CCVector2.Lerp(selected.Position, Globals.HomeBaseCenter, blend);
			//Demo.GridUtility(gridCenter);

			// draw the seeker, obstacles and home base
			CtfSeeker.Draw();
			DrawObstacles();
			DrawHomeBase();

			// draw each enemy
			for (int i = 0; i < CtfEnemies.Length; i++) CtfEnemies[i].Draw();

			// highlight vehicle nearest mouse
            GameLayer.DrawCircleHighlightOnVehicle(nearMouse, 1, CCColor4B.Magenta);
		}

		public override void Close()
		{
            // Remove seeker from game layer
            CtfSeeker.ctfNode.RemoveFromParent();
			// delete seeker
			CtfSeeker = null;
            
			// delete each enemy
			for (int i = 0; i < CtfEnemies.Length; i++)
			{
                CtfEnemies[i].ctfNode.RemoveFromParent();
				CtfEnemies[i] = null;
			}

			// clear the group of all vehicles
			_all.Clear();
		}

		public override void Reset()
		{
			// count resets
			Globals.ResetCount++;

			// reset the seeker ("hero"/"attacker") and enemies
			CtfSeeker.Reset();
			for (int i = 0; i < CtfEnemies.Length; i++) 
                CtfEnemies[i].Reset();

			// reset camera position
			//Demo.Position2dCamera(CtfSeeker);

			// make camera jump immediately to new position
			//Demo.Camera.DoNotSmoothNextMove();
		}

		public override void HandleKeys(CCKeys key)
		{
			switch (key)
			{
			case CCKeys.F1: CtfBase.AddOneObstacle(BaseRadius); break;
			case CCKeys.F2: CtfBase.RemoveOneObstacle(); break;
			}
		}

		public override void PrintMiniHelpForFunctionKeys()
		{
#if TODO
			std.ostringstream message;
			message << "Function keys handled by ";
			message << '"' << name() << '"' << ':' << std.ends;
			Demo.printMessage (message);
			Demo.printMessage ("  F1     add one obstacle.");
			Demo.printMessage ("  F2     remove one obstacle.");
			Demo.printMessage ("");
#endif
		}

        public override IEnumerable<IVehicle> Vehicles
		{
			get { return _all.ConvertAll<IVehicle>(v => (IVehicle) v); }
		}

        CCColor4B atCCColor4B = new CCColor4B((byte)(255.0f * 0.3f), (byte)(255.0f * 0.3f), (byte)(255.0f * 0.5f));
        CCColor4B noCCColor4B = CCColor4B.Gray;

	    private void DrawHomeBase()
		{
			CCVector2 up = new CCVector2(0, 0.01f);
			bool reached = CtfSeeker.State == CtfBase.SeekerState.AtGoal;
			CCColor4B baseCCColor4B = (reached ? atCCColor4B : noCCColor4B);
            Drawing.DrawXZDisk(BaseRadius, Globals.HomeBaseCenter, baseCCColor4B, 40);
            Drawing.DrawXZDisk(BaseRadius / 15, Globals.HomeBaseCenter + up, CCColor4B.Black, 20);
		}

	    private static void DrawObstacles()
		{
			CCColor4B color = new CCColor4B((byte)(255.0f * 0.8f), (byte)(255.0f * 0.6f), (byte)(255.0f * 0.4f));
			List<CircleObstacle> allSO = CtfBase.AllObstacles;
			for (int so = 0; so < allSO.Count; so++)
			{
				Drawing.DrawXZCircle(allSO[so].Radius, allSO[so].Center, color, 40);
			}
		}

		// a group (STL vector) of all vehicles in the PlugIn
	    readonly List<CtfBase> _all;
	}
}
