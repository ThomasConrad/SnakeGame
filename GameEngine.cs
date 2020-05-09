using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;
using System.Windows.Input;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.Media;




namespace SnakeGame
{
    class GameEngine
    {
        //Constants//
        public static class Constants // All the constants i use to hold information across multiple classes
        {
            public static bool controlDown = false;
            public static bool rDown = false;
            public static bool spaceDown = false;
            public static List<Double> angle { get; set; }
            public static Double turnSpeed = 0.015;
            public static bool isAudioOn = false;
            public static List<List<int[]>> coords { get; set; }
            public static int frameCount = 0;
            public static List<Double> distances { get; set; }
            public static Double avgLen = 1;
            public static int canvasWidth = 1000;
            public static int canvasHeight = 800;
            public static int sphereDiameter = 6;
            public static int playerAmount = 1;
            public static int retry = 0;
            public static bool setupMode = true;
            public static List<String[]> playerControls { get; set; }
            public static float k = 3F;
            public static List<Double[]> currentCoords { get; set; }
            public static List<int[]> snakeCoords { get; set; }
            public static List<float> xShift { get; set; }
            public static List<float> yShift { get; set; }
            public static string currentKey = "";
            public static bool waitForKey = false;
            public static List<bool[]> playerControlState { get; set; }
            public static bool eraserDrawn = false;
            public static List<int[]> eraserCoords { get; set; }
            public static bool refreshCooldown = true;
            public static bool eraserHit = false;
            public static List<int> playerPoints { get; set; }
            public static List<bool> playerAlive { get; set; }
            public static int playersDead = 0;
        }

        //Members//

        private Graphics graphics;
        private Thread renderthread;

        //Functions//

