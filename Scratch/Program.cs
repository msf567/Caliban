using System;
using CLIGL;

namespace Scratch
{
    internal class Program
    {
        public const int WINDOW_WIDTH = 50;
        public const int WINDOW_HEIGHT = 40;
        public const string WINDOW_TITLE = "WaterMeter";
        private static Random r;
        private static bool bubbling = false;
        private static int bubbleDrift = 0;
        private static int bubbleHeight = 20;
        private static int bubblePos = 0;
        private static int waterWidth = 7;
        private static int waterHeight = 20;
        private static int ContainerWidth = 9;
        private static int ContainerHeight = 30;
        private static int popCount = 2;
        private static int popPos = 0;

        public static void Main(string[] args)
        {
            r = new Random(Guid.NewGuid().GetHashCode());
            RenderingWindow renderingWindow = new RenderingWindow(WINDOW_TITLE, WINDOW_WIDTH, WINDOW_HEIGHT);
            RenderingBuffer renderingBuffer = new RenderingBuffer(50, 40);
            int loopCounter = 0;
            while (true)
            {
                renderingBuffer.ClearPixelBuffer(RenderingPixel.EmptyPixel);

                renderingBuffer.SetRectangle(10, 10, 7, 20,
                    new RenderingPixel('.', ConsoleColor.Blue, ConsoleColor.DarkBlue));

                if (loopCounter % 1500 == 0)
                {
                    bubbling = true;
                    bubbleHeight = waterHeight - 1;
                    bubblePos = r.Next(1, waterWidth - 1);
                }

                if (bubbling)
                {
                    if (loopCounter % 40 == 0)
                        bubbleHeight--;

                    renderingBuffer.SetPixel(10 + bubblePos, 10 + bubbleHeight,
                        new RenderingPixel('0', ConsoleColor.Blue, ConsoleColor.DarkBlue));

                    if (bubbleHeight < 0)
                    {
                        popPos = bubblePos;
                        popCount = 40;
                        bubbling = false;
                    }
                }

                if (popCount > 0)
                {
                    renderingBuffer.SetPixel(10 + popPos, 9,
                        new RenderingPixel('*', ConsoleColor.Blue, ConsoleColor.Black));
                    popCount--;
                }


                renderingWindow.Render(renderingBuffer);
                loopCounter++;
            }
        }
    }
}