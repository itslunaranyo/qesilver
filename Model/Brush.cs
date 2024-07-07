using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QESilver.Model
{
    class Brush
    {
		public List<Face> Faces;
		public Entity? Owner;

		public Brush()
		{
			Faces = new List<Face>();
			Owner = null;
		}

		public void AddFace(Face f)
		{
			if (f.Owner == this) return;
			Faces.Add(f);
			f.Owner = this;
		}

		public void Build()
		{
			foreach (Face f in Faces)
			{
				if (!f.Build())
					continue;

				// add to bounding box
				//f.AddBounds(mins, maxs);

				// setup s and t vectors, and set color
				//f.ColorAndTexture();
			}
		}
	}
}
