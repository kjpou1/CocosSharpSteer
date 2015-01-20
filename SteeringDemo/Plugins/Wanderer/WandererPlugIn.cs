// Copyright (c) 2002-2003, Sony Computer Entertainment America
// Copyright (c) 2002-2003, Craig Reynolds <craig_reynolds@playstation.sony.com>
// Copyright (C) 2007 Bjoern Graf <bjoern.graf@gmx.net>
// All rights reserved.
//
// This software is licensed as described in the file license.txt, which
// you should have received as part of this distribution. The terms
// are also available at http://www.codeplex.com/SharpSteer/Project/License.aspx.

using System;
using System.Collections.Generic;

using CocosSharp;
using CocosSharpSteer;

namespace SteeringDemo.PlugIns.Wanderer
{
	public class WandererPlugIn : PlugIn
	{

        private List<IVehicle> vehicles;
		public WandererPlugIn(CCNode node, IAnnotationService annotations)
            :base(node, annotations)
		{
            vehicles = new List<IVehicle>();
		}

		public override String Name { get { return "Wanderer"; } }

		public override float SelectionOrderSortKey { get { return 0.08f; } }

		public override void Open()
		{
			// create the wanderer, saving a pointer to it
            _wanderer = new Wanderer(Annotations);
            CocosSharpNode.AddChild(_wanderer.wandererNode);
            vehicles.Add(_wanderer);

			// initialize camera
			GameLayer.SelectedVehicle = _wanderer;
		}

        public override void HandleKeys(CCKeys key)
        {
            switch (key)
            {
                case CCKeys.Up:
                    _wanderer.WanderDistance += 5;
                    break;
                case CCKeys.Down:
                    _wanderer.WanderDistance -= 5;
                    break;
                case CCKeys.Left:
                    _wanderer.WanderRadius -= 5;
                    break;
                case CCKeys.Right:
                    _wanderer.WanderRadius += 5;
                    break;
                case CCKeys.S:
                    _wanderer.WanderJitter -= 5;
                    break;
                case CCKeys.D:
                    _wanderer.WanderJitter += 5;
                    break;
            }
        }

		public override void Update(float currentTime, float elapsedTime)
		{
			// update the wanderer
			_wanderer.Update(currentTime, elapsedTime);

		}

		public override void Redraw(float currentTime, float elapsedTime)
		{
			// selected vehicle (user can mouse click to select another)
			IVehicle selected = GameLayer.SelectedVehicle;

			// vehicle nearest mouse (to be highlighted)
			IVehicle nearMouse = GameLayer.VehicleNearestToMouse();

			// draw "ground plane"
			//Demo.GridUtility(selected.Position);

			// draw Wanderer
            _wanderer.Draw();

			// highlight vehicle nearest mouse
			//GameLayer.HighlightVehicleUtility(nearMouse);
			//GameLayer.CircleHighlightVehicleUtility(selected);
		}

		public override void Close()
		{
			// delete wanderer, all pursuers, and clear list
            _wanderer.wandererNode.RemoveFromParent();

            _wanderer = null;
		}

		public override void Reset()
		{
			// reset wanderer
			_wanderer.Reset();

			// immediately jump to default camera position
			//Demo.Camera.DoNotSmoothNextMove();
			//Demo.Camera.ResetLocalSpace();
		}

		//const AVGroup& allVehicles () {return (const AVGroup&) allMP;}
        public override IEnumerable<IVehicle> Vehicles
		{
            get { return vehicles; }
		}


		Wanderer _wanderer;
	}
}
