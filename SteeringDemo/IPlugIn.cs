// Copyright (c) 2002-2003, Sony Computer Entertainment America
// CopPlugins.Reynolds <craig_reynolds@playstation.sony.com>
// Copyright (C) 2007 Bjoern Graf <bjoern.graf@gmx.net>
// All rights reserved.
//
// This software is licensed as described in the file license.txt, which
// you should have received as part of this distribution. The terms
// are also available at http://www.codeplex.com/SharpSteer/Project/License.aspx.

using System;
using System.Collections.Generic;
using CocosSharpSteer;
using CocosSharp;

namespace SteeringDemo
{
	public interface IPlugIn
	{
        CCNode CocosSharpNode { get; set; }
		// generic PlugIn actions: open, update, redraw, close and reset
		void Open();
		void Update(float currentTime, float elapsedTime);
		void Redraw(float currentTime, float elapsedTime);
		void Close();
		void Reset();

		// return a pointer to this instance's character string name
		String Name { get; }

		// numeric sort key used to establish user-visible PlugIn ordering
		// ("built ins" have keys greater than 0 and less than 1)
		float SelectionOrderSortKey { get; }

		// allows a PlugIn to nominate itself as OpenSteerDemo's initially selected
		// (default) PlugIn, which is otherwise the first in "selection order"
		bool RequestInitialSelection { get; }

		// handle function keys (which are reserved by SterTest for PlugIns)
		//void HandleFunctionKeys(Keys key);

		// print "mini help" documenting function keys handled by this PlugIn
		void PrintMiniHelpForFunctionKeys();

		// return an AVGroup (an STL vector of AbstractVehicle pointers) of
		// all vehicles(/agents/characters) defined by the PlugIn
		IEnumerable<IVehicle> Vehicles { get; }
	}
}
