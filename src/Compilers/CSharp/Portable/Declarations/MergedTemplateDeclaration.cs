using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Microsoft.CodeAnalysis.CSharp
{
    class MergedTemplateDeclaration : MergedNamespaceOrTypeDeclaration
    {
        private readonly TemplateDeclaration _declaration;

        public MergedTemplateDeclaration(TemplateDeclaration declaration)
            : base(declaration.Name)
        {
            _declaration = declaration;
        }

        public TemplateDeclaration Declaration => _declaration;
        public override DeclarationKind Kind => DeclarationKind.Template;

        protected override ImmutableArray<Declaration> GetDeclarationChildren()
        {
            return _declaration.Children.Cast<SingleNamespaceOrTypeDeclaration, Declaration>();
        }
    }
}
