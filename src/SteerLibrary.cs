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
using CocosSharpSteer.Helpers;
using CocosSharpSteer.Obstacles;
using CocosSharpSteer.Pathway;

namespace CocosSharpSteer
{
	public abstract class SteerLibrary : BaseVehicle
	{
	    protected IAnnotationService annotation { get; private set; }

	    // Constructor: initializes state
	    protected SteerLibrary(IAnnotationService annotationService = null)
		{
            annotation = annotationService ?? new NullAnnotationService();

			// set inital state
			Reset();
		}

		// reset state
		public virtual void Reset()
		{

            //float theta = RandomHelpers.Random() * CCMathHelper.TwoPi;
            // initial state of wander behavior - We can give it a starting trajectory
            //wanderForward = (float)Math.Cos(theta);
            //wanderSide = (float)Math.Sin(theta);
			
            // initial state of wander behavior
            wanderForward = 0;
            wanderSide = 0;
        }

        #region steering behaviours

        private float wanderForward;
        private float wanderSide;

        protected CCVector2 SteerForWander(float dt, float wanderSpeed, float wanderRadius, float wanderDistance, IAnnotationService annotation = null)
	    {
            return this.SteerForWander(dt, ref wanderSide, ref wanderForward, wanderSpeed, wanderRadius, wanderDistance, annotation);
	    }

	    protected CCVector2 SteerForFlee(CCVector2 target)
	    {
	        return this.SteerForFlee(target, MaxSpeed);
	    }

	    protected CCVector2 SteerForSeek(CCVector2 target)
	    {
	        return this.SteerForSeek(target, MaxSpeed);
		}

        protected CCVector2 SteerForArrival(CCVector2 target, float slowingDistance)
	    {
	        return this.SteerForArrival(target, MaxSpeed, slowingDistance, annotation);
	    }

	    protected CCVector2 SteerToFollowFlowField(IFlowField field, float predictionTime)
	    {
	        return this.SteerToFollowFlowField(field, MaxSpeed, predictionTime, annotation);
	    }

        protected CCVector2 SteerToFollowPath(bool direction, float predictionTime, IPathway path)
	    {
	        return this.SteerToFollowPath(direction, predictionTime, path, MaxSpeed, annotation);
	    }

        protected CCVector2 SteerToStayOnPath(float predictionTime, IPathway path)
	    {
	        return this.SteerToStayOnPath(predictionTime, path, MaxSpeed, annotation);
	    }

        protected CCVector2 SteerToAvoidObstacle(float minTimeToCollision, IObstacle obstacle)
        {
            return this.SteerToAvoidObstacle(minTimeToCollision, obstacle, annotation);
        }

	    protected CCVector2 SteerToAvoidObstacles(float minTimeToCollision, IEnumerable<IObstacle> obstacles)
	    {
	        return this.SteerToAvoidObstacles(minTimeToCollision, obstacles, annotation);
	    }

	    protected CCVector2 SteerToAvoidNeighbors(float minTimeToCollision, IEnumerable<IVehicle> others)
		{
	        return this.SteerToAvoidNeighbors(minTimeToCollision, others, annotation);
	    }

	    protected CCVector2 SteerToAvoidCloseNeighbors<TVehicle>(float minSeparationDistance, IEnumerable<TVehicle> others) where TVehicle : IVehicle
        {
            return this.SteerToAvoidCloseNeighbors<TVehicle>(minSeparationDistance, others, annotation);
        }

	    protected CCVector2 SteerForSeparation(float maxDistance, float cosMaxAngle, IEnumerable<IVehicle> flock)
	    {
	        return this.SteerForSeparation(maxDistance, cosMaxAngle, flock, annotation);
	    }

	    protected CCVector2 SteerForAlignment(float maxDistance, float cosMaxAngle, IEnumerable<IVehicle> flock)
	    {
	        return this.SteerForAlignment(maxDistance, cosMaxAngle, flock, annotation);
	    }

	    protected CCVector2 SteerForCohesion(float maxDistance, float cosMaxAngle, IEnumerable<IVehicle> flock)
	    {
	        return this.SteerForCohesion(maxDistance, cosMaxAngle, flock, annotation);
	    }

	    protected CCVector2 SteerForPursuit(IVehicle quarry, float maxPredictionTime = float.MaxValue)
	    {
	        return this.SteerForPursuit(quarry, maxPredictionTime, MaxSpeed, annotation);
	    }

        protected CCVector2 SteerForEvasion(IVehicle menace, float maxPredictionTime)
        {
            return this.SteerForEvasion(menace, maxPredictionTime, MaxSpeed, annotation);
        }

	    protected CCVector2 SteerForTargetSpeed(float targetSpeed)
	    {
	        return this.SteerForTargetSpeed(targetSpeed, MaxForce, annotation);
	    }
        #endregion
	}
}
