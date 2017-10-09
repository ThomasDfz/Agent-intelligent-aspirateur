using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace VacuumAgent
{
    public class Agent
    {
        private int _x, _y;
        private Environment _environment;
        
        public Agent(Environment environment)
        {
            _environment = environment;
            _x = 0;
            _y = 0;
            Console.WriteLine("Agent créé");
        }

        public void AsyncWork()
        {
            while (true)
            {
                Thread.Sleep(1000);
               // Console.WriteLine("Agent is doing some shit");
            }
        }
    }
}