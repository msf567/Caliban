using System;
using System.Threading;
using CalibanLib.Windows;
using CalibanLib.ConsoleOutput;
using EventHook;

namespace WaterMeter
{
    internal static class WaterMeterProgram
    {
        private const uint MsgDeathThirst = 0x0010;
        
        private static float _targetWaterLevel = 0.7f;
        private static float _waterLevel = 0.7f;
        private static float _prevWaterLevel;
        private const int WaterMeterRenderHeight = 40;
       

        public static void Main(string[] args)
        {
            GlobalInput.RegisterOnGlobalMouse(OnGlobalMouseMove);
            GlobalInput.RegisiterOnGlobalKeyPress(OnGlobalKeyPress);
            
            /*
            Console.SetWindowSize(20, WaterMeterRenderHeight + 4);
            Console.SetBufferSize(20, WaterMeterRenderHeight + 4);
            TargetWaterLevel = WaterLevel;
            Thread t = new Thread(UpdateThread);
            t.Start();
            */
            while (true)
            {
                Thread.Sleep(20);
            }
        }

        private static void UpdateThread()
        {
            while (true)
            {
                _waterLevel = _targetWaterLevel.Clamp(0, 1);
                if (Math.Abs(_prevWaterLevel - _waterLevel) > 0.001f)
                {
                    RenderWaterLevel();
                }

                Thread.Sleep(100);
                _prevWaterLevel = _waterLevel;
            }
        }

       
        private static void SetDead()
        {
            Console.WriteLine("Sent Dead");
        }

        private static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        private static void RenderWaterLevel()
        {
            if (Math.Abs(_waterLevel) < 0.001f)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Clear();
            }
            else
                Console.SetCursorPosition(0, 0);

            ConsoleFormat.CenterWrite("Water Level: " + Math.Ceiling((_waterLevel * 100)) + "/100");
            //CenterWrite("________________");


            int numEmpty = (int) Math.Floor(((1 - _waterLevel) * WaterMeterRenderHeight));
            int numFull = (int) Math.Ceiling((_waterLevel) * WaterMeterRenderHeight);


            for (int x = 0; x < numEmpty; x++)
            {
                //CenterWrite("|              |");
            }

            for (int x = 0; x < numFull; x++)
            {
                //CenterWrite("|XXXXXXXXXXXXXX|");
            }

            //CenterWrite("----------------");
        }

        private static void OnGlobalMouseMove(object s, MouseEventArgs e)
        {
            _targetWaterLevel -= 0.0001f;
        }

        private static void OnGlobalKeyPress(object s, KeyInputEventArgs e)
        {
            if (e.KeyData.Keyname == "Q" && e.KeyData.EventType == KeyEvent.down)
            {
                SetDead();                
            }
        }

    }
}