using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CocosSharp;

namespace SteeringDemo.PlugIns.Ctf
{ 
	public class CtfNode : CCDrawNode
	{

        private float radius;
        private CCColor4B bodyColor;
        private bool isDirty;

        public CtfNode(float radius) : base()
        {
            Radius = radius;
            BodyColor = CCColor4B.White;

        }

        public float Radius
        {
            get { return radius; }
            set 
            {
                if (radius != value)
                {
                    radius = value;
                    isDirty = true;
                }
            }
        }

        public CCColor4B BodyColor
        {
            get { return bodyColor; }
            set
            {
                if (bodyColor != value)
                {
                    bodyColor = value;
                    isDirty = true;
                }
            }
        }

        public void RedrawNode()
        {
            if (isDirty)
            {
                Clear();

                DrawCircle(CCPoint.Zero, Radius, CCColor4B.White);

                var r = Radius;
                const float x = 0.5f;
                float y = (float)Math.Sqrt(1 - (x * x));

                CCVector2 u = new CCVector2(-1, 0) * r * 0.05f;
                CCVector2 f = new CCVector2(1, 0) * r;
                CCVector2 s = new CCVector2(0, 1) * -x * r;
                CCVector2 b = new CCVector2(-1, 0) * y * r;

                DrawPolygon(new CCPoint[] { f + u, b - s, b + s }, 3, BodyColor, 0, CCColor4B.Transparent);

                isDirty = false;
            }
        }

        protected override void Draw()
        {
            if (isDirty)
                RedrawNode();

            base.Draw();
        }
    }
}
