using System;
using System.Runtime.InteropServices;

namespace Leap.Unity;

public static class Maybe
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct NoneType
	{
	}

	public static readonly NoneType None = default(NoneType);

	public static Maybe<T> Some<T>(T value)
	{
		return new Maybe<T>(value);
	}

	public static void MatchAll<A, B>(Maybe<A> maybeA, Maybe<B> maybeB, Action<A, B> action)
	{
		maybeA.Match(delegate(A a)
		{
			maybeB.Match(delegate(B b)
			{
				action(a, b);
			});
		});
	}

	public static void MatchAll<A, B, C>(Maybe<A> maybeA, Maybe<B> maybeB, Maybe<C> maybeC, Action<A, B, C> action)
	{
		maybeA.Match(delegate(A a)
		{
			maybeB.Match(delegate(B b)
			{
				maybeC.Match(delegate(C c)
				{
					action(a, b, c);
				});
			});
		});
	}

	public static void MatchAll<A, B, C, D>(Maybe<A> maybeA, Maybe<B> maybeB, Maybe<C> maybeC, Maybe<D> maybeD, Action<A, B, C, D> action)
	{
		maybeA.Match(delegate(A a)
		{
			maybeB.Match(delegate(B b)
			{
				maybeC.Match(delegate(C c)
				{
					maybeD.Match(delegate(D d)
					{
						action(a, b, c, d);
					});
				});
			});
		});
	}
}
