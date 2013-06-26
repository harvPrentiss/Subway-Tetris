using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Engine;
using Engine.Input;
using Tao.OpenGl;
using Tao.DevIl;

namespace SubwayTetris
{
    public partial class Form1 : Form
    {
        bool _fullscreen = false;
        FastLoop _fastLoop;
        StateSystem _system = new StateSystem();
        Input _input = new Input();
        TextureManager _textureManager = new TextureManager();
        SoundManager _soundManager = new SoundManager();
        Engine.Font _titleFont;
        Engine.Font _generalFont;
        Engine.Font _blockFont;

        public Form1()
        {
            InitializeComponent();
            simpleOpenGlControl1.InitializeContexts();

            _input.Mouse = new Mouse(this, simpleOpenGlControl1);
            _input.Keyboard = new Keyboard(simpleOpenGlControl1);

            InitializeDisplay();
            InitializeSounds();
            InitializeTextures();
            InitializeGameData();
            InitializeFonts();
            InitializeGameState();


            _fastLoop = new FastLoop(GameLoop);
        }

        private void InitializeGameData()
        {
          
        }


        private void InitializeTextures()
        {
            // Init DevIl
            Il.ilInit();
            Ilu.iluInit();
            Ilut.ilutInit();
            Ilut.ilutRenderer(Ilut.ILUT_OPENGL);

            // Textures are loaded here.
            _textureManager.LoadTexture("title_font", "Fonts/title_font.tga");
            _textureManager.LoadTexture("general_font", "Fonts/general_font.tga");
            _textureManager.LoadTexture("block_font", "Fonts/blockFont_0.tga");
            _textureManager.LoadTexture("tetris_block", "Textures/singleTetrisBlock.tga");
            _textureManager.LoadTexture("pickle_block", "Textures/pickle.tga");
            _textureManager.LoadTexture("olive_block", "Textures/olives.tga");
            _textureManager.LoadTexture("tomato_block", "Textures/tomato.tga");
            _textureManager.LoadTexture("cucumber_block", "Textures/cucumber.tga");
            _textureManager.LoadTexture("lettuce_block", "Textures/lettuce.tga");
            _textureManager.LoadTexture("breadBot_block", "Textures/breadBotEnd.tga");
            _textureManager.LoadTexture("breadTop_block", "Textures/breadTopEnd.tga");
            _textureManager.LoadTexture("breadMid_block", "Textures/breadMid.tga");
            _textureManager.LoadTexture("breadBotRot_block", "Textures/breadBotEndRot.tga");
            _textureManager.LoadTexture("breadTopRot_block", "Textures/breadTopEndRot.tga");
            _textureManager.LoadTexture("breadMidRot_block", "Textures/breadMidRot.tga");
            _textureManager.LoadTexture("pepper_block", "Textures/pepper.tga");
        }


        private void InitializeFonts()
        {
            // Fonts are loaded here.
            _titleFont = new Engine.Font(_textureManager.Get("title_font"), Engine.FontParser.Parse("Fonts/title_font.fnt"));
            _generalFont = new Engine.Font(_textureManager.Get("general_font"), Engine.FontParser.Parse("Fonts/general_font.fnt"));
            _blockFont = new Engine.Font(_textureManager.Get("block_font"), Engine.FontParser.Parse("Fonts/blockFont.fnt"));

        }

        private void InitializeSounds()
        {
            // Sounds are loaded here.
        }

        private void InitializeGameState()
        {
            // Game states are loaded here
            _system.AddState("start_state", new StartMenuState(_blockFont, _blockFont, _input, _system, _textureManager));
            _system.AddState("playing_state", new PlayingState(_system, _textureManager, _input, _blockFont, new Vector(256, this.ClientSize.Height / 2, 0), new Vector(ClientSize.Width, ClientSize.Height, 0)));
            _system.AddState("game_over_state", new GameOverState(_blockFont, _blockFont, _input, _system));
            _system.ChangeState("start_state");
            
        }

        private void UpdateInput(double elapsedTime)
        {
            _input.Update(elapsedTime);
        }

        private void GameLoop(double elapsedTime)
        {
            UpdateInput(elapsedTime);
            _system.Update(elapsedTime);
            _system.Render();
            simpleOpenGlControl1.Refresh();
        }

        private void InitializeDisplay()
        {
            if (_fullscreen)
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                Cursor.Hide();
            }
            else
            {
                ClientSize = new Size(1024, 768);
            }
            Setup2DGraphics(ClientSize.Width, ClientSize.Height);
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            Gl.glViewport(0, 0, this.ClientSize.Width, this.ClientSize.Height);
            Setup2DGraphics(ClientSize.Width, ClientSize.Height);
        }

        private void Setup2DGraphics(double width, double height)
        {
            double halfWidth = width / 2;
            double halfHeight = height / 2;
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Gl.glOrtho(-halfWidth, halfWidth, -halfHeight, halfHeight, -100, 100);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
        }

    }

    public enum TetroType { LongBlock = 1, Square = 2, LeftHook = 3, RightHook = 4, LeftZ = 5, RightZ = 6, TBlock= 7 };
}