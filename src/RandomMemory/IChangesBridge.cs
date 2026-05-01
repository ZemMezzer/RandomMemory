using System;

namespace RandomMemory;

public interface IChangesBridge : IDisposable
{
    public IObservable<TTable> OnChange<TTable>();
    public void EnqueueChange<TTable>(TTable table);
    public void Publish();
    public void Clear();
}
