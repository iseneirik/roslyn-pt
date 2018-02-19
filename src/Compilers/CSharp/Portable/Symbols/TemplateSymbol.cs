using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis;

namespace Microsoft.CodeAnalysis.CSharp.Symbols
{
    internal class TemplateSymbol : NamespaceOrTypeSymbol, ITemplateSymbol
    {
        private readonly Symbol _container;

        internal TemplateSymbol(Symbol container)
        {
            _container = container;
        }

        public override SymbolKind Kind => SymbolKind.Template;
        public override Accessibility DeclaredAccessibility => Accessibility.Public;

        internal override ObsoleteAttributeData ObsoleteAttributeData => null;
        public override Symbol ContainingSymbol => _container;
        public override ImmutableArray<Location> Locations => throw new System.NotImplementedException("TemplateSymbol: Locations");
        public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => throw new System.NotImplementedException("TemplateSymbol: DeclaringSyntaxReferences");

        public override bool IsStatic => true;
        public override bool IsAbstract => false;
        public override bool IsSealed => true;

        public override void Accept(SymbolVisitor visitor)
        {
            throw new System.NotImplementedException();
        }

        public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
        {
            throw new System.NotImplementedException();
        }

        public override void Accept(CSharpSymbolVisitor visitor)
        {
            throw new System.NotImplementedException("TemplateSymbol: Accept(CSharpSymbolVisitor visitor)");
        }

        public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
        {
            throw new System.NotImplementedException("TemplateSymbol: Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)");
        }

        internal override TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument a)
        {
            throw new System.NotImplementedException("TemplateSymbol: Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument a)");
        }

        public override ImmutableArray<Symbol> GetMembers()
        {
            throw new System.NotImplementedException("TemplateSymbol: ImmutableArray<Symbol> GetMembers()");
        }

        public override ImmutableArray<Symbol> GetMembers(string name)
        {
            throw new System.NotImplementedException("TemplateSymbol: ImmutableArray<Symbol> GetMembers(string name)");
        }

        public override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
        {
            throw new System.NotImplementedException("TemplateSymbol: ImmutableArray<NamedTypeSymbol> GetTypeMembers()");
        }

        public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name)
        {
            throw new System.NotImplementedException("TemplateSymbol: ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name)");
        }
    }
}
