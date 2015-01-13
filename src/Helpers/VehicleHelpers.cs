using System;
using System.Collections.Generic;
using System.Linq;
using CocosSharp;
using CocosSharpSteer.Obstacles;
using CocosSharpSteer.Pathway;

namespace CocosSharpSteer.Helpers
{
    public static class VehicleHelpers
    {

        //vector to a target position on the wander circle
        public static CCVector2 wanderTarget = CCVector2.Zero;
       
        public static CCVector2 SteerForWander(this IVehicle vehicle, float dt, ref float wanderSide, ref float wanderForward
            ,  float wanderSpeed, float wanderRadius, float wanderDistance, IAnnotationService annotation = null)
        {

            //first, add a small random vector to the target’s position
            //returns a value between -1 and 1)
            wanderTarget.X = Utilities.ScalarRandomWalk(wanderForward, wanderSpeed, -1, +1);
            wanderTarget.Y = Utilities.ScalarRandomWalk(wanderSide, wanderSpeed, -1, +1);
            
            //reproject this new vector back onto a unit circle
            wanderTarget.Normalize();

            //increase the length of the vector to the same as the radius
            //of the wander circle
            wanderTarget *= wanderRadius;

            // set our referenced parameters so they can be passed in again
            wanderForward = wanderTarget.X;
            wanderSide = wanderTarget.Y;
            
            //move the target into a position WanderDist in front of the agent
            //This creates a local target
            wanderTarget.X += wanderDistance;

            var targetWorld = vehicle.GlobalizePosition(wanderTarget);

            if (annotation != null)
            {

                CCVector2 circleCenterM = new CCVector2((vehicle.Forward.X * wanderDistance) + vehicle.Position.X,
                    (vehicle.Forward.Y * wanderDistance) + vehicle.Position.Y);
                CCVector2 pointOnCircle = new CCVector2(circleCenterM.X + wanderForward, circleCenterM.Y + wanderSide);


                annotation.CircleOrDisk(wanderRadius, CCVector2.Zero, circleCenterM, CCColor4B.Green);
                annotation.CircleOrDisk(5, CCVector2.Zero, pointOnCircle, CCColor4B.Yellow, 20, true);
            }


            return targetWorld - vehicle.Position;
        }

        //public static CCVector2 SteerForWander(this IVehicle vehicle, float dt, ref float wanderSide, ref float wanderForward, IAnnotationService annotation = null)
        //{

        //    float speed = 12 *dt; // maybe this (12) should be an argument?
        //    wanderSide = Utilities.ScalarRandomWalk(wanderSide, speed, -1, +1);
        //    wanderForward = Utilities.ScalarRandomWalk(wanderForward, speed, -1, +1);

        //    //// return a pure lateral steering vector: (+/-Side) + (+/-Up)
        //    return (vehicle.Side * wanderSide) + (vehicle.Forward * wanderForward);
        //}

        public static CCVector2 SteerForFlee(this IVehicle vehicle, CCVector2 target, float maxSpeed, IAnnotationService annotation = null)
        {
            CCVector2 offset = vehicle.Position - target;
            //CCVector2 offset = target - vehicle.Position;
            CCVector2 desiredVelocity = offset.TruncateLength(maxSpeed); //xxxnew
            return desiredVelocity - vehicle.Velocity;
        }

        public static CCVector2 SteerForSeek(this IVehicle vehicle, CCVector2 target, float maxSpeed, IAnnotationService annotation = null)
        {
            CCVector2 offset = target - vehicle.Position;
            CCVector2 desiredVelocity = offset.TruncateLength(maxSpeed); //xxxnew
            return desiredVelocity - vehicle.Velocity;
        }

        public static CCVector2 SteerForArrival(this IVehicle vehicle, CCVector2 target, float maxSpeed, float slowingDistance, IAnnotationService annotation = null)
        {
            CCVector2 offset = target - vehicle.Position;
            float distance = offset.Length();
            float rampedSpeed = maxSpeed * (distance / slowingDistance);
            float clippedSpeed = Math.Min(rampedSpeed, maxSpeed);
            CCVector2 desiredVelocity = (clippedSpeed / distance) * offset;
            return desiredVelocity - vehicle.Velocity;
        }

