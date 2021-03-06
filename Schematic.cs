﻿using System;
using System.Collections;
using Cairo;
using Gtk;
using Mono.Unix;

namespace Eithne
{
	class Schematic : DrawingArea
	{
		private static bool Antialias = Config.Get("schematic/antialias", true);
		private static bool ChangeBackground = Config.Get("schematic/changebackground", false);
		private static int BackgroundRed = Config.Get("schematic/red", 128);
		private static int BackgroundGreen = Config.Get("schematic/green", 128);
		private static int BackgroundBlue = Config.Get("schematic/blue", 128);
		private static int BackgroundAlpha = Config.Get("schematic/alpha", 255);
		private static bool ConnectionAnimation = Config.Get("schematic/connectionanimation", true);

		internal new class Action
		{
			public enum Mode
			{
				Normal,
				Move,
				Connect
			}

			public static Mode m = Mode.Normal;
			public static object data;
		}

		internal enum Connection
		{
			None,
			Good,
			Bad
		}

		private Statusbar status;
		private ArrayList blocks = new ArrayList();
		private static int offset = 10;
		private object selected = null;
		private int tmpx, tmpy;

		public ArrayList Blocks
		{
			get { return blocks; }
		}

		public static void CheckGConf()
		{
			bool NewAntialias = Config.Get("schematic/antialias", true);
			bool NewChangeBackground = Config.Get("schematic/changebackground", false);
			int NewRed = Config.Get("schematic/red", 128);
			int NewGreen = Config.Get("schematic/green", 128);
			int NewBlue = Config.Get("schematic/blue", 128);
			int NewAlpha = Config.Get("schematic/alpha", 255);
			bool NewConnectionAnimation = Config.Get("schematic/connectionanimation", true);

			bool redraw =
				Antialias != NewAntialias ||
				ChangeBackground != NewChangeBackground ||
				BackgroundRed != NewRed ||
				BackgroundGreen != NewGreen ||
				BackgroundBlue != NewBlue ||
				BackgroundAlpha != NewAlpha ||
				ConnectionAnimation != NewConnectionAnimation;

			Antialias = NewAntialias;
			ChangeBackground = NewChangeBackground;
			BackgroundRed = NewRed;
			BackgroundGreen = NewGreen;
			BackgroundBlue = NewBlue;
			BackgroundAlpha = NewAlpha;
			ConnectionAnimation = NewConnectionAnimation;

			if(redraw)
				MainWindow.RedrawSchematic();
		}

		public Schematic(Statusbar status) : base()
		{
			this.status = status;

			// FIXME hardcoded working space size
			SetSizeRequest(2048, 2048);

			Events |= Gdk.EventMask.PointerMotionMask | Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask
					| Gdk.EventMask.LeaveNotifyMask;
		}

		protected override bool OnExposeEvent(Gdk.EventExpose args)
		{
			Context c = Gdk.Context.CreateDrawable(this.GdkWindow);

			if(Antialias)
				c.Antialias = Cairo.Antialias.Gray;
			else
				c.Antialias = Cairo.Antialias.None;

			Draw(c);

			((IDisposable) c.Target).Dispose();
			((IDisposable) c).Dispose();

			return true;
		}

		private object CheckSelection(int x, int y)
		{
			foreach(Block b in new ReverseIterator(blocks))
			{
				object o = b.Overlap(x, y);

				if(o != null)
					return o;
			}

			return null;
		}

		private Connection CheckConnection(Socket from, Socket to)
		{
			if(to.Type == Socket.T.Out)
				return Connection.None;
			else
				if(IsGoodConnection(from, to))
					return Connection.Good;
				else
					return Connection.Bad;
		}

		private bool IsGoodConnection(Socket from, Socket to)
		{
			// the same block
			if(from.Parent == to.Parent)
				return false;
			// something is already connected to the socket
			if(to.Other != null)
				return false;

			Plugin.Base fp = from.Parent.Plugin;
			Plugin.Base tp = to.Parent.Plugin;

			foreach(string mOut in fp.MatchOut)
			{
				string tmp = mOut;
				ArrayList matches = new ArrayList();
				matches.Add(tmp);

				while(tmp.LastIndexOf('/') != -1)
				{
					int lastIndex = tmp.LastIndexOf('/');
					tmp = tmp.Substring(0, lastIndex);
					matches.Add(tmp);
				}
				matches.Add("");

				foreach(string mIn in tp.MatchIn)
				{
					if(matches.Contains(mIn))
						return true;
				}
			}

			return false;
		}

