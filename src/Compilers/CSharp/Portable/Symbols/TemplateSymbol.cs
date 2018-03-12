using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis;
using Roslyn.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.CSharp.Symbols
{
    internal class TemplateSymbol : NamespaceOrTypeSymbol, ITemplateSymbol
    {
        private readonly Symbol _containingSymbol;
        private readonly TemplateDeclaration _declaration;

        private SymbolCompletionState _state;
        private ImmutableArray<Symbol> _lazyAllMembers;
        private Dictionary<string, ImmutableArray<NamespaceOrTypeSymbol>> _nameToMembersMap;
        private Dictionary<string, ImmutableArray<NamedTypeSymbol>> _nameToTypeMembersMap;

        internal TemplateSymbol(Symbol container, MergedTemplateDeclaration declaration)
        {
            Debug.Assert(declaration.Kind == DeclarationKind.Template);
            _containingSymbol = container;
            _declaration = declaration.Declaration;
        }

        public override SymbolKind Kind => SymbolKind.Template;
        public override Accessibility DeclaredAccessibility => Accessibility.Public;
        public override string Name => _declaration.Name;

        public TemplateDeclaration Declaration => _declaration;
        internal override ObsoleteAttributeData ObsoleteAttributeData => null;
        public override Symbol ContainingSymbol => _containingSymbol;
        public override ImmutableArray<Location> Locations => ImmutableArray.Create((Location)_declaration.Location);
        public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(_declaration.SyntaxReference);

        public override bool IsStatic => true;
        public override bool IsAbstract => false;
        public override bool IsSealed => true;

        internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var incompletePart = _state.NextIncompletePart;
                switch (incompletePart)
                {
                    case CompletionPart.NameToMembersMap:
                        {
                            var tmp = GetNameToMembersMap();
                        }
                        break;

                    case CompletionPart.MembersCompleted:
                        {
                            var members = this.GetMembers();

                            bool allCompleted = true;

                            if (this.DeclaringCompilation.Options.ConcurrentBuild)
                            {
                                var po = cancellationToken.CanBeCanceled
                                    ? new ParallelOptions() { CancellationToken = cancellationToken }
                                    : CSharpCompilation.DefaultParallelOptions;

                                Parallel.For(0, members.Length, po, UICultureUtilities.WithCurrentUICulture<int>(i =>
                                {
                                    var member = members[i];
                                    ForceCompleteMemberByLocation(locationOpt, member, cancellationToken);
                                }));

                                foreach (var member in members)
                                {
                                    if (!member.HasComplete(CompletionPart.All))
                                    {
                                        allCompleted = false;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                foreach (var member in members)
                                {
                                    ForceCompleteMemberByLocation(locationOpt, member, cancellationToken);
                                    allCompleted = allCompleted && member.HasComplete(CompletionPart.All);
                                }
                            }

                            if (allCompleted)
                            {
                                _state.NotePartComplete(CompletionPart.MembersCompleted);
                                break;
                            }
                            else
                            {
                                // NOTE: we're going to kick out of the completion part loop after this,
                                // so not making progress isn't a problem.
                                goto done;
                            }
                        }

                    case CompletionPart.None:
                        return;

                    default:
                        // any other values are completion parts intended for other kinds of symbols
                        _state.NotePartComplete(CompletionPart.All & ~CompletionPart.NamespaceSymbolAll);
                        break;
                }

                _state.SpinWaitComplete(incompletePart, cancellationToken);
            }

        done:
            // Don't return until we've seen all of the CompletionParts. This ensures all
            // diagnostics have been reported (not necessarily on this thread).
            CompletionPart allParts = (locationOpt == null) ? CompletionPart.NamespaceSymbolAll : CompletionPart.NamespaceSymbolAll & ~CompletionPart.MembersCompleted;
            _state.SpinWaitComplete(allParts, cancellationToken);
        }

        public override void Accept(SymbolVisitor visitor)
        {
            visitor.VisitTemplate(this);
        }

        public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
        {
            throw new System.NotImplementedException();
        }

        public override void Accept(CSharpSymbolVisitor visitor)
        {
            visitor.VisitTemplate(this);
        }

        public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
        {
            throw new System.NotImplementedException("TemplateSymbol: Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)");
        }

        internal override TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitTemplate(this, argument);
        }

        public override ImmutableArray<Symbol> GetMembers()
        {
            var allMembers = this.GetMembersUnordered();

            if (allMembers.Length >= 2)
            {
                // The array isn't sorted. Sort it and remember that we sorted it.
                allMembers = allMembers.Sort(LexicalOrderSymbolComparer.Instance);
                ImmutableInterlocked.InterlockedExchange(ref _lazyAllMembers, allMembers);
            }

            return allMembers;
        }
        
        internal override ImmutableArray<Symbol> GetMembersUnordered()
        {
            var result = _lazyAllMembers;

            if (result.IsDefault)
            {
                var members = StaticCast<Symbol>.From(this.GetNameToMembersMap().Flatten(null));  // don't sort.
                ImmutableInterlocked.InterlockedInitialize(ref _lazyAllMembers, members);
                result = _lazyAllMembers;
            }

#if DEBUG
            // In DEBUG, swap first and last elements so that use of Unordered in a place it isn't warranted is caught
            // more obviously.
            return result.DeOrder();
#else
            return result;
#endif
        }

        public override ImmutableArray<Symbol> GetMembers(string name)
        {
            ImmutableArray<NamespaceOrTypeSymbol> members;
            return this.GetNameToMembersMap().TryGetValue(name, out members)
                ? members.Cast<NamespaceOrTypeSymbol, Symbol>()
                : ImmutableArray<Symbol>.Empty;
        }

        public override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
        {
            throw new System.NotImplementedException("TemplateSymbol: ImmutableArray<NamedTypeSymbol> GetTypeMembers()");
        }



        public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name)
        {
            ImmutableArray<NamedTypeSymbol> members;
            return this.GetNameToTypeMembersMap().TryGetValue(name, out members)
                ? members
                : ImmutableArray<NamedTypeSymbol>.Empty;
        }

        private Dictionary<string, ImmutableArray<NamespaceOrTypeSymbol>> GetNameToMembersMap()
        {
            if (_nameToMembersMap == null)
            {
                var diagnostics = DiagnosticBag.GetInstance();
                if (Interlocked.CompareExchange(ref _nameToMembersMap, MakeNameToMembersMap(diagnostics), null) == null)
                {
                    // NOTE: the following is not cancellable.  Once we've set the
                    // members, we *must* do the following to make sure we're in a consistent state.
                    this.DeclaringCompilation.DeclarationDiagnostics.AddRange(diagnostics);
                    //RegisterDeclaredCorTypes();

                    // We may produce a SymbolDeclaredEvent for the enclosing namespace before events for its contained members
                    DeclaringCompilation.SymbolDeclaredEvent(this);
                    var wasSetThisThread = _state.NotePartComplete(CompletionPart.NameToMembersMap);
                    Debug.Assert(wasSetThisThread);
                }

                diagnostics.Free();
            }

            return _nameToMembersMap;
        }

        private Dictionary<string, ImmutableArray<NamedTypeSymbol>> GetNameToTypeMembersMap()
        {
            if (_nameToTypeMembersMap == null)
            {
                // NOTE: This method depends on MakeNameToMembersMap() on creating a proper 
                // NOTE: type of the array, see comments in MakeNameToMembersMap() for details

                var dictionary = new Dictionary<string, ImmutableArray<NamedTypeSymbol>>(StringOrdinalComparer.Instance);

                Dictionary<string, ImmutableArray<NamespaceOrTypeSymbol>> map = this.GetNameToMembersMap();
                foreach (var kvp in map)
                {
                    ImmutableArray<NamespaceOrTypeSymbol> members = kvp.Value;

                    foreach (var symbol in members)
                    {
                        if (symbol.Kind != SymbolKind.NamedType)
                        {
                            throw new System.Exception($"Only named types are allowed in Templates, found {symbol.Kind}");
                        }
                    }

                    dictionary.Add(kvp.Key, members.As<NamedTypeSymbol>());
                }
                
                Interlocked.CompareExchange(ref _nameToTypeMembersMap, dictionary, null);
            }

            return _nameToTypeMembersMap;
        }

        private Dictionary<string, ImmutableArray<NamespaceOrTypeSymbol>> MakeNameToMembersMap(DiagnosticBag diagnostics)
        {
            var builder = new NameToSymbolMapBuilder(_declaration.Children.Length);
            foreach (var declaration in _declaration.Children)
            {
                MergedNamespaceOrTypeDeclaration mergedDeclaration = null;
                if (declaration is SingleTypeDeclaration typeDeclaration)
                {
                    mergedDeclaration = new MergedTypeDeclaration(ImmutableArray.Create((SingleTypeDeclaration)declaration));
                }

                if (mergedDeclaration != null)
                {
                    builder.Add(BuildSymbol(mergedDeclaration, diagnostics));    
                }
                else
                {
                    throw ExceptionUtilities.UnexpectedValue(declaration.Kind);
                }
            }

            return builder.CreateMap();
        }

        private NamespaceOrTypeSymbol BuildSymbol(MergedNamespaceOrTypeDeclaration declaration, DiagnosticBag diagnostics)
        {
            switch (declaration.Kind)
            {
                case DeclarationKind.Template:
                    return new TemplateSymbol(this, (MergedTemplateDeclaration)declaration);

                case DeclarationKind.Class:
                    return new SourceNamedTypeSymbol(this, (MergedTypeDeclaration)declaration, diagnostics);

                default:
                    throw ExceptionUtilities.UnexpectedValue(declaration.Kind);
            }
        }

        private struct NameToSymbolMapBuilder
        {
            private readonly Dictionary<string, object> _dictionary;

            public NameToSymbolMapBuilder(int capacity)
            {
                _dictionary = new Dictionary<string, object>(capacity, StringOrdinalComparer.Instance);
            }

            public void Add(NamespaceOrTypeSymbol symbol)
            {
                string name = symbol.Name;
                object item;
                if (_dictionary.TryGetValue(name, out item))
                {
                    var builder = item as ArrayBuilder<NamespaceOrTypeSymbol>;
                    if (builder == null)
                    {
                        builder = ArrayBuilder<NamespaceOrTypeSymbol>.GetInstance();
                        builder.Add((NamespaceOrTypeSymbol)item);
                        _dictionary[name] = builder;
                    }
                    builder.Add(symbol);
                }
                else
                {
                    _dictionary[name] = symbol;
                }
            }

            public Dictionary<string, ImmutableArray<NamespaceOrTypeSymbol>> CreateMap()
            {
                var result = new Dictionary<string, ImmutableArray<NamespaceOrTypeSymbol>>(_dictionary.Count, StringOrdinalComparer.Instance);

                foreach (var kvp in _dictionary)
                {
                    object value = kvp.Value;
                    ImmutableArray<NamespaceOrTypeSymbol> members;

                    var builder = value as ArrayBuilder<NamespaceOrTypeSymbol>;
                    if (builder != null)
                    {
                        Debug.Assert(builder.Count > 1);
                        bool hasTemplates = false;
                        for (int i = 0; (i < builder.Count) && !hasTemplates; i++)
                        {
                            hasTemplates |= (builder[i].Kind == SymbolKind.Template);
                        }

                        members = hasTemplates
                            ? builder.ToImmutable()
                            : StaticCast<NamespaceOrTypeSymbol>.From(builder.ToDowncastedImmutable<NamedTypeSymbol>());

                        builder.Free();
                    }
                    else
                    {
                        NamespaceOrTypeSymbol symbol = (NamespaceOrTypeSymbol)value;
                        
                        members = symbol.Kind == SymbolKind.Template
                            ? ImmutableArray.Create<NamespaceOrTypeSymbol>(symbol)
                            : StaticCast<NamespaceOrTypeSymbol>.From(ImmutableArray.Create<NamedTypeSymbol>((NamedTypeSymbol)symbol));
                    }

                    result.Add(kvp.Key, members);
                }

                return result;
            }
        }
    }
}
