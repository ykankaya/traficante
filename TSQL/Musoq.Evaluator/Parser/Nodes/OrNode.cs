﻿using System;

namespace Traficante.TSQL.Parser.Nodes
{
    public class OrNode : BinaryNode
    {
        public OrNode(Node left, Node right)
            : base(left, right)
        {
            Id = CalculateId(this);
        }

        public override Type ReturnType => typeof(bool);

        public override string Id { get; }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"{Left.ToString()} or {Right.ToString()}";
        }
    }
}