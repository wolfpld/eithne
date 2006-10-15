using System;
using System.Collections;
using System.Reflection;
using Gtk;
using Glade;
using Mono.Unix;

namespace Eithne
{
	public class ClassDBSetup
	{
		[Widget] Window		ClassDBWindow;
		[Widget] Button		AddButton;
		[Widget] Button		RemoveButton;
		[Widget] Button		SplitButton;
		[Widget] Button		CloseButton;
		[Widget] ScrolledWindow	FileListSocket;
		[Widget] Label		ImageCount;

		private FileList	filelist;

		public ClassDBSetup(ArrayList cat, IBlock b)
		{
			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "ClassDB.glade", "ClassDBWindow", null);
			gxml.BindFields(this);

			ClassDBWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;
			AddButton.Clicked += OnAdd;
			AddButton.Image = new Image(Assembly.GetEntryAssembly(), "list-add.png");
			RemoveButton.Clicked += OnRemove;
			RemoveButton.Image = new Image(Assembly.GetEntryAssembly(), "list-remove.png");
			SplitButton.Clicked += delegate(object o, EventArgs args) { new Modify(cat, filelist, b); };

			filelist = new FileList(cat, b);
			FileListSocket.AddWithViewport(filelist);

			UpdateCount();

			ClassDBWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			ClassDBWindow.Destroy();
		}

		private void UpdateCount()
		{
			if(filelist.Count == 0)
				ImageCount.Text = Catalog.GetString("No categories");
			else
			{
				string part1 = String.Format(Catalog.GetPluralString("{0} image", "{0} images", filelist.CountImages),
						filelist.CountImages);
				string part2 = String.Format(Catalog.GetPluralString(" in {0} category", " in {0} categories",
							filelist.Count), filelist.Count);
				ImageCount.Text = part1 + part2;
			}
		}

		private void OnAdd(object o, EventArgs args)
		{
			ArrayList errors = new ArrayList();
			string basepath = AppDomain.CurrentDomain.BaseDirectory;
			FileChooserDialog fs = new FileChooserDialog(Catalog.GetString("Select directory with images to add..."),
					ClassDBWindow, FileChooserAction.SelectFolder, new object[] {Catalog.GetString("Cancel"),
					ResponseType.Cancel, Catalog.GetString("Add"), ResponseType.Accept});

			fs.SelectMultiple = true;

			fs.Response += delegate(object o, ResponseArgs args)
				{
					if(args.ResponseId == ResponseType.Accept)
						foreach(string fn in fs.Filenames)
						{
							string file;

							if(fn.Length > basepath.Length && fn.Substring(0, basepath.Length) == basepath)
								file = fn.Substring(basepath.Length);
							else
								file = fn;

							if(!filelist.Add(file))
								errors.Add(file);
						}
				};
			fs.Run();
			fs.Destroy();

			UpdateCount();

			if(errors.Count != 0)
				new LoadError(errors, Catalog.GetString("<big><b>The following directories were empty:</b></big>"));
		}

		private void OnRemove(object o, EventArgs args)
		{
			filelist.TryRemove();

			UpdateCount();
		}
	}
}
