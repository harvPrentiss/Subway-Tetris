using System;
using System.Collections.Generic;
using Engine;
using Tao.OpenGl;
using Engine.Input;

namespace SubwayTetris
{
    class StartMenuState : IGameObject
    {
        Engine.Font _generalFont;
        Input _input;
        VerticalMenu _menu;
        Renderer _renderer = new Renderer();
        Text _title;
        StateSystem _system;
        TextureManager _textureManager;
        List<Block> _menuBlocks;
        BlockManager _blockManager;

        public StartMenuState(Engine.Font titleFont, Engine.Font generalFont, Input input, StateSystem system, TextureManager textureManager)
        {
            _system = system;

            _generalFont = generalFont;
            _input = input;
            _textureManager = textureManager;
            _blockManager = new BlockManager(_textureManager, 0, 0, new Vector(1, 1, 1));
            DropBlocks();
            InitializeMenu();
            _title = new Text("Subway Tetris", titleFont);
            _title.SetColor(new Color(0.85f, 0.85f, 0.10f, 1));
            // Centerre on the x and place somewhere near the top
            _title.SetPosition(-_title.Width / 2, 300); 
        }

        private void InitializeMenu()
        {
            Color focusColor = new Color(251, 242, 0, 1);
            Color noFocusColor = new Color(0.14f, 0.57f, 0.14f, 1);

            _menu = new VerticalMenu(0, 150, _input);
            Button startGame = new Button(
                delegate(object o, EventArgs e)
                {
                    _system.ChangeState("playing_state");
                },
                new Text("Start", _generalFont), focusColor, noFocusColor);          

            Button exitGame = new Button(
                delegate(object o, EventArgs e)
                {
                    // Quit
                    System.Windows.Forms.Application.Exit();
                },
                new Text("Exit", _generalFont), focusColor, noFocusColor);

            _menu.AddButton(startGame);
            _menu.AddButton(exitGame);

        }

        public void Update(double elapsedTime)
        {
            _menu.HandleInput();
        }

