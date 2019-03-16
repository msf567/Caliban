
using System;
using CLIGL;
using Microsoft.VisualBasic;

namespace CLIGL_Tutorial
{
    public class Program
    {
        // These two constants store the width and height of the console window, as well. 
        // as the name. It should be noted however that the width and height of the window 
        // do not have to be constant. However, whenever the window width and height change, 
        // all buffers being drawn to the window must be resized as well.
        public const int WINDOW_WIDTH = 80;
        public const int WINDOW_HEIGHT = 40;
        public const string WINDOW_TITLE = "CLIGL Tutorial";

        // Main() does not necessarily have to be marked with the attribute [STAThread],
        // but if CLIGL ever incorporates Windows Forms features, it will be required.
        [STAThread]
        public static void Main(string[] args)
        {
            // Create the rendering window. All buffers we use will have to be rendered to this created
            // window here.
            RenderingWindow renderingWindow = new RenderingWindow(WINDOW_TITLE, WINDOW_WIDTH, WINDOW_HEIGHT);

            // Create a single rendering buffer that we can draw to. For the purposes of this tutorial,
            // we'll only be using a single rendering buffer but it should be kept in mind that you can
            // create as many as you like.
            RenderingBuffer renderingBuffer = new RenderingBuffer(80, 40);

            // Create a simple rendering shader. This shader will be applied to the entire console screen
            // region, and will simple convert pixels with a blue background color to pixels with a red
            // background color, and vice versa.
            RenderingShader renderingShader = new RenderingShader(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT, (x, y, pixel) =>
            {
                RenderingPixel inputPixel = pixel;
                RenderingPixel outputPixel = new RenderingPixel();

                if (inputPixel.BackgroundColor == ConsoleColor.Red || inputPixel.BackgroundColor == ConsoleColor.Blue)
                {
                    outputPixel.Character = inputPixel.Character;
                    if (inputPixel.BackgroundColor == ConsoleColor.Red)
                    {
                        outputPixel.BackgroundColor = ConsoleColor.Blue;
                    }
                    else if (inputPixel.BackgroundColor == ConsoleColor.Blue)
                    {
                        outputPixel.BackgroundColor = ConsoleColor.Red;
                    }

                    return outputPixel;
                }
                else
                {
                    return inputPixel;
                }
            });

            // Create a new RenderingTexture with a width of two and a height of two, and initialize all
            // the pixels contained within it.
            RenderingTexture renderingTexture = new RenderingTexture(2, 2);
            renderingTexture.SetPixel(0, 0, new RenderingPixel('T', ConsoleColor.White, ConsoleColor.Black));
            renderingTexture.SetPixel(1, 0, new RenderingPixel('T', ConsoleColor.White, ConsoleColor.Black));
            renderingTexture.SetPixel(1, 1, new RenderingPixel('T', ConsoleColor.White, ConsoleColor.Black));
            renderingTexture.SetPixel(0, 1, new RenderingPixel('T', ConsoleColor.White, ConsoleColor.Black));

            // Initialize a loop counter and begin an infinite loop. The loop counter is not technically
            // required, but can be useful for a number of things, like timing, etc.
            int loopCounter = 0;
            while (true)
            {
                // Clear the rendering buffer using the predefined RenderingPixel.Empty pixel. It should be noted
                // that you can clear with any kind of pixel, or not clear at all, if that's the desired effect.
                renderingBuffer.ClearPixelBuffer(RenderingPixel.EmptyPixel);

                // Set individual pixels at each corner of the console screen, with each having the character 'P',
                // a white foreground color and a red or blue background color.
                renderingBuffer.SetPixel(0, 0, new RenderingPixel('P', ConsoleColor.White, ConsoleColor.Red));
                renderingBuffer.SetPixel(WINDOW_WIDTH - 1, 0,
                    new RenderingPixel('P', ConsoleColor.White, ConsoleColor.Blue));
                renderingBuffer.SetPixel(0, WINDOW_HEIGHT - 1,
                    new RenderingPixel('P', ConsoleColor.White, ConsoleColor.Blue));
                renderingBuffer.SetPixel(WINDOW_WIDTH - 1, WINDOW_HEIGHT - 1,
                    new RenderingPixel('P', ConsoleColor.White, ConsoleColor.Red));

                // Draw two four-by-two rectangles at the positions (10, 10) and (15, 15), each with pixels having
                // the character 'R', cyan foreground colors and dark green background colors.
                renderingBuffer.SetRectangle(10, 10, 4, 2,
                    new RenderingPixel('R', ConsoleColor.Cyan, ConsoleColor.DarkGreen));
                renderingBuffer.SetRectangle(15, 15, 4, 2,
                    new RenderingPixel('R', ConsoleColor.Cyan, ConsoleColor.DarkGreen));

                // Set a string containing the text "Hello world!" at the position (20, 20), with a foreground color
                // of white and a console color of black, as well as rendering the previously created rendering texture.
                renderingBuffer.SetString(20, 20, "Hello world!", ConsoleColor.White, ConsoleColor.Black);
                renderingBuffer.SetTexture(30, 30, renderingTexture);

                // Lastly, before rendering to the window, apply the previously created shader to the rendering buffer
                // if the loop counter is divisible by X. This will cause the background colors of the corner pixels
                // to occasionally switch to their opposite colors.
                if (loopCounter % 5 == 0)
                {
                    renderingBuffer.ApplyShader(renderingShader);
                }

                // Finally, render our rendering buffer to the rendering window and increment the loop counter.
                renderingWindow.Render(renderingBuffer);
                loopCounter++;
            }
        }
    }
}