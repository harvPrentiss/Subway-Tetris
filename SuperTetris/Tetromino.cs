using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace SubwayTetris
{
    public class Tetromino 
    {
        List<Block> _blocks = new List<Block>();
        public List<Block> Blocks
        {
            get
            {
                return _blocks;
            }
        }
        public bool Moving { get; set; }
        TetroType _type;
        public TetroType Type 
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }
        public int Rotation { get; set; }
        private const double MoveDelayPeriod = 0.2;
        public double MoveDelay { get; set; }


        public Tetromino(TetroType type)
        {
            _type = type;
            MoveDelay = MoveDelayPeriod;
            Moving = false;
        }

        public Tetromino()
        {

        }       

        public void Update(double elapsedTime)
        {
            MoveDelay = Math.Max(0, (MoveDelay - elapsedTime));
            if (Moving)
            {
                foreach (Block block in _blocks)
                {
                    block.Update(elapsedTime);
                }                
            }
        }

        public void Render(Renderer renderer)
        {
            foreach (Block block in _blocks)
            {
                block.Render(renderer);
            }
        }

        public void Render(Renderer renderer, double renderCutoff)
        {
            foreach (Block block in _blocks)
            {
                if (block.GetPosition().Y < renderCutoff)
                {
                    block.Render(renderer);
                }
            }
        }

        public void ResetDelay()
        {
            MoveDelay = MoveDelayPeriod;
        }

        public void MakeShadow()
        {
            foreach (Block block in this.Blocks)
            {
                block.ShadowBlock();
            }
        }
    }   
}


