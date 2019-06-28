namespace Hotkeys
{
	public struct Result<O, E>
	{
		public O Ok { get; }
		public E Err { get; }
		public string Msg { get; }

		public Result(O ok, E err, string msg = null)
		{
			Ok = ok;
			Err = err;
			Msg = msg;
		}
	}
	public struct Result<E>
	{
		public E Err { get; }
		public string Msg { get; }

		public Result(E err, string msg = null)
		{
			Err = err;
			Msg = msg;
		}
	}
}
