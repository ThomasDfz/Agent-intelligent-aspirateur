using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

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
                    //ShortestPath(_predecessors, root, desire);
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

        public void ShortestPath(Stack<Actions> intentions, int root, int desire)
        {
            Stack<int> PathIds = new Stack<int>();
            PathIds.Push(desire);
            while (desire != root)
            {
                PathIds.Push(_predecessors[desire]);
                desire = _predecessors[desire];
            }
            PathIds.Push(_predecessors[desire]);
            int actualX = FindVertexById(root).GetX();
            int actualY = FindVertexById(root).GetY();
            int nextX, nextY;
            foreach (var pathId in PathIds)
            {
                nextX = FindVertexById(pathId).GetX();
                nextY = FindVertexById(pathId).GetY();
                if (nextX == actualX && nextY == actualY + 1 && actualY < _nbCasesY)
                {
                    intentions.Push(Actions.MoveUp);
                }
                else if (nextX == actualX && nextY == actualY - 1 && actualY > 0)
                {
                    intentions.Push(Actions.MoveDown);
                }
                else if (nextX == actualX + 1 && nextY == actualY && actualX < _nbCasesX)
                {
                    intentions.Push(Actions.MoveRight);
                }
                else if (nextX == actualX - 1 && nextY == actualY && actualX > 0)
                {
                    intentions.Push(Actions.MoveLeft);
                }
                actualX = nextX;
                actualY = nextY;
            }
        }
    }
}