using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Drawing;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace VacuumAgent
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);          
            Thread agentThread, environmentThread;

            int x = 15;
            int y = 20;
            GraphicalView view = new GraphicalView(x, y);
            Environment environment = new Environment(view, x, y);
            environment.ChanceDirt = 10;
            environment.ChanceJewelry = 5;
            environment.FactorSleep = 1;

            Agent agent = new Agent(environment);
            
            agentThread = new Thread(agent.AsyncWork);
            environmentThread = new Thread(environment.AsyncTask);
            agentThread.Start();
            environmentThread.Start();
            
            Application.Run(view);
        }
    }

    public class GraphicalView : Form
    {
        Panel[,] _roomPanels;
        
        public GraphicalView(int x, int y)
        {
            Size = new Size(40 * (y) + 16 , 40 * (x+1));
            print(x, y);
        }

        public void print(int x = 10, int y = 10)
        {      
            const int tileSize = 40;
            
            var color1 = Color.DarkGray;
            var color2 = Color.White;

            _roomPanels = new Panel[x, y];

            for (var n = 0; n < x; n++)
            {
                for (var m = 0; m < y; m++)
                {
                    var newPanel = new Panel
                    {
                        Size = new Size(tileSize, tileSize),
                        Location = new Point(tileSize * m, tileSize * n)
                    };

                    Controls.Add(newPanel);
                    
                    _roomPanels[n, m] = newPanel;

                    if ((n + m) % 2 == 0)
                        newPanel.BackColor = color1;
                    else
                        newPanel.BackColor = color2;
                }
            }
        }

        /*public void AddDirt(int n, int m, bool hasJewel)
        {
            if (hasJewel)
                _roomPanels[n, m].BackgroundImage = Image.FromFile("../../Assets/dirtjewel.png");
            else
                _roomPanels[n, m].BackgroundImage = Image.FromFile("../../Assets/dirt.png");
        }

        public void RemoveDirt(int n, int m, bool hasJewel)
        {
            if(hasJewel)
                _roomPanels[n, m].BackgroundImage = Image.FromFile("../../Assets/jewel.png");
            else
                _roomPanels[n, m].BackgroundImage = null;
        }
        
        public void AddJewel(int n, int m, bool hasDirt)
        {
            if(hasDirt)
                _roomPanels[n, m].BackgroundImage = Image.FromFile("../../Assets/dirtjewel.png");
            else
                _roomPanels[n, m].BackgroundImage = Image.FromFile("../../Assets/jewel.png");
        }

        public void RemoveJewel(int n, int m, bool hasDirt)
        {
            if (hasDirt)
                _roomPanels[n, m].BackgroundImage = Image.FromFile("../../Assets/dirt.png");
            else
                _roomPanels[n, m].BackgroundImage = null;
        }*/
        public void Refresh(Room[,] rooms, int agentXPosition, int agentYPosition)
        {
            for (int i = 0; i < rooms.GetLength(0); i++)
            {
                for (int j = 0; j < rooms.GetLength(1); j++)
                {
                    if(rooms[i, j].HasDirt() && rooms[i, j].HasJewel())
                        _roomPanels[i, j].BackgroundImage = Image.FromFile("../../Assets/dirtjewel.png");
                    else if (rooms[i, j].HasDirt())
                        _roomPanels[i, j].BackgroundImage = Image.FromFile("../../Assets/dirt.png");
                    else if (rooms[i, j].HasJewel())
                        _roomPanels[i, j].BackgroundImage = Image.FromFile("../../Assets/jewel.png");
                    else
                        _roomPanels[i, j].BackgroundImage = null;
                }
            }
            _roomPanels[agentXPosition, agentYPosition].BackgroundImage = Image.FromFile("../../Assets/agent.png");
        }
    }
}