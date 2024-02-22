namespace Hotkeys
{
	public readonly struct Result<O, E>
	{
		public Result(O ok, E err)
		{
			Ok = ok;
			Err = err;
		}
		public O Ok { get; }
		public E Err { get; }
	}
}