        public static CCVector2 SteerToFollowFlowField(this IVehicle vehicle, IFlowField flowField, float maxSpeed, float predictionDistance, IAnnotationService annotation = null)
        {
            var futurePosition = vehicle.PredictFuturePosition(predictionDistance);
            var flow = flowField.Sample(futurePosition);
            return vehicle.Velocity - flow.TruncateLength(maxSpeed);
        }

        public static CCVector2 SteerToStayOnPath(this IVehicle vehicle, float predictionTime, IPathway path, float maxSpeed, IAnnotationService annotation = null)
        {
            // predict our future position
            CCVector2 futurePosition = vehicle.PredictFuturePosition(predictionTime);

            // find the point on the path nearest the predicted future position
            CCVector2 tangent;
            float outside;
            CCVector2 onPath = path.MapPointToPath(futurePosition, out tangent, out outside);

            if (outside < 0)
                return CCVector2.Zero;    // our predicted future position was in the path, return zero steering.

            // our predicted future position was outside the path, need to
            // steer towards it.  Use onPath projection of futurePosition
            // as seek target
            if (annotation != null)
                annotation.PathFollowing(futurePosition, onPath, onPath, outside);

            return vehicle.SteerForSeek(onPath, maxSpeed);
        }

        public static CCVector2 SteerToFollowPath(this IVehicle vehicle, bool direction, float predictionTime, IPathway path, float maxSpeed, IAnnotationService annotation = null)
        {
            // our goal will be offset from our path distance by this amount
            float pathDistanceOffset = (direction ? 1 : -1) * predictionTime * vehicle.Speed;

            // predict our future position
            CCVector2 futurePosition = vehicle.PredictFuturePosition(predictionTime);

            // measure distance along path of our current and predicted positions
            float nowPathDistance = path.MapPointToPathDistance(vehicle.Position);
            float futurePathDistance = path.MapPointToPathDistance(futurePosition);

            // are we facing in the correction direction?
            bool rightway = ((pathDistanceOffset > 0) ?
                                   (nowPathDistance < futurePathDistance) :
                                   (nowPathDistance > futurePathDistance));

            // find the point on the path nearest the predicted future position
            CCVector2 tangent;
            float outside;
            CCVector2 onPath = path.MapPointToPath(futurePosition, out tangent, out outside);

            // no steering is required if (a) our future position is inside
            // the path tube and (b) we are facing in the correct direction
            if ((outside < 0) && rightway)
                return CCVector2.Zero; //all is well, return zero steering

            // otherwise we need to steer towards a target point obtained
            // by adding pathDistanceOffset to our current path position
            float targetPathDistance = nowPathDistance + pathDistanceOffset;
            CCVector2 target = path.MapPathDistanceToPoint(targetPathDistance);

            if (annotation != null)
                annotation.PathFollowing(futurePosition, onPath, target, outside);

            // return steering to seek target on path
            return SteerForSeek(vehicle, target, maxSpeed);
        }

        /// <summary>
        /// Returns a steering force to avoid a given obstacle.  The purely
        /// lateral steering force will turn our this towards a silhouette edge
        /// of the obstacle.  Avoidance is required when (1) the obstacle
        /// intersects the this's current path, (2) it is in front of the
        /// this, and (3) is within minTimeToCollision seconds of travel at the
        /// this's current velocity.  Returns a zero vector value (CCVector2::zero)
        /// when no avoidance is required.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="minTimeToCollision"></param>
        /// <param name="obstacle"></param>
        /// <param name="annotation"></param>
        /// <returns></returns>
        public static CCVector2 SteerToAvoidObstacle(this IVehicle vehicle, float minTimeToCollision, IObstacle obstacle, IAnnotationService annotation = null)
        {
            CCVector2 avoidance = obstacle.SteerToAvoid(vehicle, minTimeToCollision);

            // XXX more annotation modularity problems (assumes spherical obstacle)
            if (avoidance != CCVector2.Zero && annotation != null)
                annotation.AvoidObstacle(vehicle, minTimeToCollision * vehicle.Speed);

            return avoidance;
        }

