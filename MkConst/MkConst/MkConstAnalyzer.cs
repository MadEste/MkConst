using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;



namespace MkConst
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MkConstAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MkConst";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        //changed category... I don't know why?
        private const string Category = "Usage";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            // context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
            // changed to act on syntax, not symbols
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.LocalDeclarationStatement);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            // Find just those named type symbols with names containing lowercase letters.
            if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // should always succede since why registered for local declaration changes
            var localDeclaration = (LocalDeclarationStatementSyntax)context.Node;
            // make sure the declaration isn't already const:
            if (localDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword))
            {
                return;
            }
            // Perform data flow analysis on the local declaration.
            var dataFlowAnalysis = context.SemanticModel.AnalyzeDataFlow(localDeclaration);

            // Retrieve the local symbol for each variable in the local declaration
            // and ensure that it is not written outside of the data flow analysis region.
            var variable = localDeclaration.Declaration.Variables.Single();
            var variableSymbol = context.SemanticModel.GetDeclaredSymbol(variable);
            if (dataFlowAnalysis.WrittenOutside.Contains(variableSymbol))
            {
                return;
            }
            // if it made it here raise a diagnostic
            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
        }
    }
}
