using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;

namespace SubwayTetris
{
    public class BlockManager
    {
        List<Block> _blocks = new List<Block>();
        List<double> _elimHeights = new List<double>();
        Block _currentBlock;
        Tetromino _currentTetro, _previewTetro, _holdingTetro, _shadowTetro;
        TextureManager _textureManager;
        Vector _scalingVector;
        double _bottomBound, _topBound, _leftBound, _rightBound, _previewLeftBound, _previewBottomBound;
        bool _gameOver = false, _holdAllowed = true;
        int _completedRows = 0, _fullRow;
        public int CompletedRows { get { return _completedRows; } }
        public bool GameOver
        {
            get
            {
                return _gameOver;
            }
            set
            {
                _gameOver = value;
            }
        }
        public List<Block> BlockList
        {
            get
            {
                return _blocks;
            }
        }
        double _blockHeight, _blockWidth;
        public double BlockHeight { get { return _blockHeight; } set { _blockHeight = value; } }
        public double BlockWidth { get { return _blockWidth; } set { _blockWidth = value; } }
        double _blockFallSpeed = 30.0;
        double _blockSpeedIncrement = 0.15;

        public BlockManager(TextureManager manager, double rightSide, double topSide, Vector scaleVector)
        {
            _textureManager = manager;            
            _scalingVector = scaleVector;
            _currentBlock = new Block(_textureManager, "pickle_block", _scalingVector);
            _blockHeight = _currentBlock.GetHeight();
            _blockWidth = _currentBlock.GetWidth();
            _previewBottomBound = (topSide / 2) - _blockHeight * 6;
            _previewLeftBound = (rightSide / 2) - _blockWidth * 5;
            _previewTetro = new Tetromino();
            _currentTetro = new Tetromino();
            _shadowTetro = new Tetromino();
        }

        public void SetBounds(double bottomBound, double topBound, double leftBound, double rightBound)
        {
            _bottomBound = bottomBound;
            _topBound = topBound;
            _leftBound = leftBound;
            _rightBound = rightBound;
            _fullRow = Convert.ToInt32((_rightBound * 2) / _blockWidth);
        }

        // Moves the current Tetro and runs collision, game_over, and scoring checks
        public void Update(double elapsedTime)
        {
            if (_currentTetro != null && _shadowTetro != null)
            {
                _currentTetro.Update(elapsedTime);                
            }

            // Checks for a collision with the bottom of the screen
            if (!BlockBottomCollisionCheck(_currentTetro))
            {
                _currentTetro.Moving = false;
                foreach (Block block in _currentTetro.Blocks)
                {
                    _blocks.Add(block);
                }
                SpawnNewTetro(_previewTetro.Type);
                SpawnNewPreviewTetro();
                _holdAllowed = true;
                return;
            }

            // Checks for a collision with other blocks
            if (!BlockCollisionCheck(_currentTetro, true))
            {
                _currentTetro.Moving = false;
                foreach (Block block in _currentTetro.Blocks)
                {
                    _blocks.Add(block);
                }
                _gameOver = GameOverCheck();
                if (!_gameOver)
                {
                    SpawnNewTetro(_previewTetro.Type);
                    SpawnNewPreviewTetro();
                    _holdAllowed = true;
                }
            }
            CompletedRowCheck();
        }

        // Draws the current Tetro that is moving and all the blocks in the field of play
        public void Render(Renderer renderer)
        {
            _currentTetro.Render(renderer, _topBound );
            _previewTetro.Render(renderer);
            _shadowTetro.Render(renderer);
            if (_holdingTetro != null)
            {
                _holdingTetro.Render(renderer);
            }
            _blocks.ForEach(x => x.Render(renderer));
        }

        #region Collision Detection

