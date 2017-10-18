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

            int x = 10;
            int y = 10;
            GraphicalView view = new GraphicalView(x, y);
            
            Environment environment = new Environment(view, x, y);
            environment.SetJewelryAndDirtGenerationPercentages(17, 26);
            environment.FactorSleep = 100; //Overall waiting between 2 actions.

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
        Image dirtJewelImage = Image.FromFile("../../Assets/dirtjewel.png");
        Image jewelImage = Image.FromFile("../../Assets/jewel.png");
        Image dirtImage = Image.FromFile("../../Assets/dirt.png");
        Image agentImage = Image.FromFile("../../Assets/agent.png");
        
        public GraphicalView(int x, int y)
        {
            Size = new Size(40 * (y) + 16 , 40 * (x+1));
            Print(x, y);
        }

        public void Print(int x = 10, int y = 10)
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

        public void Refresh(Room[,] rooms, int agentXPosition, int agentYPosition)
        {
            for (int i = 0; i < rooms.GetLength(0); i++)
            {
                for (int j = 0; j < rooms.GetLength(0); j++)
                {
                    if (rooms[i, j].HasDirt() && rooms[i, j].HasJewel())
                        _roomPanels[i, j].BackgroundImage = dirtJewelImage;
                    else if (rooms[i, j].HasDirt())
                        _roomPanels[i, j].BackgroundImage = dirtImage;
                    else if (rooms[i, j].HasJewel())
                        _roomPanels[i, j].BackgroundImage = jewelImage;
                    else
                        _roomPanels[i, j].BackgroundImage = null;
                }
            }
            _roomPanels[agentXPosition, agentYPosition].BackgroundImage = agentImage;
        }
    }
}