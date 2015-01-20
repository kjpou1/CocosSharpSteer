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
using System.Text;
using CocosSharp;
using CocosSharpSteer;
using CocosSharpSteer.Database;
using CocosSharpSteer.Obstacles;

using SteeringDemo.PlugIns.Ctf;

namespace SteeringDemo.PlugIns.Boids
{ // spherical obstacle group

	public class BoidsPlugIn : PlugIn
	{
		public BoidsPlugIn(CCNode node, IAnnotationService annotations)
            :base(node, annotations)
		{
			_flock = new List<Boid>();
		}

		public override String Name { get { return "Boids"; } }

		public override float SelectionOrderSortKey
		{
			get { return -0.03f; }
		}

		public override void Open()
		{
			// make the database used to accelerate proximity queries
			_cyclePD = -1;
			NextPD();

			// make default-sized flock
			_population = 0;
			for (int i = 0; i < Globals.NUMBER_OF_BOIDS; i++)
                AddBoidToFlock();

			// initialize camera
//			GameLayer.Init3dCamera(GameLayer.SelectedVehicle);
//			GameLayer.Camera.Mode = Camera.CameraMode.Fixed;
//			GameLayer.Camera.FixedDistanceDistance = GameLayer.CAMERA_TARGET_DISTANCE;
//			GameLayer.Camera.FixedDistanceVerticalOffset = 0;
//			GameLayer.Camera.LookDownDistance = 20;
//			GameLayer.Camera.AimLeadTime = 0.5f;
//			GameLayer.Camera.PovOffset.X = 0;
//            GameLayer.Camera.PovOffset.Y = 0.5f;
//            GameLayer.Camera.PovOffset.Z = -2;

			Boid.InitializeObstacles();
		}

		public override void Update(float currentTime, float elapsedTime)
		{
			// update flock simulation for each boid
			for (int i = 0; i < _flock.Count; i++)
			{
				_flock[i].Update(currentTime, elapsedTime);
			}
		}

		public override void Redraw(float currentTime, float elapsedTime)
		{
			// selected vehicle (user can mouse click to select another)
			IVehicle selected = GameLayer.SelectedVehicle;

			// vehicle nearest mouse (to be highlighted)
			IVehicle nearMouse = GameLayer.VehicleNearestToMouse();

			// update camera
			//GameLayer.UpdateCamera(elapsedTime, selected);

			DrawObstacles();

			// draw each boid in flock
			for (int i = 0; i < _flock.Count; i++)
                _flock[i].Draw();

			// highlight vehicle nearest mouse
			GameLayer.DrawCircleHighlightOnVehicle(nearMouse, 1, CCColor4B.Magenta);

			// highlight selected vehicle
			GameLayer.DrawCircleHighlightOnVehicle(selected, 1, CCColor4B.Green);

			// display status in the upper left corner of the window
			StringBuilder status = new StringBuilder();
			status.AppendFormat("[F1/F2] {0} boids", _population);
			status.Append("\n[F3]    PD type: ");
			switch (_cyclePD)
			{
			case 0: status.Append("LQ bin lattice"); break;
			case 1: status.Append("brute force"); break;
			}
			status.Append("\n[F4]    Boundary: ");
			switch (Boid.BoundaryCondition)
			{
			case 0: status.Append("steer back when outside"); break;
			case 1: status.Append("wrap around (teleport)"); break;
			}

			//CCVector2 screenLocation = new CCVector2(15, 50);
            Drawing.UpdateStatus(status.ToString(), CCColor4B.LightGray);
			//Drawing.Draw2dTextAt2dLocation(status.ToString(), screenLocation, CCColor4B.LightGray);
		}

		public override void Close()
		{
			// delete each member of the flock
			while (_population > 0)
                RemoveBoidFromFlock();

			// delete the proximity database
			_pd = null;
		}

		public override void Reset()
		{
            // reset each boid in flock
            for (int i = 0; i < _flock.Count; i++)
                _flock[i].Reset();

            // reset camera position
            //GameLayer.Position3dCamera(GameLayer.SelectedVehicle);

            // make camera jump immediately to new position
            //GameLayer.Camera.DoNotSmoothNextMove();
		}

