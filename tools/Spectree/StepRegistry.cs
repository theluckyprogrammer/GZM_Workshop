using System.Collections.Concurrent;
using System.Reflection;

namespace Spectree;

// Finds the [Step] method for a sentence of the specification and runs it. Generated tests call RunAsync
// with their own assembly, so the steps live next to the tests.
public static class StepRegistry
{
    private const BindingFlags Methods = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
    private static readonly ConcurrentDictionary<Assembly, Lazy<IReadOnlyList<StepBinding>>> Bindings = new();

    public static Task RunAsync(Assembly assembly, string sentence)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentException.ThrowIfNullOrWhiteSpace(sentence);

        return Find(assembly, sentence).ExecuteAsync(sentence);
    }

    public static StepBinding Find(Assembly assembly, string sentence)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentException.ThrowIfNullOrWhiteSpace(sentence);

        var matches = For(assembly).Where(b => b.Matches(sentence)).ToList();
        return matches.Count switch
        {
            1 => matches[0],
            0 => throw new InvalidOperationException($"Brak definicji kroku dla zdania: „{sentence}”. Dodaj metodę z [Step(\"…\")] o dokładnie tym zdaniu."),
            _ => throw new InvalidOperationException($"Zdanie „{sentence}” pasuje do kilku kroków: {string.Join(", ", matches.Select(m => m.Name))}."),
        };
    }

    public static IReadOnlyList<StepBinding> For(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return Bindings.GetOrAdd(assembly, a => new Lazy<IReadOnlyList<StepBinding>>(() => Build(a))).Value;
    }

    private static List<StepBinding> Build(Assembly assembly) =>
        assembly.GetTypes()
            .SelectMany(type => type.GetMethods(Methods))
            .SelectMany(method => method.GetCustomAttributes<StepAttribute>().Select(a => new StepBinding(method, a.Sentence)))
            .ToList();
}
