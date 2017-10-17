using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;

namespace VacuumAgent
{
    public class Agent
    {
        private int _x, _y;
        private Environment _environment;
        
        /*BDI*/
        private Beliefs _beliefs;
        private Vertex _desire;
        private Stack<Actions> _intentions = new Stack<Actions>();
        
        private int _sensor_x, _sensor_y;
        
        public Agent(Environment environment)
        {
            _environment = environment;
            _x = 0;
            _y = 0;
            _environment.setAgentPosition(_x, _y);
            _beliefs = new Beliefs(environment.NbCaseX, environment.NbCaseY);
        }

        public void AsyncWork()
        {
            while (true)
            {
                Thread.Sleep(20 * _environment.FactorSleep);
                ObserveEnvironment();
                UpdateState();
                PickAction();
                RealiseAction();
            }
        }

        public void RealiseAction()
        {
            while (_intentions.Count != 0)
            {
                switch (_intentions.Pop())
                {
                    case Actions.MoveDown:
                        if(_y > 0) _y--;
                        break;
                    case Actions.MoveLeft:
                        if(_x > 0) _x--;
                        break;
                    case Actions.MoveRight:
                        if(_x < _environment.NbCaseX) _x++;
                        break;
                    case Actions.MoveUp:
                        if(_y < _environment.NbCaseY) _y++;
                        break;
                    case Actions.PickUpJewel:
                        if (_beliefs.GetBelievedRoomContent(_x, _y) == "jewel" ||
                            _beliefs.GetBelievedRoomContent(_x, _y) == "dirt and jewel")
                        {
                            Console.BackgroundColor = ConsoleColor.Green;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.WriteLine("~~~ Picking up Jewel at " + _x + "," + _y + " ~~~");
                            Console.ResetColor();
                            _environment.JewelPickedUp(_x, _y);
                            _beliefs.JewelSupposedlyPickedUp(_x, _y);
                        }
                        else
                        {
                            _intentions.Clear();
                        } 
                        break;
                    case Actions.Vacuum:
                        if (_beliefs.GetBelievedRoomContent(_x, _y) == "dirt")
                        {
                            Console.BackgroundColor = ConsoleColor.Green;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.WriteLine("~~~ Vacuuming dirt at " + _x + "," + _y + " ~~~");
                            Console.ResetColor();
                            _environment.DirtVaccumed(_x, _y);
                            _beliefs.DirtSupposedlyVaccumed(_x, _y);
                        }
                        else
                        {
                            _intentions.Clear();
                        }
                        break;
                }
                _environment.setAgentPosition(_x, _y);
                Thread.Sleep(_environment.FactorSleep);
            }
        }
        
