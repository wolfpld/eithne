using System;
using System.Collections;
using System.Threading;
using Gtk;
using Mono.Unix;

namespace Eithne
{
	class Engine
	{
		private static IPlugin Plugin;
		private static Exception Error;

		private static void ProcessEvents()
		{
			while(Application.EventsPending())
				Application.RunIteration();
		}

		private static void ThreadedWork()
		{
			Error = null;

			try
			{
				Plugin.Work();
			}
			catch(Exception e)
			{
				Error = e;
			}
		}

		public static void Work(Schematic s)
		{
			ArrayList l = s.Blocks;
			bool changes = true;
			Block b;

			while(changes)
			{
				changes = false;

				for(int j=0; j<l.Count; j++)
				{
					b = (Block)l[j];

					if(b.CheckState() == Block.State.Ready)
					{
						b.Working = true;
						s.Redraw();
						ProcessEvents();
						
						try
						{
							if(b.Plugin.NumIn != 0)
							{
								CommSocket cs = new CommSocket(b.Plugin.NumIn);

								for(int i=0; i<b.Plugin.NumIn; i++)
								{
									Socket other = b.SocketIn[i].Other;

									if(other.Parent.Plugin.Out == null)
									{
										b.Working = false;
										b = other.Parent;
										throw new PluginException(Catalog.GetString("Plugin has no data on output sockets."));
									}

									cs[i] = other.Parent.Plugin.Out[other.Num];
								}

								b.Plugin.In = cs;
							}

							Plugin = b.Plugin;
							Thread t = new Thread(ThreadedWork);
							t.Start();

							while(!t.Join(25))
								ProcessEvents();

							if(Error != null)
								throw Error;

							b.Working = false;
							s.Redraw();
							ProcessEvents();
						}
						catch(Exception e)
						{
							b.Working = false;
							b.ShowError = true;
							s.Redraw();
							new PluginError(e, b, s, false);
							return;
						}

						if(b.CheckState() == Block.State.Good)
							changes = true;
					}
				}
			}
		}
	}
}
