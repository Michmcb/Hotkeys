namespace Hotkeys
{
	public static class HotkeyId
	{
		public static int NextId { get; private set; } = 1;
		public static int GetNextId()
		{
			return NextId++;
		}
	}
}
