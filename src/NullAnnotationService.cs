
using CocosSharp;

namespace CocosSharpSteer
{
    class NullAnnotationService
        :IAnnotationService
    {
        public bool IsEnabled
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public void Line(CCVector2 startPoint, CCVector2 endPoint, CCColor4B color, float opacity = 1)
        {

        }

        public void CircleXZ(float radius, CCVector2 center, CCColor4B color, int segments)
        {

        }

        public void DiskXZ(float radius, CCVector2 center, CCColor4B color, int segments)
        {

        }

        public void Circle3D(float radius, CCVector2 center, CCVector2 axis, CCColor4B color, int segments)
        {

        }

        public void Disk3D(float radius, CCVector2 center, CCVector2 axis, CCColor4B color, int segments)
        {

        }

        public void CircleOrDiskXZ(float radius, CCVector2 center, CCColor4B color, int segments, bool filled)
        {

        }

        public void CircleOrDisk3D(float radius, CCVector2 center, CCVector2 axis, CCColor4B color, int segments, bool filled)
        {

        }

        public void CircleOrDisk(float radius, CCVector2 axis, CCVector2 center, CCColor4B color, int segments, bool filled, bool in3D)
        {

        }

        public void AvoidObstacle(IVehicle vehicle, float minDistanceToCollision)
        {

        }

        public void PathFollowing(CCVector2 future, CCVector2 onPath, CCVector2 target, float outside)
        {

        }

        public void AvoidCloseNeighbor(IVehicle other, float additionalDistance)
        {

        }

        public void AvoidNeighbor(IVehicle threat, float steer, CCVector2 ourFuture, CCVector2 threatFuture)
        {

        }

        public void VelocityAcceleration(IVehicle vehicle)
        {

        }

        public void VelocityAcceleration(IVehicle vehicle, float maxLength)
        {

        }

        public void VelocityAcceleration(IVehicle vehicle, float maxLengthAcceleration, float maxLengthVelocity)
        {

        }
    }
}
