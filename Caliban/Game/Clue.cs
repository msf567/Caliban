using System;
using System.Collections.Generic;
using System.IO;

namespace Caliban.Core.Game
{
    public class Clue : IDisposable
    {
        protected DirectoryInfo clueLocation;
        protected List<string> clueSteps;
        public Clue(string _clueLocation)
        {
            clueLocation = new DirectoryInfo(_clueLocation);
            clueSteps = new List<string>(clueLocation.FullName.Split(Path.DirectorySeparatorChar));
        }

        public virtual void Dispose()
        {
        }
        
    }
}