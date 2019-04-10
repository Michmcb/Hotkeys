namespace Hotkeys
{
	public class WaitToken<T> : System.Threading.ManualResetEventSlim
	{
		public T Result { get; set; }

		public WaitToken() : base()
		{

		}
		public WaitToken(bool initialState) : base(initialState)
		{

		}
		public WaitToken(bool initialState, int spinCount) : base(initialState, spinCount)
		{

		}
	}
}