        public void PickAction()
        {
            Tuple<int, int> nearestItemPosition = GetNearestBelievedItem();
            if (nearestItemPosition.Item1 != -1 && nearestItemPosition.Item2 != -1)
            {
                switch (_beliefs.GetBelievedRoomContent(nearestItemPosition.Item1, nearestItemPosition.Item2))
                {
                    case "jewel and dirt":
                        _intentions.Push(Actions.Vacuum);
                        _intentions.Push(Actions.PickUpJewel);
                        break;
                    case "jewel":
                        _intentions.Push(Actions.PickUpJewel);
                        break;
                    case "dirt":
                        _intentions.Push(Actions.Vacuum);
                        break;
                }
                if (nearestItemPosition.Item1 == _x && nearestItemPosition.Item2 == _y)
                {
                    //then we're already on the item
                }
                else
                {
                    Graph g = new Graph(_environment.NbCaseX, _environment.NbCaseY);
                    int ids = 0;
                    for (int i = 0; i < _environment.NbCaseX; i++)
                    {
                        for (int j = 0; j < _environment.NbCaseY; j++)
                        {
                            Vertex v = new Vertex(i, j, ids);
                            g.AddVertex(v);
                            ids++;
                        }
                    }
                    for (int i = 0; i < _environment.NbCaseX - 1; i++)
                    {
                        for (int j = 0; j < _environment.NbCaseY - 1; j++)
                        {
                            g.AddEdge(g.FindVertexByCoordinates(i, j).Id, g.FindVertexByCoordinates(i+1, j).Id);
                            g.AddEdge(g.FindVertexByCoordinates(i+1, j).Id, g.FindVertexByCoordinates(i, j).Id);
                            g.AddEdge(g.FindVertexByCoordinates(i, j).Id, g.FindVertexByCoordinates(i, j+1).Id);
                            g.AddEdge(g.FindVertexByCoordinates(i, j+1).Id, g.FindVertexByCoordinates(i, j).Id);
                        }
                    }
                    for (int i = 0; i < _environment.NbCaseX - 1; i++)
                    {
                        g.AddEdge(g.FindVertexByCoordinates(i, _environment.NbCaseY - 1).Id, g.FindVertexByCoordinates(i+1, _environment.NbCaseY - 1).Id);
                        g.AddEdge(g.FindVertexByCoordinates(i+1, _environment.NbCaseY - 1).Id, g.FindVertexByCoordinates(i, _environment.NbCaseY - 1).Id);
                    }
                    for (int j = 0; j < _environment.NbCaseY - 1; j++)
                    {
                        g.AddEdge(g.FindVertexByCoordinates(_environment.NbCaseX - 1, j).Id, g.FindVertexByCoordinates(_environment.NbCaseX - 1, j+1).Id);
                        g.AddEdge(g.FindVertexByCoordinates(_environment.NbCaseX - 1, j+1).Id, g.FindVertexByCoordinates(_environment.NbCaseX - 1, j).Id);
                    }
                    
                    //TODO : refacto ci dessous
                    Random randomlyChooseSearchAlgorithm = new Random();
                    if (randomlyChooseSearchAlgorithm.Next(0, 2) == 0)
                    {
                        if (g.BreadthFirstSearch(g.FindVertexByCoordinates(_x, _y).Id,
                            g.FindVertexByCoordinates(nearestItemPosition.Item1, nearestItemPosition.Item2).Id))
                        {
                            _desire = g.FindVertexByCoordinates(nearestItemPosition.Item1, nearestItemPosition.Item2);
                            g.ShortestPath(_intentions, g.FindVertexByCoordinates(_x, _y).Id, _desire.Id);
                            Console.WriteLine("Path found with BFS");
                        }
                    }
                    else
                    {
                        if (g.AstarSearch(g.FindVertexByCoordinates(_x, _y).Id,
                            g.FindVertexByCoordinates(nearestItemPosition.Item1, nearestItemPosition.Item2).Id))
                        {
                            _desire = g.FindVertexByCoordinates(nearestItemPosition.Item1, nearestItemPosition.Item2);
                            g.ReconstructPath(_intentions, g.FindVertexByCoordinates(_x, _y).Id, _desire.Id);
                            Console.WriteLine("Path found with A*");
                        }
                    }    
                }
            }
        }

        public Tuple<int, int> GetNearestBelievedItem()
        {
            int nearestX = -1;
            int nearestY = -1;
            int nearestItemDistance = Int32.MaxValue;
            for (int i = 0; i < _environment.NbCaseX; i++)
            {
                for (int j = 0; j < _environment.NbCaseY; j++)
                {
                    if (_beliefs.GetBelievedRoomContent(i, j) != "nothing")
                    {
                        if (nearestItemDistance > (Math.Abs(_x - i) + Math.Abs(_y - j)))
                        {
                            nearestItemDistance = Math.Abs(_x - i) + Math.Abs(_y - j);
                            nearestX = i;
                            nearestY = j;
                        }
                    }
                }
            }
            return Tuple.Create(nearestX, nearestY);
        }

        public void ObserveEnvironment()
        {
            for (_sensor_x = 0; _sensor_x < _environment.NbCaseX; _sensor_x++)
            {
                for (_sensor_y = 0; _sensor_y < _environment.NbCaseY; _sensor_y++)
                {
                    if (_environment.rooms[_sensor_x, _sensor_y].HasDirt())
                    {
                        _beliefs.AddNewDirtyRoom(_sensor_x, _sensor_y);
                    }
                    if (_environment.rooms[_sensor_x, _sensor_y].HasJewel())
                    {
                        _beliefs.AddNewJewelyRoom(_sensor_x, _sensor_y);
                    }   
                }
            }
        }

        private void UpdateState()
        {
            _beliefs.UpdateBelievedRooms();
        }

        public int GetX()
        {
            return _x;
        }

        public int GetY()
        {
            return _y;
        }
    }
}