        public static CCVector2 SteerToAvoidObstacles(this IVehicle vehicle, float minTimeToCollision, IEnumerable<IObstacle> obstacles, IAnnotationService annotation = null)
        {
            PathIntersection? nearest = null;
            float minDistanceToCollision = minTimeToCollision * vehicle.Speed;

            // test all obstacles for intersection with my forward axis,
            // select the one whose point of intersection is nearest
            foreach (var o in obstacles)
            {
                var next = o.NextIntersection(vehicle);
                if (!next.HasValue)
                    continue;

                if (!nearest.HasValue || (next.Value < nearest.Value.Distance))
                    nearest = new PathIntersection { Distance = next.Value, Obstacle = o };
            }

            if (nearest.HasValue)
            {
                if (annotation != null)
                {
//                    var no = (CircleObstacle)nearest.Value.Obstacle;
                    annotation.AvoidObstacle(vehicle, minDistanceToCollision);
//                    annotation.CircleOrDisk(no.Radius, CCVector2.Zero, no.Center,
//                        CCColor4B.AliceBlue, 20, true, false);
                }

                return nearest.Value.Obstacle.SteerToAvoid(vehicle, minTimeToCollision);
            }
            return CCVector2.Zero;
        }

        private struct PathIntersection
        {
            public float Distance;
            public IObstacle Obstacle;
        }

        public static CCVector2 SteerForSeparation(this IVehicle vehicle, float maxDistance, float cosMaxAngle, IEnumerable<IVehicle> others, IAnnotationService annotation = null)
        {
            // steering accumulator and count of neighbors, both initially zero
            CCVector2 steering = CCVector2.Zero;
            int neighbors = 0;

            // for each of the other vehicles...
            foreach (var other in others)
            {
                if (!IsInBoidNeighborhood(vehicle, other, vehicle.Radius * 3, maxDistance, cosMaxAngle))
                    continue;

                // add in steering contribution
                // (opposite of the offset direction, divided once by distance
                // to normalize, divided another time to get 1/d falloff)
                CCVector2 offset = other.Position - vehicle.Position;
                float distanceSquared = CCVector2.Dot(offset, offset);
                steering += (offset / -distanceSquared);

                // count neighbors
                neighbors++;
            }

            // divide by neighbors, then normalize to pure direction
            if (neighbors > 0)
            {
                steering = (steering / neighbors);
                steering.Normalize();
            }

            return steering;
        }

        /// <summary>
        /// avoidance of "close neighbors"
        /// </summary>
        /// <remarks>
        /// Does a hard steer away from any other agent who comes withing a
        /// critical distance.  Ideally this should be replaced with a call
        /// to steerForSeparation.
        /// </remarks>
        /// <typeparam name="TVehicle"></typeparam>
        /// <param name="vehicle"></param>
        /// <param name="minSeparationDistance"></param>
        /// <param name="others"></param>
        /// <param name="annotation"></param>
        /// <returns></returns>
        public static CCVector2 SteerToAvoidCloseNeighbors<TVehicle>(this IVehicle vehicle, float minSeparationDistance, IEnumerable<TVehicle> others, IAnnotationService annotation = null)
            where TVehicle : IVehicle
        {
            // for each of the other vehicles...
            foreach (IVehicle other in others)
            {
                if (other != vehicle)
                {
                    float sumOfRadii = vehicle.Radius + other.Radius;
                    float minCenterToCenter = minSeparationDistance + sumOfRadii;
                    CCVector2 offset = other.Position - vehicle.Position;
                    float currentDistance = offset.Length();

                    if (currentDistance < minCenterToCenter)
                    {
                        if (annotation != null)
                            annotation.AvoidCloseNeighbor(other, minSeparationDistance);

                        return CCVector2Helpers.PerpendicularComponent(-offset, vehicle.Forward);
                    }
                }
            }

            // otherwise return zero
            return CCVector2.Zero;
        }

