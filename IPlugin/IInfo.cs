namespace Eithne
{
	public abstract class IInfo
	{
		public abstract string Name		{ get; }
		public abstract string ShortName	{ get; }
		public abstract string Author		{ get; }
		public abstract string Description	{ get; }

		public virtual string Version	
		{
			get { return Program.Version; }
		}
	}
}