        public GameEngine(Graphics g)
        {
            graphics = g; //Creates a name for the graphics
        }
        /// <summary>
        /// Initializes thread and other useful stuff.
        /// </summary>
        public void init()
        {

            while (Constants.retry < 3) //you can enter a wrong number 3 times before it shuts down
            {
                try
                {
                    string lineSpace = "                                   ";
                    Constants.playerAmount = Convert.ToInt32(Interaction.InputBox("Enter the amount of players:" + lineSpace + "Max 6", "players?")); //gets playeramount from an inputbox
                    break; // escapes the loop
                }
                catch (Exception)
                {
                    MessageBox.Show("The string you entered isn't a number, try again");
                    Constants.retry++;
                }
            }

            renderthread = new Thread(() => render()); // creates the main graphics renderthread
            renderthread.Start(); // starts the main thread

            if (Constants.retry == 3) // if you retried too many times, it kills the program
            {
                MessageBox.Show("Too many mistakes, shutting down");
                stop();
                Application.Exit();
            }

            #region Creation of Constants

            Constants.coords = new List<List<int[]>>();
            Constants.distances = new List<Double>();
            Constants.angle = new List<double>();
            Constants.playerControls = new List<string[]>();
            Constants.playerControlState = new List<bool[]>();
            Constants.currentCoords = new List<double[]>();
            Constants.snakeCoords = new List<int[]>();
            Constants.xShift = new List<float>();
            Constants.yShift = new List<float>();
            Constants.playerAlive = new List<bool>();
            Constants.playerPoints = new List<int>();
            #endregion

        }
        /// <summary>
        /// Kills Thread
        /// </summary>
        public void stop()
        {
            graphics.Clear(Color.Black); //Clears the graphics
            renderthread.Abort(); //kills the thread
        }
        public void restart()
        {
            clearAll(); //clears everything
            renderthread.Abort(); //stops the thread
            init(); //starts everything over
        }
        /// <summary>
        /// Is responsible for rendering the graphics, either in a loop or as a setup. INSERT FUNCTIONS HERE FOR GRAPHICS.
        /// </summary>
        private void render()
        {
            /*-----UTILITY------*/

            try
            {
                Constants.distances.Clear(); //starts off by clearing the distances. This is done to catch potential errors
            }
            catch (Exception) //if it couldn't clear it, there was an error and it shuts off
            {
                stop();
                Application.Exit();
            }

            

            string[] leftRight = new string[2] { "left", "right" }; //Creates names for the custom form

            for (int i = 0; i < Constants.playerAmount; i++) //runs this playeramount times
            {
                Constants.playerControls.Add(new string[2]); //generates lists for the controls
                Constants.playerControlState.Add(new bool[2]);

                for (int p = 0; p < 2; p++) //runs twice. Once for left, and once for right
                {
                    Constants.waitForKey = true;
                    string text = string.Format("Please focus the main window and press the {0} Control key for player {1}", leftRight[p], i + 1);
                    string caption = "Key selection";
                    prompt.Show(text, caption);
                    while (Constants.waitForKey) // waits until a key is pressed
                    {
                        Thread.Sleep(100);
                    }
                    Constants.playerControls[i][p] = Constants.currentKey; //assigns the latest pressed key to the controls for the currently selected player and key

                    prompt.Close();
                }
            }
            Constants.setupMode = false; // enters the playmode

            for (int i = 0; i < Constants.playerAmount; i++) //sets up snake startcoords, ½s and constants
            {
                Thread.Sleep(250);
                Constants.playerPoints.Add(new int());
                setupSnake(i);
            }


            /*---------CONSTANTS-----------*/
            int framesRendered = 0;
            long startTime = Environment.TickCount;


            /*-------BRUSHES_AND_PENS--------*/
            List<SolidBrush>  brushes = new List<SolidBrush>();
            brushes.Add(new SolidBrush(Color.Red));
            brushes.Add(new SolidBrush(Color.Blue));
            brushes.Add(new SolidBrush(Color.Green));
            brushes.Add(new SolidBrush(Color.Pink));
            brushes.Add(new SolidBrush(Color.Gray));
            brushes.Add(new SolidBrush(Color.Brown));
            SolidBrush bgColour = new SolidBrush(Color.Black);
            Pen outline = new Pen(Brushes.Coral, Constants.sphereDiameter);
            

            /*--------GRAPHICS-----------*/
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; //turns on antialiasing
            graphics.FillRectangle(bgColour, 0, 0, Constants.canvasWidth, Constants.canvasHeight); //fills in the background
            drawWalls(graphics, outline); // Calculate coordinates for rectangle borders
            drawPoints();
            

            Print("Setup successful");


            //------Main Loop. Each iteration renders a frame------//
            while (true)
            {
                long timeStart = Environment.TickCount; // saves the time at the start of the frame

                if (Constants.spaceDown) //pauses loop while space is pressed
                {
                    Thread.Sleep(50);
                    continue;
                }

                for (int i = 0; i < Constants.playerAmount; i++) //draws the circle that is part of the snake
                {
                    if (Constants.playerAlive[i])
                    {
                        drawSnake(i, brushes[i]);
                    }
                }

                #region Keypresses 

                int count = 0;

                foreach (bool[] stateArray in Constants.playerControlState) // left key for all players
                {

                    if (stateArray[0])
                    {
                        Constants.angle[count] = Constants.angle[count] - Constants.turnSpeed;
                    }
                    count++;
                }

                count = 0;

                foreach (bool[] stateArray in Constants.playerControlState) // right key for all players
                {
                    if (stateArray[1])
                    {
                        Constants.angle[count] = Constants.angle[count] + Constants.turnSpeed;
                    }
                    count++;
                }

                if (Constants.controlDown && Constants.refreshCooldown) // clears and continues when you press Ctrl + R
                {
                    if (Constants.rDown)
                    {
                        erase(outline);
                    }
                }

                #endregion

                //UTILITY

                framesRendered++;
                if (Environment.TickCount >= startTime + 1000) //checks how many frames was rendered in a second and writes it to the console
                {
                    Constants.k = framesRendered / (framesRendered / Constants.k); // is supposed to always keep the snake moving at the same pixels per second
                    Print("" + framesRendered + " fps");
                    framesRendered = 0;
                    startTime = Environment.TickCount;
                    Constants.refreshCooldown = true;
                }
                List<List<int[]>> coordinatess = Constants.coords;


                //calculates an average distance between the thingies
                //avgLen in all but the extreme cases is very close to k. Therefore i am estimating based on k to save processor time.
                //i am still keeping the function, in case i want to make the snakes go really fast, on a larger playing field
                /*
                if (Constants.frameList[playerNum-1] > 10)
                {
                    List<Double> avgLenList = new List<Double>();
                    for (int i = 1; i <= Constants.frameList[playerNum-1]; i++)
                    {
                        avgLenList.Add( coordDistInts(Constants.coords[playerNum][i], Constants.coords[playerNum][i - 1]));
                    }

                    Constants.avgLen = avgLenList.Average();

                }
                */

                int framenum = Constants.frameCount;
                Double killDistance = Constants.sphereDiameter;
                List<List<int[]>> coordinates = Constants.coords;

                if (Constants.eraserDrawn) // if the eraser is on the screen check for this
                {
                    try
                    {
                        foreach (int[] element in Constants.eraserCoords) // if you hit the erasaer, everything is cleared, and eraserhit is set to true
                        {
                            for (int i = 0; i < Constants.playerAmount; i++)
                            {
                                if (coordDist(Constants.currentCoords[i], element) < killDistance - 4)
                                {
                                    erase(outline);
                                    Constants.eraserHit = true;
                                }
                            }
                            
                        }
                    }
                    catch (Exception) { } // catches random exception that sometimes happens
                }
               

                if (Constants.frameCount > 180) //2 sec of invulnerability, kills you after if you collide with anything
                {
                    ArenaCheck();
                    foreach (List<int[]> element in coordinates) //goes through all coordlists
                    {
                        int counter = 0;
                        foreach (int[] coord in element)
                        {

                            if (coordinates.IndexOf(element) == 0) //Doesn't include the walls
                            {
                                break;
                            }

                            //doesn't include parts of the snake that are too close to the head, the distance is calculated based on the speed and killdistance
                            if (element.IndexOf(coord) > framenum - Math.Pow(Constants.k, -1) * killDistance * 3.2)
                            {
                                break;
                            }

                            else //Checks if he hit another player
                            {
                                killCheck(coord);
                                counter++;
                            }

                        }

                        foreach (int[] coord in Constants.coords[0]) //Checks if he hits a wall
                        {
                            killCheck(coord);
                        }
                    }
                }
                int playersBefore = Constants.playersDead; 

                Constants.playersDead = 0;

                for (int i = 0; i < Constants.playerAlive.Count; i++) //sets the number of dead players
                {
                    if (!Constants.playerAlive[i])
                    {
                        Constants.playersDead++;
                    }
                }


                if (playersBefore != Constants.playersDead) //if someone died, give point to all alive players
                {
                    givePoints();
                }

                if (Constants.playersDead > Constants.playerAmount - 2 && Constants.playerAmount > 1) // if only one player is alive run this
                {
                    if (Constants.playerPoints.Max() == 300) // if five rounds has passed, end the game and show a little thingy
                    {
                        Constants.currentCoords.Clear(); // clears some constants
                        Constants.playerAlive.Clear();
                        Constants.playersDead = 0;
                        clearAll();
                        drawWalls(graphics, outline); // draws the walls again
                        drawPoints(); // draws the score
                        MessageBox.Show(string.Format("Player {0} won the game!", (Constants.playerPoints.IndexOf(300))+1));
                        stop();
                    }

                    else
                    {
                        System.IO.Stream countdown = Properties.Resources.countdown; //starts a countdown for the new round
                        SoundPlayer countdowns = new SoundPlayer(countdown);
                        countdowns.Play();

                        Constants.currentCoords.Clear(); // clears some constants
                        Constants.playerAlive.Clear();
                        Constants.playersDead = 0;
                        clearAll();
                        drawWalls(graphics, outline); // draws the walls again
                        drawPoints(); // draws the score
                        Thread.Sleep(2000); // waits while the countdown finishes

                        System.IO.Stream bgMusic = SnakeGame.Properties.Resources.Soundtrack; // resumes the soundtrack
                        SoundPlayer soundtrack = new SoundPlayer(bgMusic);
                        soundtrack.PlayLooping();

                        for (int i = 0; i < Constants.playerAmount; i++) //starts the snakes at random positions
                        {
                            setupSnake(i);
                        }
                        
                    }
                }

                

                long frameTime = Environment.TickCount - timeStart;
                try
                {
                    Thread.Sleep((1000 / 60) - (int)frameTime);//waits for the remaining bit of the 1/60'th second after the calculations are done
                }
                catch (Exception) { }

                Constants.distances.Clear();
                Constants.frameCount++; //Updates the framecount for the current thread
            }


        }

