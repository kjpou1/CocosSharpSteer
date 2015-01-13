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
using CocosSharp;
using CocosSharpSteer.Helpers;

namespace CocosSharpSteer
{
	public class SimpleVehicle : SteerLibrary
	{
	    CCVector2 _lastForward;
        CCVector2 _lastPosition;
		float _smoothedCurvature;
		// The acceleration is smoothed
        CCVector2 _acceleration;

		public SimpleVehicle(IAnnotationService annotations = null)
            :base(annotations)
		{
		}

		// reset vehicle state
		public override void Reset()
		{
            base.Reset();

			// reset LocalSpace state
			ResetLocalSpace();

			Mass = 1;          // Mass (defaults to 1 so acceleration=force)
			Speed = 0;         // speed along Forward direction.

			Radius = 10.5f;     // size of bounding sphere

			// reset bookkeeping to do running averages of these quanities
			ResetSmoothedPosition();
			ResetSmoothedCurvature();
			ResetAcceleration();
		}

		// get/set Mass
        // Mass (defaults to unity so acceleration=force)
	    public override float Mass { get; set; }

	    // get velocity of vehicle
        public override CCVector2 Velocity
		{
			get { return Forward * Speed; }
		}

		// get/set speed of vehicle  (may be faster than taking mag of velocity)
        // speed along Forward direction. Because local space is
        // velocity-aligned, velocity = Forward * Speed
	    public override float Speed { get; set; }

	    // size of bounding sphere, for obstacle avoidance, etc.
	    public override float Radius { get; set; }

	    // get/set maxForce
        // the maximum steering force this vehicle can apply
        // (steering force is clipped to this magnitude)
        public override float MaxForce
        {
            get { return 20f; }
        }

	    // get/set maxSpeed
        // the maximum speed this vehicle is allowed to move
        // (velocity is clipped to this magnitude)
	    public override float MaxSpeed
	    {
	        get { return 50; }
	    }

	    // apply a given steering force to our momentum,
		// adjusting our orientation to maintain velocity-alignment.
	    public void ApplySteeringForce(CCVector2 force, float elapsedTime)
		{
			CCVector2 adjustedForce = AdjustRawSteeringForce(force, elapsedTime);

			// enforce limit on magnitude of steering force
            CCVector2 clippedForce = adjustedForce.TruncateLength(MaxForce);

			// compute acceleration and velocity
			CCVector2 newAcceleration = (clippedForce / Mass);
			CCVector2 newVelocity = Velocity;

			// damp out abrupt changes and oscillations in steering acceleration
			// (rate is proportional to time step, then clipped into useful range)
			if (elapsedTime > 0)
			{
                float smoothRate = CCMathHelper.Clamp(9 * elapsedTime, 0.15f, 0.4f);
				Utilities.BlendIntoAccumulator(smoothRate, newAcceleration, ref _acceleration);
			}

 			// Euler integrate (per frame) acceleration into velocity
			newVelocity += _acceleration * elapsedTime;

            // enforce speed limit
            newVelocity = newVelocity.TruncateLength(MaxSpeed);

			// update Speed
			Speed = (newVelocity.Length());

			// Euler integrate (per frame) velocity into position
			Position = (Position + (newVelocity * elapsedTime));

			// regenerate local space (by default: align vehicle's forward axis with
			// new velocity, but this behavior may be overridden by derived classes.)
			RegenerateLocalSpace(newVelocity, elapsedTime);

			// maintain path curvature information
			MeasurePathCurvature(elapsedTime);

			// running average of recent positions
			Utilities.BlendIntoAccumulator(elapsedTime * 0.06f, // QQQ
								  Position,
								  ref _smoothedPosition);
		}

		// the default version: keep FORWARD parallel to velocity, change
		// UP as little as possible.
	    protected virtual void RegenerateLocalSpace(CCVector2 newVelocity, float elapsedTime)
		{
			// adjust orthonormal basis vectors to be aligned with new velocity
			if (Speed > 0)
			{
				RegenerateOrthonormalBasisUF(newVelocity / Speed);
			}
		}

		// alternate version: keep FORWARD parallel to velocity, adjust UP
		// according to a no-basis-in-reality "banking" behavior, something
		// like what birds and airplanes do.  (XXX experimental cwr 6-5-03)
	    protected void RegenerateLocalSpaceForBanking(CCVector2 newVelocity, float elapsedTime)
		{
			// the length of this global-upward-pointing vector controls the vehicle's
			// tendency to right itself as it is rolled over from turning acceleration
            CCVector2 globalForward = new CCVector2(0, 0.2f);

			// acceleration points toward the center of local path curvature, the
			// length determines how much the vehicle will roll while turning
            CCVector2 accelForward = _acceleration * 0.05f;

			// combined banking, sum of UP due to turning and global UP
            CCVector2 bankForward = accelForward + globalForward;

			// blend bankUp into vehicle's UP basis vector
			float smoothRate = elapsedTime * 3;
			CCVector2 tempUp = Forward;
			Utilities.BlendIntoAccumulator(smoothRate, bankForward, ref tempUp);
			Forward = tempUp;
            Forward.Normalize();

//			annotation.Line(Position, Position + (globalUp * 4), CCColor4B.White);
//			annotation.Line(Position, Position + (bankUp * 4), CCColor4B.Orange);
//			annotation.Line(Position, Position + (accelUp * 4), CCColor4B.Red);
//			annotation.Line(Position, Position + (Up * 1), CCColor4B.Yellow);

			// adjust orthonormal basis vectors to be aligned with new velocity
			if (Speed > 0) RegenerateOrthonormalBasisUF(newVelocity / Speed);
		}

