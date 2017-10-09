using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;


namespace VacuumAgent
{
    public class Environment
    {
        public Room[,] rooms = new Room[10, 10];
        
        public Environment()
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    rooms[i, j] = new Room();
                }
            }
            Console.WriteLine("Environnement créé");
        }

        public void AsyncTask()
        {
            GenerateDirtOrJewel();
        }
        
        public void GenerateDirtOrJewel()
        {
            while (true)
            {
                Thread.Sleep(1000);
                Random rnd = new Random();
                int dirtOrJewel = rnd.Next(0, 10);
                if (dirtOrJewel < 5)  //  1/2 chance to generate something
                {
                    int x = rnd.Next(0, 10);
                    int y = rnd.Next(0, 10);
                    if (dirtOrJewel < 2) // 40% chance of a jewel
                    {
                        rooms[x, y].JewelGenerated();
                        Console.WriteLine("Jewel generated at {0},{1}", x, y);
                    }
                    else  //60% chance of dirt
                    {
                        rooms[x, y].DirtGenerated();
                        Console.WriteLine("Dirt generated at {0},{1}", x, y);
                    }
                }
            }
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

        public bool HasDirt => _hasDirt;

        public bool HasJewel => _hasJewel;
    }
}