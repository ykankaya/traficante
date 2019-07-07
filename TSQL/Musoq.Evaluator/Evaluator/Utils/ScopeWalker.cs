﻿namespace Traficante.TSQL.Evaluator.Utils
{
    public class ScopeWalker
    {
        private readonly ScopeWalker _parent;
        private int _childIndex;


        public ScopeWalker(Scope scope)
        {
            Scope = scope;
        }

        private ScopeWalker(Scope scope, ScopeWalker parent)
            : this(scope)
        {
            _parent = parent;
        }

        public Scope Scope { get; }

        public ScopeWalker NextChild()
        {
            return new ScopeWalker(Scope.Child[_childIndex++], this);
        }

        public ScopeWalker Child()
        {
            return new ScopeWalker(Scope.Child[_childIndex], this);
        }

        public ScopeWalker PrevChild()
        {
            return new ScopeWalker(Scope.Child[_childIndex--], this);
        }

        public ScopeWalker Parent()
        {
            return _parent;
        }
    }
}