        /// <summary>
        /// Returns an intArray of x,y start coordinates
        /// </summary>
        /// <returns></returns>
        private int[] startCoords()
        {
            Random random = new Random();
            int xCord = random.Next(0, Constants.canvasHeight-24-100)+12+100; //Gives x-coordinate start placement
            int yCord = random.Next(0, Constants.canvasHeight-24-100)+12+100; //Gives y-coordinate start placement
            int[] coords = new int[2] { xCord, yCord };
            return coords;
        }

        /// <summary>
        /// Generates random movement direction
        /// </summary>
        /// <returns></returns>
        private Double[] generateMovementVector(Double inputNum)
        {
            Double direction = inputNum * Math.PI * 2;

            Double[] moveVector = new Double[2] { Math.Cos(direction), Math.Sin(direction) };



            return moveVector;
        }
        /// <summary>
        /// Gives the angle a random start value
        /// </summary>
        private void populateAngle()
        {
            Random random = new Random();
            Double temp = random.NextDouble();
            Constants.angle.Add(temp * 2 * Math.PI);
        }

        /// <summary>
        /// Calculates the distance between an integer coordinate pair and a precision floating point coordinate pair
        /// </summary>
        /// <param name="currentLocation"></param>
        /// <param name="coord"></param>
        /// <returns></returns>
        private Double coordDist(Double[] currentLocation, int[] coord)
        {
            Double temp = Math.Sqrt( Math.Pow((currentLocation[0] - coord[0] ),2) + Math.Pow((currentLocation[1] - coord[1]), 2));

            return temp;
        }

