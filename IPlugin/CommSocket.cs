namespace Eithne
{
	public class CommSocket
	{
		private ICommObject[] obj;

		public CommSocket(int n)
		{
			obj = new ICommObject[n];
		}

		public ICommObject this [int n]
		{
			get { return obj[n]; }
			set { obj[n] = value; }
		}

		public int Length
		{
			get { return obj.Length; }
		}
	}

	public interface ICommObject
	{}
}
