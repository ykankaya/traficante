﻿using System;

namespace Traficante.TSQL.Parser.Nodes
{
    public class GreaterOrEqualNode : BinaryNode
    {
        public GreaterOrEqualNode(Node left, Node right) : base(left, right)
        {
            Id = CalculateId(this);
        }

        public override string Id { get; }
        public override Type ReturnType => typeof(bool);

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"{Left.ToString()} >= {Right.ToString()}";
        }
    }
}