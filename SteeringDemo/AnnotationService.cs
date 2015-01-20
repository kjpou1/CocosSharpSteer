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
using CocosSharpSteer;
using SteeringDemo.PlugIns.Ctf;

namespace SteeringDemo
{
	public sealed class Annotation : IAnnotationService
	{
		bool _isEnabled;

	    //HACK: change the IDraw to a IDrawService
		public static Drawing Drawer;

		// constructor
		public Annotation()
		{
			_isEnabled = true;
		}

		/// <summary>
		/// Indicates whether annotation is enabled.
		/// </summary>
		public bool IsEnabled
		{
			get { return _isEnabled; }
			set { _isEnabled = value; }
		}

		// ------------------------------------------------------------------------
		// drawing of lines, circles and (filled) disks to annotate steering
		// behaviors.  When called during OpenSteerDemo's simulation update phase,
		// these functions call a "deferred draw" routine which buffer the
		// arguments for use during the redraw phase.
		//
		// note: "circle" means unfilled
		//       "disk" means filled
		//       "XZ" means on a plane parallel to the X and Z axes (perp to Y)
		//       "3d" means the circle is perpendicular to the given "axis"
		//       "segments" is the number of line segments used to draw the circle

		// draw an opaque colored line segment between two locations in space
		public void Line(CCVector2 startPoint, CCVector2 endPoint, CCColor4B color, float opacity = 1)
		{
			if (_isEnabled && Drawer != null)
			{
				Drawer.Line(startPoint, endPoint, color, opacity);
			}
		}

		// draw a circle on the XZ plane
		public void CircleXZ(float radius, CCVector2 center, CCColor4B color, int segments)
		{
			CircleOrDiskXZ(radius, center, color, segments, false);
		}

		// draw a disk on the XZ plane
		public void DiskXZ(float radius, CCVector2 center, CCColor4B color, int segments)
		{
			CircleOrDiskXZ(radius, center, color, segments, true);
		}

		// draw a circle perpendicular to the given axis
		public void Circle3D(float radius, CCVector2 center, CCVector2 axis, CCColor4B color, int segments)
		{
			CircleOrDisk3D(radius, center, axis, color, segments, false);
		}

		// draw a disk perpendicular to the given axis
		public void Disk3D(float radius, CCVector2 center, CCVector2 axis, CCColor4B color, int segments)
		{
			CircleOrDisk3D(radius, center, axis, color, segments, true);
		}

		// ------------------------------------------------------------------------
		// support for annotation circles
		public void CircleOrDiskXZ(float radius, CCVector2 center, CCColor4B color, int segments, bool filled)
		{
			CircleOrDisk(radius, CCVector2.Zero, center, color, segments, filled, false);
		}

		public void CircleOrDisk3D(float radius, CCVector2 center, CCVector2 axis, CCColor4B color, int segments, bool filled)
		{
			CircleOrDisk(radius, axis, center, color, segments, filled, true);
		}

		public void CircleOrDisk(float radius, CCVector2 axis, CCVector2 center, CCColor4B color, int segments, bool filled, bool in3D)
		{
			if (_isEnabled && Drawer != null)
			{
				Drawer.CircleOrDisk(radius, axis, center, color, segments, filled, in3D);
			}
		}

		// called when steerToAvoidObstacles decides steering is required
		// (default action is to do nothing, layered classes can overload it)
		public void AvoidObstacle(IVehicle vehicle, float minDistanceToCollision)
		{
		}

		// called when steerToFollowPath decides steering is required
		// (default action is to do nothing, layered classes can overload it)
		public void PathFollowing(CCVector2 future, CCVector2 onPath, CCVector2 target, float outside)
		{
		}

		// called when steerToAvoidCloseNeighbors decides steering is required
		// (default action is to do nothing, layered classes can overload it)
		public void AvoidCloseNeighbor(IVehicle other, float additionalDistance)
		{
		}

		// called when steerToAvoidNeighbors decides steering is required
		// (default action is to do nothing, layered classes can overload it)
		public void AvoidNeighbor(IVehicle threat, float steer, CCVector2 ourFuture, CCVector2 threatFuture)
		{
		}

		public void VelocityAcceleration(IVehicle vehicle)
		{
			VelocityAcceleration(vehicle, vehicle.MaxForce , vehicle.MaxSpeed);
		}

		public void VelocityAcceleration(IVehicle vehicle, float maxLength)
		{
			VelocityAcceleration(vehicle, maxLength, maxLength);
		}

		public void VelocityAcceleration(IVehicle vehicle, float maxLengthAcceleration, float maxLengthVelocity)
		{
			const byte desat = 102;
			CCColor4B vCCColor4B = new CCColor4B(255, desat, 255); // pinkish
			CCColor4B aCCColor4B = new CCColor4B(desat, desat, 255); // bluish

            float aScale = maxLengthAcceleration / vehicle.MaxForce;
            float vScale = maxLengthVelocity / vehicle.MaxSpeed;
			CCVector2 p = vehicle.Position;

			Line(p, p + (vehicle.Velocity * vScale), vCCColor4B);
			Line(p, p + (vehicle.Acceleration * aScale), aCCColor4B);
		}
	}
}
