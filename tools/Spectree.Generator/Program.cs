using Spectree.Generator;

// Writes the pair's test files as real code: one *.cs per spec file (scenarios/sNN/spec/*.md) into
// tests/GzmApp.Specs/Specs, so the Testing panel shows the rule sentences and "Go to Test" opens a normal
// test. Runs on every build (target SpectreeSync in tests/GzmApp.Specs) and by hand:
//   dotnet run --project tools/Spectree.Generator -- scenarios/s01/spec tests/GzmApp.Specs/Specs
if (args.Length != 2)
    return Fail("Użycie: dotnet run --project tools/Spectree.Generator -- <folder specyfikacji> <folder testów>");

return Sync.Run(args[0], args[1]);

static int Fail(string message)
{
    Console.Error.WriteLine("Spectree: " + message);
    return 1;
}
