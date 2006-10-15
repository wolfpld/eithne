using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class PCC2Info : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Pseudo-correlative C<sub>2</sub> metric"); }
		}

		public override string ShortName
		{
			get { return "C2"; }
		}

		public override string Version
		{
			get { return "0.1"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin calculates pseudo-correlative C<sub>2</sub> metric between images."); }
		}
	}

	public class PCC2Factory : IFactory
	{
		IInfo _info = new PCC2Info();
		public IInfo Info
		{
			get { return _info; }
		}

		public IType Type
		{
			get { return IType.Comparator; }
		}

		public void Initialize()
		{
		}

		public IPlugin Create()
		{
			return new PCC2Plugin();
		}
	}

	public class PCC2Plugin : IComparatorPlugin
	{
		public PCC2Plugin()
		{
			_info = new PCC2Info();
		}

		public override void Setup()
		{
		}

		public override bool HasSetup
		{
			get { return false; }
		}

		public override void Work()
		{
			ICommImage socket1 = _in[0] as ICommImage;
			ICommImage socket2 = _in[1] as ICommImage;

			IImage[] img1 = socket1.Images;
			IImage[] img2 = socket2.Images;

			_out = new CommSocket(1);

			IResult[] res = new IResult[img2.Length];

			for(int i=0; i<img2.Length; i++)
			{
				double[] data = new double[img1.Length];

				for(int j=0; j<img1.Length; j++)
					data[j] = Compare(img1[j], img2[i]);

				res[i] = new IResult(data);
			}

			_out[0] = new ICommResult(res, 1, socket1.OriginalImages, socket2.OriginalImages);

			_workdone = true;
		}

		private double Compare(IImage img1, IImage img2)
		{
			if(img1.BPP != 1 || img2.BPP != 1)
				throw new PluginException(Catalog.GetString("Image is not in greyscale."));
			if(img1.H != img2.H || img1.W != img2.W)
				throw new PluginException(Catalog.GetString("Images dimensions do not match."));

			double sum = 0;

			for(int i=0; i<img1.Data.Length; i++)
			{
				double l = Math.Abs(img1.Data[i] - img2.Data[i]);
				double m = img1.Data[i] + img2.Data[i];

				if(m == 0)
					sum += 1;
				else
					sum += 1 - l/m;
			}

			return sum / img1.Data.Length;
		}

		public override int NumIn		{ get { return 2; } }
		public override int NumOut		{ get { return 1; } }

		public override string DescIn(int n)
		{
			if(n == 0)
				return Catalog.GetString("Base images.");
			else
				return Catalog.GetString("Test images.");
		}

		public override string DescOut(int n)
		{
			return Catalog.GetString("Calculated C2 metric.");
		}
	}
}
