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
using CocosSharpSteer.Database;
using CocosSharpSteer.Helpers;
using CocosSharpSteer.Obstacles;

using SteeringDemo.PlugIns.Ctf;

namespace SteeringDemo.PlugIns.Boids
{ // spherical obstacle group

	public class Boid : SimpleVehicle
	{
	    //private readonly Trail _trail;

        internal readonly CtfNode boidNode;

	    private const float AVOIDANCE_PREDICT_TIME_MIN = Globals.AVOIDANCE_PREDICT_TIME_MIN;
		public const float AVOIDANCE_PREDICT_TIME_MAX = Globals.AVOIDANCE_PREDICT_TIME_MAX;
		public static float AvoidancePredictTime = AVOIDANCE_PREDICT_TIME_MIN;

		// a pointer to this boid's interface object for the proximity database
	    private ITokenForProximityDatabase<IVehicle> _proximityToken;

		// allocate one and share amoung instances just to save memory usage
		// (change to per-instance allocation to be more MP-safe)
	    private static readonly List<IVehicle> _neighbors = new List<IVehicle>();
		public static int BoundaryCondition = 0;
		public const float WORLD_RADIUS = 500;

        public override float MaxForce { get { return Globals.DEFAULT_MAX_FORCE * 11f; } }
        public override float MaxSpeed { get { return Globals.DEFAULT_MAX_SPEED * 3; } }

		// constructor
		public Boid(IProximityDatabase<IVehicle> pd, IAnnotationService annotations = null)
            :base(annotations)
		{
			// allocate a token for this boid in the proximity database
			_proximityToken = null;
			NewPD(pd);

            boidNode = new CtfNode(Radius);
            boidNode.BodyColor = CCColor4B.LightGray;

		    //_trail = new Trail(2f, 60);

			// reset all boid state
			Reset();
		}

