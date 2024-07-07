using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Media3D;
using OpenTK.Mathematics;
using QESilver.Model;

namespace QESilver.Renderer
{
    struct Winding
    {
		public int Count { get { return _count; } }

		public Winding()
		{
			if (_pool == null)
				Allocate(16384 * 4, false);

			_count = 0;
			_maxPts = 0;
			_index = 0x7FFFFFFF;
		}

		public void Base(Plane p, double extents)
		{
			Vector3d org, vup, vright;
			org = p.Normal * p.Distance;
			if (Math.Abs(p.Normal.Z) < 0.7)
				vup = Vector3d.Cross(p.Normal, Vector3d.UnitZ);
			else
				vup = Vector3d.Cross(p.Normal, Vector3d.UnitX);
			vright = Vector3d.Cross(vup, p.Normal);

			vup = vup * extents;
			vright = vright * extents;

			Grow(4);

			var vertices = _pool.AsSpan<Vertex>().Slice(_index, _maxPts);

			vertices[0].xyz = (Vector3)(org - vright + vup);
			vertices[1].xyz = (Vector3)(org + vright + vup);
			vertices[2].xyz = (Vector3)(org + vright - vup);
			vertices[3].xyz = (Vector3)(org - vright - vup);

			Soil(_index, _index + _count);
			_count = 4;
		}

		const int MAX_POINTS_ON_WINDING = 255;
		enum PlaneSide { Front = 0, Back = 1, On = 2 };

		/*
		==================
		Winding::Clip

		Clips the winding to the plane, leaving the remainder on the positive side
		If keepon is true, an exactly on-plane winding will be saved, otherwise
		it will be clipped away.
		Returns false if winding was clipped away.
		==================
		*/
		public bool Clip(ref Plane split, bool keepon)
		{
			int i, j;
			int[] counts = new int[3];
			int[] sides = new int[MAX_POINTS_ON_WINDING];
			double[] dists = new double[MAX_POINTS_ON_WINDING];
			double dot;
			Vector3d p1, p2, mid;

			// scratch max-size winding for working in place
			Vertex[] newWinding = new Vertex[MAX_POINTS_ON_WINDING];
			byte nwCount = 0;

			counts[(int)PlaneSide.Front] = counts[(int)PlaneSide.Back] = counts[(int)PlaneSide.On] = 0;
			var vertices = _pool.AsSpan<Vertex>().Slice(_index, _maxPts);

			// determine sides for each point
			for (i = 0; i < _count; i++)
			{
				dot = Vector3d.Dot(vertices[i].xyz, split.Normal);
				dot -= split.Distance;
				dists[i] = dot;

				if (dot > 0.01)	// ON_EPSILON
					sides[i] = (int)PlaneSide.Front;
				else if (dot< -0.01)    // ON_EPSILON
					sides[i] = (int)PlaneSide.Back;
				else
					sides[i] = (int)PlaneSide.On;

				counts[sides[i]]++;
			}

			sides[i] = sides[0];
			dists[i] = dists[0];
	
			if (keepon && counts[(int)PlaneSide.Front] == 0 && counts[(int)PlaneSide.Back] == 0)
				return true;	// coplanar to splitting plane

			if (counts[(int)PlaneSide.Front] == 0)
			{
				Free();
				return false;	// no positive points, winding clipped away
			}

			if (counts[(int)PlaneSide.Back] == 0)
				return true;    // splitting plane didn't touch us

			for (i = 0; i < _count; i++)
			{
				p1 = vertices[i].xyz;

				if (sides[i] == (int)PlaneSide.On)
				{
					newWinding[nwCount].xyz = (Vector3)p1;
					nwCount++;
					continue;
				}

				if (sides[i] == (int)PlaneSide.Front)
				{
					newWinding[nwCount].xyz = (Vector3)p1;
					nwCount++;
				}

				if (sides[i + 1] == (int)PlaneSide.On || sides[i + 1] == sides[i])
					continue;

				// generate a split point
				p2 = vertices[(i + 1) % _count].xyz;

				dot = dists[i] / (dists[i] - dists[i + 1]);

				mid = p1;
				for (j = 0; j < 3; j++)
					mid[j] = p1[j] + dot * (p2[j] - p1[j]);

				newWinding[nwCount].xyz = (Vector3)mid;
					nwCount++;
			}

			if (nwCount > _maxPts)
				Grow(nwCount);   // we need a bigger boat

			for (i = 0; i < nwCount; ++i)
				vertices[i] = newWinding[i];
			_count = nwCount;

			Soil(_index, _index + _count);
			return true;
		}

		public override string ToString()
		{
			string str = "";

			for (int i = 0; i < _count; i++)
			{
				str += _pool[_index + i].ToString();
			}

			return str + "\n";
		}

		// ========================

		private byte _count, _maxPts;	// measured in singular vertices
		private int _index;				// offset of first vert
		internal int Index { get { return _index; } }

		internal struct Vertex
		{
			public Vector3 xyz;
			public Vector2 st;
			public float shade;
			public override string ToString()
			{
				return string.Format("{0} {1} {2}, ", xyz.X, xyz.Y, xyz.Z);
			}
		}
		private const int _segSize = 4;
		bool IsHighmost() { return (_index + ((_maxPts % _segSize) + _segSize) == _margin); }

		public void Free()
		{
			_count = 0;
		}

		private void Grow(int minPoints, bool noFrags = false)
		{
			if (_maxPts >= minPoints) return;

			// we always alloc in segments of 4: 95% of brushes have only
			//	quads for all faces, and stay that way forever, and a double 
			//	alloc of 8 verts covers 95% of the rest
			int newSize = (minPoints % _segSize) + _segSize;

			// if we're the last winding below the high water mark, just get bigger
			if (IsHighmost())
			{
				_margin = _index + newSize;
				_maxPts = (byte)newSize;  // don't use vMinPoints, not guaranteed to be on seg boundary
				return;
			}

			_index = _margin;
			_maxPts = (byte)newSize;
			_margin += newSize;
		}

		// ========================

		private static int _margin;
		private static Vertex[]? _pool;
		private static int _dirtyLow, _dirtyHigh;
		public static bool IsPoolDirty { get { return _dirtyLow < _dirtyHigh; } }
		public static Vertex[] PoolStart { get { return _pool; } }
		public static int PoolCapacity { get { return _pool.Length; } }

		private static void Allocate(int size, bool flush)
		{
			_pool = new Vertex[size];
			_dirtyLow = int.MaxValue;
			_dirtyHigh = int.MinValue;
		}






		private static void Soil(int vmin, int vmax)
		{
			_dirtyLow = int.Min(vmin, _dirtyLow);
			_dirtyHigh = int.Max(vmax, _dirtyHigh);
		}
	}
}
