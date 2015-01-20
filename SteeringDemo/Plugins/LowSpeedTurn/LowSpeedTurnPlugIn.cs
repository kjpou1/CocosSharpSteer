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

namespace SteeringDemo.PlugIns.LowSpeedTurn
{
	class LowSpeedTurnPlugIn : PlugIn
	{
		const int LST_COUNT = 5;

		public LowSpeedTurnPlugIn(CCNode node, IAnnotationService annotations)
            :base(node, annotations)
		{
			_all = new List<LowSpeedTurn>();
		}

		public override String Name { get { return "Low Speed Turn"; } }

		public override float SelectionOrderSortKey { get { return 0.05f; } }

		public override void Open()
		{
			// create a given number of agents with stepped inital parameters,
			// store pointers to them in an array.
			LowSpeedTurn.ResetStarts();
            for (int i = 0; i < LST_COUNT; i++)
            {
                _all.Add(new LowSpeedTurn(Annotations));
                CocosSharpNode.AddChild(_all[i].lowSpeedTurnNode);
            }

			// initial selected vehicle
			GameLayer.SelectedVehicle = _all[0];

			// initialize camera
//			Demo.Camera.Mode = Camera.CameraMode.Fixed;
//			Demo.Camera.FixedUp = _lstPlusZ;
//			Demo.Camera.FixedTarget = _lstViewCenter;
//			Demo.Camera.FixedPosition = _lstViewCenter;
//			Demo.Camera.FixedPosition.Y += LST_LOOK_DOWN_DISTANCE;
//			Demo.Camera.LookDownDistance = LST_LOOK_DOWN_DISTANCE;
//			Demo.Camera.FixedDistanceVerticalOffset = Demo.CAMERA2_D_ELEVATION;
//			Demo.Camera.FixedDistanceDistance = Demo.CAMERA_TARGET_DISTANCE;
		}

		public override void Update(float currentTime, float elapsedTime)
		{
			// update, draw and annotate each agent
			for (int i = 0; i < _all.Count; i++)
			{
				_all[i].Update(currentTime, elapsedTime);
			}
		}

		public override void Redraw(float currentTime, float elapsedTime)
		{
			// selected vehicle (user can mouse click to select another)
			IVehicle selected = GameLayer.SelectedVehicle;

			// vehicle nearest mouse (to be highlighted)
            IVehicle nearMouse = GameLayer.VehicleNearestToMouse();

			// update camera
			//Demo.UpdateCamera(elapsedTime, selected);

			// draw "ground plane"
            //GameLayer.GridUtility(selected.Position);

			// update, draw and annotate each agent
			for (int i = 0; i < _all.Count; i++)
			{
				// draw this agent
				LowSpeedTurn agent = _all[i];
				agent.Draw();

				// display speed near agent's screen position
				CCColor4B textColor = new CCColor4F(0.8f, 0.8f, 1.0f, 1.0f);
				CCVector2 textOffset = new CCVector2(0, 0.25f);
				CCVector2 textPosition = agent.Position + textOffset;
				String annote = String.Format("{0:0.00}", agent.Speed);
				Drawing.Draw2dTextAt3dLocation(annote, textPosition, textColor);
			}

			// highlight vehicle nearest mouse
            //GameLayer.DrawCircleHighlightOnVehicle(nearMouse);
		}

		public override void Close()
		{
            _all.ForEach((ls) => ls.lowSpeedTurnNode.RemoveFromParent());
			_all.Clear();
		}

		public override void Reset()
		{
			// reset each agent
			LowSpeedTurn.ResetStarts();
			for (int i = 0; i < _all.Count; i++) _all[i].Reset();
		}

        public override IEnumerable<IVehicle> Vehicles
		{
			get { return _all.ConvertAll<IVehicle>(v => (IVehicle) v); }
		}

	    readonly List<LowSpeedTurn> _all; // for allVehicles
	}
}
