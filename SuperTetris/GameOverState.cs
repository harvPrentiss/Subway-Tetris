using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
using Tao.OpenGl;
using Engine.Input;

namespace SubwayTetris
{
    class GameOverState : IGameObject
    {
        Engine.Font _generalFont;
        Input _input;
        VerticalMenu _menu;
        Renderer _renderer = new Renderer();
        Text _title;
        StateSystem _system;

        public GameOverState(Engine.Font titleFont, Engine.Font generalFont, Input input, StateSystem system)
        {
            _system = system;

            _generalFont = generalFont;
            _input = input;
            InitializeMenu();
            _title = new Text("Game Over", titleFont);
            _title.SetColor(new Color(1, 1, 1, 0));
            // Centerre on the x and place somewhere near the top
            _title.SetPosition(-_title.Width / 2, 300);
        }

        private void InitializeMenu()
        {
            _menu = new VerticalMenu(0, 150, _input);

            Color focusColor = new Color(251, 242, 0, 1);
            Color noFocusColor = new Color(0.14f, 0.57f, 0.14f, 1);

            Button startGame = new Button(
                delegate(object o, EventArgs e)
                {
                    _system.ChangeState("start_state");
                },
                new Text("Back To Start", _generalFont), focusColor, noFocusColor);          

            Button exitGame = new Button(
                delegate(object o, EventArgs e)
                {
                    // Quit
                    System.Windows.Forms.Application.Exit();
                },
                new Text("Quit", _generalFont), focusColor, noFocusColor);

            _menu.AddButton(startGame);
            _menu.AddButton(exitGame);

        }

        public void Update(double elapsedTime)
        {
            _menu.HandleInput();
        }

        public void Render()
        {
            Gl.glClearColor(0.00f, 0.00f, 0.00f, 1);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            _renderer.DrawText(_title);
            _menu.Render(_renderer);
            _renderer.Render();
        }
    }
}
