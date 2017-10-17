using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms.VisualStyles;

namespace VacuumAgent
{

    public class Vertex
    {
        public int Id;
        private Coordinates _data;

        public Vertex(int x, int y, int id)
        {
            Id = id;
            _data = new Coordinates(x, y);
        }

        public Coordinates GetCoordinates()
        {
            return _data;
        }

        public int GetX()
        {
            return _data.x;
        }

        public int GetY()
        {
            return _data.y;
        }
    }
    
    public class Graph
    {
        private int _verticesNb;
        private int _nbCasesX;
        private int _nbCasesY;
        private List<List<int>> _adjacencyList = new List<List<int>>();
        private List<Vertex> _vertices = new List<Vertex>();
        private int[] _predecessors;
        private Dictionary<int, int> _cameFrom = new Dictionary<int, int>();
        private const int _moveCost = 1;

        public Graph(int nbCasesX, int nbCasesY)
        {
            _nbCasesX = nbCasesX;
            _nbCasesY = nbCasesY;
            _verticesNb = nbCasesX * nbCasesY;
            for (int i = 0; i < _verticesNb; i++)
            {
                List<int> sublist = new List<int>();
                _adjacencyList.Add(sublist);
            }
            _predecessors = new int[_verticesNb];
        }

        public void AddVertex(Vertex vertex)
        {
            _vertices.Add(vertex);
        }

        public Vertex FindVertexById(int id)
        {
            return _vertices.Find(vertex => vertex.Id == id);
        }

        public Vertex FindVertexByCoordinates(int x, int y)
        {
            return _vertices.Find(vertex => vertex.GetX() == x && vertex.GetY() == y);
        }

        public void AddEdge(int vertice, int w)
        {
            _adjacencyList[vertice].Add(w);
        }
        
        public bool BreadthFirstSearch(int root, int desire)
        {
            bool[] visited = new bool[_verticesNb];
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
                IEnumerator<int> i = _adjacencyList[s].GetEnumerator();
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

        public void ShortestPath(Stack<Effectors> intentions, int root, int desire)
        {
            Stack<int> PathIds = new Stack<int>();
            PathIds.Push(desire);
            while (desire != root)
            {
                PathIds.Push(_predecessors[desire]);
                desire = _predecessors[desire];
            }
            PathIds.Push(_predecessors[desire]);
            BuildIntentions(root, intentions, PathIds);
        }

        public void BuildIntentions(int root, Stack<Effectors> intentions, Stack<int> PathIds)
        {
            int actualX = FindVertexById(root).GetX();
            int actualY = FindVertexById(root).GetY();
            int nextX, nextY;
            foreach (var pathId in PathIds)
            {
                nextX = FindVertexById(pathId).GetX();
                nextY = FindVertexById(pathId).GetY();
                if (nextX == actualX && nextY == actualY + 1 && actualY < _nbCasesY)
                {
                    intentions.Push(Effectors.MoveUp);
                }
                else if (nextX == actualX && nextY == actualY - 1 && actualY > 0)
                {
                    intentions.Push(Effectors.MoveDown);
                }
                else if (nextX == actualX + 1 && nextY == actualY && actualX < _nbCasesX)
                {
                    intentions.Push(Effectors.MoveRight);
                }
                else if (nextX == actualX - 1 && nextY == actualY && actualX > 0)
                {
                    intentions.Push(Effectors.MoveLeft);
                }
                actualX = nextX;
                actualY = nextY;
            }
        }
        
        public bool AstarSearch(int root, int desire)
        {
            //Set of evaluated nodes
            List<int> closedSet = new List<int>();
            
            //Set of discovered nodes not yet evaluated
            List<int> openSet = new List<int>();

            //For each node, cose of getting from start to that node
            Dictionary<int, double> gScore = new Dictionary<int, double>();

            //For each node, total cost of getting from start to goal passing that node
            Dictionary<int, double> fScore= new Dictionary<int, double>();

            openSet.Add(root);
            for (int i = 0; i < _verticesNb; i++)
            {
                fScore[i] = Int32.MaxValue;
                gScore[i] = Int32.MaxValue;
            }
            
            gScore[root] = 0;
            fScore[root] = HeuristicCostEstimation(root, desire);

            while (openSet.Count != 0)
            {
                int current = GetLowestFValue(openSet, fScore);
                if (current == desire)
                {
                    //ReconstructPath();
                    return true;
                }
                openSet.Remove(current);
                closedSet.Add(current);

                IEnumerator<int> iterator = _adjacencyList[current].GetEnumerator();
                while (iterator.MoveNext())
                {
                    int neighbor = iterator.Current;
                    if (closedSet.Contains(neighbor))
                    {
                        continue;
                    }
                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                    double tentative_gScore = gScore[current] + _moveCost;
                    if (tentative_gScore >= gScore[neighbor])
                    {
                        continue;
                    }
                    _cameFrom[neighbor] = current;
                    gScore[neighbor] = tentative_gScore;
                    fScore[neighbor] = gScore[neighbor] + HeuristicCostEstimation(neighbor, desire);
                }
            }
            return false;
        }

        public void ReconstructPath(Stack<Effectors> intentions, int root, int desire)
        {
            Stack<int> total_path = new Stack<int>();
            total_path.Push(desire);
            while (_cameFrom.ContainsKey(desire))
            {
                desire = _cameFrom[desire];
                total_path.Push(desire);
            }
            BuildIntentions(root, intentions, total_path);
        }
        
        public double HeuristicCostEstimation(int start, int end)
        {
            int x1 = FindVertexById(start).GetX();
            int y1 = FindVertexById(start).GetY();
            int x2 = FindVertexById(end).GetX();
            int y2 = FindVertexById(end).GetY();
            return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }

        public int GetLowestFValue(List<int> openSet, Dictionary<int, double> fScore)
        {
            double min = Int32.MaxValue;
            int minNode = -1;
            foreach (var node in openSet)
            {
                if (fScore[node] < min)
                {
                    minNode = node;
                    min = fScore[node];
                }
            }
            return minNode;
        }
    }
}