		protected override bool OnMotionNotifyEvent(Gdk.EventMotion args)
		{
			if(Action.m == Action.Mode.Normal)
			{
				object oldselected = selected;

				selected = CheckSelection((int)args.X, (int)args.Y);

				if(oldselected != selected)
				{
					status.Pop(1);

					if(selected is Block)
						status.Push(1, String.Format(Catalog.GetString("{0} block selected"), (selected as Block).Plugin.Info.Name));
					else if(selected is Socket)
					{
						Socket s = selected as Socket;
						Plugin.Base p = s.Parent.Plugin;

						if(s.Type == Socket.T.In)
							status.Push(1, String.Format(Catalog.GetString("{0} block, input socket {1}. {2}"),
										p.Info.Name, s.Num, p.DescIn(s.Num)));
						else
							status.Push(1, String.Format(Catalog.GetString("{0} block, output socket {1}. {2}"),
										p.Info.Name, s.Num, p.DescOut(s.Num)));
					}

					QueueDraw();
				}
			}
			else if(Action.m == Action.Mode.Move)
			{
				int dx = (int)args.X - tmpx;
				int dy = (int)args.Y - tmpy;

				(selected as Block).Move(dx, dy);
				QueueDraw();

				tmpx = (int)args.X;
				tmpy = (int)args.Y;
			}
			else if(Action.m == Action.Mode.Connect)
			{
				tmpx = (int)args.X;
				tmpy = (int)args.Y;

				object tmp = CheckSelection((int)args.X, (int)args.Y);

				if(tmp is Socket)
					Action.data = CheckConnection(selected as Socket, tmp as Socket);
				else
					Action.data = Connection.None;

				QueueDraw();
			}

			return true;
		}

