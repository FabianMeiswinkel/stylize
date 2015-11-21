// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stylize.Engine;

namespace Stylize.EngineTests
{
    [TestClass]
    public class DirectedGraphTests
    {
        [TestMethod]
        public void DirectedGraphAllowsDuplicateEdgesTest()
        {
            var graph = new DirectedGraph<int>(new[] { 1, 2 });

            graph.AddEdge(1, 2);
            graph.AddEdge(1, 2);

            graph.FindCycle().Should().BeEmpty("Expected no cycle vertices to be found");
        }

        [TestMethod]
        public void DirectedGraphFindsCyclesTest()
        {
            var graph = new DirectedGraph<int>(new[] { 1, 2, 3, 4, 5 });

            // Add cycle
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 3);
            graph.AddEdge(3, 1);

            // Add extra edges
            graph.AddEdge(3, 4);
            graph.AddEdge(4, 5);

            graph.FindCycle().ShouldBeEquivalentTo(new[] { 1, 2, 3 }, "Expected cycle vertices to be found");
        }

        [TestMethod]
        public void DirectedGraphFindsNoCyclesInAcyclicGraphTest()
        {
            var graph = new DirectedGraph<int>(new[] { 1, 2, 3, 4, 5 });

            // Add non-cyclic edges
            graph.AddEdge(1, 2);
            graph.AddEdge(1, 5);
            graph.AddEdge(2, 3);
            graph.AddEdge(2, 4);
            graph.AddEdge(3, 4);
            graph.AddEdge(3, 5);
            graph.AddEdge(1, 5);

            graph.FindCycle().Should().BeEmpty("Expected no cycle vertices to be found");
        }

        [TestMethod]
        public void DirectedGraphSortsAcyclicVerticesTest()
        {
            var graph = new DirectedGraph<int>(new[] { 4, 2, 1, 5, 3 });

            // Add non-cyclic edges
            graph.AddEdge(1, 2);
            graph.AddEdge(1, 5);
            graph.AddEdge(2, 3);
            graph.AddEdge(2, 4);
            graph.AddEdge(3, 4);
            graph.AddEdge(3, 5);
            graph.AddEdge(4, 5);

            graph.Sort().Should().BeInAscendingOrder("Expected vertices to be sorted");
        }

        [TestMethod]
        public void DirectedGraphSortsThrowsForCyclicVerticesTest()
        {
            var graph = new DirectedGraph<int>(new[] { 4, 2, 1, 5, 3 });

            // Add cycle
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 3);
            graph.AddEdge(3, 1);

            // Add extra edges
            graph.AddEdge(3, 4);
            graph.AddEdge(4, 5);

            graph.Invoking(g => g.Sort()).ShouldThrow<InvalidOperationException>();
        }
    }
}
