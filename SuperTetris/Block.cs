using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Engine;
using Tao.OpenGl;

namespace SubwayTetris
{
    public class Block 
    {
        protected Sprite _sprite = new Sprite();
        public bool KeepAlive { get; set; }
        public double DropSpeed { get; set; }
        public Vector Direction { get; set; }
        public bool Flashing { get; set; }
        double _moveDistance;
        private double _dropCount = 0.0f;
        private double _flashCountDown = 0;
        static readonly double FlashTime = 0.50;

        #region Block Constructors

        public Block(TextureManager manager)
        {
            _sprite.Texture = manager.Get("tetris_block");
            DropSpeed = 30.0;
            Direction = new Vector(0, -1, 0);
            KeepAlive = true;
            _moveDistance = _sprite.GetWidth();            
        }

        public Block(Texture texture)
        {
            _sprite.Texture = texture;
            DropSpeed = 30.0;
            Direction = new Vector(0, -1, 0);
            KeepAlive = true;
            _moveDistance = _sprite.GetWidth();
        }

        public Block(TextureManager manager, string blockType)
        {
            _sprite.Texture = manager.Get(blockType);
            DropSpeed = 30.0;
            Direction = new Vector(0, -1, 0);
            KeepAlive = true;
            _moveDistance = _sprite.GetWidth();
        }        

        public Block(TextureManager manager, string blockType, Vector scaler)
        {
            _sprite.Texture = manager.Get(blockType);
            DropSpeed = 30.0;
            Direction = new Vector(0, -1, 0);
            KeepAlive = true;
            _sprite.SetScale(scaler.X, scaler.Y);
            _moveDistance = _sprite.GetWidth();
        }

        public Block(TextureManager manager, string blockType, Vector scaler, Vector position)
        {
            _sprite.Texture = manager.Get(blockType);
            DropSpeed = 30.0;
            Direction = new Vector(0, -1, 0);
            KeepAlive = true;
            _sprite.SetScale(scaler.X, scaler.Y);
            _sprite.SetPosition(position.X, position.Y);
            _moveDistance = _sprite.GetWidth();
        }

        public Block(Texture texture, Vector scaler, Vector position)
        {
            _sprite.Texture = texture;
            DropSpeed = 30.0;
            Direction = new Vector(0, -1, 0);
            KeepAlive = true;
            _sprite.SetScale(scaler.X, scaler.Y);
            _sprite.SetPosition(position.X, position.Y);
            _moveDistance = _sprite.GetWidth();
        }

        public Block()
        {
            KeepAlive = true;
        }

        #endregion

        public void Update(double elapsedTime)
        {
            _dropCount++;
            if (_dropCount >= DropSpeed)
            {
                Vector position = _sprite.GetPosition();
                position += Direction * _sprite.GetHeight();
                _sprite.SetPosition(position);
                _dropCount = 0.0f;
            }
            //if (_flashCountDown != 0)
            //{
            //    _flashCountDown = Math.Max(0, _flashCountDown - elapsedTime);
            //    double scaledTime = 1 - (_flashCountDown / FlashTime);
            //    _sprite.SetColor(new Engine.Color(1, 1, (float)scaledTime, 1));
            //}
        }

        public void Render(Renderer renderer)
        {
            renderer.DrawSprite(_sprite);
        }

        public RectangleF GetBoundingBox()
        {
            float width = (float)(_sprite.Texture.Width * _sprite.ScaleX);
            float height = (float)(_sprite.Texture.Height * _sprite.ScaleY);

            return new RectangleF((float)_sprite.GetPosition().X - width / 2, (float)_sprite.GetPosition().Y - height / 2, width, height);
        }

        public void FlashBlock()
        {
            _flashCountDown = FlashTime;
            _sprite.SetColor(new Engine.Color(1, 1, 0, 1));
            Flashing = true;
        }

        public void ShadowBlock()
        {
            _sprite.SetColor(new Engine.Color(1, 1, 1, 0.4f));
        }

        #region Getters and Setters

        public void SetPosition(Vector position)
        {
            _sprite.SetPosition(position);
        }

        public void SetPosition(double x, double y)
        {
            _sprite.SetPosition(x, y);
        }
        
        public void SetColor(Engine.Color color)
        {
            _sprite.SetColor(color);
        }

        public double GetHeight()
        {
            return _sprite.GetHeight();
        }

        public double GetWidth()
        {
            return _sprite.GetWidth();
        }

        public Vector GetPosition()
        {
            return _sprite.GetPosition();
        }

        public Texture GetTexture()
        {
            return _sprite.Texture;
        }

        public void SetScale(double scaleX, double scaleY)
        {
            _sprite.SetScale(scaleX, scaleY);
            _moveDistance = _sprite.GetWidth();
        }

        #endregion

        #region Movement

        public void Move(Vector direction)
        {            
            direction *= _moveDistance;
            _sprite.SetPosition(_sprite.GetPosition() + direction);
        }

        public void Readjust(Vector direction, double distance)
        {
            direction *= distance;
            _sprite.SetPosition(_sprite.GetPosition() + direction);
        }

        public void ChangeTexture(TextureManager manager, string textureID, Vector scaleFactor)
        {
            _sprite.Texture = manager.Get(textureID);
            _sprite.SetScale(scaleFactor.X, scaleFactor.Y);
        }

        #endregion
    }
}