		protected override bool OnButtonPressEvent(Gdk.EventButton args)
		{
			status.Pop(1);

			Schematic _t = this;
			object selected = _t.selected;

			object tmp = CheckSelection((int)args.X, (int)args.Y);

			if(selected != null && selected == tmp)
			{
				if(selected is Block)
				{
					Block b = selected as Block;

					if(args.Button == 1)
					{
						if(args.Type == Gdk.EventType.ButtonPress)
						{
							status.Push(1, Catalog.GetString("Move block to desired location"));

							// move clicked block to the top
							if(blocks[blocks.Count-1] != selected)
							{
								blocks.Remove(selected);
								blocks.Add(selected);
								QueueDraw();
							}

							Action.m = Action.Mode.Move;
							tmpx = (int)args.X;
							tmpy = (int)args.Y;
						}
						else if(args.Type == Gdk.EventType.TwoButtonPress)
						{
							Action.m = Action.Mode.Normal;

							if(b.Plugin is Plugin.Out && b.Plugin.WorkDone)
							{
								try
								{
									(b.Plugin as Plugin.Out).DisplayResults();
								}
								catch(Exception e)
								{
									b.ShowError = true;
									QueueDraw();
									new PluginError(e, b, true);
								}
							}
							else if(b.Plugin.HasSetup)
							{
								try
								{
									b.Plugin.Setup();
								}
								catch(Exception e)
								{
									b.ShowError = true;
									QueueDraw();
									new PluginError(e, b, true);
								}
							}
						}
					}
					else if(args.Button == 3)
					{
						ImageMenuItem mi;

						status.Push(1, String.Format(Catalog.GetString("{0} menu"), b.Plugin.Info.Name));

						Action.m = Action.Mode.Normal;

						Menu m = new Menu();

						if(b.Plugin is Plugin.Out)
						{
							mi = new ImageMenuItem(Catalog.GetString("Display _results"));
							mi.Image = new Image(null, "system-search.png");
							mi.Activated += PluginResults;
							if(!b.Plugin.WorkDone)
								mi.Sensitive = false;
							m.Append(mi);
						}

						if(b.Plugin.HasSetup)
						{
							mi = new ImageMenuItem(Catalog.GetString("_Setup"));
							mi.Image = new Image(null, "preferences-desktop.png");
							mi.Activated += PluginSetup;
							m.Append(mi);
						}

						if(b.Plugin.HasSetup || b.Plugin is Plugin.Out)
							m.Append(new SeparatorMenuItem());

						mi = new ImageMenuItem(Catalog.GetString("D_isconnect all"));
						mi.Image = new Image(null, "edit-cut.png");
						mi.Activated += delegate(object sender, EventArgs eargs)
							{
								b.Disconnect();
								QueueDraw();
								status.Pop(1);
								status.Push(1, Catalog.GetString("Removed all block's connections"));
							};
						m.Append(mi);

						mi = new ImageMenuItem(Catalog.GetString("In_validate"));
						mi.Image = new Image(null, "user-trash-full.png");
						if(b.CheckState() != Block.State.Good)
							mi.Sensitive = false;
						mi.Activated += delegate(object sender, EventArgs eargs)
							{
								b.Invalidate();

								status.Pop(1);
								status.Push(1, Catalog.GetString("Invalidated results"));
							};
						m.Append(mi);

						mi = new ImageMenuItem(Catalog.GetString("_Delete"));
						mi.Image = new Image(null, "edit-delete.png");
						mi.Activated += delegate(object sender, EventArgs eargs)
							{
								b.Disconnect();
								blocks.Remove(selected);
								QueueDraw();

								status.Pop(1);
								status.Push(1, String.Format(Catalog.GetString("Deleted {0} block"), b.Plugin.Info.Name));
							};
						m.Append(mi);

						m.Append(new SeparatorMenuItem());

						mi = new ImageMenuItem(Catalog.GetString("_About"));
						mi.Image = new Image(null, "help-browser.png");
						mi.Activated += delegate(object sender, EventArgs eargs) { new PluginAbout(b.Plugin); };
						m.Append(mi);

						m.ShowAll();
						m.Popup();
					}
				}
				else if(selected is Socket)
				{
					Socket s = selected as Socket;

					if(s.Other == null)
					{
						if(s.Type == Socket.T.Out)
							if(args.Button == 1)
							{
								status.Push(1, Catalog.GetString("Connect block with another"));
								Action.m = Action.Mode.Connect;
								Action.data = Connection.None;
								tmpx = (int)args.X;
								tmpy = (int)args.Y;
								QueueDraw();
							}
					}
					else
					{
						if(args.Button == 1 && args.Type == Gdk.EventType.TwoButtonPress)
						{
							status.Push(1, Catalog.GetString("Removed connection"));
							s.Disconnect();
							QueueDraw();
						}
						else if(args.Button == 3)
						{
							Menu m = new Menu();
							ImageMenuItem mi = new ImageMenuItem(Catalog.GetString("_Disconnect"));
							mi.Image = new Image(null, "edit-cut.png");
							mi.Activated += delegate(object sender, EventArgs eargs)
							{
								s.Disconnect();
								_t.QueueDraw();

								_t.status.Pop(1);
								_t.status.Push(1, Catalog.GetString("Removed connection"));
							};
							m.Append(mi);

							m.ShowAll();
							m.Popup();
						}
					}
				}
			}
			else
			{
				Action.m = Action.Mode.Normal;
				selected = tmp;
				QueueDraw();
			}

			return true;
		}

		private void PluginSetup(object sender, EventArgs args)
		{
			try
			{
				(selected as Block).Plugin.Setup();
			}
			catch(Exception e)
			{
				(selected as Block).ShowError = true;
				QueueDraw();
				new PluginError(e, selected as Block, true);
			}
		}

		private void PluginResults(object sender, EventArgs args)
		{
			try
			{
				((selected as Block).Plugin as Plugin.Out).DisplayResults();
			}
			catch(Exception e)
			{
				(selected as Block).ShowError = true;
				QueueDraw();
				new PluginError(e, selected as Block, true);
			}
		}

