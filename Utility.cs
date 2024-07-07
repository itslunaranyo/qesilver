using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Resources;
using OpenTK.Mathematics;

namespace QESilver
{
    static class Utility
	{
		public static double Normalize(ref Vector3d v) {
			double ox = v.X;
			v = Vector3d.Normalize(v);
			return ox / v.X;
		}
		public static float Normalize(ref Vector3 v)
		{
			float ox = v.X;
			v = Vector3.Normalize(v);
			return ox / v.X;
		}

		public static double Length(Vector3d v) { return v.X / Vector3d.Normalize(v).X; }
		public static float Length(Vector3 v) { return v.X / Vector3.Normalize(v).X; }

		public static bool TryParse(ReadOnlySpan<char> str, ref Vector3 v)
		{
			if (str.IsEmpty)
				return false;

			int start, end;
			start = 0;
			end = str.Length;
			while (Char.IsWhiteSpace(str[start]))
				++start;
			for (int i = 0; i < 3; ++i)
			{
				end = start;
				while (!Char.IsWhiteSpace(str[end]))
					++end;
				if (!float.TryParse(str.Slice(start, end - start), out float t))
					return false;
				v[i] = t;
				start = end;
			}
			return true;
		}
		public static bool TryParse(ReadOnlySpan<char> str, ref Vector3d v)
		{
			if (str.IsEmpty)
				return false;

			int start, end;
			start = 0;
			end = str.Length;
			while (Char.IsWhiteSpace(str[start]) && start < str.Length-1)
				++start;
			for (int i = 0; i < 3; ++i)
			{
				end = start;
				while (!Char.IsWhiteSpace(str[end]) && end < str.Length-1)
					++end;
				if (!double.TryParse(str.Slice(start, end - start), out double t))
					return false;
				v[i] = t;
				start = end;
				while (Char.IsWhiteSpace(str[start]) && start < str.Length-1)
					++start;
			}
			return true;
		}
		public static string ReadResourceText(string path)
		{
			StreamResourceInfo srInfo = App.GetResourceStream(new Uri(path, UriKind.Relative));
			StreamReader sr = new StreamReader(srInfo.Stream);
			string read = sr.ReadToEnd();
			return read;
		}
	}
}
