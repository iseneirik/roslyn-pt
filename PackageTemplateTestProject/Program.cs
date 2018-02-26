using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;

namespace PackageTemplateTestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            var compilation = CreateStandardCompilation(@"
namespace N
{
    template T
    {
    }
    class C
    {
    }
}
");
            var tree = compilation.SyntaxTrees[0];
            var root = tree.GetCompilationUnitRoot();
            var model = compilation.GetSemanticModel(tree);
            var namespaceDecl = (NamespaceDeclarationSyntax) root.Members[0];
            var namespaceSymbol = model.GetDeclaredSymbol(namespaceDecl);
            var classDecl = (ClassDeclarationSyntax) namespaceDecl.Members[1];
            var classSymbol = model.GetDeclaredSymbol(classDecl);
            var templateDecl = (TemplateDeclarationSyntax) namespaceDecl.Members[0];
            var templateSymbol = model.GetDeclaredSymbol(templateDecl);
        }

        public static CSharpCompilation CreateStandardCompilation(
            string text,
            IEnumerable<MetadataReference> references = null,
            CSharpCompilationOptions options = null,
            CSharpParseOptions parseOptions = null,
            string assemblyName = "",
            string sourceFileName = "")
        {
            return CreateStandardCompilation(
                new[] { CSharpTestBase.Parse(text, sourceFileName, parseOptions) },
                references: references,
                options: options,
                assemblyName: assemblyName);
        }

        public static CSharpCompilation CreateStandardCompilation(
            IEnumerable<SyntaxTree> trees,
            IEnumerable<MetadataReference> references = null,
            CSharpCompilationOptions options = null,
            string assemblyName = "")
        {
            return CreateCompilation(trees, null, options, assemblyName);
        }

        public static CSharpCompilation CreateCompilation(
            IEnumerable<SyntaxTree> trees,
            IEnumerable<MetadataReference> references = null,
            CSharpCompilationOptions options = null,
            string assemblyName = "")
        {
            if (options == null)
            {
                options = TestOptions.ReleaseDll;
            }

            // Using single-threaded build if debugger attached, to simplify debugging.
            if (Debugger.IsAttached)
            {
                options = options.WithConcurrentBuild(false);
            }

            return CSharpCompilation.Create(
                assemblyName == "" ? GetUniqueName() : assemblyName,
                trees,
                references,
                options);
        }

        private static SyntaxTree CheckSerializable(SyntaxTree tree)
        {
            var stream = new MemoryStream();
            var root = tree.GetRoot();
            root.SerializeTo(stream);
            stream.Position = 0;
            var deserializedRoot = CSharpSyntaxNode.DeserializeFrom(stream);
            return tree;
        }

        public static string GetUniqueName()
        {
            return Guid.NewGuid().ToString("D");
        }
    }
}
