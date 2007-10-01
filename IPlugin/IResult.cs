namespace Eithne
{
	public class IResult
	{
		private readonly double[] data;

		public IResult(double[] data)
		{
			this.data = data;
		}

		public double this [int i]
		{
			get { return data[i]; }
		}

		public int Length
		{
			get { return data.Length; }
		}

		public double[] Data
		{
			get { return data; }
		}
	}
}
