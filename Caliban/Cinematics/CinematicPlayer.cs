namespace Caliban.Core.Cinematics
{
    public static class CinematicPlayer
    {
        public static void PlayCinematic(Cinematic c)
        {
            c.Play();
        }

        public static void StopCinematic(Cinematic c)
        {
            c.Stop();
        }

        private static void Update(double deltaTime)
        {
            
        }
    }
}