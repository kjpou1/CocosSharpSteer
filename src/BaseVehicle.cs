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

namespace CocosSharpSteer
{
	public abstract class BaseVehicle : LocalSpace, IVehicle
	{
		public abstract float Mass { get; set; }
		public abstract float Radius { get; set; }
        public abstract CCVector2 Velocity { get; }
		public abstract CCVector2 Acceleration { get; }
		public abstract float Speed { get; set; }

        public abstract CCVector2 PredictFuturePosition(float predictionTime);

		public abstract float MaxForce { get; }
		public abstract float MaxSpeed { get; }
	}
}
