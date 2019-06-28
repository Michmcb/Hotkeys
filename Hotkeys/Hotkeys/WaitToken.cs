namespace Hotkeys
{
	/// <summary>
	/// This is just a manual reset event with a result attached
	/// </summary>
	/// <typeparam name="T">The type of the result</typeparam>
	public class WaitToken<T> : System.Threading.ManualResetEventSlim
	{
		public T Result { get; set; }
		public WaitToken() : base() { }
		public WaitToken(bool initialState) : base(initialState) { }
		public WaitToken(bool initialState, int spinCount) : base(initialState, spinCount) { }
	}
}
