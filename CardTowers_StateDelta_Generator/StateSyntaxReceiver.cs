using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CardTowers_StateDelta_Generator
{
    // Our SyntaxReceiver is responsible for processing the syntax tree of our code files and 
    // finding nodes of interest (in our case, classes with a GenerateStateDelta attribute).
    // It implements the ISyntaxReceiver interface, which is a part of the Roslyn API.
    internal class StateDeltaSyntaxReceiver : ISyntaxReceiver
    {
        // CandidateClasses will hold all the class declarations in the syntax tree that have 
        // our GenerateStateDelta attribute. We're using a List to store them.
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

        // This is the method that gets called for every syntax node in the tree. 
        // If the node represents a class declaration and it has our attribute, we add it to our list.
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // We're interested in nodes of type ClassDeclarationSyntax with attributes.
            // So, we use the "is" keyword to check if syntaxNode is of the right type.
            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax
                && classDeclarationSyntax.AttributeLists.Count > 0)
            {
                // Get all attributes of the class
                var attributes = classDeclarationSyntax.AttributeLists.SelectMany(a => a.Attributes);

                // If any of the attributes match the name of our attribute, add this class to the list
                if (attributes.Any(a => a.Name.ToString().Contains("GenerateStateDelta")))
                {
                    CandidateClasses.Add(classDeclarationSyntax);
                }
            }
        }
    }
}