        public static CCVector2 SteerForAlignment(this IVehicle vehicle, float maxDistance, float cosMaxAngle, IEnumerable<IVehicle> flock, IAnnotationService annotation = null)
        {
            // steering accumulator and count of neighbors, both initially zero
            CCVector2 steering = CCVector2.Zero;
            int neighbors = 0;

            // for each of the other vehicles...
            foreach (IVehicle other in flock.Where(other => vehicle.IsInBoidNeighborhood(other, vehicle.Radius * 3, maxDistance, cosMaxAngle)))
            {
                // accumulate sum of neighbor's heading
                steering += other.Forward;

                // count neighbors
                neighbors++;
            }

            // divide by neighbors, subtract off current heading to get error-
            // correcting direction, then normalize to pure direction
            if (neighbors > 0)
            {
                steering = ((steering / neighbors) - vehicle.Forward);

                var length = steering.Length();
                if (length > 0.025f)
                    steering /= length;
            }

            return steering;
        }

        public static CCVector2 SteerForCohesion(this IVehicle vehicle, float maxDistance, float cosMaxAngle, IEnumerable<IVehicle> flock, IAnnotationService annotation = null)
        {
            // steering accumulator and count of neighbors, both initially zero
            CCVector2 steering = CCVector2.Zero;
            int neighbors = 0;

            // for each of the other vehicles...
            foreach (IVehicle other in flock.Where(other => vehicle.IsInBoidNeighborhood(other, vehicle.Radius * 3, maxDistance, cosMaxAngle)))
            {
                // accumulate sum of neighbor's positions
                steering += other.Position;

                // count neighbors
                neighbors++;
            }

            // divide by neighbors, subtract off current position to get error-
            // correcting direction, then normalize to pure direction
            if (neighbors > 0)
            {
                steering = ((steering / neighbors) - vehicle.Position);
                steering.Normalize();
            }

            return steering;
        }

//        private readonly static float[,] _pursuitFactors = new float[3, 3]
//        {
//            { 2, 2, 0.5f },         //Behind
//            { 4, 0.8f, 1 },         //Aside
//            { 0.85f, 1.8f, 4 },     //Ahead
//        };

        private readonly static float[,] _pursuitFactors = new float[3, 3]
            {
                { 2, 2, 0.5f },         //Behind
                { 4, 0.8f, 1 },         //Aside
                { 0.85f, 1.8f, 4 },     //Ahead
            };

        public static CCVector2 SteerForPursuit(this IVehicle vehicle, IVehicle quarry, float maxPredictionTime, float maxSpeed, IAnnotationService annotation = null)
        {
            // offset from this to quarry, that distance, unit vector toward quarry
            CCVector2 offset = quarry.Position - vehicle.Position;
            float distance = offset.Length();
            CCVector2 unitOffset = offset / distance;

            // how parallel are the paths of "this" and the quarry
            // (1 means parallel, 0 is pependicular, -1 is anti-parallel)
            float parallelness = CCVector2.Dot(vehicle.Forward, quarry.Forward);

            // how "forward" is the direction to the quarry
            // (1 means dead ahead, 0 is directly to the side, -1 is straight back)
            float forwardness = CCVector2.Dot(vehicle.Forward, unitOffset);

            float directTravelTime = distance / Math.Max(0.001f, vehicle.Speed);
            int f = Utilities.IntervalComparison(forwardness, -0.707f, 0.707f);
            int p = Utilities.IntervalComparison(parallelness, -0.707f, 0.707f);

            // Break the pursuit into nine cases, the cross product of the
            // quarry being [ahead, aside, or behind] us and heading
            // [parallel, perpendicular, or anti-parallel] to us.
            float timeFactor = _pursuitFactors[f + 1, p + 1];

            // estimated time until intercept of quarry
            float et = directTravelTime * timeFactor;

            // xxx experiment, if kept, this limit should be an argument
            float etl = (et > maxPredictionTime) ? maxPredictionTime : et;

            // estimated position of quarry at intercept
            CCVector2 target = quarry.PredictFuturePosition(etl);

            // annotation
//            if (annotation != null)
//                annotation.Line(vehicle.Position, target, CCColor4B.Gray);

            return SteerForSeek(vehicle, target, maxSpeed, annotation);
        }

