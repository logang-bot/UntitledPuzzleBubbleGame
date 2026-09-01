namespace Game.Shooter
{
    /// <summary>
    /// World-space wall/ceiling bounds a trajectory reflects off of or terminates at.
    /// </summary>
    public readonly struct BoardBounds
    {
        public readonly float LeftWallX;
        public readonly float RightWallX;
        public readonly float CeilingY;

        public BoardBounds(float leftWallX, float rightWallX, float ceilingY)
        {
            LeftWallX = leftWallX;
            RightWallX = rightWallX;
            CeilingY = ceilingY;
        }
    }
}
