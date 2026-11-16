using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Assignment3;

public class Cannon
{
    private const float _DyingDuration = 1f;
    private float _dyingTimer = 0;
    private SimpleAnimation _animationAlive, _animationDying;
    private Vector2 _position;
    private float _speed;
    internal Vector2 Direction {get; set;} = Vector2.Zero;

    private Rectangle _gameBoundingBox;

    private enum State {Alive, Dying, Dead}
    private State _state = State.Alive;
    internal bool Alive { get => _state == State.Alive;}
    internal bool Dead { get => _state == State.Dead;}

    private CannonBall[] _cBalls;

    private Texture2D _texture;
    internal Rectangle BoundingBox
    {
        get 
        { 
            return new Rectangle((int)_position.X, (int)_position.Y, _texture.Width / 4, _texture.Height);
        }
    }

    // private Vector2 _direction = Vector2.Zero;
    // internal Vector2 Direction
    // {
    //     get
    //     {
    //         return _direction;
    //     }
    //     set
    //     {
    //         _direction = value;
    //     }
    // }
    internal void Initialize(Vector2 position, float speed, Rectangle gbb)
    {
        _position = position;  
        _speed = speed;  
        _cBalls = new CannonBall[10];
        for(int c = 0; c < _cBalls.Length; c++)
        {
            _cBalls[c] = new CannonBall();
            _cBalls[c].Initialize(250f, gbb);
        }

        _gameBoundingBox = gbb;
    }
    internal void LoadContent(ContentManager content)
    {
        _texture = content.Load<Texture2D>("Cannon");
        _animationAlive = new SimpleAnimation(_texture, _texture.Width / 4, _texture.Height, 4, 2f);

        Texture2D dyingTexture = content.Load<Texture2D>("Poof");
        int frameWidth = 16;
        int frameHeight = dyingTexture.Height;
        int frameCount = dyingTexture.Width / frameWidth;
        float framesPerSecond = 8f;
        _animationDying = new SimpleAnimation(dyingTexture, frameWidth, frameHeight, frameCount, framesPerSecond);

        foreach(CannonBall c in _cBalls)
            c.LoadContent(content);

    }
    internal void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        switch(_state)
        {
            case State.Alive:
                _position += Direction * _speed * dt;
                // "?" checks for null before executing the method
                if(Direction != Vector2.Zero)
                    _animationAlive?.Update(gameTime);

                foreach(CannonBall c in _cBalls)
                    c.Update(gameTime);
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
        

    }
    internal void Draw(SpriteBatch spriteBatch)
    {
        switch(_state)
        {
            case State.Alive:
                SpriteEffects effects = SpriteEffects.None;
                if(Direction.X < 0)
                    effects = SpriteEffects.FlipHorizontally;
                _animationAlive?.Draw(spriteBatch, _position, effects);

                foreach(CannonBall c in _cBalls)
                    if(c != null && c.IsActive)
                        c.Draw(spriteBatch);
                break;
            case State.Dying:
                _animationDying?.Draw(spriteBatch, _position, SpriteEffects.None);
                break;
            case State.Dead:
                break;

        }


    }

    internal void Shoot()
    {
        int index = 0;
        while(index < _cBalls.Length)
        {
            if(_cBalls[index].Shootable)
            {
                //shoot it
                float cbPositionX = BoundingBox.Center.X - _cBalls[index].BoundingBox.Width / 2;
                float cbPositionY = BoundingBox.Top - _cBalls[index].BoundingBox.Height;
                _cBalls[index].Shoot(new Vector2(cbPositionX, cbPositionY), new Vector2(0, -1));
                index = _cBalls.Length; //we've shot a Cannon Ball, now exit loop
            }
            index++;
        }
    }

    internal bool ProcessCollision(Rectangle boundingBox)
    {
        foreach(CannonBall c in _cBalls)
        {
            if(c != null && c.IsActive && c.ProcessCollision(boundingBox))
            {
                c.Destroy();
                c.IsActive = false;
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
    internal bool OutOfAmmo
    {
        get
        {
            foreach(CannonBall c in _cBalls)
            {
                if(c != null && c.Shootable)
                {
                    return false;
                }
            }
            return true;
        }
    }

    internal void Reload()
    {
        foreach(CannonBall c in _cBalls)
        {
            if(c != null)
            {
                c.Reload();
            }
        }
    }

}