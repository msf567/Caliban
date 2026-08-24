namespace Caliban.Core.Cinematics
{
    public static class CinematicPlayer
    {
        private static Cinematic active;

        public static void PlayCinematic(Cinematic _c)
        {
            if (_c == null)
                return;

            if (active != null && active != _c)
                active.Stop();

            active = _c;
            active.Play();
        }

        public static void StopCinematic(Cinematic _c)
        {
            if (_c == null)
                return;

            _c.Stop();
            if (active == _c)
                active = null;
        }

        public static void StopActive()
        {
            if (active == null)
                return;

            active.Stop();
            active = null;
        }
    }
}