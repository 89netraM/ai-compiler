using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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

        productionContext.AddSource(
            $"{className}.{methodName}.g.cs",
            $$"""
            namespace {{namespaceName}};

            partial class {{className}}
            {
                {{methodSignature}}
                {
                    throw new global::System.NotImplementedException();
                }
            }
            """
        );
    }
}
