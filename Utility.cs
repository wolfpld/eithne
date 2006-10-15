using Gdk;

namespace Eithne
{
	public class Utility
	{
		public static unsafe int GetPixel(Pixbuf buf, int x, int y)
		{
			byte *ptr = (byte*)buf.Pixels;

			int r = *(ptr + y * buf.Rowstride + x * buf.NChannels);
			int g = *(ptr + y * buf.Rowstride + x * buf.NChannels + 1);
			int b = *(ptr + y * buf.Rowstride + x * buf.NChannels + 2);

			return (r << 16) + (g << 8) + b;
		}

		public static IImage CreateImage(Pixbuf buf, int bpp)
		{
			byte[] data = new byte[buf.Height * buf.Width * bpp];

			for(int y=0; y<buf.Height; y++)
				for(int x=0; x<buf.Width; x++)
				{
					int color = Utility.GetPixel(buf, x, y);

					if(bpp == 1)
						data[y * buf.Width + x] = (byte)color;
					else
					{
						data[(y * buf.Width + x) * 3] = (byte)((color & 0xFF0000) >> 16);
						data[(y * buf.Width + x) * 3 + 1] = (byte)((color & 0x00FF00) >> 8);
						data[(y * buf.Width + x) * 3 + 2] = (byte)((color & 0x0000FF));
					}
				}

			return new IImage(bpp, buf.Width, buf.Height, data);
		}

		// strasznie durna metoda, ale pixbuf tylko rgb umie przechowywać
		public static bool IsBW(Pixbuf buf)
		{
			for(int y=0; y<buf.Height; y++)
				for(int x=0; x<buf.Width; x++)
				{
					int color = Utility.GetPixel(buf, x, y);

					int r = (color & 0xFF0000) >> 16;
					int g = (color & 0x00FF00) >> 8;
					int b = color & 0x0000FF;

					if(r != g || r != b)
						return false;
				}

			return true;
		}

		public static Pixbuf CreatePixbuf(IImage img)
		{
			byte[] data;

			if(img.BPP == 1)
			{
				// konwersja na RGB
				data = new byte[img.H * img.W * 3];

				for(int y=0; y<img.H; y++)
					for(int x=0; x<img.W; x++)
					{
						byte color = (byte)img[x, y];

						data[(x + y*img.W)*3] = color;
						data[(x + y*img.W)*3 + 1] = color;
						data[(x + y*img.W)*3 + 2] = color;
					}
			}
			else
				data = img.Data;

			Pixbuf tmp = new Pixbuf(data, false, 8, img.W, img.H, img.W * 3, null);

			// wyżej robiony jest wrapper na dane, dane po konwersji są tymczasowe, więc trzeba zrobić kopię
			if(img.BPP == 1)
				tmp = tmp.Copy();

			return tmp;
		}

		public static int[] FindResults(ICommResult r)
		{
			int[] res = new int[r.Length];

			for(int i=0; i<r.Length; i++)
			{
				double min = r.Difference(i, 0);
				int n = 0;

				for(int j=1; j<r[i].Length; j++)
					if(r.Difference(i, j) < min)
					{
						min = r.Difference(i, j);
						n = j;
					}

				res[i] = n;
			}

			return res;
		}
	}
}
