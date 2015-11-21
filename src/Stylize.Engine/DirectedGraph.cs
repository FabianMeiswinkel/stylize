// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Stylize.Engine
{
    public class DirectedGraph<T>
    {
        readonly IDictionary<T, Vertex> valueVertexMap;

        public DirectedGraph(IEnumerable<T> vertices)
        {
            this.valueVertexMap = vertices.ToDictionary(v => v, v => new Vertex(v));
        }

        public void AddEdge(T from, T to)
        {
            Vertex fromVertex;
            if (!this.valueVertexMap.TryGetValue(from, out fromVertex))
            {
                throw new ArgumentException("Cannot find value in specified vertices", nameof(from));
            }

            Vertex toVertex;
            if (!this.valueVertexMap.TryGetValue(to, out toVertex))
            {
                throw new ArgumentException("Cannot find value in specified vertices", nameof(to));
            }

            fromVertex.AddSuccessor(toVertex);
        }

        public IReadOnlyList<T> FindCycle()
        {
            try
            {
                if (this.valueVertexMap.Values.Any(v => !v.VisitInTopologicalOrder(delegate { })))
                {
                    return this.valueVertexMap.Where(v => v.Value.IsInCycle).Select(v => v.Key).ToList();
                }

                return new T[0];
            }
            finally
            {
                this.ResetMarks();
            }
        }

        void ResetMarks()
        {
            foreach (Vertex vertex in this.valueVertexMap.Values)
            {
                vertex.ResetMark();
            }
        }

        public IReadOnlyList<T> Sort()
        {
            try
            {
                var sortedVertices = new Stack<T>();
                if (this.valueVertexMap.Values.Any(v => !v.VisitInTopologicalOrder(val => sortedVertices.Push(val))))
                {
                    throw new InvalidOperationException("Cycle detected in directed graph");
                }

                return sortedVertices.ToList();
            }
            finally
            {
                this.ResetMarks();
            }
        }

        class Vertex
        {
            MarkType mark;
            readonly HashSet<Vertex> successors;
            readonly T value;

            public Vertex(T value)
            {
                this.mark = MarkType.None;
                this.successors = new HashSet<Vertex>();
                this.value = value;
            }

            public bool IsInCycle => this.mark == MarkType.Temporary;

            public void AddSuccessor(Vertex successor)
            {
                this.successors.Add(successor);
            }

            public void ResetMark()
            {
                this.mark = MarkType.None;
            }

            // Uses Tarjan's algorithm
            public bool VisitInTopologicalOrder(Action<T> visitAction)
            {
                if (this.mark != MarkType.None)
                {
                    // A temporary mark indicates that that we have a cycle and the recursive visit was not successful.
                    return this.mark == MarkType.Permanent;
                }

                this.mark = MarkType.Temporary;

                if (this.successors.Any(s => !s.VisitInTopologicalOrder(visitAction)))
                {
                    return false;
                }

                this.mark = MarkType.Permanent;
                visitAction(this.value);
                return true;
            }

            enum MarkType
            {
                None,
                Temporary,
                Permanent
            }
        }
    }
}
