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

namespace SteeringDemo.PlugIns.Ctf
{
	class Globals
	{
		public static  CCVector2 HomeBaseCenter = CCVector2.Zero;

        public static int WORLD_WIDTH = 1024;
        public static int WORLD_HEIGHT = 640;
            
        public const float DEFAULT_RADIUS = 10.5f;

        public const float DEFAULT_SPEED = 50;
        public const float DEFAULT_MAX_FORCE = DEFAULT_SPEED;
        public const float DEFAULT_MAX_SPEED = DEFAULT_SPEED;

        public const float PATH = DEFAULT_SPEED;

		public const float MIN_START_RADIUS = 230;
		public const float MAX_START_RADIUS = 340;

		public const float BRAKING_RATE = 1.00f;

		public static readonly CCColor4B EvadeColor = new CCColor4B((byte)(255.0f * 0.6f), (byte)(255.0f * 0.6f), (byte)(255.0f * 0.3f)); // annotation
		public static readonly CCColor4B SeekColor = new CCColor4B((byte)(255.0f * 0.3f), (byte)(255.0f * 0.6f), (byte)(255.0f * 0.6f)); // annotation
		public static readonly CCColor4B ClearPathColor = new CCColor4B((byte)(255.0f * 0.3f), (byte)(255.0f * 0.6f), (byte)(255.0f * 0.3f)); // annotation
        public static readonly CCColor4B NotSafeColor = CCColor4B.Magenta;

		public const float AVOIDANCE_PREDICT_TIME_MIN = 0.9f;//DEFAULT_RADIUS * 4f;
		public const float AVOIDANCE_PREDICT_TIME_MAX = 2.0f;//DEFAULT_RADIUS * 8f;
		public static float AvoidancePredictTime = AVOIDANCE_PREDICT_TIME_MIN;

		public static bool EnableAttackSeek = true; // for testing (perhaps retain for UI control?)
		public static bool EnableAttackEvade = true; // for testing (perhaps retain for UI control?)

		public static CtfSeeker Seeker = null;

        public static int NUMBER_OF_BOIDS = 100;

        public static float PATHWALKER_PREDICTION_TIME = 3.0f;

        public static float EVADE_DISTANCE_WEIGHT = 140.0f;

        // Wander steering stuff
        public static float WANDER_SPEED = 30.0f;
        public static float WANDER_RADIUS = 30.0f;
        public static float WANDER_DISTANCE = 50.0f;

		// count the number of times the simulation has reset (e.g. for overnight runs)
		public static int ResetCount = 0;
	}
}
