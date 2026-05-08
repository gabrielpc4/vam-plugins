using System;

namespace Oculus.Platform;

public abstract class Message<T> : Message
{
	public new delegate void Callback(Message<T> message);

	private T data;

	public T Data => data;

	public Message(IntPtr c_message)
		: base(c_message)
	{
		if (!base.IsError)
		{
			data = GetDataFromMessage(c_message);
		}
	}

	protected abstract T GetDataFromMessage(IntPtr c_message);
}
