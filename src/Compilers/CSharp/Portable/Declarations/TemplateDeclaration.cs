using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Microsoft.CodeAnalysis.CSharp
{
    internal sealed class TemplateDeclaration : SingleNamespaceOrTypeDeclaration
    {
        private readonly ImmutableArray<SingleTypeDeclaration> _children;

        public TemplateDeclaration(
            string name,
            SyntaxReference syntaxReference,
            SourceLocation nameLocation,
            ImmutableArray<SingleTypeDeclaration> children,
            ImmutableArray<Diagnostic> diagnostics)
            : base(name, syntaxReference, nameLocation, diagnostics)
        {
            _children = children;
        }

        public override DeclarationKind Kind => DeclarationKind.Template;

        protected override ImmutableArray<SingleNamespaceOrTypeDeclaration> GetNamespaceOrTypeDeclarationChildren()
        {
            return StaticCast<SingleNamespaceOrTypeDeclaration>.From(_children);
        }
    }
}
