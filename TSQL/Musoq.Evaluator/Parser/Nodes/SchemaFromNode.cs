﻿namespace Traficante.TSQL.Parser.Nodes
{
    public class SchemaFunctionFromNode : FromNode
    {
        public SchemaFunctionFromNode(string schema, string method, ArgsListNode parameters, string alias)
            : base(alias)
        {
            Schema = schema;
            Method = method;
            MethodParameters = parameters;
            var paramsId = parameters.Id;
            Id = $"{nameof(SchemaFunctionFromNode)}{schema}{method}{paramsId}{Alias}";
        }

        public string Database { get; set; }

        public string Schema { get; }

        public string Method { get; }

        public ArgsListNode MethodParameters { get; }

        public override string Id { get; }
        
        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {

            return $"from {Schema}.{Method}({MethodParameters.Id}) {Alias}";
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is SchemaFunctionFromNode node)
                return node.Id == Id;

            return base.Equals(obj);
        }
    }

    public class SchemaTableFromNode : FromNode
    {
        public SchemaTableFromNode(string schema, string tableOrView, string alias)
            : base(alias)
        {
            Schema = schema;
            TableOrView = tableOrView;
            Id = $"{nameof(SchemaTableFromNode)}{schema}{tableOrView}{Alias}";
        }

        public string Schema { get; }

        public string TableOrView { get; }

        public override string Id { get; }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {

            return $"from {Schema}.{TableOrView} {Alias}";
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is SchemaTableFromNode node)
                return node.Id == Id;

            return base.Equals(obj);
        }
    }
}