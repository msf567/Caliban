using System;
using System.Collections.Generic;
using System.IO;

namespace Caliban.Core.Game
{
    public class Clue : IDisposable
    {
        protected readonly DirectoryInfo locationToPointTo;
        protected readonly List<string> clueSteps;

        protected Clue(string _locationToPointTo)
        {
            locationToPointTo = new DirectoryInfo(_locationToPointTo);
            clueSteps = new List<string>(locationToPointTo.FullName.Split(Path.DirectorySeparatorChar));
        }

        public virtual void Dispose()
        {
        }
        
    }
}