namespace Spectree;

// Binds a method to one sentence of a specification. "{name}" in the sentence is a parameter: it matches any text
// and is passed to the method as a string, in order. The method may return Task.
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class StepAttribute(string sentence) : Attribute
{
    public string Sentence { get; } = sentence;
}
