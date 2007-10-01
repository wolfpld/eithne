using System;
using Mono.Unix;

namespace Eithne
{
	public class IImage
	{
		private int w, h;
		private byte[] data;
		private BPP bpp;

		public IImage(BPP bpp, int w, int h, byte[] data)
		{
			RecreateImage(bpp, w, h, data, false);
		}

		public IImage(BPP bpp, int w, int h, byte[] data, bool copy)
		{
			RecreateImage(bpp, w, h, data, copy);
		}

		public IImage(BPP bpp, int w, int h)
		{
			this.bpp = bpp;
			this.w = w;
			this.h = h;

			data = new byte[w * h * (int)bpp];
		}

		private void RecreateImage(BPP bpp, int w, int h, byte[] data, bool copy)
		{
			this.bpp = bpp;
			this.w = w;
			this.h = h;

			if(copy)
			{
				this.data = new byte[w * h * (int)bpp];
				for(int i=0; i<w*h*(int)bpp; i++)
					this.data[i] = data[i];
			}
			else
				this.data = data;
		}

		public void Invert()
		{
			if(bpp == BPP.Grayscale)
				for(int x=0; x<w; x++)
					for(int y=0; y<h; y++)
						PutPixel(x, y, (byte)(255 - (byte)GetPixel(x, y)));
			else if(bpp == BPP.RGB)
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

		public BPP BPP
		{
			get { return bpp; }
		}

		public byte[] Data
		{
			get { return data; }
		}

		private unsafe object GetPixel(int x, int y)
		{
			if(bpp == BPP.Grayscale)
				return data[x + w*y];
			else if(bpp == BPP.RGB)
				return (data[(x + w*y)*3] << 16) + (data[(x + w*y)*3 + 1] << 8) + data[(x + w*y)*3 + 2];
			else
				fixed(byte *ptr = data)
				{
					return *(((float*)ptr) + x + w*y);
				}
		}

		private unsafe void PutPixel(int x, int y, object val)
		{
			if(bpp == BPP.Grayscale)
				data[x + w*y] = (byte)val;
			else if (bpp == BPP.RGB)
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
