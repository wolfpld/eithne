using System;
using System.Threading;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class EuclidInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Euclid metric"); }
		}

		public override string ShortName
		{
			get { return "L2"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin calculates Euclid metric between images."); }
		}
	}

	public class EuclidFactory : IFactory
	{
		IInfo _info = new EuclidInfo();
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
			return new EuclidPlugin();
		}
	}

	public class TaskInfo
	{
		private IResult[] a_out;
		private IImage[] a_in1;
		private IImage[] a_in2;
		private int start;
		private int end;

		public TaskInfo(IResult[] a_out, IImage[] a_in1, IImage[] a_in2, int start, int end)
		{
			this.a_out = a_out;
			this.a_in1 = a_in1;
			this.a_in2 = a_in2;
			this.start = start;
			this.end = end;
		}

		public void TaskWork()
		{
			for(int i=start; i<end; i++)
			{
				double[] data = new double[a_in1.Length];

				for(int j=0; j<a_in1.Length; j++)
					data[j] = Compare(a_in1[j], a_in2[i]);

				a_out[i] = new IResult(data);
			}
		}

		private double Compare(IImage img1, IImage img2)
		{
			if(img1.BPP != 1 && img1.BPP != 4)
				throw new PluginException(Catalog.GetString("Image is not greyscale or floating point."));
			if(img1.BPP != img2.BPP)
				throw new PluginException(Catalog.GetString("Images BPP do not match."));
			if(img1.H != img2.H || img1.W != img2.W)
				throw new PluginException(Catalog.GetString("Images dimensions do not match."));

			double sum = 0;

			if(img1.BPP == 1)
				for(int i=0; i<img1.Data.Length; i++)
				{
					int diff = img1.Data[i] - img2.Data[i];
					sum += diff * diff;
				}
			else
				for(int y=0; y<img1.H; y++)
					for(int x=0; x<img1.W; x++)
					{
						float diff = (float)img1[x, y] - (float)img2[x, y];
						sum += diff * diff;
					}


			return Math.Sqrt(sum);
		}
	}

	public class EuclidPlugin : IComparatorPlugin
	{
		public EuclidPlugin()
		{
			_info = new EuclidInfo();
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
			bool MultiThreading = Eithne.Config.Get("engine/blockthreads", false);

			ICommImage socket1 = _in[0] as ICommImage;
			ICommImage socket2 = _in[1] as ICommImage;

			IImage[] img1 = socket1.Images;
			IImage[] img2 = socket2.Images;

			_out = new CommSocket(1);

			IResult[] res = new IResult[img2.Length];

			if(MultiThreading)
			{
				TaskInfo ti1 = new TaskInfo(res, img1, img2, 0, img2.Length/2);
				TaskInfo ti2 = new TaskInfo(res, img1, img2, img2.Length/2, img2.Length);
				
				Thread t1 = new Thread(ti1.TaskWork);
				Thread t2 = new Thread(ti2.TaskWork);

				t1.Start();
				t2.Start();

				t1.Join();
				t2.Join();
			}
			else
			{
				TaskInfo t = new TaskInfo(res, img1, img2, 0, img2.Length);
				t.TaskWork();
			}

			_out[0] = new ICommResult(res, 0, socket1.OriginalImages, socket2.OriginalImages, socket1.Categories, socket2.Categories);

			_workdone = true;
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
			return Catalog.GetString("Calculated Euclid metric.");
		}

		private static string[] matchin   = new string[] { "image/grayscale", "image/float" };
		private static string[] matchout  = new string[] { "result" };

		public override string[] MatchIn	{ get { return matchin; } }
		public override string[] MatchOut	{ get { return matchout; } }
	}
}