        /// <summary>
        /// Calculates the distance between to integer coordinate pairs.
        /// </summary>
        /// <param name="currentLocation"></param>
        /// <param name="coord"></param>
        /// <returns></returns>
        private Double coordDistInts(int[] currentLocation, int[] coord)
        {
            Double temp = Math.Sqrt(Math.Pow((currentLocation[0] - coord[0]), 2) + Math.Pow((currentLocation[1] - coord[1]), 2));

            return temp;
        }

        /// <summary>
        /// Draws the walls that encloses the snakes, and adds the coordinates to the list of coords at index 0.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="brush"></param>
        private void drawWalls(Graphics graphics, Pen outLine)
        {
            Constants.coords.Add(new List<int[]>());

            for (int i = 0; i < Constants.canvasHeight - 24; i++)
            {
                Constants.coords[0].Add(new int[2] { i + 12, 12 });
                Constants.coords[0].Add(new int[2] { 12, i + 12 });
                Constants.coords[0].Add(new int[2] { Constants.canvasHeight - 12, i + 12 });
                Constants.coords[0].Add(new int[2] { i + 12, Constants.canvasHeight - 12 });
            }

            graphics.DrawRectangle(outLine, 12, 12, Constants.canvasHeight - 24, Constants.canvasHeight - 24);
        }

        /// <summary>
        /// Prints a string to the console
        /// </summary>
        /// <param name="input"></param>
        private void Print(string input)
        {
            Console.WriteLine(input);
        }