        private void DropBlocks()
        {
            _menuBlocks = new List<Block>();
            Vector scalerVector = new Vector(1,1,1);
            // Tomato Block
            Block block = new Block(_textureManager, "tomato_block", scalerVector, new Vector(-335, -100, 0));
            _menuBlocks.Add(block);            
            block = new Block(_textureManager, "tomato_block", scalerVector, new Vector(block.GetPosition().X, block.GetPosition().Y + block.GetHeight(), 0));
            _menuBlocks.Add(block);
            block = new Block(_textureManager, "tomato_block", scalerVector, new Vector(block.GetPosition().X, block.GetPosition().Y + block.GetHeight(), 0));
            _menuBlocks.Add(block);
            block = new Block(_textureManager, "tomato_block", scalerVector, new Vector(block.GetPosition().X + block.GetWidth(), -100, 0));
            _menuBlocks.Add(block);

            //Pickle Block
            block = new Block(_textureManager, "pickle_block", scalerVector, new Vector(block.GetPosition().X + 48, block.GetPosition().Y, 0));
            _menuBlocks.Add(block);
            block = new Block(_textureManager, "pickle_block", scalerVector, new Vector(block.GetPosition().X, block.GetPosition().Y + block.GetHeight(), 0));
            _menuBlocks.Add(block);
            block = new Block(_textureManager, "pickle_block", scalerVector, new Vector(block.GetPosition().X + block.GetWidth(), block.GetPosition().Y, 0));
            _menuBlocks.Add(block);
            block = new Block(_textureManager, "pickle_block", scalerVector, new Vector(block.GetPosition().X, block.GetPosition().Y - block.GetHeight(), 0));
            _menuBlocks.Add(block);

            // Pepper Block
            block = new Block(_textureManager, "pepper_block", scalerVector, new Vector(block.GetPosition().X + 48, -100, 0));
            _menuBlocks.Add(block);
            block = new Block(_textureManager, "pepper_block", scalerVector, new Vector(block.GetPosition().X + block.GetWidth(), -100, 0));
            _menuBlocks.Add(block);
            block = new Block(_textureManager, "pepper_block", scalerVector, new Vector(block.GetPosition().X, block.GetPosition().Y + block.GetWidth(), 0));
            _menuBlocks.Add(block);
            block = new Block(_textureManager, "pepper_block", scalerVector, new Vector(block.GetPosition().X + block.GetWidth(), block.GetPosition().Y - block.GetHeight(), 0));
            _menuBlocks.Add(block);

            // Olive Block
            block = new Block(_textureManager, "olive_block", scalerVector, new Vector(block.GetPosition().X + 48, -100, 0));
            _menuBlocks.Add(block);
            block = new Block(_textureManager, "olive_block", scalerVector, new Vector(block.GetPosition().X + block.GetWidth(), -100, 0));
            _menuBlocks.Add(block);
            block = new Block(_textureManager, "olive_block", scalerVector, new Vector(block.GetPosition().X, block.GetPosition().Y + block.GetHeight(), 0));
            _menuBlocks.Add(block);
            block = new Block(_textureManager, "olive_block", scalerVector, new Vector(block.GetPosition().X + block.GetWidth(), block.GetPosition().Y, 0));
            _menuBlocks.Add(block);

            // Lettuce Block
            block = new Block(_textureManager, "lettuce_block", scalerVector, new Vector(block.GetPosition().X + 48, block.GetPosition().Y, 0));
            _menuBlocks.Add(block);
            block = new Block(_textureManager, "lettuce_block", scalerVector, new Vector(block.GetPosition().X + block.GetWidth(), block.GetPosition().Y, 0));
            _menuBlocks.Add(block);
            block = new Block(_textureManager, "lettuce_block", scalerVector, new Vector(block.GetPosition().X, block.GetPosition().Y - block.GetHeight(), 0));
            _menuBlocks.Add(block);
            block = new Block(_textureManager, "lettuce_block", scalerVector, new Vector(block.GetPosition().X + block.GetWidth(), block.GetPosition().Y, 0));
            _menuBlocks.Add(block);

            // Bread Block
            block = new Block(_textureManager, "breadBotRot_block", scalerVector, new Vector(block.GetPosition().X + 48, block.GetPosition().Y, 0));
            _menuBlocks.Add(block);
            block = new Block(_textureManager, "breadMidRot_block", scalerVector, new Vector(block.GetPosition().X + block.GetWidth(), block.GetPosition().Y, 0));
            _menuBlocks.Add(block);
            block = new Block(_textureManager, "breadMidRot_block", scalerVector, new Vector(block.GetPosition().X + block.GetWidth(), block.GetPosition().Y, 0));
            _menuBlocks.Add(block);
            block = new Block(_textureManager, "breadTopRot_block", scalerVector, new Vector(block.GetPosition().X + block.GetWidth(), block.GetPosition().Y, 0));
            _menuBlocks.Add(block);

            // Cucumber Block
            block = new Block(_textureManager, "cucumber_block", scalerVector, new Vector(block.GetPosition().X + 48, block.GetPosition().Y, 0));
            _menuBlocks.Add(block);
            block = new Block(_textureManager, "cucumber_block", scalerVector, new Vector(block.GetPosition().X + block.GetWidth(), block.GetPosition().Y, 0));
            _menuBlocks.Add(block);
            block = new Block(_textureManager, "cucumber_block", scalerVector, new Vector(block.GetPosition().X, block.GetPosition().Y + block.GetHeight(), 0));
            _menuBlocks.Add(block);
            block = new Block(_textureManager, "cucumber_block", scalerVector, new Vector(block.GetPosition().X, block.GetPosition().Y + +block.GetHeight(), 0));
            _menuBlocks.Add(block);
        }

        public void Render()
        {
            Gl.glClearColor(0.00f, 0.00f, 0.00f, 1);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            foreach (Block block in _menuBlocks)
            {
                block.Render(_renderer);
            }
            _renderer.DrawText(_title);
            _menu.Render(_renderer);
            _renderer.Render();
        }
    }
}
