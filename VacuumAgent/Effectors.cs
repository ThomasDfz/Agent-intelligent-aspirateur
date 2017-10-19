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
    public static class EffectorDuration
    {
        public static int GetEffectorDuration(Effectors eff)
        {
            switch(eff)
            {
                case Effectors.Vacuum :
                    return 20;
                case Effectors.PickUpJewel:
                    return 10;
                default:
                    return 2;
            }
        }
    }
}