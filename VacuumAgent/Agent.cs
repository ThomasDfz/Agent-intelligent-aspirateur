using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;

namespace VacuumAgent
{
    public class Agent
    {
        private int _x, _y; //Agent position
        private Environment _environment;
        private int _sensorX, _sensorY, _sensorPerf;

        private int _maxActionsBeforeNewObservation;
        
        /*BDI*/
        private Beliefs _beliefs;
        private Vertex _desire;
        private Stack<Effectors> _intentions = new Stack<Effectors>();


        public Agent(Environment environment)
        {
            _environment = environment;
            _x = 0;
            _y = 0;
            _environment.ExecuteAgentAction(_x, _y);
            _beliefs = new Beliefs(_environment.GetPerf(), environment.NbCaseX, environment.NbCaseY);
            _maxActionsBeforeNewObservation = _environment.NbCaseX + _environment.NbCaseY + 2; 
        }
        
        public int GetX() { return _x; }
        public int GetY() { return _y; }

        public void AsyncWork()
        {
            while (true)
            {
                //Thread.Sleep(18 * _environment.FactorSleep);
                ObserveEnvironment();
                UpdateState();
                PickAction();
                RealiseAction();
            }
        }

        /*Notice dirty rooms and rooms containing a lost Jewel*/
        private void ObserveEnvironment()
        {
            for (_sensorX = 0; _sensorX < _environment.NbCaseX; _sensorX++)
            {
                for (_sensorY = 0; _sensorY < _environment.NbCaseY; _sensorY++)
                {
                    if (_environment.Rooms[_sensorX, _sensorY].HasDirt())
                    {
                        _beliefs.AddNewDirtyRoom(_sensorX, _sensorY);
                    }
                    if (_environment.Rooms[_sensorX, _sensorY].HasJewel())
                    {
                        _beliefs.AddNewJewelyRoom(_sensorX, _sensorY);
                    }   
                }
            }
        }
        
        /*Update agent's beliefs about how the whole manor actually is*/
        private void UpdateState() 
        {
            _beliefs.UpdateBelievedRooms();
            
            _sensorPerf = _environment.GetPerf();
            if (_sensorPerf > _beliefs.GetCurrentPerformance() && _maxActionsBeforeNewObservation > 2)
            {
                _maxActionsBeforeNewObservation--;
            }
            else
            {
                _maxActionsBeforeNewObservation += 2;
            }
            _beliefs.SetCurrentPerformance(_sensorPerf);
            Console.BackgroundColor = ConsoleColor.Green;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine("Max actions : " + _maxActionsBeforeNewObservation);
            Console.ResetColor();
        }
        