        public static CCVector2 SteerForEvasion(this IVehicle vehicle, IVehicle menace, float maxPredictionTime, float maxSpeed, IAnnotationService annotation = null)
        {
            // offset from this to menace, that distance, unit vector toward menace
            CCVector2 offset = menace.Position - vehicle.Position;
            float distance = offset.Length();

            float roughTime = distance / menace.Speed;
            float predictionTime = ((roughTime > maxPredictionTime) ? maxPredictionTime : roughTime);

            CCVector2 target = menace.PredictFuturePosition(predictionTime);

            return SteerForFlee(vehicle, target, maxSpeed, annotation);
        }

        /// <summary>
        /// tries to maintain a given speed, returns a maxForce-clipped steering
        /// force along the forward/backward axis
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="targetSpeed"></param>
        /// <param name="maxForce"></param>
        /// <param name="annotation"></param>
        /// <returns></returns>
        public static CCVector2 SteerForTargetSpeed(this IVehicle vehicle, float targetSpeed, float maxForce, IAnnotationService annotation = null)
        {
            float mf = maxForce;
            float speedError = targetSpeed - vehicle.Speed;
            return vehicle.Forward * CCMathHelper.Clamp(speedError, -mf, +mf);
        }

        /// <summary>
        /// Unaligned collision avoidance behavior: avoid colliding with other
        /// nearby vehicles moving in unconstrained directions.  Determine which
        /// (if any) other other this we would collide with first, then steers
        /// to avoid the site of that potential collision.  Returns a steering
        /// force vector, which is zero length if there is no impending collision.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="minTimeToCollision"></param>
        /// <param name="others"></param>
        /// <param name="annotation"></param>
        /// <returns></returns>
        public static CCVector2 SteerToAvoidNeighbors(this IVehicle vehicle, float minTimeToCollision, IEnumerable<IVehicle> others, IAnnotationService annotation = null)
        {
            // first priority is to prevent immediate interpenetration
            CCVector2 separation = SteerToAvoidCloseNeighbors(vehicle, 0, others, annotation);
            if (separation != CCVector2.Zero)
                return separation;

            // otherwise, go on to consider potential future collisions
            float steer = 0;
            IVehicle threat = null;

            // Time (in seconds) until the most immediate collision threat found
            // so far.  Initial value is a threshold: don't look more than this
            // many frames into the future.
            float minTime = minTimeToCollision;

            // xxx solely for annotation
            CCVector2 xxxThreatPositionAtNearestApproach = CCVector2.Zero;
            CCVector2 xxxOurPositionAtNearestApproach = CCVector2.Zero;

            // for each of the other vehicles, determine which (if any)
            // pose the most immediate threat of collision.
            foreach (IVehicle other in others)
            {
                if (other != vehicle)
                {
                    // avoid when future positions are this close (or less)
                    float collisionDangerThreshold = vehicle.Radius * 2;

                    // predicted time until nearest approach of "this" and "other"
                    float time = PredictNearestApproachTime(vehicle, other);

                    // If the time is in the future, sooner than any other
                    // threatened collision...
                    if ((time >= 0) && (time < minTime))
                    {
                        // if the two will be close enough to collide,
                        // make a note of it
                        if (ComputeNearestApproachPositions(vehicle, other, time) < collisionDangerThreshold)
                        {
                            minTime = time;
                            threat = other;
                        }
                    }
                }
            }

            // if a potential collision was found, compute steering to avoid
            if (threat != null)
            {
                // parallel: +1, perpendicular: 0, anti-parallel: -1
                float parallelness = CCVector2.Dot(vehicle.Forward, threat.Forward);
                const float angle = 0.707f;

                if (parallelness < -angle)
                {
                    // anti-parallel "head on" paths:
                    // steer away from future threat position
                    CCVector2 offset = xxxThreatPositionAtNearestApproach - vehicle.Position;
                    float sideDot = CCVector2.Dot(offset, vehicle.Side);
                    steer = (sideDot > 0) ? -1.0f : 1.0f;
                }
                else
                {
                    if (parallelness > angle)
                    {
                        // parallel paths: steer away from threat
                        CCVector2 offset = threat.Position - vehicle.Position;
                        float sideDot = CCVector2.Dot(offset, vehicle.Side);
                        steer = (sideDot > 0) ? -1.0f : 1.0f;
                    }
                    else
                    {
                        // perpendicular paths: steer behind threat
                        // (only the slower of the two does this)
                        if (threat.Speed <= vehicle.Speed)
                        {
                            float sideDot = CCVector2.Dot(vehicle.Side, threat.Velocity);
                            steer = (sideDot > 0) ? -1.0f : 1.0f;
                        }
                    }
                }

                if (annotation != null)
                    annotation.AvoidNeighbor(threat, steer, xxxOurPositionAtNearestApproach, xxxThreatPositionAtNearestApproach);
            }

            return vehicle.Side * steer;
        }

