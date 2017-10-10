using System;
using System.Collections.Generic;
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
        
        public Agent(Environment environment)
        {
            _environment = environment;
            _x = 0;
            _y = 0;
            _environment.setAgentPosition(_x, _y);
            _beliefs = new Beliefs(environment.NbCaseX, environment.NbCaseY);
            Console.WriteLine("Agent créé");
        }

        public void AsyncWork()
        {
            while (true)
            {
                Thread.Sleep(30 * _environment.FactorSleep);
                ObserveEnvironment();
                UpdateState();
                //Pick action
                //Realise action
                Random rnd = new Random();
                _x += rnd.Next(2);
                _y += rnd.Next(2);
                _environment.executeAgentAction(_x, _y);
                _environment.setAgentPosition(_x, _y);
            }
        }

        public void ObserveEnvironment()
        {
            for (int i = 0; i < _environment.NbCaseX; i++)
            {
                for (int j = 0; j < _environment.NbCaseY; j++)
                {
                    if (_environment.rooms[i, j].HasDirt())
                    {
                        Console.WriteLine("Ajout dirt");
                        _beliefs.AddNewDirtyRoom(i, j);
                    }
                    if (_environment.rooms[i, j].HasJewel())
                    {
                        Console.WriteLine("Ajout jewel");
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