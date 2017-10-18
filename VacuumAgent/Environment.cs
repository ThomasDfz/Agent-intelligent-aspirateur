using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;


namespace VacuumAgent
{
    public class Environment
    {
        private int _caseX = 10;
        private int _caseY = 10;
        public int NbCaseX
        {
            get { return _caseX; }
        }
        public int NbCaseY
        {
            get { return _caseY; }
        }

        public int FactorSleep { get; set; } = 100;
        public int ChanceDirt { get; set; } = 10;
        public int ChanceJewelry { get; set; } = 5;

        private int _perf = 0;

        public Room[,] rooms = new Room[10, 10];
        private GraphicalView _view;
        private int _agentXPosition, _agentYPosition;
        
        public Environment(GraphicalView view, int x = 10, int y = 10)
        {
            _caseX = x;
            _caseY = y;
            rooms = new Room[x, y];

            _view = view;
            for (int i = 0; i < NbCaseX; i++)
            {
                for (int j = 0; j < NbCaseY; j++)
                {
                    rooms[i, j] = new Room();
                }
            }
            _view.FormClosing += EndGame;
        }

        private void EndGame(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(1);
        }
        
        public void AsyncTask()
        {
            GenerateDirtOrJewel();
        }

        public void setAgentPosition(int x, int y)
        {
            _agentXPosition = x;
            _agentYPosition = y;
            _view.Refresh(rooms, _agentXPosition, _agentYPosition);
        }
        
        public void GenerateDirtOrJewel()
        {
            while (true)
            {
                Thread.Sleep(10 * FactorSleep);
                Random rnd = new Random();
                int dirtOrJewel = rnd.Next(0, 100);
                if (dirtOrJewel < ChanceDirt || dirtOrJewel < ChanceJewelry)
                {
                    int x = rnd.Next(0, NbCaseX);
                    int y = rnd.Next(0, NbCaseY);
                    if (dirtOrJewel < ChanceJewelry)
                    {
                        if (!rooms[x, y].HasJewel())
                        {
                            rooms[x, y].JewelGenerated();
                            Console.BackgroundColor = ConsoleColor.Blue;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine($"Jewel generated at {x},{y}");
                            Console.ResetColor();
                            _view.Refresh(rooms, _agentXPosition, _agentYPosition);
                        }        
                    }
                    else
                    {
                        if (!rooms[x, y].HasDirt())
                        {
                            rooms[x, y].DirtGenerated();
                            Console.BackgroundColor = ConsoleColor.Blue;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine($"Dirt generated at {x},{y}");
                            Console.ResetColor();
                            _view.Refresh(rooms, _agentXPosition, _agentYPosition);
                        }
                    }
                }
            }
        }

        public void JewelPickedUp(int x, int y)
        {
            if (rooms[x, y].HasJewel()) _perf += 2;
            else _perf -= 2; //pick up nothing
            rooms[x, y].RemoveJewel();
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("PERF : " + _perf);
            Console.ResetColor();
        }

        public void DirtVaccumed(int x, int y)
        {
            if (rooms[x, y].HasJewel())  _perf -= 10; //what a mistake !
            if (rooms[x, y].HasDirt()) _perf += 1;
            else _perf -= 2; //vacuum nothing
            rooms[x, y].RemoveDirt();
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("PERF : " + _perf);
            Console.ResetColor();
        }
    }

    public class Room
    {
        private bool _hasDirt, _hasJewel;

        public Room(){
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

        public void RemoveDirt()
        {
            _hasJewel = false; //vaccuumed too
            _hasDirt = false;
        }
    }
}