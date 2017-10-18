using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
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

        public int GetX() { return _data.x; }
        public int GetY() { return _data.y; }
    }
    
    public class Graph
    {
        private int _verticesNb;
        private int _nbCasesX;
        private int _nbCasesY;
        private List<List<int>> _adjacencyList = new List<List<int>>();
        private List<Vertex> _vertices = new List<Vertex>();
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
        }

        public int GetMoveCost() { return _moveCost; }
        public int GetVerticesNb() { return _verticesNb; }
        public List<int> GetAdjacencyList(int s) { return _adjacencyList[s]; }

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
    }
}