		// reset state
		public override void Reset()
		{
			// reset the vehicle
			base.Reset();

			// initial slow speed
			Speed = (MaxSpeed * 0.3f);

			// randomize initial orientation
			//RegenerateOrthonormalBasisUF(CCVector2Helpers.RandomUnitVector());
			CCVector2 d = CCVector2Helpers.RandomUnitVector();
			d.X = Math.Abs(d.X);
            d.Y = Math.Abs(d.Y);
			//d.Z = Math.Abs(d.Z);
			RegenerateOrthonormalBasisUF(d);

			// randomize initial position
			//Position = Globals.HomeBaseCenter + CCVector2.UnitX * 10 + (CCVector2Helpers.RandomVectorInUnitRadiusCircle() * 20);

            RandomizeStartingPositionAndHeading();

			// notify proximity database that our position has changed
			//FIXME: SimpleVehicle::SimpleVehicle() calls reset() before proximityToken is set
			if (_proximityToken != null) 
                _proximityToken.UpdateForNewPosition(Position);
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

		// draw this boid into the scene
		public void Draw()
		{
		    //_trail.Draw(annotation);

			//Drawing.DrawBasic2dCircularVehicle(this, CCColor4B.LightGray);
		}

		// per frame simulation update
		public void Update(float currentTime, float elapsedTime)
		{
		    //_trail.Record(currentTime, Position);
            boidNode.Position = Position;

            var angle = CCMathHelper.ToDegrees(-Forward.Angle);
            boidNode.Rotation = angle;

			// steer to flock and perhaps to stay within the spherical boundary
		    ApplySteeringForce(SteerToFlock() + HandleBoundary(), elapsedTime);

			// notify proximity database that our position has changed
			_proximityToken.UpdateForNewPosition(Position);
		}

		// basic flocking
	    private CCVector2 SteerToFlock()
		{
            const float separationRadius = 50.0f; //5.0f;
			const float separationAngle = -0.707f;
            const float separationWeight = 120.0f; //12.0f;

            const float alignmentRadius = 70.5f; //7.5f;
			const float alignmentAngle = 0.7f;
            const float alignmentWeight = 80.0f; //8.0f;

            const float cohesionRadius = 90.0f; //9.0f;
			const float cohesionAngle = -0.15f;
            const float cohesionWeight = 80.0f; //8.0f;

			float maxRadius = Math.Max(separationRadius, Math.Max(alignmentRadius, cohesionRadius));

			// find all flockmates within maxRadius using proximity database
			_neighbors.Clear();
			_proximityToken.FindNeighbors(Position, maxRadius, _neighbors);

			// determine each of the three component behaviors of flocking
            CCVector2 separation = SteerForSeparation(separationRadius, separationAngle, _neighbors);
            CCVector2 alignment = SteerForAlignment(alignmentRadius, alignmentAngle, _neighbors);
            CCVector2 cohesion = SteerForCohesion(cohesionRadius, cohesionAngle, _neighbors);

			// apply weights to components (save in variables for annotation)
			CCVector2 separationW = separation * separationWeight;
			CCVector2 alignmentW = alignment * alignmentWeight;
			CCVector2 cohesionW = cohesion * cohesionWeight;

			CCVector2 avoidance = SteerToAvoidObstacles(AVOIDANCE_PREDICT_TIME_MIN, AllObstacles);

			// saved for annotation
			bool avoiding = (avoidance != CCVector2.Zero);
			CCVector2 steer = separationW + alignmentW + cohesionW;
			if (avoiding)
			{
				steer = avoidance;
				//System.Diagnostics.Debug.WriteLine(String.Format("Avoiding: [{0}, {1}", avoidance.X, avoidance.Y));
			}
#if IGNORED
			// annotation
			const float s = 0.1f;
			AnnotationLine(Position, Position + (separationW * s), Color.Red);
			AnnotationLine(Position, Position + (alignmentW * s), Color.Orange);
			AnnotationLine(Position, Position + (cohesionW * s), Color.Yellow);
#endif
			return steer;
		}

		// Take action to stay within sphereical boundary.  Returns steering
		// value (which is normally zero) and may take other side-effecting
		// actions such as kinematically changing the Boid's position.
	    private CCVector2 HandleBoundary()
		{
			// while inside the sphere do noting
			if (Position.Length() < WORLD_RADIUS)
				return CCVector2.Zero;

			// once outside, select strategy
			switch (BoundaryCondition)
			{
			case 0:
				{
					// steer back when outside
					//CCVector2 seek = SteerForSeek(CCVector2.Zero);
                        CCVector2 seek = SteerForSeek(Globals.HomeBaseCenter);
                    CCVector2 lateral = CCVector2Helpers.PerpendicularComponent(seek, Forward);
                    return lateral;
				}
			case 1:
				{
					// wrap around (teleport)
                    //Position = (Position.SphericalWrapAround(CCVector2.Zero, WORLD_RADIUS));
                        Position = (Position.SphericalWrapAround(Globals.HomeBaseCenter, WORLD_RADIUS));
					return CCVector2.Zero;
				}
			}
			return CCVector2.Zero; // should not reach here
		}

		// make boids "bank" as they fly
	    protected override void RegenerateLocalSpace(CCVector2 newVelocity, float elapsedTime)
		{
			RegenerateLocalSpaceForBanking(newVelocity, elapsedTime);
		}

		// switch to new proximity database -- just for demo purposes
		public void NewPD(IProximityDatabase<IVehicle> pd)
		{
			// delete this boid's token in the old proximity database
			if (_proximityToken != null)
			{
				_proximityToken.Dispose();
				_proximityToken = null;
			}

			// allocate a token for this boid in the proximity database
			_proximityToken = pd.AllocateToken(this);
		}

		// cycle through various boundary conditions
		public static void NextBoundaryCondition()
		{
			const int max = 2;
			BoundaryCondition = (BoundaryCondition + 1) % max;
		}

		// dynamic obstacle registry
		public static void InitializeObstacles()
		{
			// start with 40% of possible obstacles
			if (_obstacleCount == -1)
			{
				_obstacleCount = 0;
				for (int i = 0; i < (MAX_OBSTACLE_COUNT * 1.0); i++)
					AddOneObstacle();
			}
		}

	    private static void AddOneObstacle()
		{
	        if (_obstacleCount >= MAX_OBSTACLE_COUNT)
	            return;

	        // pick a random center and radius,
	        // loop until no overlap with other obstacles and the home base
	        //float r = 15;
	        //CCVector2 c = CCVector2.Up * r * (-0.5f * maxObstacleCount + obstacleCount);
	        float r = RandomHelpers.Random(10f, 30f);
	        CCVector2 c = Globals.HomeBaseCenter + CCVector2Helpers.RandomVectorInUnitRadiusCircle() * WORLD_RADIUS * 1.1f;

	        // add new non-overlapping obstacle to registry
	        AllObstacles.Add(new CircleObstacle(r, c));
	        _obstacleCount++;
		}

		public static void RemoveOneObstacle()
		{
			if (_obstacleCount > 0)
			{
				_obstacleCount--;
				AllObstacles.RemoveAt(_obstacleCount);
			}
		}

		public float MinDistanceToObstacle(CCVector2 point)
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
			if (minClearance > clearance)
				minClearance = clearance;
			return minClearance;
		}

	    private static int _obstacleCount = -1;
	    private const int MAX_OBSTACLE_COUNT = 30;
		public static readonly List<CircleObstacle> AllObstacles = new List<CircleObstacle>();
	}
}