        /*Pick a list of intended actions according to his beliefs and desires*/
        private void PickAction()
        {
            Coordinates nearestItemPosition = GetNearestBelievedItem();
            if (nearestItemPosition.x != -1 && nearestItemPosition.y != -1)
            {
                switch (_beliefs.GetBelievedRoomContent(nearestItemPosition.x, nearestItemPosition.y))
                {
                    case "dirt and jewel":
                        _intentions.Push(Effectors.Vacuum);
                        _intentions.Push(Effectors.PickUpJewel);
                        break;
                    case "jewel":
                        _intentions.Push(Effectors.PickUpJewel);
                        break;
                    case "dirt":
                        _intentions.Push(Effectors.Vacuum);
                        break;
                }
                if (nearestItemPosition.x == _x && nearestItemPosition.y == _y)
                {
                    //then we're already on the item
                }
                else
                {
                    Graph g = BuildGraphAccordingToEnvironment();
                    
                    //Randomly choose between the informed and uninformed algorithm.
                    Random randomlyChosenSearchAlgorithm = new Random();
                    Vertex localRoom = g.FindVertexByCoordinates(_x, _y);
                    _desire = g.FindVertexByCoordinates(nearestItemPosition.x, nearestItemPosition.y);
                    if (randomlyChosenSearchAlgorithm.Next(0, 2) == 0)
                    {
                        BreadthFirstSearch bfs = new BreadthFirstSearch(g);
                        if (bfs.ExploreAndSearch(localRoom.Id, _desire.Id))
                        {
                            Stack<int> pathIds = bfs.BuildShortestPath(localRoom.Id, _desire.Id);
                            UpdateIntentions(pathIds, g);
                            Console.BackgroundColor = ConsoleColor.Green;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.WriteLine("Path found with BFS");
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        AstarSearch astar = new AstarSearch(g);
                        if (astar.ExploreAndSearch(localRoom.Id, _desire.Id))
                        {
                            Stack<int> pathIds = astar.BuildShortestPath(localRoom.Id, _desire.Id);
                            UpdateIntentions(pathIds, g);
                            Console.BackgroundColor = ConsoleColor.Green;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.WriteLine("Path found with A*");
                            Console.ResetColor();
                        }
                    }    
                }
            }
        }
        
        //Realise agent intentions
        private void RealiseAction()
        {
            int actionsDone = 0;
            while (_intentions.Count != 0)
            {
                if (actionsDone == _maxActionsBeforeNewObservation)
                {
                    _intentions.Clear();
                    break;
                }
                Effectors intention = _intentions.Pop();
                switch (intention)
                {
                    case Effectors.MoveDown:
                        if(_y > 0) _y--;
                        break;
                    case Effectors.MoveLeft:
                        if(_x > 0) _x--;
                        break;
                    case Effectors.MoveRight:
                        if(_x < _environment.NbCaseX) _x++;
                        break;
                    case Effectors.MoveUp:
                        if(_y < _environment.NbCaseY) _y++;
                        break;
                    case Effectors.PickUpJewel:
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
                    case Effectors.Vacuum:
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
                actionsDone++;
                _environment.ExecuteAgentAction(_x, _y);
                int duration = EffectorDuration.getEffectorDuration(intention);
                Thread.Sleep(duration * _environment.FactorSleep);
            }
        }

        //Based on current beliefs, get the coordinates of the nearest jewel or dirt
        private Coordinates GetNearestBelievedItem()
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
            return new Coordinates(nearestX, nearestY);
        }
        
        //Get new intentions based on a path of rooms returned by a search algorithm.
        private void UpdateIntentions(Stack<int> pathIds, Graph g)
        {
            int actualX = _x;
            int actualY = _y;
            int nextX, nextY;
            foreach (var pathId in pathIds)
            {
                nextX = g.FindVertexById(pathId).GetX();
                nextY = g.FindVertexById(pathId).GetY();
                if (nextX == actualX && nextY == actualY + 1 && actualY < _environment.NbCaseY)
                {
                    _intentions.Push(Effectors.MoveUp);
                }
                else if (nextX == actualX && nextY == actualY - 1 && actualY > 0)
                {
                    _intentions.Push(Effectors.MoveDown);
                }
                else if (nextX == actualX + 1 && nextY == actualY && actualX < _environment.NbCaseX)
                {
                    _intentions.Push(Effectors.MoveRight);
                }
                else if (nextX == actualX - 1 && nextY == actualY && actualX > 0)
                {
                    _intentions.Push(Effectors.MoveLeft);
                }
                actualX = nextX;
                actualY = nextY;
            }
        }

        //Building a rectangular directed graph.
        public Graph BuildGraphAccordingToEnvironment()
        {
            Graph g = new Graph(_environment.NbCaseX, _environment.NbCaseY);
            var ids = 0;
            for (var i = 0; i < _environment.NbCaseX; i++)
            {
                for (var j = 0; j < _environment.NbCaseY; j++)
                {
                    Vertex v = new Vertex(i, j, ids);
                    g.AddVertex(v);
                    ids++;
                }
            }
            for (var i = 0; i < _environment.NbCaseX - 1; i++)
            {
                for (var j = 0; j < _environment.NbCaseY - 1; j++)
                {
                    //Edges are directed so must be both ways
                    g.AddEdge(g.FindVertexByCoordinates(i, j).Id, g.FindVertexByCoordinates(i+1, j).Id);
                    g.AddEdge(g.FindVertexByCoordinates(i+1, j).Id, g.FindVertexByCoordinates(i, j).Id);
                    g.AddEdge(g.FindVertexByCoordinates(i, j).Id, g.FindVertexByCoordinates(i, j+1).Id);
                    g.AddEdge(g.FindVertexByCoordinates(i, j+1).Id, g.FindVertexByCoordinates(i, j).Id);
                }
            }
            for (var i = 0; i < _environment.NbCaseX - 1; i++)
            {
                g.AddEdge(g.FindVertexByCoordinates(i, _environment.NbCaseY - 1).Id, 
                          g.FindVertexByCoordinates(i+1, _environment.NbCaseY - 1).Id);
                g.AddEdge(g.FindVertexByCoordinates(i+1, _environment.NbCaseY - 1).Id,
                          g.FindVertexByCoordinates(i, _environment.NbCaseY - 1).Id);
            }
            for (var j = 0; j < _environment.NbCaseY - 1; j++)
            {
                g.AddEdge(g.FindVertexByCoordinates(_environment.NbCaseX - 1, j).Id, 
                          g.FindVertexByCoordinates(_environment.NbCaseX - 1, j+1).Id);
                g.AddEdge(g.FindVertexByCoordinates(_environment.NbCaseX - 1, j+1).Id, 
                          g.FindVertexByCoordinates(_environment.NbCaseX - 1, j).Id);
            }
            return g;
        }    
    }
}