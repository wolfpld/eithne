namespace Eithne
{
	public interface IFactory
	{
		IInfo Info	{ get; }
		IType Type	{ get; }

		void Initialize();							// TODO: remove?
		IPlugin Create();
	}
}
