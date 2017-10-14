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
        private Beliefs _beliefs;
        private Stack<Actions> _intentions = new Stack<Actions>();
        
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
                        Console.WriteLine("~~~ Picking up Jewel at " + _x + "," + _y + " ~~~");
                        _environment.JewelPickedUp(_x, _y);
                        _beliefs.JewelSupposedlyPickedUp(_x, _y);
                        break;
                    case Actions.Vacuum:
                        Console.WriteLine("~~~ Vacuuming dirt at " + _x + "," + _y + " ~~~");
                        _environment.DirtVaccumed(_x, _y);
                        _beliefs.DirtSupposedlyVaccumed(_x, _y);
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
                if (nearestItemPosition.Item1 == _x && nearestItemPosition.Item2 == _y)
                {
                    switch (_beliefs.GetBelievedRoomContent(_x, _y))
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
                }
                else
                {
                    if (nearestItemPosition.Item1 < _x)
                    {
                        for (int i = 0; i < _x - nearestItemPosition.Item1; i++)
                        {
                            _intentions.Push(Actions.MoveLeft);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < nearestItemPosition.Item1 - _x; i++)
                        {
                            _intentions.Push(Actions.MoveRight);
                        }
                    }
                    if (nearestItemPosition.Item2 < _y)
                    {
                        for (int i = 0; i < _y - nearestItemPosition.Item2; i++)
                        {
                            _intentions.Push(Actions.MoveDown);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < nearestItemPosition.Item2 - _y; i++)
                        {
                            _intentions.Push(Actions.MoveUp);
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
            for (int i = 0; i < _environment.NbCaseX; i++)
            {
                for (int j = 0; j < _environment.NbCaseY; j++)
                {
                    if (_environment.rooms[i, j].HasDirt())
                    {
                        _beliefs.AddNewDirtyRoom(i, j);
                    }
                    if (_environment.rooms[i, j].HasJewel())
                    {
                        _beliefs.AddNewJewelyRoom(i, j);
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