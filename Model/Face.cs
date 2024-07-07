using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using OpenTK.Mathematics;
using QESilver.Renderer;

namespace QESilver.Model
{
    class Face
    {
		public Brush? Owner;
		public Plane Plane;
		public Winding Winding;
		public Face()
		{
			Owner = null;
			Plane = new Plane();
			Winding = new Winding();
		}

		public bool Build()
		{
			bool past;
			Plane p = new();

			// get a poly that covers an effectively infinite area
			Winding.Base(Plane, 4096);

			// chop the poly by all of the other faces
			past = false;
			foreach (Face clip in Owner.Faces)//; clip && winding.Count(); clip = clip->fnext)
			{
				if (clip == this)
				{
					past = true;
					continue;
				}

				if (Vector3d.Dot(Plane.Normal, clip.Plane.Normal) > 0.999 &&
					Math.Abs(Plane.Distance - clip.Plane.Distance) < 0.01)
				{   // identical plane, use the later one
					if (past)
					{
						Winding.Free();
						return false;
					}
					continue;
				}

				// flip the plane, because we want to keep the back side
				p.Normal = Vector3d.Zero - clip.Plane.Normal;
				p.Distance = -clip.Plane.Distance;

				if (!Winding.Clip(ref p, false))
					return false;
			}

			if (Winding.Count < 3)
			{
				Winding.Free();
				return false;
			}

			return true;
		}
    }
}
