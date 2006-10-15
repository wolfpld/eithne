using System;
using System.IO;
using System.Reflection;
using Cairo;

namespace Eithne
{
	class Socket
	{
		private readonly int num;
		private int x, y;
		private readonly Block parent;
		private readonly T type;
		private Socket other;

		public enum T
		{
			In,
			Out
		}

		public int X
		{
			get { return x; }
		}

		public int Y
		{
			get { return y; }
			set { y = value; }
		}

		public int PX
		{
			get { return parent.X + x; }
		}

		public int PY
		{
			get { return parent.Y + y; }
		}

		public Block Parent
		{
			get { return parent; }
		}

		public T Type
		{
			get { return type; }
		}

		public Socket Other
		{
			get { return other; }
		}

		public int Num
		{
			get { return num; }
		}

		public Socket(int x, int y, Block parent, T type, int num)
		{
			this.x = x;
			this.y = y;
			this.parent = parent;
			this.type = type;
			this.num = num;

			other = null;
		}

		public void Draw(Context c)
		{
			int xc = parent.X + x;
			int yc = parent.Y + y + 5;

			if(other == null)
			{
				if(type == T.Out)
				{
					c.Arc(xc+6.5, yc, 4, 0, 2*Math.PI);
					c.Color = new Color(0.2, 0.2, 1, 0.6);
					c.Fill();
				}
			}
			else
			{
				int r = 6;

				if(type == T.Out)
					xc += 7;
				else
					xc += 3;

				c.Arc(xc, yc, r, 0, 2*Math.PI);
				c.Color = new Color(1, 0, 0);
				c.LineWidth = 2.5;
				c.FillPreserve();
				c.Color = new Color(0.3, 0, 0);
				c.Stroke();
				c.MoveTo(xc-0.707*r, yc-0.707*r);
				c.LineTo(xc+0.707*r, yc+0.707*r);
				c.Stroke();
				c.MoveTo(xc-0.707*r, yc+0.707*r);
				c.LineTo(xc+0.707*6, yc-0.707*6);
				c.Stroke();
			}
		}

		public void Disconnect()
		{
			if(other != null)
			{
				if(type == T.In)
					parent.Invalidate();
				else
					other.parent.Invalidate();

				other.other = null;
				other = null;
			}
		}

		public void Connect(Socket to)
		{
			Disconnect();
			to.Disconnect();

			other = to;
			to.other = this;
		}
	}

	class Block : IBlock
	{
		private readonly IPlugin plugin;
		private readonly Schematic schematic;
		private int x, y;
		private int h = 0, w = 0, th = 0;
		private int tx = 0, ty = 0;
		private int sx = 0, sy = 0;
		private int cx = 0, cy = 0;
		private Socket[] socketin = null;
		private Socket[] socketout = null;
		private bool working = false;
		private State connstate = State.NotReady;
		private bool showerror = false;

