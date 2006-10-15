using System;
using System.Collections;
using Gtk;
using Mono.Unix;

namespace Eithne
{
	class Engine
	{
		private static void ProcessEvents()
		{
			while(Application.EventsPending())
				Application.RunIteration();
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

					if(b.CheckState() == Block.State.Bad && b.WorkPossible())
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

							b.Plugin.Work();

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