        // Checks to see if the tetro hit the bottom of the play area
        private bool BlockBottomCollisionCheck(Tetromino tetro)
        {            
            foreach (Block testBlock in tetro.Blocks)
            {
                if (testBlock != null)
                {
                    if (testBlock.GetBoundingBox().Bottom < _bottomBound + testBlock.GetHeight())
                    {
                        // Gets the difference in space between where the colliding block is and the bottom. Moves the whole tetro back
                        // by that amount to set it in the correct place.
                        double position = -1 * ((testBlock.GetPosition().Y - (testBlock.GetHeight() /2)) - _bottomBound);
                        foreach (Block block in tetro.Blocks)
                        {
                            block.Readjust(new Vector(0, 1, 0), position);                            
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        // Checks to see if the tetro is colliding with the sides of play area
        private bool BlockSideCollisionCheck(Tetromino tetro)
        {
            foreach (Block testBlock in tetro.Blocks)
            {
                if (testBlock != null)
                {
                    if (testBlock.GetBoundingBox().Left < _leftBound)
                    {
                        // Gets the difference in space between where the colliding block is and the left.
                        double position = -(testBlock.GetPosition().X  + (_blockWidth / 2) - _leftBound);
                        foreach (Block block in tetro.Blocks)
                        {
                            block.Readjust(new Vector(1, 0, 0), position);
                        }
                        return false;
                    }

                    if (testBlock.GetBoundingBox().Right > _rightBound)
                    {
                        // Gets the difference in space between where the colliding block is and the right.
                        double position = -(testBlock.GetPosition().X - (_blockWidth / 2) - _rightBound);
                        foreach (Block block in tetro.Blocks)
                        {
                            block.Readjust(new Vector(1, 0, 0), position);
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        // Checks to see if the tetro it hit another block
        private bool BlockCollisionCheck(Tetromino tetro, bool fallingBlock)
        {            
            foreach (Block testBlock in tetro.Blocks)
            {                
                if (_blocks.Count != 0 && testBlock != null)
                {
                    foreach (Block block in _blocks)
                    {
                        if (testBlock.GetBoundingBox().IntersectsWith(block.GetBoundingBox()))
                        {
                            if (fallingBlock)
                            {
                                // Gets the difference in space between where the colliding block is and the block it hit. Moves the whole tetro back
                                // by that amount to set it in the correct place.
                                double position = -1 * ((testBlock.GetPosition().Y - (testBlock.GetHeight() / 2)) - (block.GetPosition().Y + (block.GetHeight() / 2)));
                                foreach (Block tetroBlock in tetro.Blocks)
                                {
                                    tetroBlock.Readjust(new Vector(0, 1, 0), position);
                                }
                            }
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        #endregion

        #region Game Functions

        // Checks to see if the last block is above the play field if so the game is over
        public bool GameOverCheck()
        {
            //Checks to see if the blocks reached the top of the screen
            if (_blocks[_blocks.Count-1].GetBoundingBox().Top > _topBound - _blocks[_blocks.Count-1].GetHeight())
            {
                return true;
            }
            return false;
        }

        // Resets the _block List, _gamOver bool, _currentBlock, and _currentTetro
        public void Reset()
        {
            _gameOver = false;
            _blocks.Clear();
            _completedRows = 0;
            _currentBlock = new Block();
            _currentTetro = new Tetromino();
        }

        #endregion

        #region Block Spawning

        // Creates a new block that is used to build a tetromino. Stored in _currentBlock
        public void SpawnNewBlock(string blockType, string callerType, Color color)
        {
            _currentBlock = new Block(_textureManager, blockType);
            _currentBlock.SetColor(color);
            if (callerType == "active")
            {
                _currentBlock.SetPosition(_blockWidth / 2, _topBound + _currentBlock.GetHeight());
            }
            if (callerType == "preview")
            {
                _currentBlock.SetPosition(_previewLeftBound + ((_blockWidth * 5) / 2), _previewBottomBound + (_blockHeight * 2));
            }
            if (callerType == "holding")
            {
                _currentBlock.SetPosition(-(_previewLeftBound) - ((_blockWidth * 5) / 2), _previewBottomBound + (_blockHeight * 2));
            }
            _currentBlock.DropSpeed = _blockFallSpeed;
        }

        public void SpawnNewBlock(string blockType, string callerType)
        {
            _currentBlock = new Block(_textureManager, blockType, _scalingVector);
            if (callerType == "active")
            {
                _currentBlock.SetPosition(_blockWidth / 2, _topBound + _currentBlock.GetHeight());
            }
            if (callerType == "preview")
            {
                _currentBlock.SetPosition(_previewLeftBound + ((_blockWidth * 5) / 2), _previewBottomBound + (_blockHeight * 2));
            }
            if (callerType == "holding")
            {
                _currentBlock.SetPosition(-(_previewLeftBound) - ((_blockWidth * 5) / 2), _previewBottomBound + (_blockHeight * 2));
            }
            _currentBlock.DropSpeed = _blockFallSpeed;
        }

        #endregion

        #region Tetro Construction

        // Spawns a new tetromino just off the top of the screen. Stored in _currentTetro
        public void SpawnNewTetro(TetroType type)
        {
            Color color = new Color(1, 1, 1, 1);
            _currentTetro = new Tetromino(type);
            string blockType = "";
            switch (_currentTetro.Type)
            {
                case TetroType.LongBlock:
                    {
                        /*
                         *         ****
                         **/
                        color.Red = 1.0f;
                        color.Green = 0.2f;
                        color.Blue = 0.0f;
                        blockType = "breadBot_block";
                        SpawnNewBlock(blockType, "active");
                        _currentTetro.Blocks.Add(_currentBlock);
                        blockType = "breadMid_block";
                        SpawnNewBlock(blockType, "active");
                        _currentBlock.SetPosition(_blockWidth / 2, _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _currentTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "active");
                        _currentBlock.SetPosition(_blockWidth / 2, _currentBlock.GetPosition().Y + (2 * _currentBlock.GetHeight()));
                        _currentTetro.Blocks.Add(_currentBlock);
                        blockType = "breadTop_block";
                        SpawnNewBlock(blockType, "active");
                        _currentBlock.SetPosition(_blockWidth / 2, _currentBlock.GetPosition().Y + (3 * _currentBlock.GetHeight()));
                        _currentTetro.Blocks.Add(_currentBlock);
                        break;
                    }
                case TetroType.Square:
                    {
                        /*
                         *         **
                         *         **
                         **/
                        color.Red = 1.0f;
                        color.Green = 0.0f;
                        color.Blue = 0.0f;
                        blockType = "pickle_block";
                        SpawnNewBlock(blockType, "active");
                        _currentTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "active");
                        _currentBlock.SetPosition(_blockWidth / 2, _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _currentTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "active");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X + _currentBlock.GetWidth(), _currentBlock.GetPosition().Y);
                        _currentTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "active");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X + _currentBlock.GetWidth(), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _currentTetro.Blocks.Add(_currentBlock);
                        break;
                    }
                case TetroType.LeftHook:
                    {
                        /*
                         *         **
                         *          *
                         *          *
                         **/
                        color.Red = 0.0f;
                        color.Green = 1.0f;
                        color.Blue = 0.0f;
                        blockType = "cucumber_block";
                        SpawnNewBlock(blockType, "active");
                        _currentTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "active");
                        _currentBlock.SetPosition(_blockWidth / 2, _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _currentTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "active");
                        _currentBlock.SetPosition(_blockWidth / 2, _currentBlock.GetPosition().Y + (2 * _currentBlock.GetHeight()));
                        _currentTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "active");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X - _currentBlock.GetWidth(), _currentBlock.GetPosition().Y + (2 * _currentBlock.GetHeight()));
                        _currentTetro.Blocks.Add(_currentBlock);
                        break;
                    }
                case TetroType.RightHook:
                    {
                        /*
                         *         **
                         *         *
                         *         *
                         **/
                        color.Red = 0.0f;
                        color.Green = 0.0f;
                        color.Blue = 1.0f;
                        blockType = "tomato_block";
                        SpawnNewBlock(blockType, "active");
                        _currentTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "active");
                        _currentBlock.SetPosition(_blockWidth / 2, _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _currentTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "active");
                        _currentBlock.SetPosition(_blockWidth / 2, _currentBlock.GetPosition().Y + (2 * _currentBlock.GetHeight()));
                        _currentTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "active");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X + _currentBlock.GetWidth(), _currentBlock.GetPosition().Y + (2 * _currentBlock.GetHeight()));
                        _currentTetro.Blocks.Add(_currentBlock);
                        break;
                    }
                case TetroType.RightZ:
                    {
                        /*
                         *         **
                         *          **
                         **/
                        color.Red = 0.5f;
                        color.Green = 0.0f;
                        color.Blue = 0.5f;
                        blockType = "olive_block";
                        SpawnNewBlock(blockType, "active");
                        _currentTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "active");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X + _currentBlock.GetWidth(), _currentBlock.GetPosition().Y);
                        _currentTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "active");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X + _currentBlock.GetWidth(), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _currentTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "active");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X + (2 * _currentBlock.GetWidth()), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _currentTetro.Blocks.Add(_currentBlock);
                        break;
                    }
                case TetroType.LeftZ:
                    {
                        /*
                         *         **
                         *        **
                         **/
                        color.Red = 1.0f;
                        color.Green = 1.0f;
                        color.Blue = 0f;
                        blockType = "lettuce_block";
                        SpawnNewBlock(blockType, "active");
                        _currentTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "active");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X - _currentBlock.GetWidth(), _currentBlock.GetPosition().Y);
                        _currentTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "active");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X - _currentBlock.GetWidth(), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _currentTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "active");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X - (2 * _currentBlock.GetWidth()), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _currentTetro.Blocks.Add(_currentBlock);
                        break;
                    }
                case TetroType.TBlock:
                    {
                        /*
                         *      ***
                         *       *
                         **/
                        color.Red = 0.5f;
                        color.Green = 0.5f;
                        color.Blue = 0.5f;
                        blockType = "pepper_block";
                        SpawnNewBlock(blockType, "active");
                        _currentTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "active");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X, _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _currentTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "active");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X - _currentBlock.GetWidth(), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _currentTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "active");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X + _currentBlock.GetWidth(), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _currentTetro.Blocks.Add(_currentBlock);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            _currentTetro.Moving = true;
            SpawnNewShadowTetro();
        }

        // Spawns a new tetromino just for preview. Stored in _previewTetro
        public void SpawnNewPreviewTetro()
        {
            Random rand = new Random();
            Color color = new Color(1, 1, 1, 1);
            int tetro = rand.Next(0, 7);
            string blockType = "";
            if (tetro == 0)
            {
                _previewTetro = new Tetromino(TetroType.LongBlock);
            }
            if (tetro == 1)
            {
                _previewTetro = new Tetromino(TetroType.Square);
            }
            if (tetro == 2)
            {
                _previewTetro = new Tetromino(TetroType.LeftZ);
            }
            if (tetro == 3)
            {
                _previewTetro = new Tetromino(TetroType.RightZ);
            }
            if (tetro == 4)
            {
                _previewTetro = new Tetromino(TetroType.LeftHook);
            }
            if (tetro == 5)
            {
                _previewTetro = new Tetromino(TetroType.RightHook);
            }
            if (tetro == 6)
            {
                _previewTetro = new Tetromino(TetroType.TBlock);
            }

            switch (_previewTetro.Type)
            {
                case TetroType.LongBlock:
                    {
                        /*
                         *         *
                         *         *
                         *         *
                         *         *
                         **/
                        color.Red = 1.0f;
                        color.Green = 0.2f;
                        color.Blue = 0.0f;
                        blockType = "breadBot_block";
                        SpawnNewBlock(blockType, "preview");
                        _previewTetro.Blocks.Add(_currentBlock);
                        blockType = "breadMid_block";
                        SpawnNewBlock(blockType, "preview");
                        _currentBlock.SetPosition(_previewLeftBound + ((_blockWidth * 5) /2), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _previewTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "preview");
                        _currentBlock.SetPosition(_previewLeftBound + ((_blockWidth * 5) /2), _currentBlock.GetPosition().Y + (2 * _currentBlock.GetHeight()));
                        _previewTetro.Blocks.Add(_currentBlock);
                        blockType = "breadTop_block";
                        SpawnNewBlock(blockType, "preview");
                        _currentBlock.SetPosition(_previewLeftBound + ((_blockWidth * 5) /2), _currentBlock.GetPosition().Y + (3 * _currentBlock.GetHeight()));
                        _previewTetro.Blocks.Add(_currentBlock);
                        break;
                    }
                case TetroType.Square:
                    {
                        /*
                         *         **
                         *         **
                         **/
                        color.Red = 1.0f;
                        color.Green = 0.0f;
                        color.Blue = 0.0f;
                        blockType = "pickle_block";
                        SpawnNewBlock(blockType, "preview");
                        _previewTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "preview");
                        _currentBlock.SetPosition(_previewLeftBound + ((_blockWidth * 5) /2), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _previewTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "preview");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X + _currentBlock.GetWidth(), _currentBlock.GetPosition().Y);
                        _previewTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "preview");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X + _currentBlock.GetWidth(), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _previewTetro.Blocks.Add(_currentBlock);
                        break;
                    }
                case TetroType.LeftHook:
                    {
                        /*
                         *         **
                         *          *
                         *          *
                         **/
                        color.Red = 0.0f;
                        color.Green = 1.0f;
                        color.Blue = 0.0f;
                        blockType = "cucumber_block";
                        SpawnNewBlock(blockType, "preview");
                        _previewTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "preview");
                        _currentBlock.SetPosition(_previewLeftBound + ((_blockWidth * 5) /2), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _previewTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "preview");
                        _currentBlock.SetPosition(_previewLeftBound + ((_blockWidth * 5) /2), _currentBlock.GetPosition().Y + (2 * _currentBlock.GetHeight()));
                        _previewTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "preview");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X - _currentBlock.GetWidth(), _currentBlock.GetPosition().Y + (2 * _currentBlock.GetHeight()));
                        _previewTetro.Blocks.Add(_currentBlock);
                        break;
                    }
                case TetroType.RightHook:
                    {
                        /*
                         *         **
                         *         *
                         *         *
                         **/
                        color.Red = 0.0f;
                        color.Green = 0.0f;
                        color.Blue = 1.0f;
                        blockType = "tomato_block";
                        SpawnNewBlock(blockType, "preview");
                        _previewTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "preview");
                        _currentBlock.SetPosition(_previewLeftBound + ((_blockWidth * 5) /2), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _previewTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "preview");
                        _currentBlock.SetPosition(_previewLeftBound + ((_blockWidth * 5) /2), _currentBlock.GetPosition().Y + (2 * _currentBlock.GetHeight()));
                        _previewTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "preview");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X + _currentBlock.GetWidth(), _currentBlock.GetPosition().Y + (2 * _currentBlock.GetHeight()));
                        _previewTetro.Blocks.Add(_currentBlock);
                        break;
                    }
                case TetroType.RightZ:
                    {
                        /*
                         *         **
                         *          **
                         **/
                        color.Red = 0.5f;
                        color.Green = 0.0f;
                        color.Blue = 0.5f;
                        blockType = "olive_block";
                        SpawnNewBlock(blockType, "preview");
                        _previewTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "preview");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X + _currentBlock.GetWidth(), _currentBlock.GetPosition().Y);
                        _previewTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "preview");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X + _currentBlock.GetWidth(), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _previewTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "preview");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X + (2 * _currentBlock.GetWidth()), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _previewTetro.Blocks.Add(_currentBlock);
                        break;
                    }
                case TetroType.LeftZ:
                    {
                        /*
                         *         **
                         *        **
                         **/
                        color.Red = 1.0f;
                        color.Green = 1.0f;
                        color.Blue = 0f;
                        blockType = "lettuce_block";
                        SpawnNewBlock(blockType, "preview");
                        _previewTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "preview");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X - _currentBlock.GetWidth(), _currentBlock.GetPosition().Y);
                        _previewTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "preview");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X - _currentBlock.GetWidth(), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _previewTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "preview");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X - (2 * _currentBlock.GetWidth()), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _previewTetro.Blocks.Add(_currentBlock);
                        break;
                    }
                case TetroType.TBlock:
                    {
                        /*
                         *      ***
                         *       *
                         **/
                        color.Red = 0.5f;
                        color.Green = 0.5f;
                        color.Blue = 0.5f;
                        blockType = "pepper_block";
                        SpawnNewBlock(blockType, "preview");
                        _previewTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "preview");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X, _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _previewTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "preview");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X - _currentBlock.GetWidth(), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _previewTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "preview");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X + _currentBlock.GetWidth(), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _previewTetro.Blocks.Add(_currentBlock);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            _previewTetro.Moving = false;
        }
        
        // Spawns a new tetromino just for holding. Stored in _holdingTetro
        public void SpawnNewHoldingTetro(TetroType type)
        {
            Color color = new Color(1, 1, 1, 1);
            _holdingTetro = new Tetromino(type);
            string blockType = "";
            switch (_holdingTetro.Type)
            {
                case TetroType.LongBlock:
                    {
                        /*
                         *         *
                         *         *
                         *         *
                         *         *
                         **/
                        color.Red = 1.0f;
                        color.Green = 0.2f;
                        color.Blue = 0.0f;
                        blockType = "breadBot_block";
                        SpawnNewBlock(blockType, "holding");
                        _holdingTetro.Blocks.Add(_currentBlock);
                        blockType = "breadMid_block";
                        SpawnNewBlock(blockType, "holding");
                        _currentBlock.SetPosition(-_previewLeftBound - ((_blockWidth * 5) / 2), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _holdingTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "holding");
                        _currentBlock.SetPosition(-_previewLeftBound - ((_blockWidth * 5) / 2), _currentBlock.GetPosition().Y + (2 * _currentBlock.GetHeight()));
                        _holdingTetro.Blocks.Add(_currentBlock);
                        blockType = "breadTop_block";
                        SpawnNewBlock(blockType, "holding");
                        _currentBlock.SetPosition(-_previewLeftBound - ((_blockWidth * 5) / 2), _currentBlock.GetPosition().Y + (3 * _currentBlock.GetHeight()));
                        _holdingTetro.Blocks.Add(_currentBlock);
                        break;
                    }
                case TetroType.Square:
                    {
                        /*
                         *         **
                         *         **
                         **/
                        color.Red = 1.0f;
                        color.Green = 0.0f;
                        color.Blue = 0.0f;
                        blockType = "pickle_block";
                        SpawnNewBlock(blockType, "holding");
                        _holdingTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "holding");
                        _currentBlock.SetPosition(-_previewLeftBound - ((_blockWidth * 5) / 2), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _holdingTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "holding");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X - _currentBlock.GetWidth(), _currentBlock.GetPosition().Y);
                        _holdingTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "holding");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X - _currentBlock.GetWidth(), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _holdingTetro.Blocks.Add(_currentBlock);
                        break;
                    }
                case TetroType.LeftHook:
                    {
                        /*
                         *         **
                         *          *
                         *          *
                         **/
                        color.Red = 0.0f;
                        color.Green = 1.0f;
                        color.Blue = 0.0f;
                        blockType = "cucumber_block";
                        SpawnNewBlock(blockType, "holding");
                        _holdingTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "holding");
                        _currentBlock.SetPosition(-_previewLeftBound - ((_blockWidth * 5) / 2), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _holdingTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "holding");
                        _currentBlock.SetPosition(-_previewLeftBound - ((_blockWidth * 5) / 2), _currentBlock.GetPosition().Y + (2 * _currentBlock.GetHeight()));
                        _holdingTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "holding");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X - _currentBlock.GetWidth(), _currentBlock.GetPosition().Y + (2 * _currentBlock.GetHeight()));
                        _holdingTetro.Blocks.Add(_currentBlock);
                        break;
                    }
                case TetroType.RightHook:
                    {
                        /*
                         *         **
                         *         *
                         *         *
                         **/
                        color.Red = 0.0f;
                        color.Green = 0.0f;
                        color.Blue = 1.0f;
                        blockType = "tomato_block";
                        SpawnNewBlock(blockType, "holding");
                        _holdingTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "holding");
                        _currentBlock.SetPosition(-_previewLeftBound - ((_blockWidth * 5) / 2), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _holdingTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "holding");
                        _currentBlock.SetPosition(-_previewLeftBound - ((_blockWidth * 5) / 2), _currentBlock.GetPosition().Y + (2 * _currentBlock.GetHeight()));
                        _holdingTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "holding");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X + _currentBlock.GetWidth(), _currentBlock.GetPosition().Y + (2 * _currentBlock.GetHeight()));
                        _holdingTetro.Blocks.Add(_currentBlock);
                        break;
                    }
                case TetroType.RightZ:
                    {
                        /*
                         *         **
                         *          **
                         **/
                        color.Red = 0.5f;
                        color.Green = 0.0f;
                        color.Blue = 0.5f;
                        blockType = "olive_block";
                        SpawnNewBlock(blockType, "holding");
                        _holdingTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "holding");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X + _currentBlock.GetWidth(), _currentBlock.GetPosition().Y);
                        _holdingTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "holding");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X + _currentBlock.GetWidth(), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _holdingTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "holding");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X + (2 * _currentBlock.GetWidth()), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _holdingTetro.Blocks.Add(_currentBlock);
                        break;
                    }
                case TetroType.LeftZ:
                    {
                        /*
                         *         **
                         *        **
                         **/
                        color.Red = 1.0f;
                        color.Green = 1.0f;
                        color.Blue = 0f;
                        blockType = "lettuce_block";
                        SpawnNewBlock(blockType, "holding");
                        _holdingTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "holding");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X - _currentBlock.GetWidth(), _currentBlock.GetPosition().Y);
                        _holdingTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "holding");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X - _currentBlock.GetWidth(), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _holdingTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "holding");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X - (2 * _currentBlock.GetWidth()), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _holdingTetro.Blocks.Add(_currentBlock);
                        break;
                    }
                case TetroType.TBlock:
                    {
                        /*
                         *      ***
                         *       *
                         **/
                        color.Red = 0.5f;
                        color.Green = 0.5f;
                        color.Blue = 0.5f;
                        blockType = "pepper_block";
                        SpawnNewBlock(blockType, "holding");
                        _holdingTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "holding");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X, _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _holdingTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "holding");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X - _currentBlock.GetWidth(), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _holdingTetro.Blocks.Add(_currentBlock);
                        SpawnNewBlock(blockType, "holding");
                        _currentBlock.SetPosition(_currentBlock.GetPosition().X + _currentBlock.GetWidth(), _currentBlock.GetPosition().Y + _currentBlock.GetHeight());
                        _holdingTetro.Blocks.Add(_currentBlock);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            _holdingTetro.Moving = false;
        }        

        // Spawns a new tetromino just for shadow. Stored in _shadowTetrp
        public void SpawnNewShadowTetro()
        {
            _shadowTetro = new Tetromino(_currentTetro.Type);
            foreach(Block block in _currentTetro.Blocks)
            {                
                _shadowTetro.Blocks.Add(new Block(block.GetTexture(), _scalingVector, new Vector(block.GetPosition().X, block.GetPosition().Y, 0)));
            }
            _shadowTetro.MakeShadow();
            DropBlock(new Vector(0, -1, 0), _shadowTetro);
        }

        #endregion

        #region Move, Rotate, and Drop, Hold Tetro Logic

        // Moves the current block for the given movement vector
        public void MoveCurrentBlock(Vector movement)
        {
            bool goodMove = true;
            foreach (Block block in _currentTetro.Blocks)
            {
                block.Move(movement);
            }
            if (!BlockCollisionCheck(_currentTetro, false) || !BlockBottomCollisionCheck(_currentTetro) || !BlockSideCollisionCheck(_currentTetro))
                {
                    goodMove = false;
                }
            if (!goodMove)
            {
                movement *= -1;
                foreach (Block block in _currentTetro.Blocks)
                {
                    block.Move(movement);
                }
            }
            for (int i = 0; i < _shadowTetro.Blocks.Count; i++)
            {
                _shadowTetro.Blocks[i].SetPosition(_currentTetro.Blocks[i].GetPosition());
            }
            DropBlock(new Vector(0, -1, 0), _shadowTetro);
        }

        public void MoveCurrentBlockSmooth(Vector movement)
        {
            if (_currentTetro.MoveDelay == 0)
            {
                bool goodMove = true;
                foreach (Block block in _currentTetro.Blocks)
                {
                    block.Move(movement);
                }
                if (!BlockCollisionCheck(_currentTetro, false) || !BlockBottomCollisionCheck(_currentTetro) || !BlockSideCollisionCheck(_currentTetro))
                {
                    goodMove = false;
                }
                if (!goodMove)
                {
                    movement *= -1;
                    foreach (Block block in _currentTetro.Blocks)
                    {
                        block.Move(movement);
                    }
                }
                _currentTetro.ResetDelay();
            }
        }

        // Drops the active Tetro
        public void DropBlock(Vector movement)
        {
            while (BlockCollisionCheck(_currentTetro, true) && BlockBottomCollisionCheck(_currentTetro))
            {
                foreach (Block block in _currentTetro.Blocks)
                {
                    block.Move(movement);
                }
            }
        }

        // Drops a specified Tetro
        public void DropBlock(Vector movement, Tetromino tetro)
        {
            while (BlockCollisionCheck(tetro, true) && BlockBottomCollisionCheck(tetro))
            {
                foreach (Block block in tetro.Blocks)
                {
                    block.Move(movement);
                }
            }
        }    

        // Rotates the current Tetro
        public void RotateBlock()
        {
            switch (_currentTetro.Type)
            {
                case TetroType.LongBlock:
                    {                        
                        if (_currentTetro.Rotation == 0)
                        {
                            _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X + _blockWidth, _currentTetro.Blocks[0].GetPosition().Y + _blockHeight);
                            _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X - _blockWidth, _currentTetro.Blocks[2].GetPosition().Y - _blockHeight);
                            _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X - (2 * _blockWidth), _currentTetro.Blocks[3].GetPosition().Y - (2 * _blockHeight));
                            _currentTetro.Rotation += 90;
                            _currentTetro.Blocks[0] = new Block(_textureManager, "breadTopRot_block", _scalingVector, new Vector(_currentTetro.Blocks[0].GetPosition().X, _currentTetro.Blocks[0].GetPosition().Y,0));
                            _currentTetro.Blocks[1] = new Block(_textureManager, "breadMidRot_block", _scalingVector, new Vector(_currentTetro.Blocks[1].GetPosition().X, _currentTetro.Blocks[1].GetPosition().Y, 0));
                            _currentTetro.Blocks[2] = new Block(_textureManager, "breadMidRot_block", _scalingVector, new Vector(_currentTetro.Blocks[2].GetPosition().X, _currentTetro.Blocks[2].GetPosition().Y, 0));
                            _currentTetro.Blocks[3] = new Block(_textureManager, "breadBotRot_block", _scalingVector, new Vector(_currentTetro.Blocks[3].GetPosition().X, _currentTetro.Blocks[3].GetPosition().Y, 0));
                            _shadowTetro.Blocks[0] = new Block(_textureManager, "breadTopRot_block", _scalingVector, new Vector(_currentTetro.Blocks[0].GetPosition().X, _currentTetro.Blocks[0].GetPosition().Y, 0));
                            _shadowTetro.Blocks[1] = new Block(_textureManager, "breadMidRot_block", _scalingVector, new Vector(_currentTetro.Blocks[1].GetPosition().X, _currentTetro.Blocks[1].GetPosition().Y, 0));
                            _shadowTetro.Blocks[2] = new Block(_textureManager, "breadMidRot_block", _scalingVector, new Vector(_currentTetro.Blocks[2].GetPosition().X, _currentTetro.Blocks[2].GetPosition().Y, 0));
                            _shadowTetro.Blocks[3] = new Block(_textureManager, "breadBotRot_block", _scalingVector, new Vector(_currentTetro.Blocks[3].GetPosition().X, _currentTetro.Blocks[3].GetPosition().Y, 0));
                            _shadowTetro.MakeShadow();
                            DropBlock(new Vector(0, -1, 0), _shadowTetro);
                            if (!BlockCollisionCheck(_currentTetro, false) || !BlockSideCollisionCheck(_currentTetro))
                            {
                                _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X - _blockWidth, _currentTetro.Blocks[0].GetPosition().Y - _blockHeight);
                                _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X + _blockWidth, _currentTetro.Blocks[2].GetPosition().Y + _blockHeight);
                                _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X + (2 * _blockWidth), _currentTetro.Blocks[3].GetPosition().Y + (2 * _blockHeight));
                                _currentTetro.Rotation -= 90;
                                _currentTetro.Blocks[0] = new Block(_textureManager, "breadBot_block", _scalingVector, new Vector(_currentTetro.Blocks[0].GetPosition().X, _currentTetro.Blocks[0].GetPosition().Y, 0));
                                _currentTetro.Blocks[1] = new Block(_textureManager, "breadMid_block", _scalingVector, new Vector(_currentTetro.Blocks[1].GetPosition().X, _currentTetro.Blocks[1].GetPosition().Y, 0));
                                _currentTetro.Blocks[2] = new Block(_textureManager, "breadMid_block", _scalingVector, new Vector(_currentTetro.Blocks[2].GetPosition().X, _currentTetro.Blocks[2].GetPosition().Y, 0));
                                _currentTetro.Blocks[3] = new Block(_textureManager, "breadTop_block", _scalingVector, new Vector(_currentTetro.Blocks[3].GetPosition().X, _currentTetro.Blocks[3].GetPosition().Y, 0));
                                _shadowTetro.Blocks[0] = new Block(_textureManager, "breadBot_block", _scalingVector, new Vector(_currentTetro.Blocks[0].GetPosition().X, _currentTetro.Blocks[0].GetPosition().Y, 0));
                                _shadowTetro.Blocks[1] = new Block(_textureManager, "breadMid_block", _scalingVector, new Vector(_currentTetro.Blocks[1].GetPosition().X, _currentTetro.Blocks[1].GetPosition().Y, 0));
                                _shadowTetro.Blocks[2] = new Block(_textureManager, "breadMid_block", _scalingVector, new Vector(_currentTetro.Blocks[2].GetPosition().X, _currentTetro.Blocks[2].GetPosition().Y, 0));
                                _shadowTetro.Blocks[3] = new Block(_textureManager, "breadTop_block", _scalingVector, new Vector(_currentTetro.Blocks[3].GetPosition().X, _currentTetro.Blocks[3].GetPosition().Y, 0));
                                _shadowTetro.MakeShadow();
                                DropBlock(new Vector(0, -1, 0), _shadowTetro);
                            }                            
                        }
                        else
                        {
                            _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X - _blockWidth, _currentTetro.Blocks[0].GetPosition().Y - _blockHeight);
                            _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X + _blockWidth, _currentTetro.Blocks[2].GetPosition().Y + _blockHeight);
                            _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X + (2 * _blockWidth), _currentTetro.Blocks[3].GetPosition().Y + (2 * _blockHeight));
                            _currentTetro.Rotation -= 90;
                            _currentTetro.Blocks[0] = new Block(_textureManager, "breadBot_block", _scalingVector, new Vector(_currentTetro.Blocks[0].GetPosition().X, _currentTetro.Blocks[0].GetPosition().Y, 0));
                            _currentTetro.Blocks[1] = new Block(_textureManager, "breadMid_block", _scalingVector, new Vector(_currentTetro.Blocks[1].GetPosition().X, _currentTetro.Blocks[1].GetPosition().Y, 0));
                            _currentTetro.Blocks[2] = new Block(_textureManager, "breadMid_block", _scalingVector, new Vector(_currentTetro.Blocks[2].GetPosition().X, _currentTetro.Blocks[2].GetPosition().Y, 0));
                            _currentTetro.Blocks[3] = new Block(_textureManager, "breadTop_block", _scalingVector, new Vector(_currentTetro.Blocks[3].GetPosition().X, _currentTetro.Blocks[3].GetPosition().Y, 0));
                            _shadowTetro.Blocks[0] = new Block(_textureManager, "breadBot_block", _scalingVector, new Vector(_currentTetro.Blocks[0].GetPosition().X, _currentTetro.Blocks[0].GetPosition().Y, 0));
                            _shadowTetro.Blocks[1] = new Block(_textureManager, "breadMid_block", _scalingVector, new Vector(_currentTetro.Blocks[1].GetPosition().X, _currentTetro.Blocks[1].GetPosition().Y, 0));
                            _shadowTetro.Blocks[2] = new Block(_textureManager, "breadMid_block", _scalingVector, new Vector(_currentTetro.Blocks[2].GetPosition().X, _currentTetro.Blocks[2].GetPosition().Y, 0));
                            _shadowTetro.Blocks[3] = new Block(_textureManager, "breadTop_block", _scalingVector, new Vector(_currentTetro.Blocks[3].GetPosition().X, _currentTetro.Blocks[3].GetPosition().Y, 0));
                            _shadowTetro.MakeShadow();
                            DropBlock(new Vector(0, -1, 0), _shadowTetro);
                            if (!BlockCollisionCheck(_currentTetro, false) || !BlockSideCollisionCheck(_currentTetro))
                            {
                                _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X + _blockWidth, _currentTetro.Blocks[0].GetPosition().Y + _blockHeight);
                                _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X - _blockWidth, _currentTetro.Blocks[2].GetPosition().Y - _blockHeight);
                                _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X - (2 * _blockWidth), _currentTetro.Blocks[3].GetPosition().Y - (2 * _blockHeight));
                                _currentTetro.Rotation += 90;
                                _currentTetro.Blocks[0] = new Block(_textureManager, "breadTopRot_block", _scalingVector, new Vector(_currentTetro.Blocks[0].GetPosition().X, _currentTetro.Blocks[0].GetPosition().Y, 0));
                                _currentTetro.Blocks[1] = new Block(_textureManager, "breadMidRot_block", _scalingVector, new Vector(_currentTetro.Blocks[1].GetPosition().X, _currentTetro.Blocks[1].GetPosition().Y, 0));
                                _currentTetro.Blocks[2] = new Block(_textureManager, "breadMidRot_block", _scalingVector, new Vector(_currentTetro.Blocks[2].GetPosition().X, _currentTetro.Blocks[2].GetPosition().Y, 0));
                                _currentTetro.Blocks[3] = new Block(_textureManager, "breadBotRot_block", _scalingVector, new Vector(_currentTetro.Blocks[3].GetPosition().X, _currentTetro.Blocks[3].GetPosition().Y, 0));
                                _shadowTetro.Blocks[0] = new Block(_textureManager, "breadTopRot_block", _scalingVector, new Vector(_currentTetro.Blocks[0].GetPosition().X, _currentTetro.Blocks[0].GetPosition().Y, 0));
                                _shadowTetro.Blocks[1] = new Block(_textureManager, "breadMidRot_block", _scalingVector, new Vector(_currentTetro.Blocks[1].GetPosition().X, _currentTetro.Blocks[1].GetPosition().Y, 0));
                                _shadowTetro.Blocks[2] = new Block(_textureManager, "breadMidRot_block", _scalingVector, new Vector(_currentTetro.Blocks[2].GetPosition().X, _currentTetro.Blocks[2].GetPosition().Y, 0));
                                _shadowTetro.Blocks[3] = new Block(_textureManager, "breadBotRot_block", _scalingVector, new Vector(_currentTetro.Blocks[3].GetPosition().X, _currentTetro.Blocks[3].GetPosition().Y, 0));
                                _shadowTetro.MakeShadow();
                                DropBlock(new Vector(0, -1, 0), _shadowTetro);
                            }
                            
                        }
                        foreach (Block block in _currentTetro.Blocks)
                        {
                            block.SetScale(_scalingVector.X, _scalingVector.Y);
                        }
                        break;
                    }
                case TetroType.LeftHook:
                    {
                        if (_currentTetro.Rotation == 0)
                        {
                            _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X - _blockWidth, _currentTetro.Blocks[0].GetPosition().Y + _blockHeight);
                            _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X + _blockWidth, _currentTetro.Blocks[2].GetPosition().Y - _blockHeight);
                            _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X + (2 * _blockWidth), _currentTetro.Blocks[3].GetPosition().Y);
                            _currentTetro.Rotation += 90;
                            if (!BlockCollisionCheck(_currentTetro, false) || !BlockSideCollisionCheck(_currentTetro))
                            { 
                                _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X + _blockWidth, _currentTetro.Blocks[0].GetPosition().Y - _blockHeight);
                                _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X - _blockWidth, _currentTetro.Blocks[2].GetPosition().Y + _blockHeight);
                                _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X - (2 * _blockWidth), _currentTetro.Blocks[3].GetPosition().Y);
                                _currentTetro.Rotation -= 90;
                            }
                        }
                        else if (_currentTetro.Rotation == 90)
                        {
                            _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X + _blockWidth, _currentTetro.Blocks[0].GetPosition().Y + _blockHeight);
                            _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X - _blockWidth, _currentTetro.Blocks[2].GetPosition().Y - _blockHeight);
                            _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X, _currentTetro.Blocks[3].GetPosition().Y - (2 * _blockHeight));
                            _currentTetro.Rotation += 90;
                            if (!BlockCollisionCheck(_currentTetro, false) || !BlockSideCollisionCheck(_currentTetro))
                            {
                                _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X - _blockWidth, _currentTetro.Blocks[0].GetPosition().Y - _blockHeight);
                                _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X + _blockWidth, _currentTetro.Blocks[2].GetPosition().Y + _blockHeight);
                                _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X, _currentTetro.Blocks[3].GetPosition().Y + (2 * _blockHeight));
                                _currentTetro.Rotation -= 90;
                            }
                        }
                        else if (_currentTetro.Rotation == 180)
                        {
                            _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X + _blockWidth, _currentTetro.Blocks[0].GetPosition().Y - _blockHeight);
                            _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X - _blockWidth, _currentTetro.Blocks[2].GetPosition().Y + _blockHeight);
                            _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X - (2 * _blockWidth), _currentTetro.Blocks[3].GetPosition().Y);
                            _currentTetro.Rotation += 90;
                            if (!BlockCollisionCheck(_currentTetro, false) || !BlockSideCollisionCheck(_currentTetro))
                            {
                                _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X - _blockWidth, _currentTetro.Blocks[0].GetPosition().Y + _blockHeight);
                                _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X + _blockWidth, _currentTetro.Blocks[2].GetPosition().Y - _blockHeight);
                                _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X + (2 * _blockWidth), _currentTetro.Blocks[3].GetPosition().Y);
                                _currentTetro.Rotation -= 90;
                            }
                        }
                        else if (_currentTetro.Rotation == 270)
                        {
                            _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X - _blockWidth, _currentTetro.Blocks[0].GetPosition().Y - _blockHeight);
                            _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X + _blockWidth, _currentTetro.Blocks[2].GetPosition().Y + _blockHeight);
                            _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X, _currentTetro.Blocks[3].GetPosition().Y + (2 * _blockHeight));
                            _currentTetro.Rotation = 0;
                            if (!BlockCollisionCheck(_currentTetro, false) || !BlockSideCollisionCheck(_currentTetro))
                            {                                
                                _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X + _blockWidth, _currentTetro.Blocks[0].GetPosition().Y + _blockHeight);
                                _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X - _blockWidth, _currentTetro.Blocks[2].GetPosition().Y - _blockHeight);
                                _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X, _currentTetro.Blocks[3].GetPosition().Y - (2 * _blockHeight));
                                _currentTetro.Rotation = 270;
                            }
                        }
                        for (int i = 0; i < _shadowTetro.Blocks.Count; i++)
                        {
                            _shadowTetro.Blocks[i].SetPosition(_currentTetro.Blocks[i].GetPosition());
                        }
                        DropBlock(new Vector(0, -1, 0), _shadowTetro);
                        break;
                    }
                case TetroType.RightHook:
                    {
                        if (_currentTetro.Rotation == 0)
                        {
                            _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X - _blockWidth, _currentTetro.Blocks[0].GetPosition().Y + _blockHeight);
                            _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X + _blockWidth, _currentTetro.Blocks[2].GetPosition().Y - _blockHeight);
                            _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X, _currentTetro.Blocks[3].GetPosition().Y - (2 * _blockHeight));
                            _currentTetro.Rotation += 90;
                            if (!BlockCollisionCheck(_currentTetro, false) || !BlockSideCollisionCheck(_currentTetro))
                            {
                                _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X + _blockWidth, _currentTetro.Blocks[0].GetPosition().Y - _blockHeight);
                                _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X - _blockWidth, _currentTetro.Blocks[2].GetPosition().Y + _blockHeight);
                                _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X, _currentTetro.Blocks[3].GetPosition().Y + (2 * _blockHeight));
                                _currentTetro.Rotation -= 90;
                            }
                        }
                        else if (_currentTetro.Rotation == 90)
                        {
                            _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X + _blockWidth, _currentTetro.Blocks[0].GetPosition().Y + _blockHeight);
                            _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X - _blockWidth, _currentTetro.Blocks[2].GetPosition().Y - _blockHeight);
                            _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X - (2 * _blockWidth), _currentTetro.Blocks[3].GetPosition().Y);
                            _currentTetro.Rotation += 90;
                            if (!BlockCollisionCheck(_currentTetro, false) || !BlockSideCollisionCheck(_currentTetro))
                            {
                                _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X - _blockWidth, _currentTetro.Blocks[0].GetPosition().Y - _blockHeight);
                                _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X + _blockWidth, _currentTetro.Blocks[2].GetPosition().Y + _blockHeight);
                                _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X + (2 * _blockWidth), _currentTetro.Blocks[3].GetPosition().Y);
                                _currentTetro.Rotation -= 90;
                            }
                        }
                        else if (_currentTetro.Rotation == 180)
                        {
                            _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X + _blockWidth, _currentTetro.Blocks[0].GetPosition().Y - _blockHeight);
                            _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X - _blockWidth, _currentTetro.Blocks[2].GetPosition().Y + _blockHeight);
                            _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X, _currentTetro.Blocks[3].GetPosition().Y + (2 * _blockHeight));
                            _currentTetro.Rotation += 90;
                            if (!BlockCollisionCheck(_currentTetro, false) || !BlockSideCollisionCheck(_currentTetro))
                            {
                                _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X - _blockWidth, _currentTetro.Blocks[0].GetPosition().Y + _blockHeight);
                                _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X + _blockWidth, _currentTetro.Blocks[2].GetPosition().Y - _blockHeight);
                                _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X, _currentTetro.Blocks[3].GetPosition().Y - (2 * _blockHeight));
                                _currentTetro.Rotation -= 90;
                            }
                        }
                        else if (_currentTetro.Rotation == 270)
                        {
                            _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X - _blockWidth, _currentTetro.Blocks[0].GetPosition().Y - _blockHeight);
                            _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X + _blockWidth, _currentTetro.Blocks[2].GetPosition().Y + _blockHeight);
                            _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X + (2 * _blockWidth), _currentTetro.Blocks[3].GetPosition().Y);
                            _currentTetro.Rotation = 0;
                            if (!BlockCollisionCheck(_currentTetro, false) || !BlockSideCollisionCheck(_currentTetro))
                            {
                                _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X + _blockWidth, _currentTetro.Blocks[0].GetPosition().Y + _blockHeight);
                                _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X - _blockWidth, _currentTetro.Blocks[2].GetPosition().Y - _blockHeight);
                                _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X - (2 * _blockWidth), _currentTetro.Blocks[3].GetPosition().Y);
                                _currentTetro.Rotation = 270;
                            }
                        }
                        for (int i = 0; i < _shadowTetro.Blocks.Count; i++)
                        {
                            _shadowTetro.Blocks[i].SetPosition(_currentTetro.Blocks[i].GetPosition());
                        }
                        DropBlock(new Vector(0, -1, 0), _shadowTetro);
                        break;
                    }
                case TetroType.LeftZ:
                    {
                        if (_currentTetro.Rotation == 0)
                        {
                            _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X + (2 * _blockWidth), _currentTetro.Blocks[3].GetPosition().Y);
                            _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X, _currentTetro.Blocks[0].GetPosition().Y + (2 * _blockHeight));
                            _currentTetro.Rotation += 90;
                            if (!BlockCollisionCheck(_currentTetro, false) || !BlockSideCollisionCheck(_currentTetro))
                            {
                                _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X - (2 * _blockWidth), _currentTetro.Blocks[3].GetPosition().Y);
                                _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X, _currentTetro.Blocks[0].GetPosition().Y - (2 * _blockHeight));
                                _currentTetro.Rotation -= 90;
                            }
                        }
                        else
                        {
                            _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X - (2 * _blockWidth), _currentTetro.Blocks[3].GetPosition().Y);
                            _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X, _currentTetro.Blocks[0].GetPosition().Y - (2 * _blockHeight));
                            _currentTetro.Rotation -= 90;
                            if (!BlockCollisionCheck(_currentTetro, false) || !BlockSideCollisionCheck(_currentTetro))
                            {
                                _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X + (2 * _blockWidth), _currentTetro.Blocks[3].GetPosition().Y);
                                _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X, _currentTetro.Blocks[0].GetPosition().Y + (2 * _blockHeight));
                                _currentTetro.Rotation += 90;
                            }
                        }
                        for (int i = 0; i < _shadowTetro.Blocks.Count; i++)
                        {
                            _shadowTetro.Blocks[i].SetPosition(_currentTetro.Blocks[i].GetPosition());
                        }
                        DropBlock(new Vector(0, -1, 0), _shadowTetro);
                        break;
                    }
                case TetroType.RightZ:
                    {
                        if (_currentTetro.Rotation == 0)
                        {
                            _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X - (2 * _blockWidth), _currentTetro.Blocks[3].GetPosition().Y);
                            _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X, _currentTetro.Blocks[0].GetPosition().Y + (2 * _blockHeight));
                            _currentTetro.Rotation += 90;
                            if (!BlockCollisionCheck(_currentTetro, false) || !BlockSideCollisionCheck(_currentTetro))
                            {
                                _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X + (2 * _blockWidth), _currentTetro.Blocks[3].GetPosition().Y);
                                _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X, _currentTetro.Blocks[0].GetPosition().Y - (2 * _blockHeight));
                                _currentTetro.Rotation -= 90;
                            }
                        }
                        else
                        {
                            _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X + (2 * _blockWidth), _currentTetro.Blocks[3].GetPosition().Y);
                            _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X, _currentTetro.Blocks[0].GetPosition().Y - (2 * _blockHeight));
                            _currentTetro.Rotation -= 90;
                            if (!BlockCollisionCheck(_currentTetro, false) || !BlockSideCollisionCheck(_currentTetro))
                            {
                                _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X - (2 * _blockWidth), _currentTetro.Blocks[3].GetPosition().Y);
                                _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X, _currentTetro.Blocks[0].GetPosition().Y + (2 * _blockHeight));
                                _currentTetro.Rotation += 90;
                            }
                        }
                        for (int i = 0; i < _shadowTetro.Blocks.Count; i++)
                        {
                            _shadowTetro.Blocks[i].SetPosition(_currentTetro.Blocks[i].GetPosition());
                        }
                        DropBlock(new Vector(0, -1, 0), _shadowTetro);
                        break;
                    }
                case TetroType.TBlock:
                    {
                        if (_currentTetro.Rotation == 0)
                        {
                            _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X - _blockWidth, _currentTetro.Blocks[0].GetPosition().Y + _blockHeight);
                            _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X + _blockWidth, _currentTetro.Blocks[2].GetPosition().Y + _blockHeight);
                            _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X - _blockWidth, _currentTetro.Blocks[3].GetPosition().Y - _blockHeight);
                            _currentTetro.Rotation += 90;
                            if (!BlockCollisionCheck(_currentTetro, false) || !BlockSideCollisionCheck(_currentTetro))
                            {
                                _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X + _blockWidth, _currentTetro.Blocks[0].GetPosition().Y - _blockHeight);
                                _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X - _blockWidth, _currentTetro.Blocks[2].GetPosition().Y - _blockHeight);
                                _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X + _blockWidth, _currentTetro.Blocks[3].GetPosition().Y + _blockHeight);
                                _currentTetro.Rotation -= 90;
                            }
                        }
                        else if (_currentTetro.Rotation == 90)
                        {
                            _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X + _blockWidth, _currentTetro.Blocks[0].GetPosition().Y + _blockHeight);
                            _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X + _blockWidth, _currentTetro.Blocks[2].GetPosition().Y - _blockHeight);
                            _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X - _blockWidth, _currentTetro.Blocks[3].GetPosition().Y + _blockHeight);
                            _currentTetro.Rotation += 90;
                            if (!BlockCollisionCheck(_currentTetro, false) || !BlockSideCollisionCheck(_currentTetro))
                            {
                                _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X - _blockWidth, _currentTetro.Blocks[0].GetPosition().Y - _blockHeight);
                                _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X - _blockWidth, _currentTetro.Blocks[2].GetPosition().Y + _blockHeight);
                                _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X + _blockWidth, _currentTetro.Blocks[3].GetPosition().Y - _blockHeight);
                                _currentTetro.Rotation -= 90;
                            }
                        }
                        else if (_currentTetro.Rotation == 180)
                        {
                            _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X + _blockWidth, _currentTetro.Blocks[0].GetPosition().Y - _blockHeight);
                            _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X - _blockWidth, _currentTetro.Blocks[2].GetPosition().Y - _blockHeight);
                            _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X + _blockWidth, _currentTetro.Blocks[3].GetPosition().Y + _blockHeight);
                            _currentTetro.Rotation += 90;
                            if (!BlockCollisionCheck(_currentTetro, false) || !BlockSideCollisionCheck(_currentTetro))
                            {
                                _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X - _blockWidth, _currentTetro.Blocks[0].GetPosition().Y + _blockHeight);
                                _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X + _blockWidth, _currentTetro.Blocks[2].GetPosition().Y + _blockHeight);
                                _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X - _blockWidth, _currentTetro.Blocks[3].GetPosition().Y - _blockHeight);
                                _currentTetro.Rotation -= 90;
                            }
                        }
                        else if (_currentTetro.Rotation == 270)
                        {
                            _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X - _blockWidth, _currentTetro.Blocks[0].GetPosition().Y - _blockHeight);
                            _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X - _blockWidth, _currentTetro.Blocks[2].GetPosition().Y + _blockHeight);
                            _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X + _blockWidth, _currentTetro.Blocks[3].GetPosition().Y - _blockHeight);
                            _currentTetro.Rotation = 0;
                            if (!BlockCollisionCheck(_currentTetro, false) || !BlockSideCollisionCheck(_currentTetro))
                            {
                                _currentTetro.Blocks[0].SetPosition(_currentTetro.Blocks[0].GetPosition().X + _blockWidth, _currentTetro.Blocks[0].GetPosition().Y + _blockHeight);
                                _currentTetro.Blocks[2].SetPosition(_currentTetro.Blocks[2].GetPosition().X + _blockWidth, _currentTetro.Blocks[2].GetPosition().Y - _blockHeight);
                                _currentTetro.Blocks[3].SetPosition(_currentTetro.Blocks[3].GetPosition().X - _blockWidth, _currentTetro.Blocks[3].GetPosition().Y + _blockHeight);
                                _currentTetro.Rotation = 270;
                            }
                        }
                        for (int i = 0; i < _shadowTetro.Blocks.Count; i++)
                        {
                            _shadowTetro.Blocks[i].SetPosition(_currentTetro.Blocks[i].GetPosition());
                        }
                        DropBlock(new Vector(0, -1, 0), _shadowTetro);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        // Puts the currentBlock in holding. If there is a block in hold its swapped
        public void HoldBlock()
        {
            if (_holdAllowed)
            {
                if (_holdingTetro == null)
                {
                    SpawnNewHoldingTetro(_currentTetro.Type);
                    SpawnNewTetro(_previewTetro.Type);
                    SpawnNewPreviewTetro();
                }
                else
                {
                    TetroType type = _holdingTetro.Type;
                    SpawnNewHoldingTetro(_currentTetro.Type);
                    SpawnNewTetro(type);
                    _holdAllowed = false;
                }
            }
        }

        #endregion

        #region Block Removal and Reposition

        // Checks for completed rows
        private void CompletedRowCheck()
        {
            double heightTracker = _bottomBound + (_blockHeight / 2);
            List<int> blockRow = new List<int>();
            while (heightTracker < _topBound - (_blockHeight / 2))
            {
                for(int i = 0; i < _blocks.Count; i++)
                {
                    if (_blocks[i].GetPosition().Y == heightTracker)
                    {
                        blockRow.Add(i);
                    }
                }
                if (blockRow.Count == _fullRow)
                {
                    _elimHeights.Add(heightTracker);
                    _completedRows++;
                    _blockFallSpeed -= _blockSpeedIncrement;
                    foreach (int blockIndex in blockRow)
                    {
                        _blocks[blockIndex].KeepAlive = false;
                    }                   
                }                
                heightTracker += _currentBlock.GetHeight();
                blockRow.Clear(); 
            }
            blockRow.Clear();
            //FlashBlocks();
            RemoveDeadBlocks();
        }

        // Flash Blocks being removed
        private void FlashBlocks()
        {
            foreach (Block block in _blocks)
            {
                if (!block.KeepAlive)
                {
                    block.FlashBlock();
                }
            }
        }

        //Removes dead blocks
        private void RemoveDeadBlocks()
        {
            for(int i = _blocks.Count - 1; i >= 0; i--)
            {
                if (!_blocks[i].KeepAlive)
                {
                    //if (!_blocks[i].Flashing)
                    //{
                        _blocks.RemoveAt(i);
                   // }
                }
            }
            RepositionBlocks();
        }

        // Resets blocks after rows are deleted
        private void RepositionBlocks()
        {
            _elimHeights.Sort();
            _elimHeights.Reverse();
            foreach (double cutoffPoint in _elimHeights)
            {
                foreach (Block block in _blocks)
                {
                    if (block.GetPosition().Y >= cutoffPoint)
                    {
                        block.SetPosition(block.GetPosition().X, block.GetPosition().Y - _blockHeight);
                    }
                }
            }
            _elimHeights.Clear();
        }

        #endregion
    }
}
