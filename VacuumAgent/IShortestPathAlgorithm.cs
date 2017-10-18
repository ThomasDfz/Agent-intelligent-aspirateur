using System.Collections.Generic;

namespace VacuumAgent
{
    public interface IShortestPathAlgorithm
    {
        bool ExploreAndSearch(int root, int desire);
        Stack<int> BuildShortestPath(int root, int desire);
    }
}