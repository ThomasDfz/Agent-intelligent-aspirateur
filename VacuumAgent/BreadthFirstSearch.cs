using System;
using System.Collections.Generic;

namespace VacuumAgent
{
    public class BreadthFirstSearch : IShortestPathAlgorithm
    {
        private Graph _g;
        private int[] _predecessors;

        public BreadthFirstSearch(Graph g)
        {
            _g = g;
            _predecessors = new int[g.GetVerticesNb()];
        }
        
        public bool ExploreAndSearch(int root, int desire)
        {
            bool[] visited = new bool[_g.GetVerticesNb()];
            Queue<int> queue = new Queue<int>();
            int s = root;
            visited[s] = true;
            queue.Enqueue(s);
            while (queue.Count != 0)
            {
                s = queue.Dequeue();
                if (s == desire)
                {
                    return true;
                }
                IEnumerator<int> i = _g.GetAdjacencyList(s).GetEnumerator();
                while (i.MoveNext())
                {
                    int n = i.Current;
                    if (!visited[n])
                    {
                        visited[n] = true;
                        _predecessors[n] = s;
                        queue.Enqueue(n);
                    }
                }
            }
            return false;
        }

        public Stack<int> BuildShortestPath(int root, int desire)
        {
            Stack<int> pathIds = new Stack<int>();
            pathIds.Push(desire);
            while (desire != root)
            {
                pathIds.Push(_predecessors[desire]);
                desire = _predecessors[desire];
            }
            pathIds.Push(_predecessors[desire]);
            return pathIds;
        }
        
    }
}