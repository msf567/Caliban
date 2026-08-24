using System;
using System.Collections.Generic;
using System.Globalization;

namespace Caliban.Core.Cinematics
{
    public static class CinematicScriptParser
    {
        public static List<CinematicCue> Parse(string _scriptText)
        {
            var cues = new List<CinematicCue>();
            if (string.IsNullOrEmpty(_scriptText))
                return cues;

            string[] lines = _scriptText.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            foreach (var rawLine in lines)
            {
                string line = rawLine.Trim();
                if (line.Length == 0)
                    continue;

                string[] fields = line.Split('\t');
                if (fields.Length < 2)
                    continue;

                if (!double.TryParse(fields[0], NumberStyles.Float, CultureInfo.InvariantCulture,
                        out double seconds))
                    continue;

                string label = fields[fields.Length - 1].Trim();
                if (label.Length == 0)
                    continue;

                cues.Add(new CinematicCue(TimeSpan.FromSeconds(seconds), label));
            }

            return cues;
        }
    }
}