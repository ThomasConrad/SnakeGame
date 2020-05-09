using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Media;



namespace SnakeGame
{
    public partial class GameWindow : Form
    {
        private Game game = new Game();


        public GameWindow()
        {
            InitializeComponent();
        }

        private void GameWindow_Load(object sender, EventArgs e)
        {
            

            AllocConsole();//allocates the console
            try //plays the theme music
            {
                System.IO.Stream stream = Properties.Resources.Soundtrack;
                SoundPlayer soundtrack = new SoundPlayer(stream);
                if (!GameEngine.Constants.isAudioOn)//if the audio is not on, turn it on
                {
                    soundtrack.PlayLooping();
                    audioIcon.Image = Properties.Resources.Speaker_mute_icon;
                    GameEngine.Constants.isAudioOn = true;
                }
                else //if the audio is already on, turn it off
                {
                    soundtrack.Stop();
                    audioIcon.Image = Properties.Resources.Speaker_icon;
                    GameEngine.Constants.isAudioOn = false;
                }
            }
            catch (Exception ex) //If some error should happen
            {
                MessageBox.Show(ex.Message, "error playing sound");
            }

            Graphics g = canvas.CreateGraphics();
            game.startGraphics(g);//starts the game

            Random random = new Random();
            eraserTimer.Interval = random.Next(8000, 12000);
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            

        }

        private void GameWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            game.stopGame();//stops the game
        }

        private void canvas_MouseClick(object sender, MouseEventArgs e)
        {
        }

        private void GameWindow_KeyUp(object sender, KeyEventArgs e)
        {
            #region KeyConstants to false

            int count = 0;

            foreach (string[] element in GameEngine.Constants.playerControls)
            {

                for (int i = 0; i < 2; i++)
                {
                    if (e.KeyCode.ToString() == element[i])
                    {
                        GameEngine.Constants.playerControlState[count][i] = false;
                    }
                }
                count++;
            }

            if (e.KeyCode == Keys.ControlKey)
            {
                GameEngine.Constants.controlDown = false;
            }
            if(e.KeyCode == Keys.R)
            {
                GameEngine.Constants.rDown = false;
            }
            if (e.KeyCode == Keys.Space)
            {
                GameEngine.Constants.spaceDown = false;
            }
            #endregion
        }

        private void GameWindow_KeyDown(object sender, KeyEventArgs e)
        {
            #region Keyconstants to true

            if (GameEngine.Constants.setupMode)
            {
                GameEngine.Constants.currentKey = e.KeyCode.ToString();
                GameEngine.Constants.waitForKey = false;
            }

            else
            {
                int count = 0;

                foreach (string[] element in GameEngine.Constants.playerControls)
                {

                    for (int i = 0; i < 2; i++)
                    {
                        if (e.KeyCode.ToString() == element[i])
                        {
                            GameEngine.Constants.playerControlState[count][i] = true;
                        }
                    }
                    count++;
                }

                if (e.KeyCode == Keys.ControlKey)
                {
                    GameEngine.Constants.controlDown = true;
                }
                if (e.KeyCode == Keys.R)
                {
                    GameEngine.Constants.rDown = true;
                }
                if (e.KeyCode == Keys.Space)
                {
                    GameEngine.Constants.spaceDown = true;
                }
                #endregion
            }
        }

        private void audioIcon_Click(object sender, EventArgs e)
        {
            try //plays the theme music
            {
                System.IO.Stream stream = SnakeGame.Properties.Resources.Soundtrack;
                SoundPlayer soundtrack = new SoundPlayer(stream);
                if (!GameEngine.Constants.isAudioOn)//if the audio is not on, turn it on
                {
                    soundtrack.PlayLooping();
                    audioIcon.Image = SnakeGame.Properties.Resources.Speaker_mute_icon;
                    GameEngine.Constants.isAudioOn = true;
                }
                else //if the audio is already on, turn it off
                {
                    soundtrack.Stop();
                    audioIcon.Image = SnakeGame.Properties.Resources.Speaker_icon;
                    GameEngine.Constants.isAudioOn = false;
                }
            }
            catch (Exception ex) //If some error should happen
            {
                MessageBox.Show(ex.Message, "error playing sound");
            }
        }

        //Opens the console for debugging and other useful things
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        static extern bool AllocConsole();



        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!GameEngine.Constants.setupMode && !GameEngine.Constants.eraserDrawn)
            {
                GameEngine.Constants.eraserCoords = new List<int[]>();
                Eraser.Image = SnakeGame.Properties.Resources.Eraserhead;
                Random random = new Random();
                int[] location = new int[2] { random.Next(12, (GameEngine.Constants.canvasHeight - 24 - Eraser.Width)), random.Next(12, GameEngine.Constants.canvasHeight - 24 - Eraser.Height) };
                Eraser.Location = new Point(location[0], location[1]);
                GameEngine.Constants.eraserDrawn = true;
                for (int i = 0; i < Eraser.Height; i++)
                {
                    GameEngine.Constants.eraserCoords.Add(new int[2] { location[0], location[1] + i });
                    GameEngine.Constants.eraserCoords.Add(new int[2] { location[0] + i, location[1] });
                    GameEngine.Constants.eraserCoords.Add(new int[2] { location[0] + Eraser.Height, location[1] + i });
                    GameEngine.Constants.eraserCoords.Add(new int[2] { location[0] + i, location[1] + Eraser.Height });
                }
            }
        }

        #region Unused Handlers

        private void canvas_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
        }

        private void GameWindow_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void pictureBox1_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
        }

        private void Eraser_Paint(object sender, PaintEventArgs e)
        {
        }

        #endregion

        private void eraserCheck_Tick(object sender, EventArgs e)
        {
            if (GameEngine.Constants.eraserHit)
            {
                GameEngine.Constants.eraserDrawn = false;
                 Eraser.Location = new Point(1100, 0);
                GameEngine.Constants.eraserHit = false;
            }
        }
    }
}
