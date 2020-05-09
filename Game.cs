using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;


namespace SnakeGame
{
    class Game
    {

        private GameEngine gEngine;


        public void startGraphics(Graphics g)
        {
            gEngine = new GameEngine(g);
            gEngine.init();
        }

        public void stopGame()
        {
            gEngine.stop();
        }

        public void restartGame()
        {
            gEngine.restart();
        }
    }
}
