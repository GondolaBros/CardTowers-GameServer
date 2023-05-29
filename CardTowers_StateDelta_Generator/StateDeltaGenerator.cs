using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace CardTowers_StateDelta_Generator
{
    [Generator]
    public class StateDeltaGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Register our syntax receiver with the generator initialization context.
            // The RegisterForSyntaxNotifications method is part of the Roslyn API and it expects
            // a Func that returns an instance of a class implementing ISyntaxReceiver.
            // Here, we're providing a lambda function that creates a new instance of our StateDeltaSyntaxReceiver.
            context.RegisterForSyntaxNotifications(() => new StateDeltaSyntaxReceiver());
        }


        public void Execute(GeneratorExecutionContext context)
        {
            // Logging to see if Execute method is being called.
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("CT001", "StateDeltaGenerator", "Execute method was called", "StateDeltaGenerator", DiagnosticSeverity.Info, true),
                Location.None));
#if DEBUG
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Launch();
            }
#endif

            // Step 1: Retrieve the candidate classes from the syntax receiver
            if (!(context.SyntaxReceiver is StateDeltaSyntaxReceiver receiver))
                return;

            // Step 2: Prepare to generate source code
            var sourceBuilder = new StringBuilder();

            foreach (var classDeclaration in receiver.CandidateClasses)
            {
                // Step 3: Extract class information
                var namespaceName = classDeclaration.Parent is NamespaceDeclarationSyntax namespaceDeclaration ? namespaceDeclaration.Name.ToString() : "";
                var className = classDeclaration.Identifier.ToString();

                // Step 4: Generate the source code for the delta class
                sourceBuilder.Clear();
                sourceBuilder.Append($@"
                namespace {namespaceName}.Deltas
                {{
                    public class {className}Delta : IDelta
                    {{
                        // Add your delta properties and methods here...
                    }}
                }}");
                // Step 5: Add the generated source code to the compilation
                context.AddSource($"{className}Delta.g.cs", sourceBuilder.ToString());

                context.AddSource("test", SourceText.From("public class Test { }", Encoding.UTF8));
            }
        }
    }
}


