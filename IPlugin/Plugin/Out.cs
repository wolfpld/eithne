namespace Eithne
{
	namespace Plugin
	{
		public abstract class Out : Base
		{
			public override void Setup()
			{}

			public override bool HasSetup
			{
				get { return false; }
			}

			public abstract void DisplayResults();

			public override int NumOut			{ get { return 0; } }
			public override string DescOut(int n)		{ return null; }
			public override string[] MatchOut		{ get { return null; } }
		}
	}
}
