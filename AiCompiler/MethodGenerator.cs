using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.ML.OnnxRuntimeGenAI;

namespace AiCompiler;

[Generator]
public class MethodGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(GenerateAiAttribute);

        var aiGeneratedMethods = context.SyntaxProvider.ForAttributeWithMetadataName(
            "AiCompiler.AiGeneratedAttribute",
            (node, cancellationToken) => true,
            (context, cancellationToken) => context
        );

        context.RegisterSourceOutput(aiGeneratedMethods, GenerateAiMethod);
    }

    private static void GenerateAiAttribute(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource(
            "AiGeneratedAttribute.g.cs",
            """
            namespace AiCompiler;

            [global::System.AttributeUsage(global::System.AttributeTargets.Method)]
            public class AiGeneratedAttribute : global::System.Attribute;
            """
        );
    }

    private static void GenerateAiMethod(
        SourceProductionContext productionContext,
        GeneratorAttributeSyntaxContext attributeContext
    )
    {
        var methodNode = attributeContext.TargetNode;
        if (attributeContext.SemanticModel.GetDeclaredSymbol(methodNode) is not IMethodSymbol methodSymbol)
        {
            return;
        }

        var namespaceName = methodSymbol.ContainingNamespace.ToDisplayString();
        var className = methodSymbol.ContainingType.Name;

        var methodComments = string.Join(
            "\n",
            methodNode
                .GetLeadingTrivia()
                .Where(t =>
                    t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                    || t.IsKind(SyntaxKind.SingleLineCommentTrivia)
                )
                .Select(t => t.ToFullString())
        );

        var methodAccessibility = methodSymbol.DeclaredAccessibility.ToString().ToLower();
        var methodStatic = methodSymbol.IsStatic ? "static " : "";
        var methodReadOnly = methodSymbol.IsReadOnly ? "readonly " : "";
        var methodReturnType = methodSymbol.ReturnType.ToDisplayString();
        var methodName = methodSymbol.Name;
        var methodParameters = string.Join(
            ", ",
            methodSymbol.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}")
        );
        var methodSignature =
            $"{methodAccessibility} partial {methodStatic} {methodReadOnly} {methodReturnType} {methodName}({methodParameters})";

        var methodBody = GenerateMethodBody(methodComments, methodSignature, productionContext.CancellationToken);

        productionContext.AddSource(
            $"{className}.{methodName}.g.cs",
            $$"""
            namespace {{namespaceName}};

            partial class {{className}}
            {
                {{methodSignature}}
                {
                    {{methodBody}}
                }
            }
            """
        );
    }

    public static string GenerateMethodBody(
        string methodComments,
        string methodSignature,
        CancellationToken cancellationToken
    )
    {
        using var model = ModelDownloader.GetModel(cancellationToken);
        using var tokenizer = new Tokenizer(model);

        using var tokens = tokenizer.Encode(
            $"""
            <|system|>
            You are an expert C# developer. You are tasked with implementing a method
            given the documentation and signature.
            When using libraries, you can not assume they are imported. Always fully
            qualify types, with global:: and namespaces. Even for the standard library.
            You only ever respond with the body of the method. No additional information
            or comments are needed. The code should be correct and complete. No
            formatting. No imports. No boilerplate code. No additional methods. No
            method declaration. Only the body of the method.
            <|end|>
            <|user|>
            {methodComments}
            {methodSignature}
            <|end|>
            <|assistant|>
            """
        );

        using var generatorParams = new GeneratorParams(model);
        generatorParams.SetInputSequences(tokens);

        using var generator = new Generator(model, generatorParams);

        var sb = new StringBuilder();
        while (!generator.IsDone())
        {
            generator.ComputeLogits();
            generator.GenerateNextToken();
            var outputTokens = generator.GetSequence(0);
            var newToken = outputTokens.Slice(outputTokens.Length - 1, 1);
            var output = tokenizer.Decode(newToken);
            sb.Append(output);
        }

        return sb.ToString();
    }
}
