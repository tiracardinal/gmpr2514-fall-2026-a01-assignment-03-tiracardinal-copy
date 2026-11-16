using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Assignment3;

public class Mosquito
{
    private const float _DyingDuration = 1f;
    private float _dyingTimer = 0;

    private SimpleAnimation _animationAlive, _animationDying;
    private Vector2 _position;
    private float _speed;
    private Texture2D _texture;
    private Rectangle _gameBoundingBox;

    private static readonly Random _rng = new Random();
    private double _shootTimer;

    private enum State {Alive, Dying, Dead}
    private State _state = State.Alive;
    internal bool Alive { get => _state == State.Alive;}

    private FireBall[] _fBalls;

    internal Vector2 Direction {get; set;} = Vector2.Zero;
    // todo: refactor so that it doesn't depend on knowing there are 11 frames to find out the width
    internal Rectangle BoundingBox
    {
        get 
        { 
            return new Rectangle((int)_position.X, (int)_position.Y, _texture.Width / 11, _texture.Height);
        }
    }

    internal void Initialize(Vector2 position, float speed, Vector2 direction, Rectangle gameBoundingBox)
    {
        _position = position;
        _speed = speed;
        Direction = direction;
        _fBalls = new FireBall[2];
        for(int f = 0; f < _fBalls.Length; f++)
        {
            _fBalls[f] = new FireBall();
            _fBalls[f].Initialize(40, gameBoundingBox);
        }
        _gameBoundingBox = gameBoundingBox;
        ResetShootTimer();
    }
    internal void LoadContent(ContentManager content)
    {
        _texture = content.Load<Texture2D>("Mosquito");
        _animationAlive = new SimpleAnimation(_texture, _texture.Width / 11, _texture.Height, 11, 8f);
        
        Texture2D dyingTexture = content.Load<Texture2D>("Poof");
        int frameWidth = 16;
        int frameHeight = dyingTexture.Height;
        int frameCount = dyingTexture.Width / frameWidth;
        float framesPerSecond = 8f;
        _animationDying = new SimpleAnimation(dyingTexture, frameWidth, frameHeight, frameCount, framesPerSecond);

        foreach(FireBall f in _fBalls)
            f.LoadContent(content);

    }
    internal void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        switch(_state)
        {
            case State.Alive:
                _position += Direction * _speed * dt;
                if(BoundingBox.Left < _gameBoundingBox.Left || BoundingBox.Right > _gameBoundingBox.Right)
                    Direction = new Vector2(Direction.X * -1, Direction.Y);
                _animationAlive?.Update(gameTime);

                _shootTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                if(_shootTimer <= 0)
                {
                    Shoot();
                    ResetShootTimer();
                }
                break;
            case State.Dying:
                _dyingTimer += dt;
                if(_dyingTimer >= _DyingDuration)
                    _state = State.Dead;
                _animationDying?.Update(gameTime);
                break;
            case State.Dead:
                break;
        }
        foreach (FireBall f in _fBalls)
        {
            if(f != null && !f.Shootable)
            {
                f.Update(gameTime);
            }
        }
    }
    internal void Draw(SpriteBatch spriteBatch)
    {
        switch(_state)
        {
            case State.Alive:
                _animationAlive?.Draw(spriteBatch, _position, SpriteEffects.None);
                break;
            case State.Dying:
                _animationDying?.Draw(spriteBatch, _position, SpriteEffects.None);
                break;
            case State.Dead:
                break;
        }
        foreach(FireBall f in _fBalls)
            f.Draw(spriteBatch); 
        
    }

    internal void Shoot()
    {
        int index = 0;
        while(index < _fBalls.Length)
        {
            if(_fBalls[index].Shootable)
            {
                //shoot it
                float fbPositionX = BoundingBox.Center.X - _fBalls[index].BoundingBox.Width / 2;
                float fbPositionY = BoundingBox.Bottom;
                _fBalls[index].Shoot(new Vector2(fbPositionX, fbPositionY), new Vector2(0, 1));
                index = _fBalls.Length; //shot a Fire Ball, now exit loop
            }
            index++;
        }
    }
    private void ResetShootTimer()
    {
        _shootTimer = 2.5 + (_rng.NextDouble() * 7.0);
    }

    internal bool ProcessCollision(Rectangle boundingBox)
    {
        foreach(FireBall f in _fBalls)
        {
            if (f != null && !f.Shootable && f.ProcessCollision(boundingBox))
            {
                f.Destroy();
                return true;
            }
        }
        return false;
    }

    internal void Die()
    {
        _state = State.Dying;
        _dyingTimer = 0;
    }

}