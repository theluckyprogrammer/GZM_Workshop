using System.Reflection;
using System.Text.RegularExpressions;

namespace Spectree;

// One [Step] sentence of one method: the sentence as a regular expression and the call with the matched parameters.
public sealed partial class StepBinding
{
    private readonly MethodInfo _method;
    private readonly Regex _pattern;

    public StepBinding(MethodInfo method, string sentence)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentException.ThrowIfNullOrWhiteSpace(sentence);

        _method = method;
        Sentence = sentence;
        _pattern = new Regex("^" + Parameter().Replace(Regex.Escape(sentence), "(.+?)") + "$", RegexOptions.CultureInvariant);
        var expected = _pattern.GetGroupNumbers().Length - 1;
        if (method.GetParameters().Length != expected || method.GetParameters().Any(p => p.ParameterType != typeof(string)))
            throw new InvalidOperationException($"{Name}: zdanie „{sentence}” ma {expected} parametrów {{…}}, a metoda musi mieć tyle samo parametrów typu string.");
    }

    public string Sentence { get; }

    public string Name => $"{_method.DeclaringType?.Name}.{_method.Name}";

    public bool Matches(string sentence)
    {
        ArgumentNullException.ThrowIfNull(sentence);

        return _pattern.IsMatch(sentence);
    }

    public async Task ExecuteAsync(string sentence)
    {
        ArgumentNullException.ThrowIfNull(sentence);

        var arguments = _pattern.Match(sentence).Groups.Values.Skip(1).Select(g => (object?)g.Value).ToArray();
        var target = _method.IsStatic ? null : Activator.CreateInstance(_method.DeclaringType!);
        var result = Invoke(target, arguments);
        if (result is Task task)
            await task;
    }

    // Unwraps TargetInvocationException, so the test shows the step's own assertion message.
    private object? Invoke(object? target, object?[] arguments)
    {
        try
        {
            return _method.Invoke(target, arguments);
        }
        catch (TargetInvocationException e) when (e.InnerException is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            throw;
        }
    }

    // "{name}" after Regex.Escape is "\{name}".
    [GeneratedRegex(@"\\\{\w+\}")]
    private static partial Regex Parameter();
}
