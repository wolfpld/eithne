using System;
using Mono.Unix;

namespace Eithne
{
	public class IImage
	{
		private int w, h;
		private byte[] data;
		private int bpp;

		public IImage(int bpp, int w, int h, byte[] data)
		{
			RecreateImage(bpp, w, h, data, false);
		}

		public IImage(int bpp, int w, int h, byte[] data, bool copy)
		{
			RecreateImage(bpp, w, h, data, copy);
		}

		private void RecreateImage(int bpp, int w, int h, byte[] data, bool copy)
		{
			if(bpp != 1 && bpp != 3 && bpp != 4)
				throw new Exception(Catalog.GetString("BPP must be 1, 3 or 4"));

			this.bpp = bpp;
			this.w = w;
			this.h = h;

			if(copy)
			{
				this.data = new byte[w * h * bpp];
				for(int i=0; i<w*h*bpp; i++)
					this.data[i] = data[i];
			}
			else
				this.data = data;
		}

		public IImage(int bpp, int w, int h)
		{
			if(bpp != 1 && bpp != 3 && bpp != 4)
				throw new Exception(Catalog.GetString("BPP must be 1, 3 or 4"));

			this.bpp = bpp;
			this.w = w;
			this.h = h;

			data = new byte[w * h * bpp];
		}

		public void Invert()
		{
			if(bpp == 1)
				for(int x=0; x<w; x++)
					for(int y=0; y<h; y++)
						PutPixel(x, y, (byte)(255 - (byte)GetPixel(x, y)));
			else if(bpp == 3)
				for(int x=0; x<w; x++)
					for(int y=0; y<h; y++)
					{
						int c = (int)GetPixel(x, y);
						
						byte r = (byte)((c & 0xFF0000) >> 16);
						byte g = (byte)((c & 0x00FF00) >> 8);
						byte b = (byte)(c & 0x0000FF);

						r = (byte)(255 - r);
						g = (byte)(255 - g);
						b = (byte)(255 - b);

						PutPixel(x, y, (r << 16) + (g << 8) + b);
					}
			else
				throw new Exception(Catalog.GetString("Image inversion not supported for floating point data"));
		}

		// zwracamy object, bo nie wiadomo czy będzie bajt, czy int, czy float, ale wewnątrz wszystko
		// jest jako int traktowane
		public object this [int x, int y]
		{
			get { return GetPixel(x, y); }
			set { PutPixel(x, y, value); }
		}

		public int W
		{
			get { return w; }
		}

		public int H
		{
			get { return h; }
		}

		public int BPP
		{
			get { return bpp; }
		}

		public byte[] Data
		{
			get { return data; }
		}

		private unsafe object GetPixel(int x, int y)
		{
			if(bpp == 1)
				return data[x + w*y];
			else if(bpp == 3)
				return (data[(x + w*y)*3] << 16) + (data[(x + w*y)*3 + 1] << 8) + data[(x + w*y)*3 + 2];
			else
				fixed(byte *ptr = data)
				{
					return *(((float*)ptr) + x + w*y);
				}
		}

		private unsafe void PutPixel(int x, int y, object val)
		{
			if(bpp == 1)
				data[x + w*y] = (byte)val;
			else if (bpp == 3)
			{
				data[(x + w*y)*3] =	(byte)(((int)val & 0xFF0000) >> 16);
				data[(x + w*y)*3 + 1] =	(byte)(((int)val & 0x00FF00) >> 8);
				data[(x + w*y)*3 + 2] = (byte)((int)val & 0x0000FF);
			}
			else
				fixed(byte *ptr = data)
				{
					*(((float*)ptr) + x + w*y) = (float)val;
				}
		}

		public void Clear(object val)
		{
			for(int y=0; y<h; y++)
				for(int x=0; x<w; x++)
					PutPixel(x, y, val);
		}
	}
}
