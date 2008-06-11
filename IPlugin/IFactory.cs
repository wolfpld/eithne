namespace Eithne
{
	public interface IFactory
	{
		IInfo Info	{ get; }
		IType Type	{ get; }

		Plugin.Base Create();
	}
}
