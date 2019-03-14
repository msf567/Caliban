// INSTRUCTIONS FOR COMPILING AND USING CLIGL WITH THIS PROJECT
//
// 1.  Download the CLIGL project from the repository here: https://github.com/Ethan-Bierlein/CLIGL 
// 2.  Open up the CLIGL project solution in Visual Studio.
// 3.  Set the build mode to 'Release'.
// 4.  Naviate to the 'Build' dropdown and hit 'Build Solution'.
// 5.  Minimize Visual Studio and navigate to the 'Release' folder in your file explorer of choice.
// 6.  Verify that a CLIGL.dll file has been produced.
// 7.  Create a new Visual Studio project of the type 'Console Application' in the language 'C#'.
// 8.  Copy-paste this code into the main file.
// 9.  Navigate to 'References' in the solution explorer.
// 10. Right-click 'References' and click 'Add Reference'.
// 11. Navigate down to the 'Browse' category and click it.
// 12. Click the 'Browse' button and navigate to the CLIGL Release folder.
// 13. Click the CLIGL.dll file to add it as a reference.
// 14. Run the code within this file.

using System;
using CLIGL;
using Microsoft.VisualBasic;

namespace CLIGL_Tutorial
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            for (var i = 0; i <= 60000; i++)
            {
                Console.Write(Strings.ChrW(i));
                if (i % 50 == 0)
                {
                    // break every 50 chars
                    Console.WriteLine();
                }
            }

            Console.ReadKey();
        }
    }
}