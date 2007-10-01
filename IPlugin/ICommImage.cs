namespace Eithne
{
	public class ICommImage : ICommObject
	{
		private readonly IImage[] images;
		private readonly IImage[] orig;
		private readonly int[] categories;

		public ICommImage(IImage[] images, IImage[] orig, int[] categories)
		{
			this.images = images;
			this.orig = orig;
			this.categories = categories;
		}

		public IImage this [int n]
		{
			get { return images[n]; }
			set { images[n] = value; }
		}

		public int Length
		{
			get { return images.Length; }
		}

		public IImage[] Images
		{
			get { return images; }
		}

		public IImage[] OriginalImages
		{
			get { return orig; }
		}

		public IImage OriginalImage(int n)
		{
			return orig[n];
		}

		public int[] Categories
		{
			get { return categories; }
		}

		public int Category(int n)
		{
			return categories[n];
		}
	}
}
