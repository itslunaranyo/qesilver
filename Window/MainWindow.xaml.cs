using QESilver.IO;
using QESilver.Model;
using QESilver.Renderer;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
//using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QESilver
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			List<Brush> brushes = new List<Brush>(128);
			List<Entity> entities = new List<Entity>(32);
			using (var mapfile = File.OpenRead("c:\\projects\\proframming\\qesilver\\qes.map"))
			{
				try
				{
					var mapstream = new StreamReader(mapfile);
					MapParserStandard mp = new MapParserStandard(mapstream.ReadToEnd());
					mp.Read(ref brushes, ref entities);
					Debug.WriteLine(string.Format("Parsed {0} brushes and {1} entities", brushes.Count, entities.Count));
				}
				catch(Exception e)
				{
					Debug.Write(e.Message);
					return;
				}
			}
			foreach (Brush b in brushes)
			{
				b.Build();
			}
			BrushRenderer br = new BrushRenderer(brushes);
			CamWin.SetBR(br);
		}
	}
}