		private static ImageSurface StateBad = new ImageSurface(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data/state-bad.png"));
		private static ImageSurface StateGood = new ImageSurface(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data/state-good.png"));
		private static ImageSurface StateNotReady = new ImageSurface(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data/state-not-ready.png"));
		private static ImageSurface Clock = new ImageSurface(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data/clock.png"));

		public enum State
		{
			Good,
			Bad,
			NotReady
		}

		public bool ShowError
		{
			set { showerror = value; }
		}

		public State ConnState
		{
			get { return connstate; }
			set { connstate = value; }
		}

		public IPlugin Plugin
		{
			get { return plugin; }
		}

		public int X
		{
			get { return x; }
		}

		public int Y
		{
			get { return y; }
		}

		public Socket[] SocketIn
		{
			get { return socketin; }
		}

		public Socket[] SocketOut
		{
			get { return socketout; }
		}

		public bool Working
		{
			get { return working; }
			set { working = value; }
		}

		public Block(Schematic s, Context c, IPlugin plugin, int x, int y)
		{
			this.schematic = s;
			this.plugin = plugin;
			this.x = x;
			this.y = y;

			plugin.Block = this;

			UpdateCoordinates(c);

			socketin = new Socket[plugin.NumIn];
			socketout = new Socket[plugin.NumOut];

			int curpos = 10+(h-20-CalcHeight(plugin.NumIn))/2;
			for(int i=0; i<plugin.NumIn; i++)
			{
				socketin[i] = new Socket(0, curpos, this, Socket.T.In, i);
				curpos += 15;
			}

			curpos = 10+(h-20-CalcHeight(plugin.NumOut))/2;
			for(int i=0; i<plugin.NumOut; i++)
			{
				socketout[i] = new Socket(w-10, curpos, this, Socket.T.Out, i);
				curpos += 15;
			}
		}

		public object Overlap(int x, int y)
		{
			if(x < this.x || x > this.x + w || y < this.y || y > this.y + h)
				return null;

			if(x < this.x + 10)
				// porty wejściowe
			{
				int curpos = this.y+10+(h-20-CalcHeight(socketin.Length))/2;

				for(int i=0; i<socketin.Length; i++)
				{
					if(y > curpos && y < curpos + 10)
						return socketin[i];

					curpos += 15;
				}
			}
			else if(x > this.x + w - 10)
			{
				// porty wyjściowe
				int curpos = this.y+10+(h-20-CalcHeight(socketout.Length))/2;

				for(int i=0; i<socketout.Length; i++)
				{
					if(y > curpos && y < curpos + 10)
						return socketout[i];

					curpos += 15;
				}
			}
			
			return this;
		}

		// FIXME wpisany na sztywno rozmiar obszaru roboczego
		public void Move(int dx, int dy)
		{
			x += dx;
			y += dy;

			if(x < 0)
				x = 0;
			else if(x+w >= 2048)
				x = 2048-w;

			if(y < 0)
				y = 0;
			else if(y+h >= 2048)
				y = 2048-h;
		}

		// usuwa wszystkie połączenia
		public void Disconnect()
		{
			for(int i=0; i<socketin.Length; i++)
				socketin[i].Disconnect();

			for(int i=0; i<socketout.Length; i++)
				socketout[i].Disconnect();
		}

		// wykonywane po zmianie liczby slotów we wtyczce
		private void UpdateCoordinates()
		{
			int h1 = CalcHeight(plugin.NumIn);
			int h2 = CalcHeight(plugin.NumOut);

			// rozmiar bloku
			h = Math.Max(Math.Max(th, h1), h2) + 20;

			// położenie tekstu
			ty = h/2 + th/2;

			if(ty > h-15)
				ty = h-15;

			// położenie stanu
			if(plugin is ConnectorPlugin)
				sy = h/2 - 4;
			else
				sy = h - 11;

			// położenie zegarka
			cy = h/2 - 16;
		}

		// wykonywane tylko raz, przez konstruktor
		private void UpdateCoordinates(Context c)
		{
			TextExtents t = c.TextExtents(plugin.Info.ShortName);

			th = (int)t.Height;

			// rozmiar bloku
			w = (int)t.Width + 30;

			// położenie tekstu
			tx = 15;

			// położenie stanu
			if(plugin is ConnectorPlugin)
				sx = w/2 - 4;
			else
				sx = w/2 - 3;

			// położenie zegarka
			cx = w/2 - 16;

			UpdateCoordinates();
		}

		public void Draw(Context c, object selected)
		{
			DrawSockets(c);

			if(this == selected || (selected is Socket && (selected as Socket).Parent == this))
				DrawBlock(c, true);
			else
				DrawBlock(c, false);

			DrawState(c);
			if(working)
				DrawClock(c);

			if(selected is Socket)
			{
				for(int i=0; i<socketin.Length; i++)
					if(socketin[i] == selected)
					{
						socketin[i].Draw(c);
						return;
					}

				for(int i=0; i<socketout.Length; i++)
					if(socketout[i] == selected)
					{
						socketout[i].Draw(c);
						return;
					}
			}

			if(showerror)
			{
				c.Color = new Color(1, 0, 0, 0.5);
				DrawPath(c);
				c.Fill();
			}
		}

		public State CheckState()
		{
			for(int i=0; i<socketin.Length; i++)
				if(SocketIn[i].Other == null)
					return State.NotReady;

			if(Plugin.WorkDone)
				return State.Good;
			
			return State.Bad;
		}

		public bool WorkPossible()
		{
			for(int i=0; i<socketin.Length; i++)
				if(socketin[i].Other.Parent.CheckState() != State.Good)
					return false;

			return true;
		}

		private void DrawClock(Context c)
		{
			c.Save();
			c.Translate(x + cx, y + cy);
			Clock.Show(c, 0, 0);
			c.Stroke();
			c.Restore();
		}

		private void DrawState(Context c)
		{
			State s = CheckState();

			c.Color = new Color(1, 1, 1);
			c.Save();
			c.Translate(x + sx, y + sy);

			if(s == State.NotReady)
				StateNotReady.Show(c, 0, 0);
			else if(s == State.Bad)
				StateBad.Show(c, 0, 0);
			else
				StateGood.Show(c, 0, 0);

			c.Stroke();
			c.Restore();
		}

		private void DrawBlock(Context c, bool IsSelected)
		{
			if(plugin is IInPlugin)
				c.Color = new Color(0.35, 0.55, 0.95, 0.75);
			else if(plugin is IOutPlugin)
				c.Color = new Color(0.95, 0.55, 0.95, 0.75);
			else if(plugin is IImgProcPlugin)
				c.Color = new Color(0.45, 0.95, 0.45, 0.75);
			else if(plugin is IResProcPlugin)
				c.Color = new Color(0.95, 0.45, 0.45, 0.75);
			else if(plugin is IComparatorPlugin)
				c.Color = new Color(0.95, 0.95, 0.45, 0.75);
			else if(plugin is IOtherPlugin)
				c.Color = (plugin as IOtherPlugin).Color;

			DrawPath(c);
			c.FillPreserve();

			if(showerror)
			{
				c.Color = new Color(1, 0, 0);
				c.LineWidth = 5.0;
			}
			else if(IsSelected)
			{
				c.Color = new Color(0.2, 0.2, 1);
				c.LineWidth = 4.0;
			}
			else
			{
				c.Color = new Color(0, 0, 0);
				c.LineWidth = 2.0;
			}

			c.Stroke();

			c.Color = new Color(0, 0, 0);

			c.Save();
			c.Translate(x + tx, y + ty);
			c.ShowText(plugin.Info.ShortName);
			c.Stroke();
			c.Restore();
		}

		private void DrawSockets(Context c)
		{
			int curpos = y+10+(h-20-CalcHeight(socketout.Length))/2;

			for(int i=0; i<socketout.Length; i++)
			{
				if(socketout[i].Other == null)
				{
					c.LineWidth = 1.5;
					c.Arc(x+w-3, curpos+5, 4, 0, 2*Math.PI);
					c.Color = new Color(1, 1, 1, 0.8);
					c.FillPreserve();
					c.Color = new Color(0, 0, 0);
					c.Stroke();
				}
				else
				{
					c.LineWidth = 2.0;
					c.MoveTo(x+w, curpos);
					c.CurveTo(x+w-10, curpos, x+w-10, curpos+10, x+w, curpos+10);
					c.LineTo(x+w, curpos);
					c.Color = new Color(1, 1, 1, 0.8);
					c.Fill();
					c.MoveTo(x+w, curpos);
					c.LineTo(x+w, curpos+10);
					c.Color = new Color(0, 0, 0);
					c.Stroke();
				}

				curpos += 15;
			}

			curpos = y+10+(h-20-CalcHeight(socketin.Length))/2;

			for(int i=0; i<socketin.Length; i++)
			{
				if(socketin[i].Other != null)
				{
					c.LineWidth = 2.0;
					c.MoveTo(x, curpos);
					c.CurveTo(x+10, curpos, x+10, curpos+10, x, curpos+10);
					c.LineTo(x, curpos);
					c.Color = new Color(1, 1, 1, 0.8);
					c.Fill();
					c.MoveTo(x, curpos);
					c.LineTo(x, curpos+10);
					c.Color = new Color(0, 0, 0);
					c.Stroke();
				}

				curpos += 15;
			}
		}

		public void DrawConnections(Context c)
		{
			int curpos = y+15+(h-20-CalcHeight(socketout.Length))/2;

			c.LineWidth = 1.0;
			c.Color = new Color(0, 0, 0);
			for(int i=0; i<socketout.Length; i++)
			{
				if(socketout[i].Other != null)
				{
					c.MoveTo(x+w, curpos);
					c.CurveTo(x+w+10, curpos, socketout[i].Other.PX-10, socketout[i].Other.PY+5, socketout[i].Other.PX, socketout[i].Other.PY+5);
					c.Stroke();
				}

				curpos += 15;
			}
		}

		private int CalcHeight(int sockets)
		{
			return Math.Max(sockets*10 + (sockets-1)*5, 0);
		}

		private void DrawPath(Context c)
		{
			//	1-------2
			//	|	|
			//	|	|
			//	4-------3

			// 1
			c.MoveTo(x+10, y);
			c.LineTo(x+w-10, y);
			// 2
			c.CurveTo(x+w, y, x+w, y, x+w, y+10);

			// gniazda wyjściowe
			if(socketout.Length == 0)
				c.LineTo(x+w, y+h-10);
			else
			{
				int curpos = y+10+(h-20-CalcHeight(socketout.Length))/2;

				c.LineTo(x+w, curpos);

				for(int i=0; i<socketout.Length; i++)
				{
					if(i!=0)
					{
						curpos += 5;
						c.LineTo(x+w, curpos);
					}
					
					c.CurveTo(x+w-10, curpos, x+w-10, curpos+10, x+w, curpos+10);
					curpos += 10;
				}

				c.LineTo(x+w,y+h-10);
			}

			// 3
			c.CurveTo(x+w, y+h, x+w, y+h, x+w-10, y+h);
			c.LineTo(x+10, y+h);
			// 4
			c.CurveTo(x, y+h, x, y+h, x, y+h-10);

			// gniazda wejściowe
			if(socketin.Length == 0)
				c.LineTo(x, y+10);
			else
			{
				int curpos = y+h-10-(h-20-CalcHeight(socketin.Length))/2;

				c.LineTo(x, curpos);

				for(int i=0; i<socketin.Length; i++)
				{
					if(i!=0)
					{
						curpos -= 5;
						c.LineTo(x, curpos);
					}
					
					c.CurveTo(x+10, curpos, x+10, curpos-10, x, curpos-10);
					curpos -= 10;
				}

				c.LineTo(x,y+10);
			}

			// 1
			c.CurveTo(x, y, x, y, x+10, y);
		}

		private void Invalidate(bool x)
		{
			Plugin.WorkDone = false;

			for(int i=0; i<socketout.Length; i++)
			{
				Socket o = socketout[i].Other;

				if(o != null && o.Parent.CheckState() == State.Good)
					o.Parent.Invalidate(true);
			}
		}

		public void Invalidate()
		{
			Invalidate(true);
			schematic.QueueDraw();
		}

		public void SlotsChanged()
		{
			UpdateCoordinates();

			if(socketin.Length != plugin.NumIn)
			{
				Socket[] newsocketin = new Socket[plugin.NumIn];
				int curpos = 10+(h-20-CalcHeight(plugin.NumIn))/2;

				for(int i=0; i<plugin.NumIn; i++)
				{
					if(i < socketin.Length)
						newsocketin[i] = socketin[i];
					else
						newsocketin[i] = new Socket(0, curpos, this, Socket.T.In, i);
					curpos += 15;
				}
				for(int i=plugin.NumIn; i<socketout.Length; i++)
					socketout[i].Disconnect();

				socketin = newsocketin;
			}
			else
			{
				int curpos = 10+(h-20-CalcHeight(socketin.Length))/2;

				for(int i=0; i<socketin.Length; i++)
				{
					socketin[i].Y = curpos;
					curpos += 15;
				}
			}

			if(socketout.Length != plugin.NumOut)
			{
				Socket[] newsocketout = new Socket[plugin.NumOut];
				int curpos = 10+(h-20-CalcHeight(plugin.NumOut))/2;

				for(int i=0; i<plugin.NumOut; i++)
				{
					if(i < socketout.Length)
						newsocketout[i] = socketout[i];
					else
						newsocketout[i] = new Socket(w-10, curpos, this, Socket.T.Out, i);
					curpos += 15;
				}
				for(int i=plugin.NumOut; i<socketout.Length; i++)
					socketout[i].Disconnect();

				socketout = newsocketout;
			}
			else
			{
				int curpos = 10+(h-20-CalcHeight(socketout.Length))/2;

				for(int i=0; i<socketout.Length; i++)
				{
					socketout[i].Y = curpos;
					curpos += 15;
				}
			}

			schematic.QueueDraw();
			Invalidate();
		}
	}
}
