public class DIContainer
{
    private readonly Dictionary<Type, Func<object>> _registrations = new();

    public void Register<T>(Func<T> factory) => _registrations[typeof(T)] = () => factory();

    public T Resolve<T>()
    {
        if (_registrations.ContainsKey(typeof(T)))
            return (T)_registrations[typeof(T)]();

        throw new InvalidOperationException($"Тип {typeof(T)} не зарегистрирован в контейнере");
    }
}