        /// <summary>
        /// Given two vehicles, based on their current positions and velocities,
        /// determine the time until nearest approach
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        private static float PredictNearestApproachTime(IVehicle vehicle, IVehicle other)
        {
            // imagine we are at the origin with no velocity,
            // compute the relative velocity of the other this
            CCVector2 myVelocity = vehicle.Velocity;
            CCVector2 otherVelocity = other.Velocity;
            CCVector2 relVelocity = otherVelocity - myVelocity;
            float relSpeed = relVelocity.Length();

            // for parallel paths, the vehicles will always be at the same distance,
            // so return 0 (aka "now") since "there is no time like the present"
            if (Math.Abs(relSpeed - 0) < float.Epsilon)
                return 0;

            // Now consider the path of the other this in this relative
            // space, a line defined by the relative position and velocity.
            // The distance from the origin (our this) to that line is
            // the nearest approach.

            // Take the unit tangent along the other this's path
            CCVector2 relTangent = relVelocity / relSpeed;

            // find distance from its path to origin (compute offset from
            // other to us, find length of projection onto path)
            CCVector2 relPosition = vehicle.Position - other.Position;
            float projection = CCVector2.Dot(relTangent, relPosition);

            return projection / relSpeed;
        }

        /// <summary>
        /// Given the time until nearest approach (predictNearestApproachTime)
        /// determine position of each this at that time, and the distance
        /// between them
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="other"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private static float ComputeNearestApproachPositions(IVehicle vehicle, IVehicle other, float time)
        {
            CCVector2 myTravel = vehicle.Forward * vehicle.Speed * time;
            CCVector2 otherTravel = other.Forward * other.Speed * time;

            CCVector2 myFinal = vehicle.Position + myTravel;
            CCVector2 otherFinal = other.Position + otherTravel;

            return CCVector2.Distance(myFinal, otherFinal);
        }

        public static bool IsAhead(this IVehicle vehicle, CCVector2 target, float cosThreshold = 0.707f)
        {
            CCVector2 targetDirection = (target - vehicle.Position);
            targetDirection.Normalize();
            return CCVector2.Dot(vehicle.Forward, targetDirection) > cosThreshold;
        }

        public static bool IsAside(this IVehicle vehicle, CCVector2 target, float cosThreshold = 0.707f)
        {
            CCVector2 targetDirection = (target - vehicle.Position);
            targetDirection.Normalize();
            float dp = CCVector2.Dot(vehicle.Forward, targetDirection);
            return (dp < cosThreshold) && (dp > -cosThreshold);
        }

        public static bool IsBehind(this IVehicle vehicle, CCVector2 target, float cosThreshold = -0.707f)
        {
            CCVector2 targetDirection = (target - vehicle.Position);
            targetDirection.Normalize();
            return CCVector2.Dot(vehicle.Forward, targetDirection) < cosThreshold;
        }

        private static bool IsInBoidNeighborhood(this ILocalSpaceBasis vehicle, ILocalSpaceBasis other, float minDistance, float maxDistance, float cosMaxAngle)
        {
            if (other == vehicle)
                return false;

            CCVector2 offset = other.Position - vehicle.Position;
            float distanceSquared = offset.LengthSquared();

            // definitely in neighborhood if inside minDistance sphere
            if (distanceSquared < (minDistance * minDistance))
                return true;

            // definitely not in neighborhood if outside maxDistance sphere
            if (distanceSquared > (maxDistance * maxDistance))
                return false;

            // otherwise, test angular offset from forward axis
            CCVector2 unitOffset = offset / (float)Math.Sqrt(distanceSquared);
            float forwardness = CCVector2.Dot(vehicle.Forward, unitOffset);
            return forwardness > cosMaxAngle;
        }
    }
}
