namespace VacuumAgent
{
    public enum Effectors
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        Vacuum,
        PickUpJewel
    }
    static public class EffectorDuration
    {
        static public int getEffectorDuration(Effectors eff)
        {
            switch(eff)
            {
                case Effectors.Vacuum :
                    return 25;
                case Effectors.PickUpJewel:
                    return 10;
                default:
                    return 5;
            }
        }
    }
}