using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;


namespace VacuumAgent
{
    public class Environment
    {
        public int NbCaseX { get; }
        public int NbCaseY { get; }

        public int FactorSleep { get; set; } = 100;
        private int _chanceDirt = 10;
        private int _chanceJewel = 5;

        private int _perf = 1;
        private int _electricityCost = 1;
        private int _goodActionReward;
        
        public Room[,] Rooms;
        private GraphicalView _view;
        private int _agentXPosition, _agentYPosition;
        
        public Environment(GraphicalView view, int x = 10, int y = 10)
        {
            NbCaseX = x;
            NbCaseY = y;
            Rooms = new Room[x, y];
            _view = view;
            
            for (int i = 0; i < NbCaseX; i++)
            {
                for (int j = 0; j < NbCaseY; j++)
                {
                    Rooms[i, j] = new Room();
                }
            }
            
            //Reward depends on the average length traveled from one random room to another.
            _goodActionReward = (int) (Math.Floor(Math.Sqrt(NbCaseX*NbCaseX + NbCaseY*NbCaseY)) / 2) + 1;
            _view.FormClosing += EndGame;
        }

        public int GetPerf() { return _perf; }

        public void SetJewelryAndDirtGenerationPercentages(int chanceJewel, int chanceDirt)
        {
            _chanceDirt = chanceDirt;
            _chanceJewel = chanceJewel;       
            if (chanceDirt + chanceJewel > 100)
            {
                _chanceDirt = 60;
                _chanceJewel = 40;
            }
        }

        private static void EndGame(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(1);
        }
        
        public void AsyncTask()
        { 
            while (true)
            {
                Thread.Sleep(5 * FactorSleep);
                GenerateDirtOrJewel();
            }
        }

        public void ExecuteAgentAction(int x, int y)
        {
            _agentXPosition = x;
            _agentYPosition = y;
            _perf -= _electricityCost; //cost of any action
            _view.Refresh(Rooms, _agentXPosition, _agentYPosition);
        }
             
        public void GenerateDirtOrJewel()
        {
            Random rnd = new Random();
            int dirtOrJewel = rnd.Next(0, 100);
            if (dirtOrJewel <= _chanceJewel + _chanceDirt)
            {
                int x = rnd.Next(0, NbCaseX);
                int y = rnd.Next(0, NbCaseY);
                if (_chanceJewel < _chanceDirt && dirtOrJewel <= _chanceJewel)
                {
                    GenerateJewel(x, y);
                }
                else if (_chanceDirt < _chanceJewel && dirtOrJewel <= _chanceDirt)
                {
                    GenerateDirt(x, y);       
                }
                else if (_chanceJewel >= _chanceDirt)
                {
                    GenerateJewel(x, y);
                }
                else if(_chanceDirt >= _chanceJewel)
                {
                    GenerateDirt(x, y);
                }
            }
        }

        public void GenerateJewel(int x, int y)
        {
            if (!Rooms[x, y].HasJewel())
            {
                Rooms[x, y].JewelGenerated();
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Jewel generated at {x},{y}");
                Console.ResetColor();
                _view.Refresh(Rooms, _agentXPosition, _agentYPosition);
            }
        }

        public void GenerateDirt(int x, int y)
        {
            if (!Rooms[x, y].HasDirt())
            {
                Rooms[x, y].DirtGenerated();
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Dirt generated at {x},{y}");
                Console.ResetColor();
                _view.Refresh(Rooms, _agentXPosition, _agentYPosition);
            }
        }

        public void JewelPickedUp(int x, int y)
        {
            if (Rooms[x, y].HasJewel()) _perf += _goodActionReward;
            Rooms[x, y].RemoveJewel();
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("PERF : " + _perf);
            Console.ResetColor();
        }

        public void DirtVaccumed(int x, int y)
        {
            if (Rooms[x, y].HasJewel())  _perf -= 50; //what a mistake !
            if (Rooms[x, y].HasDirt()) _perf += _goodActionReward;
            Rooms[x, y].Vacuum();
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("PERF : " + _perf);
            Console.ResetColor();
        }
    }

    public class Room
    {
        private bool _hasDirt, _hasJewel;

        public Room()
        {
            _hasDirt = _hasJewel = false;
        }

        public void DirtGenerated()
        {
            _hasDirt = true;
        }
        
        public void JewelGenerated()
        {
            _hasJewel = true;
        }

        public bool HasDirt()
        {
            return _hasDirt;
        }

        public bool HasJewel()
        {
            return _hasJewel;
        }

        public void RemoveJewel()
        {
            _hasJewel = false;
        }

        public void Vacuum()
        {
            _hasJewel = false; //vaccuumed too
            _hasDirt = false;
        }
    }
}