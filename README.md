# AiCompiler

Do you want
- Slower compile times
- Unreliable output, or
- The worst code imaginable?

Then `AiCompiler` is the NuGet package for you!

`AiCompiler` is **the** easiest way to write C#, by not writing any C# at all!
With a sleek `AiGenerated` attribute applied to an empty method, the compiler
writes the implementation for you!

Look at [this](./AiCompiler.Test/AdditionExample.cs) amazing example

```csharp
/// <summary>
/// Adds two integers.
/// </summary>
/// <param name="a">The first integer.</param>
/// <param name="b">The second integer.</param>
/// <returns>
/// The sum of the two integers.
/// </returns>
[AiGenerated]
private partial int Add(int a, int b);
```

With no effort[^1] from the developer at all the compiler generates code that
passes the tests

```csharp
Add(2, 3).Should().Be(5)
```

[^1]: Except a few comments

Use it in production today!

```sh
dotnet add package AiCompiler --version 0.1.0.0
```
