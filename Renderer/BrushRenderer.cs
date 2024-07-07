using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QESilver.Model;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Windows.Interop;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace QESilver.Renderer
{
	internal class BrushRenderer : IDisposable
	{
		private Shader _shFlat;

		private int _vertexBufferObject;
		private int _vertexArrayObject;
		private int _elementBufferObject;
		private int _count;
		private int _elements;

		private readonly int _attribPos = 0;
		private readonly int _attribUV = 1;
		private readonly int _attribShade = 2;

		public BrushRenderer(List<Brush> brushes)
		{
			_shFlat = new Shader("shaders/default_v.shader", "shaders/flat_f.shader");

			int[] indices = new int[512];
			int i = 0;

			foreach (Brush b in brushes)
			{
				foreach (Face f in b.Faces)
				{
					for (int j = 0; j < f.Winding.Count; ++j)
					{
						indices[i++] = f.Winding.Index + j;
					}
					indices[i++] = int.MaxValue;
				}
			}
			_elements = i - 1;

			_vertexBufferObject = GL.GenBuffer();
			_elementBufferObject = GL.GenBuffer();
			_vertexArrayObject = GL.GenVertexArray();

			GL.BindVertexArray(_vertexArrayObject);
			GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
			GL.BufferData(BufferTarget.ArrayBuffer, Winding.PoolCapacity * sizeof(float), Winding.PoolStart, BufferUsageHint.StaticDraw);
			GL.BufferData(BufferTarget.ElementArrayBuffer, _elements * sizeof(int), indices, BufferUsageHint.StaticDraw);

			
		}

		public void Bind()
		{
			GL.BindVertexArray(_vertexArrayObject);
			GL.VertexAttribPointer(_attribPos, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
			GL.EnableVertexAttribArray(_attribPos);
			GL.VertexAttribPointer(_attribUV, 2, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
			GL.EnableVertexAttribArray(_attribUV);
			GL.VertexAttribPointer(_attribShade, 1, VertexAttribPointerType.Float, false, 6 * sizeof(float), 5 * sizeof(float));
			GL.EnableVertexAttribArray(_attribShade);

			GL.Enable(EnableCap.PrimitiveRestart);
			GL.PrimitiveRestartIndex(int.MaxValue);
		}

		internal void Render(float asp)
		{
			//float asp = _width / _height;
			float vfov = MathHelper.DegreesToRadians(60f);

			Matrix4 matpersp = Matrix4.CreatePerspectiveFieldOfView(vfov, asp, 1f, 2000.0f);
			Matrix4 matview = Matrix4.LookAt(Vector3.Zero, new Vector3(1,1,0), Vector3.UnitZ);
			Matrix4 matmodel = Matrix4.CreateRotationZ(0);

			Bind();

			_shFlat.Use();
			_shFlat.SetUniform("model", matmodel);
			_shFlat.SetUniform("view", matview);
			_shFlat.SetUniform("projection", matpersp);

			GL.DrawElements(PrimitiveType.TriangleFan, _elements, DrawElementsType.UnsignedInt, 0);
			
		}












		private bool _disposed;
		public void Dispose()
		{
			if (_disposed) return;

			// delete stuff

			_disposed = true;
		}
	}
}
