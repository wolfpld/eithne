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

		public static ArrayList Transform(IImage img)
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

			ArrayList ret = new ArrayList();
			ret.Add(tl);
			ret.Add(tr);
			ret.Add(bl);
			ret.Add(br);

			return ret;
		}
	}
}
