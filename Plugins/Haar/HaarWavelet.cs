using System;
using System.Collections;

namespace Eithne
{
	class HarrWavelet
	{
		public static IImage Merge(ArrayList hw)
		{
			return Merge((IImage)hw[0], (IImage)hw[1], (IImage)hw[2], (IImage)hw[3]);
		}

		public static IImage Merge(IImage tl, IImage tr, IImage bl, IImage br)
		{
			int w = ((IImage)tl).W;
			int h = ((IImage)tl).H;

			IImage ret = new IImage(1, w*2, h*2);

			for(int y=0; y<h; y++)
				for(int x=0; x<w; x++)
				{
					ret[x, y] = tl[x, y];
					ret[x + w, y] = tr[x, y];
					ret[x, y + h] = bl[x, y];
					ret[x + w, y + h] = br[x, y];
				}

			return ret;
		}

		public static ArrayList Transform(IImage img, int cutoff)
		{
			// transformata po wierszach
			IImage tmp1 = new IImage(1, img.W/2, img.H);
			IImage tmp2 = new IImage(1, img.W/2, img.H);

			for(int y=0; y<img.H; y++)
				for(int x=0; x<img.W/2; x++)
				{
					tmp1[x, y] = (byte)(((byte)img[x*2, y] + (byte)img[x*2+1, y])/2);
					tmp2[x, y] = (byte)(127 + ((byte)img[x*2, y] - (byte)tmp1[x, y]));
				}

			// transformata po kolumnach
			IImage tl = new IImage(1, img.W/2, img.H/2);
			IImage tr = new IImage(1, img.W/2, img.H/2);
			IImage bl = new IImage(1, img.W/2, img.H/2);
			IImage br = new IImage(1, img.W/2, img.H/2);

			for(int y=0; y<img.H/2; y++)
				for(int x=0; x<img.W/2; x++)
				{
					tl[x, y] = (byte)(((byte)tmp1[x, y*2] + (byte)tmp1[x, y*2+1])/2);
					bl[x, y] = (byte)(127 + ((byte)tmp1[x, y*2] - (byte)tl[x, y]));

					tr[x, y] = (byte)(((byte)tmp2[x, y*2] + (byte)tmp2[x, y*2+1])/2);
					br[x, y] = (byte)(127 + ((byte)tmp2[x, y*2] - (byte)tr[x, y]));
				}

			// kompresja
			if(cutoff != 0)
				for(int y=0; y<tl.H; y++)
					for(int x=0; x<tl.W; x++)
					{
						if(Math.Abs((byte)tr[x, y] - 127) <= cutoff)
							tr[x, y] = (byte)127;
						if(Math.Abs((byte)bl[x, y] - 127) <= cutoff)
							bl[x, y] = (byte)127;
						if(Math.Abs((byte)br[x, y] - 127) <= cutoff)
							br[x, y] = (byte)127;
					}

			ArrayList ret = new ArrayList();
			ret.Add(tl);
			ret.Add(tr);
			ret.Add(bl);
			ret.Add(br);

			return ret;
		}

		public static IImage Transform(IImage img, int levels, int cutoff)
		{
			ArrayList hw = Transform(img, cutoff);

			if(levels == 0)
				return Merge(hw);
			else
				return Merge(Transform((IImage)hw[0], levels - 1, cutoff), (IImage)hw[1], (IImage)hw[2], (IImage)hw[3]);
		}

		public static IImage Inverse(IImage img, int levels)
		{
			int w = img.W/2;
			int h = img.H/2;

			IImage tl = new IImage(1, w, h);
			IImage tr = new IImage(1, w, h);
			IImage bl = new IImage(1, w, h);
			IImage br = new IImage(1, w, h);

			for(int y=0; y<h; y++)
				for(int x=0; x<w; x++)
				{
					tl[x, y] = img[x, y];
					tr[x, y] = img[x+w, y];
					bl[x, y] = img[x, y+h];
					br[x, y] = img[x+w, y+h];
				}

			if(levels == 0)
				return Inverse(tl, tr, bl, br);
			else
				return Inverse(Inverse(tl, levels - 1), tr, bl, br);
		}

		public static IImage Inverse(IImage img)
		{
			int w = img.W/2;
			int h = img.H/2;

			IImage tl = new IImage(1, w, h);
			IImage tr = new IImage(1, w, h);
			IImage bl = new IImage(1, w, h);
			IImage br = new IImage(1, w, h);

			for(int y=0; y<h; y++)
				for(int x=0; x<w; x++)
				{
					tl[x, y] = img[x, y];
					tr[x, y] = img[x+w, y];
					bl[x, y] = img[x, y+h];
					br[x, y] = img[x+w, y+h];
				}

			return Inverse(tl, tr, bl, br);
		}

		public static IImage Inverse(IImage tl, IImage tr, IImage bl, IImage br)
		{
			IImage tmp1 = new IImage(1, tl.W, tl.H*2);
			IImage tmp2 = new IImage(1, tl.W, tl.H*2);

			for(int y=0; y<tl.H; y++)
				for(int x=0; x<tl.W; x++)
				{
					tmp1[x, y*2] = (byte)((byte)tl[x, y] + ((byte)bl[x, y] - 127));
					tmp1[x, y*2+1] = (byte)((byte)tl[x, y] - ((byte)bl[x, y] - 127));

					tmp2[x, y*2] = (byte)((byte)tr[x, y] + ((byte)br[x, y] - 127));
					tmp2[x, y*2+1] = (byte)((byte)tr[x, y] - ((byte)br[x, y] - 127));
				}

			IImage ret = new IImage(1, tl.W*2, tl.H*2);

			for(int y=0; y<tl.H*2; y++)
				for(int x=0; x<tl.W; x++)
				{
					ret[x*2, y] = (byte)((byte)tmp1[x, y] + ((byte)tmp2[x, y] - 127));
					ret[x*2+1, y] = (byte)((byte)tmp1[x, y] - ((byte)tmp2[x, y] - 127));
				}
			
			return ret;
		}
	}
}
