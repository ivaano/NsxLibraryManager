namespace NsxLibraryManager.Utils;

public sealed class EmptyLookup<T, K> : ILookup<T, K> 
{
    public static EmptyLookup<T, K> Instance { get; } = new EmptyLookup<T, K>();

    private EmptyLookup() { }

    public bool Contains(T key) => false;

    public int Count => 0;

    public IEnumerable<K> this[T key] => Enumerable.Empty<K>();

    public IEnumerator<IGrouping<T, K>> GetEnumerator() => Enumerable.Empty<IGrouping<T, K>>().GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}