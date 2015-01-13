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
	public interface IVehicle : ILocalSpaceBasis
	{
        /// <summary>
        /// mass (defaults to unity so acceleration=force)
        /// </summary>
		float Mass { get; }

        /// <summary>
        /// size of bounding sphere, for obstacle avoidance, etc.
        /// </summary>
		float Radius { get; }

        /// <summary>
        /// velocity of vehicle
        /// </summary>
        CCVector2 Velocity { get; }

		/// <summary>
		/// Gets the acceleration of the vehicle.
		/// </summary>
		CCVector2 Acceleration { get; }
		
		/// <summary>
        /// speed of vehicle (may be faster than taking magnitude of velocity)
		/// </summary>
		float Speed { get; }

        /// <summary>
        /// predict position of this vehicle at some time in the future (assumes velocity remains constant)
        /// </summary>
        /// <param name="predictionTime"></param>
        /// <returns></returns>
        CCVector2 PredictFuturePosition(float predictionTime);

        /// <summary>
        /// the maximum steering force this vehicle can apply
        /// </summary>
		float MaxForce { get; }

        /// <summary>
        /// the maximum speed this vehicle is allowed to move
        /// </summary>
		float MaxSpeed { get; }
	}
}
