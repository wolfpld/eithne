using System.Drawing;
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

		// very dumb method, but pixbuf can only hold rgb data
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

		public static Bitmap LoadBitmap(string name)
		{
			Bitmap tmp = new Bitmap(name);

			// file is now locked, so create copy of bitmap and release it
			Bitmap bmp = new Bitmap(tmp);
			tmp.Dispose();

			return bmp;
		}
	}
}
