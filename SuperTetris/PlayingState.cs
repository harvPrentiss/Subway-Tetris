using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
using Engine.Input;
using Tao.OpenGl;
using System.Windows.Forms;

namespace SubwayTetris
{
    class PlayingState : IGameObject
    {
        StateSystem _system;
        TextureManager _textureManager;
        Input _input;
        Renderer _renderer = new Renderer();
        Engine.Font _infoFont;
        BlockManager _blockManager;
        Text _scoreText, _pausedText;
        Vector _playArea;
        Vector _clientSize;
        bool _paused = false;
        VerticalMenu _pauseMenu;
        private const double _scalingFactor = 1.0;

        public PlayingState(StateSystem system, TextureManager manager, Input input, Engine.Font infoFont, Vector playArea, Vector clientSize)
        {
            _system = system;
            _textureManager = manager;
            _input = input;
            _infoFont = infoFont;
            _playArea = playArea;
            _clientSize = clientSize;
            InitializeMenu();
            _paused = false;
            _blockManager = new BlockManager(_textureManager, clientSize.X, clientSize.Y, new Vector(_scalingFactor, _scalingFactor, 0));
            if (_scalingFactor < 1.0 && _scalingFactor > 0.5)
            {
                _playArea.X += _blockManager.BlockWidth * _scalingFactor;
            }
            _blockManager.SetBounds(-(_playArea.Y), _playArea.Y, -(_playArea.X), _playArea.X);
            _scoreText = new Text("Score: " + _blockManager.CompletedRows, _infoFont);
            _scoreText.SetPosition((clientSize.X / 2) - 250, 0);
            _scoreText.SetColor(new Color(0.19f, 0.8f, 0.19f, 1));
            _pausedText = new Text("PAUSED", _infoFont);
            _pausedText.SetPosition(-_pausedText.Width/2, 250);
            _pausedText.SetColor(new Color(0.35f, 0.35f, 0.67f, 1));
            GameStart();
        }

        public void InitializeMenu()
        {
            _pauseMenu = new VerticalMenu(0, 150, _input);

            Color focusColor = new Color(251, 242, 0, 1);
            Color noFocusColor = new Color(0.14f, 0.57f, 0.14f, 1);

            Engine.Button quit = new Engine.Button(
                delegate(object o, EventArgs e)
                {
                    _system.ChangeState("start_state");
                },
                new Text("Quit Game", _infoFont), focusColor, noFocusColor);

            Engine.Button resume = new Engine.Button(
                delegate(object o, EventArgs e)
                {
                    _paused = false;
                },
                new Text("Resume", _infoFont), focusColor, noFocusColor);

            _pauseMenu.AddButton(quit);
            _pauseMenu.AddButton(resume);
        }

        public void GameStart()
        {
            Random rand = new Random();
            TetroType type = TetroType.LeftHook;
            int tetro = rand.Next(0, 7);
            if (tetro == 0)
            {
                type = TetroType.LongBlock;
            }
            if (tetro == 1)
            {
                type =TetroType.Square;
            }
            if (tetro == 2)
            {
                type =TetroType.LeftZ;
            }
            if (tetro == 3)
            {
                type =TetroType.RightZ;
            }
            if (tetro == 4)
            {
                type =TetroType.LeftHook;
            }
            if (tetro == 5)
            {
                type =TetroType.RightHook;
            }
            if (tetro == 6)
            {
                type =TetroType.TBlock;
            }
            _blockManager.SpawnNewTetro(type);
            _blockManager.SpawnNewPreviewTetro();
        }

        public void Update(double elapsedTime)
        {            
            if (!_paused)
            {
                _blockManager.Update(elapsedTime);
                UpdateInput(elapsedTime);                
                _scoreText.ChangeText("Score: " + (_blockManager.CompletedRows * 50), _infoFont);
                _scoreText.SetPosition((_clientSize.X / 2) - 225, 0);
                _scoreText.SetColor(new Color(0.19f, 0.8f, 0.19f, 1));
                if (_blockManager.GameOver)
                {
                    _blockManager.Reset();
                    GameStart();
                    _system.ChangeState("game_over_state");
                }
            }
            else
            {
                _pauseMenu.HandleInput();
            }
        }

