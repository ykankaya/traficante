﻿namespace Traficante.TSQL.Parser.Tokens
{
    public class ExceptToken : SetOperatorToken
    {
        public ExceptToken(TextSpan span)
            : base(ExceptOperatorText, TokenType.Except, span)
        {
        }
    }
}