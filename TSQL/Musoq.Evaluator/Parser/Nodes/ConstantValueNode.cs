﻿namespace Traficante.TSQL.Parser.Nodes
{
    public abstract class ConstantValueNode : Node
    {
        public abstract object ObjValue { get; }
    }
}