        public void Render()
        {
            Gl.glClearColor(0.00f, 0.00f, 0.00f, 1);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glDisable(Gl.GL_TEXTURE_2D);
            #region Draws Boundaries
            Gl.glBegin(Gl.GL_LINES);
            {
                Gl.glColor3f(0.14f, 0.57f, 0.14f);
                // Playing area bounds
                // Left bound
                Gl.glVertex2d(-(_playArea.X), -(_playArea.Y));
                Gl.glVertex2d(-(_playArea.X), (_playArea.Y));
                // Bottom bound
                Gl.glVertex2d(-(_playArea.X), -(_playArea.Y));
                Gl.glVertex2d(_playArea.X, -(_playArea.Y));
                // Top bound
                Gl.glVertex2d(-(_playArea.X), _playArea.Y);
                Gl.glVertex2d(_playArea.X, _playArea.Y);
                // Right bound
                Gl.glVertex2d(_playArea.X + 1, _playArea.Y);
                Gl.glVertex2d(_playArea.X + 1, -(_playArea.Y));

                // Preview bounds
                // Left bound
                Gl.glVertex2d((_clientSize.X / 2) -160, (_clientSize.Y / 2) - 192);
                Gl.glVertex2d(_clientSize.X / 2, (_clientSize.Y / 2) - 192);
                // Bottom bound
                Gl.glVertex2d((_clientSize.X / 2) - 160, (_clientSize.Y / 2) - 192);
                Gl.glVertex2d((_clientSize.X / 2) - 160, _clientSize.Y / 2);
                // Top bound
                Gl.glVertex2d((_clientSize.X / 2) - 160, _clientSize.Y / 2);
                Gl.glVertex2d(_clientSize.X / 2, _clientSize.Y / 2);
                //Right bound
                Gl.glVertex2d(_clientSize.X / 2, _clientSize.Y / 2);
                Gl.glVertex2d(_clientSize.X / 2, (_clientSize.Y / 2) - 192);

                // Holding Box Bounds
                // Left bound
                Gl.glVertex2d(-(_clientSize.X / 2), _clientSize.Y / 2);
                Gl.glVertex2d(-(_clientSize.X / 2), (_clientSize.Y / 2) - 192);
                // Bottom bound
                Gl.glVertex2d(-(_clientSize.X / 2), (_clientSize.Y / 2) - 192);
                Gl.glVertex2d(-_clientSize.X / 2 + 160, (_clientSize.Y / 2) - 192);
                // Top bound
                Gl.glVertex2d(-(_clientSize.X / 2), _clientSize.Y / 2);
                Gl.glVertex2d(-(_clientSize.X / 2) + 160, _clientSize.Y / 2);
                // Right bound
                Gl.glVertex2d(-(_clientSize.X / 2) + 160, (_clientSize.Y / 2) - 192);
                Gl.glVertex2d(-(_clientSize.X / 2) + 160, _clientSize.Y / 2);
            }
            Gl.glEnd();
            #endregion
            Gl.glEnable(Gl.GL_TEXTURE_2D);
            //Gl.glBindTexture(Gl.GL_TEXTURE_2D, _textureManager.Get("tetris_block").Id);
            _blockManager.Render(_renderer);
            _renderer.DrawText(_scoreText);
            if (_paused)
            {
                _renderer.DrawText(_pausedText);
                _pauseMenu.Render(_renderer);
            }
            _renderer.Render();
        }

        private void UpdateInput(double elapsedTime)
        {
            Vector controlInput = new Vector(0, 0, 0);

            if (_input.Keyboard.IsKeyPressed(Keys.Right))
            {
                controlInput.X = 1;
            }
            if (_input.Keyboard.IsKeyPressed(Keys.Left))
            {
                controlInput.X = -1;
            }
            if (_input.Keyboard.IsKeyPressed(Keys.Down))
            {
                controlInput.Y = -1;
            }
            if (controlInput.X != 0 || controlInput.Y != 0)
            {
                _blockManager.MoveCurrentBlock(controlInput);
            }
            if (_input.Keyboard.IsKeyPressed(Keys.Up))
            {
                controlInput.Y = -1;
                _blockManager.DropBlock(controlInput);
            }
            //if (_input.Keyboard.IsKeyHeld(Keys.Right))
            //{
            //    controlInput.X = 1;
            //    _blockManager.MoveCurrentBlockSmooth(controlInput);
            //}
            //if (_input.Keyboard.IsKeyHeld(Keys.Left))
            //{
            //    controlInput.X = -1;
            //    _blockManager.MoveCurrentBlockSmooth(controlInput);
            //}
            //if (_input.Keyboard.IsKeyHeld(Keys.Down))
            //{
            //    controlInput.Y = -1;
            //    _blockManager.MoveCurrentBlockSmooth(controlInput);
            //}            
            if (_input.Keyboard.IsKeyPressed(Keys.Space))
            {
                _blockManager.RotateBlock();
            }
            if (_input.Keyboard.IsKeyPressed(Keys.Escape))
            {
                if (_paused)
                {
                    _paused = false;
                }
                else
                {
                    _paused = true;
                }
            }
            if (_input.Keyboard.IsKeyPressed(Keys.H))
            {
                _blockManager.HoldBlock();
            }

            
        }
    }
}
