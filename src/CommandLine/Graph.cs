using System;
using System.Collections.Generic;
using System.Linq;

namespace Scrap.CommandLine
{
    public static class GraphSearch
    {
        public static IEnumerable<T> DepthFirstSearch<T>(T v, Func<T, IEnumerable<T>> adj)
        {
            HashSet<T> visited = new(new EC<T>());
            Stack<T> stack = new();
            stack.Push(v);
            while (stack.Any())
            {
                var currentNode = stack.Pop();
                visited.Add(currentNode);
                yield return currentNode;

                foreach (var adjacentNode in adj(currentNode).Where(n => !visited.Contains(n)))
                {
                    stack.Push(adjacentNode);
                }
            }
        }
        public static IEnumerable<TExp> DepthFirstSearch<T, TExp>(T v, Func<T, TExp> expensive, Func<TExp, IEnumerable<T>> adj)
        {
            HashSet<T> visited = new(EqualityComparer<T>.Default);
            Stack<T> stack = new();
            stack.Push(v);
            while (stack.Any())
            {
                var currentNode = stack.Pop();
                if (visited.Contains(currentNode))
                {
                    continue;
                }

                var ex = expensive(currentNode);
                visited.Add(currentNode);
                yield return ex;

                var adjacentNodes = adj(ex).Where(n => !visited.Contains(n)).Reverse().ToList();
                foreach (var adjacentNode in adjacentNodes)
                {
                    stack.Push(adjacentNode);
                }
            }
        }

        public static IEnumerable<T> BreadthFirstSearch<T>(T s, Func<T, IEnumerable<T>> adj)
        {
            // Mark all the vertices as not
            // visited(By default set as false)
            var visited = new HashSet<T>();

            // Create a queue for BFS
            Queue<T> queue = new();
     
            // Mark the current node as
            // visited and enqueue it
            visited.Add(s);
            yield return s;
            queue.Enqueue(s);        
 
            while(queue.Any())
            {
         
                // Dequeue a vertex from queue
                // and print it
                s = queue.Dequeue();

                // Get all adjacent vertices of the
                // dequeued vertex s. If a adjacent
                // has not been visited, then mark it
                // visited and enqueue it
                var list = adj(s);
 
                foreach (var val in list.Where(val => !visited.Contains(val)))
                {
                    visited.Add(val);
                    yield return val;
                    queue.Enqueue(val);
                }
            }
        }

        private class EC<T> : IEqualityComparer<T>
        {
            public bool Equals(T? x, T? y)
            {
                return x?.Equals(y) ?? false;
            }

            public int GetHashCode(T obj)
            {
                return obj?.GetHashCode() ?? 0;
            }
        }
    }
}