        /// <summary>
        /// Sets up the snake from null, and generates random starting values
        /// </summary>
        /// <param name="playerNum"></param>
        private void setupSnake(int playerNum)
        {
            
            Constants.playerAlive.Add(new bool());
            Constants.currentCoords.Add(new Double[2]);
            Constants.snakeCoords.Add(new int[2]);
            Constants.xShift.Add(new float());
            Constants.yShift.Add(new float());
            Constants.coords.Add(new List<int[]>());

            populateAngle();
            Constants.snakeCoords[playerNum] = startCoords(); //generates a random set of coordinates
            Constants.playerAlive[playerNum] = true;
            /*MOVEMENT*/
            Double[] vector = generateMovementVector(Constants.angle[playerNum]);
            Constants.xShift[playerNum] = (float)vector[0];
            Constants.yShift[playerNum] = (float)vector[1];
            Thread.Sleep(100);
        }

        /// <summary>
        /// Sets up the snake, but starts it at its latest position 
        /// </summary>
        /// <param name="playerNum"></param>
        /// <param name="currentcoords"></param>
        /// <param name="currentAngle"></param>
        private void continueSnake(int playerNum, Double[] currentcoords, Double currentAngle)
        {
            Constants.snakeCoords.Add(new int[2]);
            Constants.xShift.Add(new float());
            Constants.yShift.Add(new float());
            Constants.coords.Add(new List<int[]>());

            Constants.angle[playerNum] = currentAngle;

            Constants.snakeCoords[playerNum] = currentcoords.Select(Convert.ToInt32).ToArray(); //generates a random set of coordinates

            /*MOVEMENT*/
            Double[] vector = generateMovementVector(Constants.angle[playerNum]);
            Constants.xShift[playerNum] = (float)vector[0];
            Constants.yShift[playerNum] = (float)vector[1];
            Thread.Sleep(100);
        }
        
        /// <summary>
        /// Draws the snake. Must run on every main loop ilteration.
        /// </summary>
        /// <param name="playerNum"></param>
        /// <param name="brush"></param>
        private void drawSnake(int playerNum, Brush brush)
        {
            Double[] vector = generateMovementVector(Constants.angle[playerNum]); //generates the array of vector components which are used for the movement
            vector[0] = vector[0] * Constants.k; //multiplies with speed
            vector[1] = vector[1] * Constants.k; //multiplies with speed
            Constants.currentCoords[playerNum][0] = Constants.snakeCoords[playerNum][0] + Constants.xShift[playerNum];
            Constants.currentCoords[playerNum][1] = Constants.snakeCoords[playerNum][1] + Constants.yShift[playerNum];


            graphics.FillEllipse(brush, (float)Constants.currentCoords[playerNum][0], (float)Constants.currentCoords[playerNum][1], Constants.sphereDiameter, Constants.sphereDiameter); //draws the sphere

            Constants.xShift[playerNum] = Constants.xShift[playerNum] + (float)vector[0]; //updates x coordinates
            Constants.yShift[playerNum]  = Constants.yShift[playerNum] + (float)vector[1]; // updates y coordinates
            Constants.coords[playerNum+1].Add(new int[2] { (int)(Constants.snakeCoords[playerNum][0] + Constants.xShift[playerNum]), (int)(Constants.snakeCoords[playerNum][1] + Constants.yShift[playerNum]) }); //adds the coordinate to the list of painted coordinates

        }

        public class prompt // new form for dialogbox
        {
            private static Thread dialogThread;

            /// <summary>
            /// Sets up a function to open a form, that looks like a dialogbox
            /// </summary>
            /// <param name="text"></param>
            /// <param name="caption"></param>
            public static void ShowDialog(string text, string caption)
            {
                Form prompt = new Form();
                prompt.Width = 500;
                prompt.Height = 100;
                prompt.Text = caption;
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                Label textLabel = new Label() { Left = 50, Top = 25, Text = text };
                textLabel.Size = new Size(textLabel.PreferredWidth, textLabel.PreferredHeight);
                textLabel.Text = text;
                prompt.Controls.Add(textLabel);
                prompt.ShowDialog();
            }

