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

            try
            {
                x = int.Parse(args[0])>=1? int.Parse(args[0]) : x;
                y = int.Parse(args[1])>=1? int.Parse(args[1]) : y;
            }
            catch (Exception excp)
            {
                Console.WriteLine("First 2 arguments : lenght X and Y of the grid, 10 by default"); 
            }

            GraphicalView view = new GraphicalView(x, y);
            
            Environment environment = new Environment(view, x, y);
            int chanceJ = 6;
            int chanceD = 12;
            int factorSleep = 100;
            try
            {
                chanceJ = (int.Parse(args[2]) >= 5 && int.Parse(args[2]) <= 100) ? int.Parse(args[2]) : chanceJ;
                chanceD = (int.Parse(args[3]) >= 5 && int.Parse(args[3]) <= 100) ? int.Parse(args[3]) : chanceD;
                factorSleep = int.Parse(args[4]) >= 20 ? int.Parse(args[4]) : factorSleep;
            }
            catch (Exception excp)
            {
                Console.WriteLine("3rd and 4rth arguments : chances of jewel and dirt to appear (in %, 6 & 12 by default)");
                Console.WriteLine("last argument : factor sleep (the higher the slower, 100 by default)");
            }

            environment.SetJewelryAndDirtGenerationPercentages(chanceJ, chanceD);
            environment.FactorSleep = factorSleep;

            Agent agent = new Agent(environment);

            /*Starting environment and agent in separate threads*/
            /*Agent starts at (0, 0)*/
            /*Environment starts with a dirt and a jewel randomly generated*/
            environmentThread = new Thread(environment.AsyncTask);
            agentThread = new Thread(agent.AsyncWork);
            environmentThread.Start();
            agentThread.Start();

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

        /*Called once, prints the empty manor*/
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

        /*Called at every action, updates each room BackgroundImages property according to its content*/
        public void Refresh(Room[,] rooms, int agentXPosition, int agentYPosition)
        {
            for (int i = 0; i < rooms.GetLength(0); i++)
            {
                for (int j = 0; j < rooms.GetLength(1); j++)
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