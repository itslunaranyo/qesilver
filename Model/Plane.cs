using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using OpenTK.Mathematics;

namespace QESilver.Model
{
	public struct Plane
    {
		public Vector3d[] Points;
		public Vector3d Normal;
		public double Distance;

		public Plane()
		{
			Points = new Vector3d[3];
			Normal = new Vector3d(0, 0, 0);
			Distance = 0;
		}

		public bool Make()
		{
			Vector3d norm;
			norm = Vector3d.Cross(Points[0] - Points[1], Points[2] - Points[1]);
			if ((norm == Vector3d.Zero) || Utility.Normalize(ref norm) < 0.05)
			{
				Debug.Write("Brush plane with no normal");	// warning
				return false;
			}
			Normal = norm;
			Distance = Vector3d.Dot(Points[1], Normal);
			return true;
		}
	}
}
