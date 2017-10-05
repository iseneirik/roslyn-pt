// Package Template Implementation

using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.Syntax
{
    public partial class TemplateDeclarationSyntax
    {
        new internal Syntax.InternalSyntax.TemplateDeclarationSyntax Green
        {
            get
            {
                return (Syntax.InternalSyntax.TemplateDeclarationSyntax)base.Green;
            }
        }
    }
}
