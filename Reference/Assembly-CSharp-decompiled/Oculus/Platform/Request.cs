namespace Oculus.Platform;

public sealed class Request<T> : Request
{
	public Request(ulong requestID)
		: base(requestID)
	{
	}

	public Request<T> OnComplete(Message<T>.Callback callback)
	{
		Callback.OnComplete(this, callback);
		return this;
	}
}