            /// <summary>
            /// Closes the thrad that runs for form
            /// </summary>
            public static void Close()
            {
                dialogThread.Abort();
            }
            /// <summary>
            /// Starts the thread that runs the form
            /// </summary>
            /// <param name="text"></param>
            /// <param name="caption"></param>
            public static void Show(string text, string caption)
            {
                dialogThread = new Thread(() => ShowDialog(text, caption));
                dialogThread.Start();
            }
            /// <summary>
            /// sleeps the form for a set amount of time
            /// </summary>
            /// <param name="time"></param>
            public static void sleep(int time)
            {
                Thread.Sleep(time);
            }

        }
        /// <summary>
        /// Takes a coordinate, and calculates the distances from that coordinate to all the playerheads
        /// </summary>
        /// <param name="coord"></param>
        private void addDistancesToHeads(int[] coord)
        {
            foreach (Double[] currentCoords in Constants.currentCoords)
            {
                Constants.distances.Add(coordDist(currentCoords, coord));
            }
        }
        /// <summary>
        /// Is made to clear all the graphics, and clear the relevant constants
        /// </summary>
        private void clearAll()
        {
            if (Constants.eraserDrawn)
            {
                Constants.eraserHit = true;
                Constants.eraserCoords.Clear();
            }
            graphics.Clear(Color.Black);
            Constants.coords.Clear();
            Constants.distances.Clear();
            Constants.coords = new List<List<int[]>>();
            Constants.frameCount = 0;
            Constants.distances = new List<Double>();
            Constants.coords.Add(new List<int[]>());
        }
        /// <summary>
        /// Erases everyting, but keeps the snakes' positions
        /// </summary>
        /// <param name="wallColour"></param>
        private void erase(Pen wallColour)
        {
            List<Double[]> coords = Constants.currentCoords;
            List<Double> angles = Constants.angle;
            Constants.refreshCooldown = false;
            clearAll();
            drawWalls(graphics, wallColour); // redraws the walls
            drawPoints();

            for (int i = 0; i < Constants.playerAmount; i++)
            {
                Thread.Sleep(250); // waits to ensure that the randomly generated numbers are different
                continueSnake(i, coords[i], angles[i]); // continues the snake from where it last was
            }
        }
        /// <summary>
        /// Checks if a distance between two coords is too short
        /// </summary>
        /// <param name="coord"></param>
        private void killCheck(int[] coord)
        {
            foreach (Double[] currentCoords in Constants.currentCoords)
            {
                if (coordDist(currentCoords, coord) < Constants.sphereDiameter)
                {
                    int player = Constants.currentCoords.IndexOf(currentCoords) + 1;
                    Constants.playerAlive[player - 1] = false;
                }
            }
        }
        /// <summary>
        /// Gives points to all players currently alive
        /// </summary>
        private void givePoints()
        {
            for (int i = 0; i < Constants.playerAmount; i++)
            {
                if (Constants.playerAlive[i])
                {
                    Constants.playerPoints[i] += 60/(Constants.playerAmount-1);
                }
            }

            int count = 1;

            foreach (int point in Constants.playerPoints)
            {
                Print("player " + count + ": " + point.ToString()); // prints points to console
                count++;
            }
        }
        /// <summary>
        /// Draws the score on the canvas
        /// </summary>
        private void drawPoints()
        {
            Font textFont = new Font("Arial", 20);
            SolidBrush textBrush = new SolidBrush(Color.White);

            for (int i = 0; i < Constants.playerAmount; i++)
            {
                string textString = string.Format("Player {0}: {1}", i+1, Constants.playerPoints[i]);
                Point textPoint = new Point(812, 32 * i + 12);
                graphics.DrawString(textString, textFont, textBrush, textPoint);
            }
        }
        /// <summary>
        /// Checks if all players are inside the arena
        /// </summary>
        private void ArenaCheck()
        {
            foreach (Double[] coord in Constants.currentCoords)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (coord[i] < 12 || coord[i] > Constants.canvasHeight + 12)
                    {
                        int player = Constants.currentCoords.IndexOf(coord) + 1;
                        Constants.playerAlive[player - 1] = false;
                    }
                }
            }
        }
    }
}
