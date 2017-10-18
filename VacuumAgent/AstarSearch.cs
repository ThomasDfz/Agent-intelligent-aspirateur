using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

namespace VacuumAgent
{
    public class AstarSearch : IShortestPathAlgorithm
    {
        //For each node, from which node it can most efficiently be reached
        private Dictionary<int, int> _cameFrom = new Dictionary<int, int>();
        
        //Set of evaluated nodes
        private List<int> _closedSet = new List<int>();
            
        //Set of discovered nodes not yet evaluated
        private List<int> _openSet = new List<int>();

        //For each node, cose of getting from start to that node
        private Dictionary<int, double> _gScore = new Dictionary<int, double>();

        //For each node, total cost of getting from start to goal passing that node
        private Dictionary<int, double> _fScore= new Dictionary<int, double>();
        
        
        private Graph _g;

        public AstarSearch(Graph g)
        {
            _g = g;
        }
        
        public bool ExploreAndSearch(int root, int desire)
        {
            _openSet.Add(root);
            for (int i = 0; i < _g.GetVerticesNb(); i++)
            {
                _fScore[i] = Int32.MaxValue;
                _gScore[i] = Int32.MaxValue;
            }
            
            _gScore[root] = 0;
            _fScore[root] = HeuristicCostEstimation(root, desire);

            while (_openSet.Count != 0)
            {
                int current = GetLowestFValue();
                if (current == desire)
                {
                    return true;
                }
                _openSet.Remove(current);
                _closedSet.Add(current);

                IEnumerator<int> iterator = _g.GetAdjacencyList(current).GetEnumerator();
                while (iterator.MoveNext())
                {
                    int neighbor = iterator.Current;
                    if (_closedSet.Contains(neighbor))
                    {
                        continue;
                    }
                    if (!_openSet.Contains(neighbor))
                    {
                        _openSet.Add(neighbor);
                    }
                    double tentative_gScore = _gScore[current] + _g.GetMoveCost();
                    if (tentative_gScore >= _gScore[neighbor])
                    {
                        continue;
                    }
                    _cameFrom[neighbor] = current;
                    _gScore[neighbor] = tentative_gScore;
                    _fScore[neighbor] = _gScore[neighbor] + HeuristicCostEstimation(neighbor, desire);
                }
            }
            return false;
        }

        public Stack<int> BuildShortestPath(int root, int desire)
        {
            Stack<int> pathIds = new Stack<int>();
            pathIds.Push(desire);
            while (_cameFrom.ContainsKey(desire))
            {
                desire = _cameFrom[desire];
                pathIds.Push(desire);
            }
            return pathIds;
        }
        
        public double HeuristicCostEstimation(int start, int end)
        {
            int x1 = _g.FindVertexById(start).GetX();
            int y1 = _g.FindVertexById(start).GetY();
            int x2 = _g.FindVertexById(end).GetX();
            int y2 = _g.FindVertexById(end).GetY();
            return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }

        public int GetLowestFValue()
        {
            double min = Int32.MaxValue;
            int minNode = -1;
            foreach (var node in _openSet)
            {
                if (_fScore[node] < min)
                {
                    minNode = node;
                    min = _fScore[node];
                }
            }
            return minNode;
        }
    }
}