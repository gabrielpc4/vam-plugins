namespace Battlehub.RTSaveLoad;

public class StoragePayload<T>
{
	public T Path { get; private set; }

	public StoragePayload(T path)
	{
		Path = path;
	}
}
