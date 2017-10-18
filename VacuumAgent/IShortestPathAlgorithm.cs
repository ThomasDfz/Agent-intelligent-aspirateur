using System.Collections.Generic;

namespace VacuumAgent
{
    public interface IShortestPathAlgorithm
    {
        bool ExploreAndSearch(int root, int desire);
        Stack<int> BuildShortestPath(Stack<Effectors> intentions, int root, int desire);
    }
}