using System;
using System.Collections;
using Gtk;
using Mono.Unix;

namespace Eithne
{
	class PluginToolbox : TreeView
	{
		private Statusbar status;
		private Schematic schematic;

		private void Populate(TreeStore store, TreeIter parent, ArrayList plugins)
		{
			foreach(IFactory p in plugins)
				store.AppendValues(parent, p.Info.Name, p);
		}

		private bool HasParent(TreeModel model, TreeIter iter)
		{
			TreeIter ti;

			return model.IterParent(out ti, iter);
		}

		private bool HasParent(TreePath path)
		{
			return path.Depth > 1;
		}

		private void RenderCell(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			if(HasParent(model, iter))
				(cell as CellRendererText).Font = "Normal";
			else
				(cell as CellRendererText).Font = "Bold";

			(cell as CellRendererText).Markup = (string)model.GetValue(iter, 0);
		}

		protected override void OnRowActivated(TreePath path, TreeViewColumn column)
		{
			if(HasParent(path))
			{
				TreeIter iter;
				Selection.GetSelected(out iter);
				IFactory f = Model.GetValue(iter, 1) as IFactory;
				IPlugin p = f.Create();
				p.Source = PluginDB.Origin[f];
				schematic.Add(p);
			}
		}

		protected override bool OnButtonPressEvent(Gdk.EventButton ev)
		{
			base.OnButtonPressEvent(ev);

			status.Pop(1);

			TreeIter iter;
			Selection.GetSelected(out iter);
			if(HasParent(Model, iter))
			{
				IFactory f = Model.GetValue(iter, 1) as IFactory;

				status.Push(1, String.Format(Catalog.GetString("{0} plugin selected"), f.Info.Name));

				if(ev.Button == 3)
				{
					Menu m = new Menu();
					ImageMenuItem m1 = new ImageMenuItem(Catalog.GetString("A_dd"));
					m1.Image = new Image(null, "list-add.png");
					m1.Activated += delegate(object sender, EventArgs e)
						{
							IPlugin p = f.Create();
							p.Source = PluginDB.Origin[f];
							schematic.Add(p);
						};
					m.Append(m1);

					SeparatorMenuItem s = new SeparatorMenuItem();
					m.Append(s);

					ImageMenuItem m2 = new ImageMenuItem(Catalog.GetString("_About"));
					m2.Image = new Image(null, "help-browser.png");
					m2.Activated += delegate(object sender, EventArgs e) { new PluginAbout(f); };
					m.Append(m2);

					m.ShowAll();
					m.Popup();
				}
			}

			return true;
		}

		public PluginToolbox(Statusbar status, Schematic schematic) : base()
		{
			this.status = status;
			this.schematic = schematic;

			TreeStore store = new TreeStore(typeof(string), typeof(IFactory));

			TreeIter iter = store.AppendValues(Catalog.GetString("Input"));
			Populate(store, iter, PluginDB.In);

			iter = store.AppendValues(Catalog.GetString("Image processing"));
			Populate(store, iter, PluginDB.ImgProc);

			iter = store.AppendValues(Catalog.GetString("Comparator"));
			Populate(store, iter, PluginDB.Comparator);

			iter = store.AppendValues(Catalog.GetString("Result processing"));
			Populate(store, iter, PluginDB.ResProc);

			iter = store.AppendValues(Catalog.GetString("Output"));
			Populate(store, iter, PluginDB.Out);

			iter = store.AppendValues(Catalog.GetString("Other"));
			Populate(store, iter, PluginDB.Other);

			Model = store;
			HeadersVisible = false;

			CellRendererText cr = new CellRendererText();
			AppendColumn("Name", cr, "text", 0);
			Columns[0].SetCellDataFunc(cr, RenderCell);

			ExpandAll();
		}
	}
}
