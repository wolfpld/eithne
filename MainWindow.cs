using System;
using System.Collections;
using System.IO;
using Gtk;
using Glade;
using Mono.Unix;

namespace Eithne
{
	public class MainWindow
	{
		[Widget] Window		EithneWindow;
		[Widget] Statusbar	StatusBar;
		[Widget] ImageMenuItem	MenuFileNew;
		[Widget] ImageMenuItem	MenuFileOpen;
		[Widget] ImageMenuItem	MenuFileSave;
		[Widget] ImageMenuItem	MenuFileSaveAs;
		[Widget] ImageMenuItem	MenuFilePrint;
		[Widget] ImageMenuItem	MenuFilePreferences;
		[Widget] ImageMenuItem	MenuFileQuit;
		[Widget] ImageMenuItem	MenuSystemRun;
		[Widget] ImageMenuItem	MenuSystemStop;
		[Widget] ImageMenuItem	MenuHelpPluginList;
		[Widget] ImageMenuItem	MenuHelpAbout;
		[Widget] ScrolledWindow	PluginToolboxSocket;
		[Widget] ScrolledWindow	SchematicSocket;
		[Widget] ToolButton	ToolbarNew;
		[Widget] ToolButton	ToolbarOpen;
		[Widget] ToolButton	ToolbarSave;
		[Widget] ToolButton	ToolbarRun;
		[Widget] VBox		SchematicBox;

		private Schematic schematic;
		private Engine2 engine;
		private ProgressBar progress = new ProgressBar();
		private string filename = "";

		private static MainWindow mw = null;

		private static bool AlphaChannel = false;

		private string Filename
		{
			get { return filename; }
			set
			{
				filename = value;

				if(filename == "")
					EithneWindow.Title = About.Name;
				else
					EithneWindow.Title = About.Name + " (" + filename + ")";
			}
		}

		public MainWindow(string[] args)
		{
			Glade.XML gxml = new Glade.XML("MainWindow.glade", "EithneWindow");
			gxml.BindFields(this);

			if(EithneWindow.Screen.RgbaColormap != null)
			{
				AlphaChannel = true;
				EithneWindow.Colormap = EithneWindow.Screen.RgbaColormap;
			}

			EithneWindow.IconList = new Gdk.Pixbuf[2] {new Gdk.Pixbuf(null, "icon-48.png"), new Gdk.Pixbuf(null, "icon-16.png")};

			EithneWindow.DeleteEvent += OnWindowDelete;

			EithneWindow.Title = About.Name;

			StatusBar.Push(1, String.Format(Catalog.GetString("Welcome to {0}!"), About.Name));

			MenuFileNew.Image = new Image(null, "document-new.png");
			MenuFileNew.Activated += OnNew;
			MenuFileOpen.Image = new Image(null, "document-open.png");
			MenuFileOpen.Activated += OnLoad;
			MenuFileSave.Image = new Image(null, "document-save.png");
			MenuFileSave.Activated += OnSave;
			MenuFileSaveAs.Image = new Image(null, "document-save-as.png");
			MenuFileSaveAs.Activated += OnSaveAs;
			MenuFilePrint.Activated += OnPrint;
			MenuFilePreferences.Image = new Image(null, "preferences-desktop.png");
			MenuFilePreferences.Activated += delegate(object o, EventArgs eargs) { new Preferences(); };
			MenuFileQuit.Image = new Image(null, "system-log-out.png");
			MenuFileQuit.Activated += OnWindowDelete;
			MenuSystemRun.Image = new Image(null, "media-playback-start.png");
			MenuSystemRun.Activated += OnRun;
			MenuSystemStop.Image = new Image(null, "media-playback-stop.png");
			MenuSystemStop.Activated += OnRun;
			MenuHelpPluginList.Image = new Image(null, "plugin-16.png");
			MenuHelpPluginList.Activated += delegate(object o, EventArgs eargs) { new PluginList(); };
			MenuHelpAbout.Image = new Image(null, "help-browser.png");
			MenuHelpAbout.Activated += delegate(object o, EventArgs eargs) { new About(); };

			ToolbarNew.IconWidget = new Image(null, "document-new-22.png");
			ToolbarNew.Clicked += OnNew;
			ToolbarOpen.IconWidget = new Image(null, "document-open-22.png");
			ToolbarOpen.Clicked += OnLoad;
			ToolbarSave.IconWidget = new Image(null, "document-save-22.png");
			ToolbarSave.Clicked += OnSave;
			ToolbarRun.IconWidget = new Image(null, "media-playback-start-22.png");
			ToolbarRun.Clicked += OnRun;

			schematic = new Schematic(StatusBar);
			PluginToolboxSocket.AddWithViewport(new PluginToolbox(StatusBar, schematic));
			SchematicSocket.AddWithViewport(schematic);

			engine = new Engine2(schematic, SetRunToStart, progress.Pulse);

			progress.PulseStep = 0.05;
		}

		private void Run()
		{
			EithneWindow.ShowAll();

			Application.Run();
		}

		private void OnWindowDelete(object o, EventArgs args)
		{
			if(engine.Running)
				engine.Stop();

			Application.Quit();
		}

		private void OnNew(object o, EventArgs args)
		{
			DialogQuestion q = new DialogQuestion(Catalog.GetString("Do you really want to discard old schematic?"));

			if(q.Run())
			{
				schematic.Clear();
				Filename = "";

				StatusBar.Pop(1);
				StatusBar.Push(1, Catalog.GetString("New system schematic ready"));
			}
		}

