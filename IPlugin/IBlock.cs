namespace Eithne
{
	public interface IBlock
	{
		void Invalidate();
		void SlotsChanged();
	}
}