		// for purposes of demonstration, allow cycling through various
		// types of proximity databases.  this routine is called when the
		// GameLayer user pushes a function key.
	    private void NextPD()
		{
	        // allocate new PD
			const int totalPD = 1;
			switch (_cyclePD = (_cyclePD + 1) % totalPD)
			{
			case 0:
				{
					CCVector2 center = CCVector2.Zero;
                    center = Globals.HomeBaseCenter;
					const float div = 10.0f;
					CCVector2 divisions = new CCVector2(div);
					const float diameter = Boid.WORLD_RADIUS * 1.1f * 2;
					CCVector2 dimensions = new CCVector2(diameter);
					_pd = new LocalityQueryProximityDatabase<IVehicle>(center, dimensions, divisions);
					break;
				}
			}

			// switch each boid to new PD
			for (int i = 0; i < _flock.Count; i++) _flock[i].NewPD(_pd);

			// delete old PD (if any)
		}

		public override void HandleKeys(CCKeys key)
		{
			switch (key)
			{
			case CCKeys.F1: AddBoidToFlock(); break;
			case CCKeys.F2: RemoveBoidFromFlock(); break;
			case CCKeys.F3: NextPD(); break;
			case CCKeys.F4: Boid.NextBoundaryCondition(); break;
			}
		}

		public override void PrintMiniHelpForFunctionKeys()
		{
#if IGNORED
        std.ostringstream message;
        message << "Function keys handled by ";
        message << '"' << name() << '"' << ':' << std.ends;
        GameLayer.printMessage (message);
        GameLayer.printMessage ("  F1     add a boid to the flock.");
        GameLayer.printMessage ("  F2     remove a boid from the flock.");
        GameLayer.printMessage ("  F3     use next proximity database.");
        GameLayer.printMessage ("  F4     next flock boundary condition.");
        GameLayer.printMessage ("");
#endif
		}

	    private void AddBoidToFlock()
		{
			_population++;
			Boid boid = new Boid(_pd, Annotations);
            CocosSharpNode.AddChild(boid.boidNode);
			_flock.Add(boid);
			if (_population == 1) GameLayer.SelectedVehicle = boid;
		}

	    private void RemoveBoidFromFlock()
		{
	        if (_population <= 0)
	            return;

	        // save a pointer to the last boid, then remove it from the flock
	        _population--;
	        Boid boid = _flock[_population];
	        _flock.RemoveAt(_population);
            boid.boidNode.RemoveFromParent();

	        // if it is GameLayer's selected vehicle, unselect it
	        if (boid == GameLayer.SelectedVehicle)
	            GameLayer.SelectedVehicle = null;
		}

		// return an AVGroup containing each boid of the flock
		public override IEnumerable<IVehicle> Vehicles
		{
			get { return _flock.ConvertAll<IVehicle>(v => (IVehicle) v); }
		}

		// flock: a group (STL vector) of pointers to all boids
	    private readonly List<Boid> _flock;

		// pointer to database used to accelerate proximity queries
	    private IProximityDatabase<IVehicle> _pd;

		// keep track of current flock size
	    private int _population;

		// which of the various proximity databases is currently in use
	    private int _cyclePD;

	    private static void DrawObstacles()
		{
			//Color color = new Color((byte)(255.0f * 0.8f), (byte)(255.0f * 0.6f), (byte)(255.0f * 0.4f));
			List<CircleObstacle> allSO = Boid.AllObstacles;
			for (int so = 0; so < allSO.Count; so++)
			{
				//Drawing.DrawBasic3dSphere(allSO[so].Center, allSO[so].Radius, CCColor4B.Red);
				Drawing.Draw3dCircleOrDisk(allSO[so].Radius, allSO[so].Center, CCVector2.UnitY, CCColor4B.Red, 10, true);
				//Drawing.Draw3dCircleOrDisk(allSO[so].Radius, allSO[so].Center, CCVector2.UnitX, CCColor4B.Red, 10, true);
				//Drawing.Draw3dCircleOrDisk(allSO[so].Radius, allSO[so].Center, CCVector2.UnitZ, CCColor4B.Red, 10, true);
			}
		}
	}
}
