using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class ResultDataInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Result Data"); }
		}

		public override string ShortName
		{
			get { return "ResData"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin shows raw data coming to block's input socket."); }
		}
	}

	public class ResultDataFactory : IFactory
	{
		IInfo _info = new ResultDataInfo();
		public IInfo Info
		{
			get { return _info; }
		}

		public IType Type
		{
			get { return IType.Out; }
		}

		public void Initialize()
		{
		}

		public Plugin.Base Create()
		{
			return new ResultDataPlugin();
		}
	}

	public class ResultDataPlugin : Plugin.Out
	{
		Gdk.Pixbuf[] itest = null;
		Gdk.Pixbuf[] ibase = null;
		IResult[] res = null;
		int[] match = null;

		public ResultDataPlugin()
		{
			_info = new ResultDataInfo();
		}

		public override void DisplayResults()
		{
			if(!_workdone)
				throw new PluginException(Catalog.GetString("Plugin is not ready to display results."));

			new ResultData(ibase, itest, res, match);
		}

		public override void Work()
		{
			ICommResult r = _in[0] as ICommResult;

			itest = new Gdk.Pixbuf[r.Length];
			ibase = new Gdk.Pixbuf[r.OriginalBaseImages.Length];

			double scale;

			for(int i=0; i<itest.Length; i++)
			{
				IImage img = r.OriginalTestImages[i];

				if(img.W > img.H)
					scale = img.W / 32.0;
				else
					scale = img.H / 32.0;

				Gdk.Pixbuf tmp = img.CreatePixbuf();
				itest[i] = tmp.ScaleSimple(Scale(img.W, scale), Scale(img.H, scale), Gdk.InterpType.Bilinear);
			}

			for(int i=0; i<ibase.Length; i++)
			{
				IImage img = r.OriginalBaseImages[i];

				if(img.W > img.H)
					scale = img.W / 32.0;
				else
					scale = img.H / 32.0;

				Gdk.Pixbuf tmp = img.CreatePixbuf();
				ibase[i] = tmp.ScaleSimple(Scale(img.W, scale), Scale(img.H, scale), Gdk.InterpType.Bilinear);
			}

			res = new IResult[r.Length];
			for(int i=0; i<r.Length; i++)
				res[i] = r[i];

			match = r.FindResultsSimple();

			for(int i=0; i<r.Length; i++)
				if(!r.Match[i])
					match[i] = -1;

			_workdone = true;
		}

		private int Scale(int s, double scale)
		{
			int val = (int)(s/scale);

			if(val == 0)
				return 1;
			else
				return val;
		}

		public override int NumIn		{ get { return 1; } }

		public override string DescIn(int n)
		{
			return Catalog.GetString("Results to be displayed in raw form.");
		}

		private static string[] matchin   = new string[] { "result" };
		public override string[] MatchIn	{ get { return matchin; } }
	}
}
