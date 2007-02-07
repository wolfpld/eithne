using System;
using System.Threading;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class PCC1Info : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Pseudo-correlative C<sub>1</sub> metric"); }
		}

		public override string ShortName
		{
			get { return "C1"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin calculates pseudo-correlative C<sub>1</sub> metric between images."); }
		}
	}

	public class PCC1Factory : IFactory
	{
		IInfo _info = new PCC1Info();
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
			return new PCC1Plugin();
		}
	}

	public class TaskInfo
	{
		public IResult[] a_out;
		public IImage[] a_in1;
		public IImage[] a_in2;
		public int start;
		public int end;

		public TaskInfo(IResult[] a_out, IImage[] a_in1, IImage[] a_in2, int start, int end)
		{
			this.a_out = a_out;
			this.a_in1 = a_in1;
			this.a_in2 = a_in2;
			this.start = start;
			this.end = end;
		}
	}

	public class PCC1Plugin : IComparatorPlugin
	{
		public PCC1Plugin()
		{
			_info = new PCC1Info();
		}

		public override void Setup()
		{
		}

		public override bool HasSetup
		{
			get { return false; }
		}

		private void TaskWork(object _info)
		{
			TaskInfo info = (TaskInfo)_info;

			for(int i=info.start; i<info.end; i++)
			{
				double[] data = new double[info.a_in1.Length];

				for(int j=0; j<info.a_in1.Length; j++)
					data[j] = Compare(info.a_in1[j], info.a_in2[i]);

				info.a_out[i] = new IResult(data);
			}
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
				Thread t1 = new Thread(TaskWork);
				Thread t2 = new Thread(TaskWork);

				t1.Start(new TaskInfo(res, img1, img2, 0, img2.Length/2));
				t2.Start(new TaskInfo(res, img1, img2, img2.Length/2, img2.Length));

				t1.Join();
				t2.Join();
			}
			else
				TaskWork(new TaskInfo(res, img1, img2, 0, img2.Length));

			_out[0] = new ICommResult(res, 1, socket1.OriginalImages, socket2.OriginalImages,
					socket1.Categories, socket2.Categories);

			_workdone = true;
		}

		private double Compare(IImage img1, IImage img2)
		{
			if(img1.BPP != 1 && img1.BPP != 4)
				throw new PluginException(Catalog.GetString("Image is not greyscale or floating point."));
			if(img1.BPP != img2.BPP)
				throw new PluginException(Catalog.GetString("Images BPP do not match."));
			if(img1.H != img2.H || img1.W != img2.W)
				throw new PluginException(Catalog.GetString("Images dimensions do not match."));

			double sum1 = 0;
			double sum2 = 0;

			if(img1.BPP == 1)
				for(int i=0; i<img1.Data.Length; i++)
				{
					sum1 += Math.Abs(img1.Data[i] - img2.Data[i]);
					sum2 += img1.Data[i] + img2.Data[i];
				}
			else
				for(int y=0; y<img1.H; y++)
					for(int x=0; x<img1.W; x++)
					{
						sum1 += Math.Abs((float)img1[x, y] - (float)img2[x, y]);
						sum2 += (float)img1[x, y] + (float)img2[x, y];
					}

			if(sum2 == 0)
				return 1;
			else
				return 1 - sum1 / sum2;
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
			return Catalog.GetString("Calculated C1 metric.");
		}
	}
}
