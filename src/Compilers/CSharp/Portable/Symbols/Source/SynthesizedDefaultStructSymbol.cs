﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp.Symbols
{
    /// <summary>
    /// A struct containing default implementations of concept methods.
    /// This is boiled down into a normal struct in the metadata.
    /// </summary>
    internal sealed class SynthesizedDefaultStructSymbol : SynthesizedContainer
    {
        /// <summary>
        /// The concept in which this default struct is located.
        /// </summary>
        private readonly SourceNamedTypeSymbol _concept;

        /// <summary>
        /// Syntax references for the default bodies to synthesise here.
        /// </summary>
        private readonly ImmutableArray<SyntaxReference> _methodSyntax;

        /// <summary>
        /// Constructs a new SynthesizedDefaultStructSymbol.
        /// </summary>
        /// <param name="name">
        /// The name of the default struct.
        /// </param>
        /// <param name="concept">
        /// The parent concept of the default struct.
        /// </param>
        public SynthesizedDefaultStructSymbol(string name, SourceNamedTypeSymbol concept)
            : base(
                  name,
                  ImmutableArray.Create(
                      (TypeParameterSymbol) new SynthesizedWitnessParameterSymbol(
                          // @t-mawind
                          //   need to make this not clash with any typar in
                          //   the parent scopes, hence generated name.
                          GeneratedNames.MakeAnonymousTypeParameterName("witness"),
                          Location.None,
                          0,
                          null, // @t-mawind cyclic dependency!  Fixed below for now.
                          _ => ImmutableArray.Create((TypeSymbol) concept),
                          _ => TypeParameterConstraintKind.ValueType
                      )
                  ),
                  TypeMap.Empty
              )
        {

            _concept = concept;
            // We can't get the default members here: it'll cause an infinite
            // loop...

            // @t-mawind cyclic dependency: as above: this is horrible.
            (TypeParameters[0] as SynthesizedWitnessParameterSymbol).SetOwner(this);
        }

        public override Symbol ContainingSymbol => _concept;

        // @t-mawind
        //   A default struct is, of course, a struct...
        public override TypeKind TypeKind => TypeKind.Struct;

        // @t-mawind
        //   ...and, as it has no fields, its layout is specified as the
        //   minimum allowed by the CLI spec (1).
        //   This override is necessary, as otherwise the generated PE is
        //   invalid.
        internal sealed override TypeLayout Layout =>
            new TypeLayout(LayoutKind.Sequential, 1, alignment: 0);

        private ImmutableArray<Symbol> _members;

        public override ImmutableArray<Symbol> GetMembers()
        {
            // @t-mawind
            //   Not making this lazy results in new symbols being created every
            //   time we call GetMembers(), which is not only inefficient but
            //   breaks reference equality.
            if (_members.IsDefault)
            {
                var mb = ArrayBuilder<Symbol>.GetInstance();
                mb.AddRange(base.GetMembers());

                // @t-mawind
                //   This is slightly wrong, but we don't have any syntax to
                //   cling onto apart from this...
                var binder = DeclaringCompilation.GetBinder(ContainingType.GetNonNullSyntaxNode());
                var diagnostics = DiagnosticBag.GetInstance();

                var memberSyntax = _concept.GetConceptDefaultMethods();
                foreach (var m in memberSyntax)
                {
                    var ms = m.GetSyntax() as MethodDeclarationSyntax;
                    if (ms == null) continue;

                    mb.Add(SourceMemberMethodSymbol.CreateMethodSymbol(this, binder, ms, diagnostics));
                }

                AddDeclarationDiagnostics(diagnostics);

                ImmutableInterlocked.InterlockedInitialize(ref _members, mb.ToImmutableAndFree());
            }
            return _members;
        }
    }
}