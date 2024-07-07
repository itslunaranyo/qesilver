using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Wpf;
//using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Security.Cryptography;
using System.Windows.Input;

namespace QESilver.Renderer
{
	public partial class CameraWindow : GLWpfControl
	{
		BrushRenderer? _brushRenderer;

		public CameraWindow()
		{
			var settings = new GLWpfControlSettings
			{
				MajorVersion = 3,
				MinorVersion = 3,
				Profile = OpenTK.Windowing.Common.ContextProfile.Core
			};
			Focusable = false;
			Start(settings);

			Render += OnRender;
			SizeChanged += new SizeChangedEventHandler(OnSizeChanged);

			GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Front);

			//MouseMove += new MouseEventHandler(OnMouseMove);
			//MouseDown += new MouseButtonEventHandler(OnMouseDown);
		}
		/*
		public void SetMode(int mode)
		{
			_brushRenderer.SetMode(mode);
		}

		internal void LoadModel(ModelAsset mdl)
		{
			_brushRenderer.DisplayModel(mdl);
			_brushRenderer.Resize((int)ActualWidth, (int)ActualHeight);
			Camera.Reset(mdl.CenterOfFrame(0), mdl.RadiusOfFrame(0));
		}
		*/

		internal void SetBR(BrushRenderer br)
		{
			_brushRenderer = br;
		}

		public void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			//_brushRenderer.Resize((int)ActualWidth, (int)ActualHeight);
			GL.Viewport(0, 0, (int)ActualWidth, (int)ActualHeight);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		}

		public void OnRender(TimeSpan delta)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			GL.Viewport(0, 0, (int)ActualWidth, (int)ActualHeight);
			_brushRenderer?.Render((float)ActualWidth/(float)ActualHeight);
		}
		public void OnUnload(object sender, RoutedEventArgs e)
		{
			_brushRenderer?.Dispose();
		}
		/*
		private System.Windows.Point _mousePos;
		private bool _buttonDownLeft;
		private bool _buttonDownRight;
		private bool _buttonDownMiddle;

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ButtonState == MouseButtonState.Pressed)
			{
				if (e.ChangedButton == MouseButton.Left)   _buttonDownLeft = true;
				if (e.ChangedButton == MouseButton.Right)  _buttonDownRight = true;
				if (e.ChangedButton == MouseButton.Middle) _buttonDownMiddle = true;
			}
		}
		private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			var newPos = e.GetPosition(this);
			var delta = newPos - _mousePos;

			if (_buttonDownLeft)
				Camera.Orbit((float)delta.X, (float)delta.Y);
			else if (_buttonDownRight)
				Camera.Dolly(-(float)delta.Y);
			else if (_buttonDownMiddle)
				Camera.Pan((float)delta.X, (float)delta.Y);

			if (Mouse.LeftButton != MouseButtonState.Pressed)
				_buttonDownLeft = false;
			if (Mouse.RightButton != MouseButtonState.Pressed)
				_buttonDownRight = false;
			if (Mouse.MiddleButton != MouseButtonState.Pressed)
				_buttonDownMiddle = false;

			_mousePos = newPos;
			e.Handled = true;
		}*/
	}
}