		private void OnLoad(object o, EventArgs args)
		{
			FileChooserDialog fs = new FileChooserDialog(Catalog.GetString("Select file to load..."), EithneWindow,
					FileChooserAction.Open, new object[] {Catalog.GetString("Cancel"), ResponseType.Cancel,
					Catalog.GetString("Load"), ResponseType.Accept});

			FileFilter filter = new FileFilter();
			filter.Name = String.Format(Catalog.GetString("{0} system schematic"), About.Name);
			filter.AddPattern("*.xml");
			fs.AddFilter(filter);
			filter = new FileFilter();
			filter.Name = Catalog.GetString("All files");
			filter.AddPattern("*");
			fs.AddFilter(filter);

			fs.Response += delegate(object obj, ResponseArgs eargs)
				{
					if(eargs.ResponseId == ResponseType.Accept)
					{
						ArrayList tmp = SaveLoad.Load(fs.Filename, schematic);
						if(tmp != null)
						{
							schematic.Load(tmp);
							Filename = fs.Filename;

							StatusBar.Pop(1);
							StatusBar.Push(1, String.Format(Catalog.GetString("{0} loaded"), Filename));
						}
					}
				};
			fs.Run();
			fs.Destroy();
		}

		private void OnSaveAs(object o, EventArgs args)
		{
			FileChooserDialog fs = new FileChooserDialog(Catalog.GetString("Save as..."), EithneWindow, FileChooserAction.Save,
					new object[] {Catalog.GetString("Cancel"), ResponseType.Cancel, Catalog.GetString("Save"),
					ResponseType.Accept});

			FileFilter filter = new FileFilter();
			filter.Name = String.Format(Catalog.GetString("{0} system schematic"), About.Name);
			filter.AddPattern("*.xml");
			fs.AddFilter(filter);
			filter = new FileFilter();
			filter.Name = Catalog.GetString("All files");
			filter.AddPattern("*");
			fs.AddFilter(filter);

			fs.Response += delegate(object obj, ResponseArgs eargs)
				{
					if(eargs.ResponseId == ResponseType.Accept)
					{
						Filename = fs.Filename;

						if(!Path.HasExtension(Filename))
							Filename = Path.ChangeExtension(Filename, ".xml");

						SaveLoad.Save(Filename, schematic);

						StatusBar.Pop(1);
						StatusBar.Push(1, String.Format(Catalog.GetString("{0} saved"), Filename));
					}
				};
			fs.Run();
			fs.Destroy();
		}

		private void OnSave(object o, EventArgs args)
		{
			if(Filename == "")
				OnSaveAs(o, args);
			else
			{
				SaveLoad.Save(Filename, schematic);

				StatusBar.Pop(1);
				StatusBar.Push(1, String.Format(Catalog.GetString("{0} saved"), Filename));
			}
		}

		private void OnPrint(object o, EventArgs args)
		{
			Cairo.SvgSurface svg = new Cairo.SvgSurface("print.svg", 2048, 2048);
			Cairo.Context c = new Cairo.Context(svg);

			schematic.Draw(c);

			((IDisposable) c.Target).Dispose();
			((IDisposable) c).Dispose();
		}

		private void OnRun(object o, EventArgs args)
		{
			if(engine.Running)
				engine.Stop();
			else
			{
				SetRunToStop();
				engine.Start();
			}
		}

		private void SetRunToStop()
		{
			((Image)ToolbarRun.IconWidget).Pixbuf = new Gdk.Pixbuf(null, "media-playback-stop-22.png");
			ToolbarRun.Label = Catalog.GetString("Stop");

			MenuSystemRun.Sensitive = false;
			MenuSystemStop.Sensitive = true;

			SchematicBox.PackEnd(progress, false, true, 0);
			SchematicBox.ShowAll();
		}

		private void SetRunToStart()
		{
			((Image)ToolbarRun.IconWidget).Pixbuf = new Gdk.Pixbuf(null, "media-playback-start-22.png");
			ToolbarRun.Label = Catalog.GetString("Run");

			MenuSystemRun.Sensitive = true;
			MenuSystemStop.Sensitive = false;

			StatusBar.Pop(1);
			StatusBar.Push(1, String.Format(Catalog.GetString("Elapsed time: {0}"), engine.ElapsedTime));

			SchematicBox.Remove(progress);
		}

		public void EmergencySave()
		{
			SaveLoad.Save("rescue.xml", schematic);
		}

		public static void RedrawSchematic()
		{
			mw.schematic.Redraw();
		}

		public static void Main(string[] args)
		{
			Catalog.Init("eithne", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "locale"));

			Application.Init();
			Splash s = new Splash();

			Config.Init(UpdateHandler);

			PluginDB.LoadPlugins(s);

			s.Close();

			try
			{
				mw = new MainWindow(args);
				mw.Run();
			}
			catch(Exception e)
			{
				Console.WriteLine(e);

				new FatalError(e, mw);
			}
		}

		private static void UpdateHandler()
		{
			Block.CheckGConf();
			Schematic.CheckGConf();
			Engine2.CheckGConf();
		}

		public static bool HaveAlpha
		{
			get { return AlphaChannel; }
		}
	}
}
