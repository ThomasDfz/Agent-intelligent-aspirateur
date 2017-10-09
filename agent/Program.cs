using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace VacuumAgent
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Thread agentThread, environmentThread;
            Environment environment = new Environment();
            Agent agent = new Agent(environment);
            agentThread = new Thread(new ThreadStart(agent.AsyncWork));
            environmentThread = new Thread(new ThreadStart(environment.AsyncTask));
            agentThread.Start();
            environmentThread.Start();
            GraphicalView view = new GraphicalView(environment, agent);
            view.print();
        }
    }

    class GraphicalView
    {
        private Environment _environment;
        private Agent _agent;
        
        public GraphicalView(Environment environment, Agent agent)
        {
            _environment = environment;
            _agent = agent;
        }

        public void print()
        {
            while (true)
            {
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if (_environment.rooms[i, j].isDirty())
                        {
                            Console.Write("D");
                        }
                        else if (_environment.rooms[i, j].isJewely())
                        {
                            Console.Write("J");
                        }
                        else
                        {
                            Console.Write(".");
                        }
                    }
                    Console.WriteLine("");
                }
                Thread.Sleep(2000);
            }
        }
    }
}