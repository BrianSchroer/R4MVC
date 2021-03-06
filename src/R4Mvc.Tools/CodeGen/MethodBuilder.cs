﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using R4Mvc.Tools.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace R4Mvc.Tools.CodeGen
{
    public class MethodBuilder
    {
        protected BaseMethodDeclarationSyntax _method;
        private SyntaxKind[] _modifiers = null;
        private IList<ParameterSyntax> _parameters = new List<ParameterSyntax>();
        private BlockSyntax _bodyBlock;
        private bool _useGeneratedAttributes = false, _useNonActionAttribute = false, _noBody = false;

        protected MethodBuilder() { }
        public MethodBuilder(string name, string returnType = null)
        {
            TypeSyntax returnTypeValue = returnType != null
                ? (TypeSyntax)IdentifierName(returnType)
                : PredefinedType(Token(SyntaxKind.VoidKeyword));
            _method = MethodDeclaration(returnTypeValue, Identifier(name));
        }

        public MethodBuilder WithModifiers(params SyntaxKind[] modifiers)
        {
            _modifiers = modifiers;
            return this;
        }

        public MethodBuilder WithGeneratedNonUserCodeAttributes()
        {
            _useGeneratedAttributes = true;
            return this;
        }

        public MethodBuilder WithNonActionAttribute()
        {
            _useNonActionAttribute = true;
            return this;
        }

        public MethodBuilder WithStringParameter(string name, bool defaultsToNull = false)
        {
            var parameter = Parameter(Identifier(name)).WithType(PredefinedType(Token(SyntaxKind.StringKeyword)));
            if (defaultsToNull)
                parameter = parameter.WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression)));
            _parameters.Add(parameter);
            return this;
        }

        public MethodBuilder WithParameter(string name, string type)
        {
            var parameter = Parameter(Identifier(name)).WithType(ParseTypeName(type));
            _parameters.Add(parameter);
            return this;
        }

        public MethodBuilder WithBody(Action<BodyBuilder> bodyParts)
        {
            var bodyBuilder = new BodyBuilder();
            bodyParts(bodyBuilder);
            _bodyBlock = bodyBuilder.Build();
            return this;
        }

        public MethodBuilder WithNoBody()
        {
            _noBody = true;
            return this;
        }

        public MethodBuilder ForEach<TEntity>(IEnumerable<TEntity> items, Action<MethodBuilder, TEntity> action)
        {
            if (items != null)
                foreach (var item in items)
                    action(this, item);
            return this;
        }

        public virtual MemberDeclarationSyntax Build()
        {
            switch (_method)
            {
                case MethodDeclarationSyntax method:
                    if (_modifiers != null)
                        method = method.WithModifiers(_modifiers);
                    if (_parameters.Count > 0)
                        method = method.AddParameterListParameters(_parameters.ToArray());
                    if (_useNonActionAttribute)
                        method = SyntaxNodeHelpers.WithNonActionAttribute(method);
                    if (_useGeneratedAttributes)
                        method = method.AddAttributeLists(SyntaxNodeHelpers.GeneratedNonUserCodeAttributeList());

                    if (_bodyBlock != null || !_noBody)
                        method = method.WithBody(_bodyBlock ?? Block());
                    else
                        method = method.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
                    return method;

                case ConstructorDeclarationSyntax constructor:
                    if (_modifiers != null)
                        constructor = constructor.WithModifiers(_modifiers);
                    if (_parameters.Count > 0)
                        constructor = constructor.AddParameterListParameters(_parameters.ToArray());
                    if (_useGeneratedAttributes)
                        constructor = constructor.AddAttributeLists(SyntaxNodeHelpers.GeneratedNonUserCodeAttributeList());

                    constructor = constructor.WithBody(_bodyBlock ?? Block());
                    return constructor;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
