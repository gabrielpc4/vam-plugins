namespace Leap.Unity;

public abstract class MultiTypedReference<BaseType> where BaseType : class
{
	public abstract BaseType Value { get; set; }

	public abstract void Clear();
}
