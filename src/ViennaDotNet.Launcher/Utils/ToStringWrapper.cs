namespace ViennaDotNet.Launcher.Utils;

internal readonly struct ToStringWrapper<T>
{
    private readonly T _value;
    private readonly Func<T, string> _valueToString;

    public ToStringWrapper(T value, Func<T, string> valueToString)
    {
        _value = value;
        _valueToString = valueToString;
    }

    public T Value => _value;

    public override string ToString()
        => _valueToString(_value);
}
