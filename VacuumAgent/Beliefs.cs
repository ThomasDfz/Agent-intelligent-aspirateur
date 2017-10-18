using System;
using System.Collections.Generic;

namespace VacuumAgent
{
    public class Beliefs
    {
        private Room[,] _believedRooms = new Room[10, 10];
        private List<Coordinates> _newDirtyRooms = new List<Coordinates>();
        private List<Coordinates> _newJewelyRooms = new List<Coordinates>();

        public Beliefs(int x = 10, int y = 10)
        {
            _believedRooms = new Room[x, y];
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    _believedRooms[i, j] = new Room();
                }
            }
        }

        public void AddNewDirtyRoom(int x, int y)
        {
            Coordinates newCoords = new Coordinates(x, y);
            _newDirtyRooms.Add(newCoords);
        }
        
        public void AddNewJewelyRoom(int x, int y)
        {
            Coordinates newCoords = new Coordinates(x, y);
            _newJewelyRooms.Add(newCoords);
        }

        public void UpdateBelievedRooms()
        {
            foreach (var coord in _newDirtyRooms)
            {
                _believedRooms[coord.x, coord.y].DirtGenerated();
            }
            foreach (var coord in _newJewelyRooms)
            {
                _believedRooms[coord.x, coord.y].JewelGenerated();
            }
            _newDirtyRooms.Clear();
            _newJewelyRooms.Clear();
        }

        public string GetBelievedRoomContent(int x, int y)
        {
            if (_believedRooms[x, y].HasJewel() && _believedRooms[x, y].HasDirt())
            {
                return "dirt and jewel";
            }
            if (_believedRooms[x, y].HasDirt())
            {
                return "dirt";
            }
            if (_believedRooms[x, y].HasJewel())
            {
                return "jewel";
            }
            return "nothing";
        }

        public void JewelSupposedlyPickedUp(int x, int y)
        {
            _believedRooms[x, y].RemoveJewel();
        }

        public void DirtSupposedlyVaccumed(int x, int y)
        {
            _believedRooms[x, y].Vacuum();
        }
    }

    
    public struct Coordinates
    {
        public int x;
        public int y;
        
        public Coordinates(int x, int y)
        {
            this.x = x;
            this.y = y;
        }  
    }
}