﻿using System;
using System.Globalization;
using System.Linq;
using System.Web.Razor.Generator;
using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Text;
using System.Web.Razor.Tokenizer.Symbols;

namespace FubuMVC.Razor.Core
{
    // from System.Web.Mvc
    public class FubuVBCodeParser : VBCodeParser
    {
        internal const string ModelTypeKeyword = "ModelType";
        private const string GenericTypeFormatString = "{0}(Of {1})";
        private SourceLocation? _endInheritsLocation;
        private bool _modelStatementFound;

        public FubuVBCodeParser()
        {
            MapDirective(ModelTypeKeyword, ModelTypeDirective);
        }

        protected override bool InheritsStatement()
        {
            // Verify we're on the right keyword and accept
            Assert(VBKeyword.Inherits);
            VBSymbol inherits = CurrentSymbol;
            NextToken();
            _endInheritsLocation = CurrentLocation;
            PutCurrentBack();
            PutBack(inherits);
            EnsureCurrent();

            bool result = base.InheritsStatement();
            CheckForInheritsAndModelStatements();
            return result;
        }

        private void CheckForInheritsAndModelStatements()
        {
            if (_modelStatementFound && _endInheritsLocation.HasValue)
            {
                Context.OnError(_endInheritsLocation.Value, String.Format(CultureInfo.CurrentCulture, "The 'inherits' keyword is not allowed when a '{0}' keyword is used.", ModelTypeKeyword));
            }
        }

        protected virtual bool ModelTypeDirective()
        {
            AssertDirective(ModelTypeKeyword);

            Span.CodeGenerator = SpanCodeGenerator.Null;
            Context.CurrentBlock.Type = BlockType.Directive;

            AcceptAndMoveNext();
            SourceLocation endModelLocation = CurrentLocation;

            if (At(VBSymbolType.WhiteSpace))
            {
                Span.EditHandler.AcceptedCharacters = AcceptedCharacters.None;
            }

            AcceptWhile(VBSymbolType.WhiteSpace);
            Output(SpanKind.MetaCode);

            if (_modelStatementFound)
            {
                Context.OnError(endModelLocation, String.Format(CultureInfo.CurrentCulture, "Only one '{0}' statement is allowed in a file.", ModelTypeKeyword));
            }
            _modelStatementFound = true;

            if (EndOfFile || At(VBSymbolType.WhiteSpace) || At(VBSymbolType.NewLine))
            {
                Context.OnError(endModelLocation, "The '{0}' keyword must be followed by a type name on the same line.", ModelTypeKeyword);
            }

            // Just accept to a newline
            AcceptUntil(VBSymbolType.NewLine);
            if (!Context.DesignTimeMode)
            {
                // We want the newline to be treated as code, but it causes issues at design-time.
                Optional(VBSymbolType.NewLine);
            }

            string baseType = String.Concat(Span.Symbols.Select(s => s.Content)).Trim();
            Span.CodeGenerator = new SetModelTypeCodeGenerator(baseType, GenericTypeFormatString);

            CheckForInheritsAndModelStatements();
            Output(SpanKind.Code);
            return false;
        }
    }
}