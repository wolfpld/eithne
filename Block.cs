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
		private readonly Plugin.Base plugin;
		private readonly Schematic schematic;
		private int x, y;
		private int h = 0, w = 0, th = 0;
		private int tx = 0, ty = 0;
		private int sx = 0, sy = 0;
		private int cx = 0, cy = 0;
		private Socket[] socketin = null;
		private Socket[] socketout = null;
		private bool working = false;
		private bool showerror = false;
		private float progressTimer = 0;

		private static bool ConfigGradient = Config.Get("block/gradient", true);
		private static bool ConfigInner = Config.Get("block/innerpath", true);
		private static bool ConfigRound = Config.Get("block/round", true);
		private static bool ConfigSmoothConnections = Config.Get("block/smoothconnections", true);
		private static bool ConfigProgress = Config.Get("block/progress", true);

		public enum State
		{
			Good,
			Bad,
			NotReady,
			Ready
		}

		public bool ShowError
		{
			set { showerror = value; }
		}

		public Plugin.Base Plugin
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

		public Block(Schematic s, Context c, Plugin.Base plugin, int x, int y)
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

		public static void CheckGConf()
		{
			bool NewConfigGradient = Config.Get("block/gradient", true);
			bool NewConfigInner = Config.Get("block/innerpath", true);
			bool NewConfigRound = Config.Get("block/round", true);
			bool NewConfigSmoothConnections = Config.Get("block/smoothconnections", true);
			bool NewConfigProgress = Config.Get("block/progress", true);

			bool redraw =
				ConfigGradient != NewConfigGradient ||
				ConfigInner != NewConfigInner ||
				ConfigRound != NewConfigRound ||
				ConfigSmoothConnections != NewConfigSmoothConnections ||
				ConfigProgress != NewConfigProgress;

			ConfigGradient = NewConfigGradient;
			ConfigInner = NewConfigInner;
			ConfigRound = NewConfigRound;
			ConfigSmoothConnections = NewConfigSmoothConnections;
			ConfigProgress = NewConfigProgress;

			if(redraw)
				MainWindow.RedrawSchematic();
		}

		public object Overlap(int x, int y)
		{
			if(x < this.x || x > this.x + w || y < this.y || y > this.y + h)
				return null;

			if(x < this.x + 10)
				// input sockets
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
				// output sockets
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

		// FIXME hardcoded working area size
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

		// removes all connections
		public void Disconnect()
		{
			for(int i=0; i<socketin.Length; i++)
				socketin[i].Disconnect();

			for(int i=0; i<socketout.Length; i++)
				socketout[i].Disconnect();
		}

		// called after slot number is changed
		private void UpdateCoordinates()
		{
			int h1 = CalcHeight(plugin.NumIn);
			int h2 = CalcHeight(plugin.NumOut);

			// block size
			h = Math.Max(Math.Max(th, h1), h2) + 20;

			// text location
			ty = h/2 + th/2;

			if(ty > h-15)
				ty = h-15;

			// status location
			if(plugin is ConnectorPlugin)
				sy = h/2;
			else
				sy = h - 7;

			// clock location
			cy = h/2;
		}

		// called only once, by constructor
		private void UpdateCoordinates(Context c)
		{
			TextExtents t = c.TextExtents(plugin.Info.ShortName);

			th = (int)t.Height;

			// block size
			w = (int)t.Width + 30;

			// text location
			tx = 15;

			// status location
			if(plugin is ConnectorPlugin)
				sx = w/2;
			else
				sx = w/2 + 1;

			// clock location
			cx = w/2;

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
			{
				DrawClock(c);
				if(ConfigProgress)
					DrawClockProgress(c, plugin.Progress);
			}

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
			
			if(WorkPossible())
				return State.Ready;
			else
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
			LinearGradient g;

			c.LineWidth = 1;

			c.Save();
			c.Translate(x + cx, y + cy);

			// shadow
			c.Color = new Color(0, 0, 0, 0.1);
			c.Arc(0, 0, 14, 0, 2 * Math.PI);
			c.Fill();
			c.Arc(0, 0, 13, 0, 2 * Math.PI);
			c.Fill();

			// background
			c.Arc(0, 0, 12, 0, 2 * Math.PI);
			c.Color = new Color(0.525, 0.525, 0.525);
			c.FillPreserve();
			c.Color = new Color(0.455, 0.455, 0.455);
			c.Stroke();

			// outline
			c.Color = new Color(1, 1, 1, 0.25);
			c.Arc(0, 0, 11, 0, 2 * Math.PI);
			c.Stroke();

			// clock face
			c.Arc(0, 0, 8, 0, 2 * Math.PI);
			g = new LinearGradient(7, 7, -7, -7);
			g.AddColorStopRgb(0, new Color(0.796, 0.796, 0.796));
			g.AddColorStopRgb(1, new Color(1, 1, 1));
			c.Pattern = g;
			c.FillPreserve();
			g = new LinearGradient(7, 7, -7, -7);
			g.AddColorStopRgb(0, new Color(1, 1, 1));
			g.AddColorStopRgb(1, new Color(0.627, 0.627, 0.627));
			c.Pattern = g;
			c.Stroke();

			// hand center
			c.LineWidth = 0.5;
			c.Arc(0.5, 0.5, 1.25, 0, 2 * Math.PI);
			c.Color = new Color(0.95, 0.95, 0.95);
			c.FillPreserve();
			c.Color = new Color(0.1, 0.1, 0.1);
			c.Stroke();

			// minute hand
			c.MoveTo(0.5+Math.Sin(-1.15)*1.5, 0.5+Math.Cos(-1.15)*1.5);
			c.LineTo(0.5+Math.Sin(-1.15)*6.5, 0.5+Math.Cos(-1.15)*6.5);
			c.Stroke();

			// hour hand
			c.LineWidth = 0.75;

			c.MoveTo(0.5+Math.Sin(1.15)*1.5, 0.5+Math.Cos(1.15)*1.5);
			c.LineTo(0.5+Math.Sin(1.15)*5, 0.5+Math.Cos(1.15)*5);
			c.Stroke();

			// hour markers
			c.LineWidth = 1;

			c.Arc(0.5, 6.5, 0.75, 0, 2 * Math.PI);
			c.Color = new Color(0.058, 0.058, 0.058);
			c.Fill();

			c.Arc(0.5, -5.5, 0.75, 0, 2 * Math.PI);
			c.Color = new Color(0.44, 0.44, 0.44);
			c.Fill();

			c.Arc(6.5, 0.5, 0.75, 0, 2 * Math.PI);
			c.Color = new Color(0.113, 0.113, 0.113);
			c.Fill();

			c.Arc(-5.5, 0.5, 0.75, 0, 2 * Math.PI);
			c.Color = new Color(0.38, 0.38, 0.38);
			c.Fill();

			c.Restore();
		}

		private void DrawClockProgress(Context c, float progress)
		{
			c.Save();
			c.Color = new Color(0.5, 0.5, 1, 0.75);
			c.Translate(x + w/2, y + h/2);
			c.MoveTo(0, 0);
			if(progress == -1)
			{
				progressTimer += 0.2f;
				c.Arc(0, 0, 8, progressTimer, progressTimer + 1.5);
			}
			else
			{
				c.ArcNegative(0, 0, 8, -90 * Math.PI/180, (-89 + 356 * progress ) * Math.PI/180);
			}
			c.ClosePath();
			c.Fill();
			c.Restore();
		}

		private void DrawState(Context c)
		{
			State s = CheckState();

			LinearGradient GradientOutline = new LinearGradient(x+sx-1, y+sy-3, x+sx+1, y+sy+3);
			GradientOutline.AddColorStopRgb(0, new Color(0.06, 0.06, 0.627));
			GradientOutline.AddColorStopRgb(1, new Color(0.06, 0.06, 0.25));

			LinearGradient GradientInside = new LinearGradient(x+sx-2, y+sy-2, x+sx-1, y+sy+1);

			if(s == State.NotReady)
			{
				GradientInside.AddColorStopRgb(0, new Color(0.5, 0.5, 0.5));
				GradientInside.AddColorStopRgb(1, new Color(0.06, 0.06, 0.06));
			}
			else if(s == State.Bad)
			{
				GradientInside.AddColorStopRgb(0, new Color(1, 0.25, 0.25));
				GradientInside.AddColorStopRgb(1, new Color(0.627, 0.03, 0.03));
			}
			else if(s == State.Good)
			{
				GradientInside.AddColorStopRgb(0, new Color(0.5, 1, 0.5));
				GradientInside.AddColorStopRgb(1, new Color(0.25, 0.75, 0.25));
			}
			else
			{
				GradientInside.AddColorStopRgb(0, new Color(1, 0.75, 0));
				GradientInside.AddColorStopRgb(1, new Color(1, 0.5, 0));
			}

			c.Pattern = GradientInside;
			c.Arc(x+sx, y+sy, 4, 0, 2 * Math.PI);
			c.FillPreserve();

			c.Pattern = GradientOutline;
			c.Stroke();

			c.Color = new Color(1, 1, 1, 0.25);
			c.Arc(x+sx, y+sy, 3, 0, 2 * Math.PI);
			c.Stroke();
		}

		private void DrawBlockGradient(Context c)
		{
			LinearGradient g = new LinearGradient(0, y, 0, y+h);
			if(plugin is Plugin.In)
			{
				g.AddColorStop(0, new Color(0.65, 0.85, 1.00, 0.85));
				g.AddColorStop(0.33, new Color(0.45, 0.65, 1.00, 0.85));
				g.AddColorStop(1, new Color(0.20, 0.50, 0.80, 0.85));
			}
			else if(plugin is Plugin.Out)
			{
				g.AddColorStop(0, new Color(1.00, 0.85, 1.00, 0.85));
				g.AddColorStop(0.33, new Color(1.00, 0.65, 1.00, 0.85));
				g.AddColorStop(1, new Color(0.80, 0.40, 0.80, 0.85));
			}
			else if(plugin is Plugin.ImgProc)
			{
				g.AddColorStop(0, new Color(0.75, 1.00, 0.75, 0.85));
				g.AddColorStop(0.33, new Color(0.55, 1.00, 0.55, 0.85));
				g.AddColorStop(1, new Color(0.30, 0.80, 0.30, 0.85));
			}
			else if(plugin is Plugin.ResProc)
			{
				g.AddColorStop(0, new Color(1.00, 0.75, 0.75, 0.85));
				g.AddColorStop(0.33, new Color(1.00, 0.55, 0.55, 0.85));
				g.AddColorStop(1, new Color(0.80, 0.30, 0.30, 0.85));
			}
			else if(plugin is Plugin.Comparator)
			{
				g.AddColorStop(0, new Color(1.00, 1.00, 0.75, 0.85));
				g.AddColorStop(0.33, new Color(1.00, 1.00, 0.55, 0.85));
				g.AddColorStop(1, new Color(0.80, 0.80, 0.30, 0.85));
			}
			else if(plugin is Plugin.Other)
			{
				g.AddColorStop(0, new Color(0.7, 0.7, 0.7, 0.85));
				g.AddColorStop(0.33, new Color(0.5, 0.5, 0.5, 0.85));
				g.AddColorStop(1, new Color(0.35, 0.35, 0.35, 0.85));
			}

			DrawPath(c);
			c.Pattern = g;
			c.FillPreserve();
		}

		private void DrawBlockNoGradient(Context c)
		{
			if(plugin is Plugin.In)
				c.Color = new Color(0.35, 0.55, 0.95, 0.85);
			else if(plugin is Plugin.Out)
				c.Color = new Color(0.95, 0.55, 0.95, 0.85);
			else if(plugin is Plugin.ImgProc)
				c.Color = new Color(0.45, 0.95, 0.45, 0.85);
			else if(plugin is Plugin.ResProc)
				c.Color = new Color(0.95, 0.45, 0.45, 0.85);
			else if(plugin is Plugin.Comparator)
				c.Color = new Color(0.95, 0.95, 0.45, 0.85);
			else if(plugin is Plugin.Other)
				c.Color = new Color(0.5, 0.5, 0.5, 0.85);

			DrawPath(c);
			c.FillPreserve();
		}

		private void DrawBlock(Context c, bool IsSelected)
		{
			if(ConfigGradient)
				DrawBlockGradient(c);
			else
				DrawBlockNoGradient(c);

			if(showerror)
				c.Color = new Color(1, 0, 0);
			else if(IsSelected)
				c.Color = new Color(0.2, 0.2, 1);
			else
				c.Color = new Color(0, 0, 0);

			c.LineWidth = 2.0;

			c.Stroke();

			if(ConfigInner)
			{
				c.Color = new Color(1, 1, 1, 0.5);
				c.LineWidth = 1;
				DrawPathInner(c);
				c.Stroke();
			}

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
					c.MoveTo(x+w-1, curpos);

					if(ConfigRound)
						c.CurveTo(x+w-10, curpos, x+w-10, curpos+10, x+w-1, curpos+10);
					else
					{
						c.LineTo(x+w-8, curpos);
						c.LineTo(x+w-8, curpos+10);
						c.LineTo(x+w-1, curpos+10);
					}

					c.ClosePath();
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
					c.MoveTo(x+1, curpos);

					if(ConfigRound)
						c.CurveTo(x+10, curpos, x+10, curpos+10, x+1, curpos+10);
					else
					{
						c.LineTo(x+8, curpos);
						c.LineTo(x+8, curpos+10);
						c.LineTo(x+1, curpos+10);
					}

					c.ClosePath();
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

					if(ConfigSmoothConnections)
						c.CurveTo(x+w+10, curpos, socketout[i].Other.PX-10, socketout[i].Other.PY+5, socketout[i].Other.PX, socketout[i].Other.PY+5);
					else
						c.LineTo(socketout[i].Other.PX, socketout[i].Other.PY+5);

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
			if(ConfigRound)
				DrawPathRound(c);
			else
				DrawPathSquare(c);
		}

		private void DrawPathRound(Context c)
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

			// output sockets
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

			// input sockets
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

		private void DrawPathSquare(Context c)
		{
			//	1-------2
			//	|	|
			//	|	|
			//	4-------3

			// 1
			c.MoveTo(x, y);
			c.LineTo(x+w, y);

			// 2
			// output sockets
			if(socketout.Length == 0)
				c.LineTo(x+w, y+h);
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
					
					c.LineTo(x+w-8, curpos);
					c.LineTo(x+w-8, curpos+10);
					c.LineTo(x+w, curpos+10);

					curpos += 10;
				}

				c.LineTo(x+w,y+h);
			}

			// 3
			c.LineTo(x, y+h);

			// 4
			// input sockets
			if(socketin.Length == 0)
				c.LineTo(x, y);
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
					
					c.LineTo(x+8, curpos);
					c.LineTo(x+8, curpos-10);
					c.LineTo(x, curpos-10);

					curpos -= 10;
				}

				c.LineTo(x,y+10);
			}

			//1
			c.LineTo(x, y);
			c.LineTo(x+1, y);
		}

		private void DrawPathInner(Context c)
		{
			if(ConfigRound)
				DrawPathInnerRound(c);
//			TODO
//			else
//				DrawPathInnerSquare(c);

		}

		private void DrawPathInnerRound(Context c)
		{
			//	1-------2
			//	|	|
			//	|	|
			//	4-------3

			// 1
			c.MoveTo(x+11.5, y+1.5);
			c.LineTo(x+w-11.5, y+1.5);
			// 2
			c.CurveTo(x+w-1.5, y+1.5, x+w-1.5, y+1.5, x+w-1.5, y+8.5);

			// output sockets
			if(socketout.Length == 0)
				c.LineTo(x+w-1.5, y+h-11.5);
			else
			{
				int curpos = y+10+(h-20-CalcHeight(socketout.Length))/2;

				c.LineTo(x+w-1.5, curpos-1.5);

				for(int i=0; i<socketout.Length; i++)
				{
					if(i!=0)
					{
						curpos += 5;
						c.LineTo(x+w-1.5, curpos-1.5);
					}
					
					c.CurveTo(x+w-11.5, curpos-1.5, x+w-11.5, curpos+11.5, x+w-1.5, curpos+11.5);
					curpos += 10;
				}

				c.LineTo(x+w-1.5, y+h-8.5);
			}

			// 3
			c.CurveTo(x+w-1.5, y+h-1.5, x+w-1.5, y+h-1.5, x+w-11.5, y+h-1.5);
			c.LineTo(x+11.5, y+h-1.5);
			// 4
			c.CurveTo(x+1.5, y+h-1.5, x+1.5, y+h-1.5, x+1.5, y+h-8.5);

			// input sockets
			if(socketin.Length == 0)
				c.LineTo(x+1.5, y+11.5);
			else
			{
				int curpos = y+h-10-(h-20-CalcHeight(socketin.Length))/2;

				c.LineTo(x+1.5, curpos+1.5);

				for(int i=0; i<socketin.Length; i++)
				{
					if(i!=0)
					{
						curpos -= 5;
						c.LineTo(x+1.5, curpos+1.5);
					}
					
					c.CurveTo(x+11.5, curpos+1.5, x+11.5, curpos-11.5, x+1.5, curpos-11.5);
					curpos -= 10;
				}

				c.LineTo(x+1.5, y+8.5);
			}

			// 1
			c.CurveTo(x+1.5, y+1.5, x+1.5, y+1.5, x+11.5, y+1.5);
		}

		private void Invalidate(bool x)
		{
			Plugin.WorkDone = false;
			Plugin.Invalidate();

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