		/// <summary>
        /// adjust the steering force passed to applySteeringForce.
        /// allows a specific vehicle class to redefine this adjustment.
        /// default is to disallow backward-facing steering at low speed.
		/// </summary>
		/// <param name="force"></param>
		/// <param name="deltaTime"></param>
		/// <returns></returns>
		protected virtual CCVector2 AdjustRawSteeringForce(CCVector2 force, float deltaTime)
		{
			float maxAdjustedSpeed = 0.2f * MaxSpeed;

			if ((Speed > maxAdjustedSpeed) || (force == CCVector2.Zero))
				return force;

            float range = Speed / maxAdjustedSpeed;
            float cosine = CCMathHelper.Lerp(1, -1, (float)Math.Pow(range, 20));
            return force.LimitMaxDeviationAngle(cosine, Forward);
		}

		/// <summary>
        /// apply a given braking force (for a given dt) to our momentum.
		/// </summary>
		/// <param name="rate"></param>
		/// <param name="deltaTime"></param>
	    public void ApplyBrakingForce(float rate, float deltaTime)
		{
			float rawBraking = Speed * rate;
			float clipBraking = ((rawBraking < MaxForce) ? rawBraking : MaxForce);
			Speed = (Speed - (clipBraking * deltaTime));
		}

		/// <summary>
        /// predict position of this vehicle at some time in the future (assumes velocity remains constant)
		/// </summary>
		/// <param name="predictionTime"></param>
		/// <returns></returns>
        public override CCVector2 PredictFuturePosition(float predictionTime)
		{
			return Position + (Velocity * predictionTime);
		}

		// get instantaneous curvature (since last update)
	    protected float Curvature { get; private set; }

	    // get/reset smoothedCurvature, smoothedAcceleration and smoothedPosition
		public float SmoothedCurvature
		{
			get { return _smoothedCurvature; }
		}

	    private void ResetSmoothedCurvature(float value = 0)
		{
			_lastForward = CCVector2.Zero;
			_lastPosition = CCVector2.Zero;
	        _smoothedCurvature = value;
            Curvature = value;
		}

		public override CCVector2 Acceleration
		{
			get { return _acceleration; }
		}

	    protected void ResetAcceleration()
	    {
	        ResetAcceleration(CCVector2.Zero);
	    }

	    private void ResetAcceleration(CCVector2 value)
	    {
	        _acceleration = value;
	    }

        CCVector2 _smoothedPosition;
	    public CCVector2 SmoothedPosition
		{
			get { return _smoothedPosition; }
		}

	    private void ResetSmoothedPosition()
	    {
	        ResetSmoothedPosition(CCVector2.Zero);
	    }

	    protected void ResetSmoothedPosition(CCVector2 value)
	    {
	        _smoothedPosition = value;
	    }

	    // set a random "2D" heading: set local Up to global Y, then effectively
		// rotate about it by a random angle (pick random forward, derive side).
//	    protected void RandomizeHeadingOnXZPlane()
//		{
//			//Up = CCVector2.Up;
//            Forward = CCVector2Helpers.RandomUnitVectorOnXYPlane();
//	        //Side = CCVector2.Cross(Forward, Up);
//		}

        // set a random "2D" heading: set local Up to global Y, then effectively
        // rotate about it by a random angle (pick random forward, derive side).
        protected void RandomizeHeadingOnXYPlane()
        {
            Up = CCVector2.UnitY;
            Forward = CCVector2Helpers.RandomUnitVectorOnXYPlane();
            Side = CCVector2.PerpendicularCCW(Forward);

        }

        // measure path curvature (1/turning-radius), maintain smoothed version
		void MeasurePathCurvature(float elapsedTime)
		{
			if (elapsedTime > 0)
			{
				CCVector2 dP = _lastPosition - Position;
				CCVector2 dF = (_lastForward - Forward) / dP.Length();
                CCVector2 lateral = CCVector2Helpers.PerpendicularComponent(dF, Forward);
                float sign = (CCVector2.Dot(lateral, Side) < 0) ? 1.0f : -1.0f;
				Curvature = lateral.Length() * sign;
				Utilities.BlendIntoAccumulator(elapsedTime * 4.0f, Curvature, ref _smoothedCurvature);
				_lastForward = Forward;
				_lastPosition = Position;
			}
		}
	}
}
