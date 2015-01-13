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

namespace CocosSharpSteer.Database
{
	public interface ITokenForProximityDatabase<ContentType> : IDisposable
	{
		/// <summary>
        /// the client object calls this each time its position changes
		/// </summary>
		/// <param name="position"></param>
        void UpdateForNewPosition(CCVector2 position);

		/// <summary>
        /// find all neighbors within the given sphere (as center and radius)
		/// </summary>
		/// <param name="center"></param>
		/// <param name="radius"></param>
		/// <param name="results"></param>
        void FindNeighbors(CCVector2 center, float radius, List<ContentType> results);
	}
}
