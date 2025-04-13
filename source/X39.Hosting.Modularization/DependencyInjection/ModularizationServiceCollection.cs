using System.Collections;
using Microsoft.Extensions.DependencyInjection;

namespace X39.Hosting.Modularization.DependencyInjection;

/// <inheritdoc />
public class ModularizationServiceCollection : IServiceCollection
{
    private List<ServiceDescriptor> Services { get; } = new();

    /// <inheritdoc />
    public IEnumerator<ServiceDescriptor> GetEnumerator()
    {
        return Services.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable) Services).GetEnumerator();
    }

    /// <inheritdoc />
    public void Add(ServiceDescriptor item)
    {
        Services.Add(item);
    }

    /// <inheritdoc />
    public void Clear()
    {
        Services.Clear();
    }

    /// <inheritdoc />
    public bool Contains(ServiceDescriptor item)
    {
        return Services.Contains(item);
    }

    /// <inheritdoc />
    public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
    {
        Services.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
    public bool Remove(ServiceDescriptor item)
    {
        return Services.Remove(item);
    }

    /// <inheritdoc />
    public int Count => Services.Count;

    /// <inheritdoc />
    public bool IsReadOnly => ((ICollection<ServiceDescriptor>) Services).IsReadOnly;

    /// <inheritdoc />
    public int IndexOf(ServiceDescriptor item)
    {
        return Services.IndexOf(item);
    }

    /// <inheritdoc />
    public void Insert(int index, ServiceDescriptor item)
    {
        Services.Insert(index, item);
    }

    /// <inheritdoc />
    public void RemoveAt(int index)
    {
        Services.RemoveAt(index);
    }

    /// <inheritdoc />
    public ServiceDescriptor this[int index]
    {
        get => Services[index];
        set => Services[index] = value;
    }
}