		protected override bool OnButtonReleaseEvent(Gdk.EventButton args)
		{
			if(args.Button != 1)
				return true;

			if(Action.m != Action.Mode.Normal)
				status.Pop(1);

			Action.m = Action.Mode.Normal;

			if(selected is Socket)
			{
				Socket from = selected as Socket;
	
				selected = CheckSelection((int)args.X, (int)args.Y);

				if(selected is Socket)
				{
					Socket to = selected as Socket;

					if(CheckConnection(from, to) == Connection.Good)
					{
						status.Push(1, Catalog.GetString("Connected blocks"));
						from.Connect(to);
					}
				}

				QueueDraw();
			}

			return true;
		}

		public void Add(Plugin.Base plugin)
		{
			Context c = Gdk.Context.CreateDrawable(this.GdkWindow);

			blocks.Add(new Block(this, c, plugin, 10, offset));

			((IDisposable) c.Target).Dispose();
			((IDisposable) c).Dispose();

			offset += 50;
			if(offset > 400)
				offset = 10;
			QueueDraw();
		}

		public void Load(ArrayList blocks)
		{
			this.blocks = blocks;
			offset = 10;
			selected = null;
			QueueDraw();
		}

		public void Clear()
		{
			blocks = new ArrayList();
			offset = 10;
			selected = null;
			QueueDraw();
		}

		public void Draw(Context c)
		{
			if(ChangeBackground)
			{
				if(MainWindow.HaveAlpha)
					c.Color = new Color(BackgroundRed/255.0, BackgroundGreen/255.0, BackgroundBlue/255.0, BackgroundAlpha/255.0);
				else
					c.Color = new Color(BackgroundRed/255.0, BackgroundGreen/255.0, BackgroundBlue/255.0, 1.0);

				c.Operator = Operator.Source;
				c.Paint();
				c.Operator = Operator.Over;
			}

			foreach(Block b in blocks)
				b.Draw(c, selected);

			foreach(Block b in blocks)
				b.DrawConnections(c);

			if(Action.m == Action.Mode.Connect)
			{
				c.Color = new Color(0, 0, 0, 0.75);
				c.SetDash(new double[2] {4,2}, 0);
				c.LineWidth = 1.0;
				c.MoveTo(tmpx, tmpy);
				c.LineTo((selected as Socket).PX + 6.5, (selected as Socket).PY + 5);
				c.Stroke();

				c.SetDash(new double[0] {}, 0);
				switch((Connection)Action.data)
				{
					case Connection.None:
						c.Color = new Color(1, 1, 1, 0.8);
						c.Arc(tmpx, tmpy, 4, 0, 2*Math.PI);
						break;

					case Connection.Good:
						c.Color = new Color(0.3, 1, 0.3, 0.8);
						if(ConnectionAnimation)
							c.Arc(tmpx, tmpy, 7 + Math.Sin(DateTime.Now.Ticks/1000000.0d), 0, 2*Math.PI);
						else
							c.Arc(tmpx, tmpy, 7, 0, 2*Math.PI);
						break;

					case Connection.Bad:
						c.Color = new Color(1, 0.3, 0.3, 0.8);
						if(ConnectionAnimation)
							c.Arc(tmpx, tmpy, 7 + Math.Sin(DateTime.Now.Ticks/500000.0d) * 1.5f, 0, 2*Math.PI);
						else
							c.Arc(tmpx, tmpy, 7, 0, 2*Math.PI);
						break;
				}
				c.FillPreserve();
				switch((Connection)Action.data)
				{
					case Connection.None:
						c.LineWidth = 1.5;
						c.Color = new Color(0, 0, 0);
						break;

					case Connection.Good:
						c.LineWidth = 3.0;
						c.Color = new Color(0, 0.5, 0);
						break;

					case Connection.Bad:
						c.LineWidth = 3.0;
						c.Color = new Color(0.5, 0, 0);
						break;
				}
				c.Stroke();

				if(ConnectionAnimation)
					GLib.Timeout.Add(200, new GLib.TimeoutHandler(AnimTick));
			}
		}

		public void Redraw()
		{
			QueueDraw();
		}

		private bool AnimTick()
		{
			QueueDraw();
			return false;
		}
	}
}
