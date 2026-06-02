using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Project_Development___Arcane_Knight;

public class Zombie
{
    private int zombieWidth = 32;
    private int zombieHeight = 52;
    private float zombieSpeed = 2;
    private float zombieGravity = 0.5f;
    private float zombieVelocityY = 0f;
    private bool isGrounded = false;
    public int zombieHP = 2;
    private int zombieTimerCount = 0;
    public float zombieDeathTimer = 0f;
    public bool zombieDead = false;
    public bool wasSummoned = false;

    public Rectangle zombieRect;
    public Vector2 zombieAnchorPoint;
    public Vector2 zombiePosition;
    public Vector2 zombieSpawn;
    public bool canRespawn = false;
    public bool zombieGas = false;
    private readonly List<Rectangle> _collisionRects;
    private readonly List<Rectangle> _platformRects;
    public Color zombieColor;
    private Vector2 knockbackVelocity = Vector2.Zero;

    int frameWidth = 96;
    int frameHeight = 96;
    int frameCounter = 0;
    float frameTimer = 0f;
    Vector2 previousPosition;
    String facingDirection = "left";
    public Rectangle currentFrame;
    public Texture2D currentSpriteSheet;
    public Texture2D idle;
    public Texture2D walk;
    public Texture2D crawl;
    public Texture2D crawlIdle;
    public Texture2D gas;

    private SoundEffectInstance _snarlInstance;

    public Zombie(Vector2 position, List<Rectangle> collisionRects, List<Rectangle> platformRects, float gravity)
    {
        this.zombiePosition = position;
        this.previousPosition = position;
        this.zombieSpawn = position;
        this.zombieRect = new Rectangle((int)zombiePosition.X, (int)zombiePosition.Y,
                                          this.zombieWidth, this.zombieHeight);
        this._collisionRects = collisionRects;
        this._platformRects = platformRects;
        this.zombieGravity = gravity;
        this.zombieColor = Color.YellowGreen;
    }

    public void Init()
    {
        _snarlInstance = Game1.snarlSound.CreateInstance();
        _snarlInstance.Volume = 0.75f;
    }

    public void Update(GameTime gameTime, Player player)
    {
        if (zombieGas && _snarlInstance.State == SoundState.Playing)
        {
            _snarlInstance.Stop();
            Game1.zombieGasSound.Play();
        }
        if (!zombieDead)
        {
            this.zombieRect = new Rectangle((int)zombiePosition.X, (int)zombiePosition.Y,
                                          this.zombieWidth, this.zombieHeight);
            zombieAnchorPoint = new Vector2(zombieRect.X + zombieRect.Width / 2, zombieRect.Y + zombieRect.Height / 2);
            Vector2 direction = player.AnchorPoint - this.zombieAnchorPoint;
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
            }

            if (!player.dead)
            {
                UpdatePlayerInteraction(gameTime, direction, player);
            }
            UpdateMovement();
            UpdateZombieRectangle(gameTime);
            UpdateCollision();
            AnimateSprite(gameTime);
        }
        else
        {
            if (canRespawn)
            {
                zombieGas = false;
                zombieHP = 2;
                zombieHeight = 52;
                zombieWidth = 32;
                zombieSpeed = 2;
                zombieColor = Color.YellowGreen;
                zombiePosition = zombieSpawn;
                zombieDead = false;
                zombieDeathTimer = 0f;
                zombieTimerCount = 0;
                canRespawn = false;
            }
        }

    }
    public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, GameTime gameTime)
    {
        if (currentSpriteSheet == null)
        {
            currentSpriteSheet = idle;
        }
        if (!zombieDead)
        {
            if (zombieGas)
            {
                spriteBatch.Draw(
                                    currentSpriteSheet,
                                    new Vector2(zombiePosition.X - 16, zombiePosition.Y - 32),
                                    currentFrame,
                                    Color.White,
                                    0f,
                                    Vector2.Zero,
                                    1,
                                    SpriteEffects.None,  // mirrors left-right
                                    0f
                                );
            }
            else
            {
                if (facingDirection == "right")
                {
                    spriteBatch.Draw(
                                        currentSpriteSheet,
                                        new Vector2(zombiePosition.X - 32, zombiePosition.Y - 42),
                                        currentFrame,
                                        Color.White,
                                        0f,
                                        Vector2.Zero,
                                        1,
                                        SpriteEffects.None,  // mirrors left-right
                                        0f
                                    );
                }
                else
                {
                    spriteBatch.Draw(
                                        currentSpriteSheet,
                                        new Vector2(zombiePosition.X - 32, zombiePosition.Y - 42),
                                        currentFrame,
                                        Color.White,
                                        0f,
                                        Vector2.Zero,
                                        1,
                                        SpriteEffects.FlipHorizontally,  // mirrors left-right
                                        0f
                                    );
                }
            }
        }
    }
    public void UpdatePlayerInteraction(GameTime gameTime, Vector2 direction, Player player)
    {
        bool canSee = HasLineOfSight(zombieAnchorPoint, player.AnchorPoint);
        if (direction.X >= 0)
        {
            facingDirection = "right";
        }
        else
        {
            facingDirection = "left";
        }
        if (canSee &&
            Vector2.DistanceSquared(player.Position, zombieAnchorPoint) <= 300 * 300 &&
            Vector2.DistanceSquared(player.Position, zombieAnchorPoint) >= 28 &&
            zombieHP > 0)
        {
            this.zombiePosition.X += direction.X * zombieSpeed;
            if (_snarlInstance.State != SoundState.Playing)
            {
                _snarlInstance.Volume = 0.75f;
                _snarlInstance.Play();
            }
        }
        if (zombieRect.Intersects(player.Rect))
        {
            if (zombieWidth == 64 && zombieHeight == 64)
                player.TakeHit(direction, true, 0.05f);
            else
                player.TakeHit(direction, false, 2f);
        }
    }
    public void UpdateMovement()
    {
        if (knockbackVelocity != Vector2.Zero)
        {
            // Step X
            zombiePosition.X += knockbackVelocity.X;
            Rectangle testRect = new Rectangle((int)zombiePosition.X, (int)zombiePosition.Y, zombieWidth, zombieHeight);
            foreach (Rectangle rect in _collisionRects)
            {
                if (testRect.Intersects(rect))
                {
                    // Push back out and kill horizontal knockback
                    if (knockbackVelocity.X > 0)
                        zombiePosition.X = rect.Left - zombieWidth;
                    else
                        zombiePosition.X = rect.Right;
                    knockbackVelocity.X = 0;
                    break;
                }
            }

            // Step Y
            zombiePosition.Y += knockbackVelocity.Y;
            testRect = new Rectangle((int)zombiePosition.X, (int)zombiePosition.Y, zombieWidth, zombieHeight);
            foreach (Rectangle rect in _collisionRects)
            {
                if (testRect.Intersects(rect))
                {
                    if (knockbackVelocity.Y > 0)
                        zombiePosition.Y = rect.Top - zombieHeight;
                    else
                        zombiePosition.Y = rect.Bottom;
                    knockbackVelocity.Y = 0;
                    break;
                }
            }

            knockbackVelocity *= 0.8f;
            if (knockbackVelocity.LengthSquared() < 0.01f)
                knockbackVelocity = Vector2.Zero;
        }

        // --- zombieGravity ---
        if (!isGrounded)
            zombieVelocityY += zombieGravity;
    }
    public void UpdateZombieRectangle(GameTime gameTime)
    {

        if (zombieHP <= 0 && zombieDeathTimer <= 3f)
        {
            zombieDeathTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            zombieTimerCount++;
            zombieColor = Color.Yellow;
        }
        else if (zombieHP == 1)
        {
            zombieHeight = 32;
            zombieWidth = 35;
            //zombieRect.X = zombieWidth;
            //zombieRect.Y += zombieHeight;
            zombieSpeed = 0.5f;
        }
        else
        {
            zombieRect.X = (int)zombiePosition.X;
            zombieRect.Y = (int)zombiePosition.Y;
        }
        if (zombieTimerCount == 1)
        {
            zombiePosition.Y -= 32;
            zombieHeight = 64;
            zombieWidth = 64;
            zombieGas = true;
        }
    }
    public void UpdateCollision()
    {
        Rectangle zombieRect = new Rectangle((int)zombiePosition.X, (int)zombiePosition.Y,
                                             zombieWidth, zombieHeight);
        foreach (Rectangle rectangle in _collisionRects)
        {
            if (!zombieRect.Intersects(rectangle))
                continue;


            if (zombiePosition.X + zombieWidth / 2 < rectangle.Center.X)
                zombiePosition.X = rectangle.Left - zombieWidth;
            else
                zombiePosition.X = rectangle.Right;

            zombieRect = new Rectangle((int)zombiePosition.X, (int)zombiePosition.Y,
                                       zombieWidth, zombieHeight);
        }
        foreach (Rectangle rectangle in _platformRects)
        {
            if (!zombieRect.Intersects(rectangle))
                continue;


            if (zombiePosition.X + zombieWidth / 2 < rectangle.Center.X)
                zombiePosition.X = rectangle.Left - zombieWidth;
            else
                zombiePosition.X = rectangle.Right;

            zombieRect = new Rectangle((int)zombiePosition.X, (int)zombiePosition.Y,
                                       zombieWidth, zombieHeight);
        }

        // --- Vertical move + collision ---
        zombiePosition.Y += zombieVelocityY;

        bool landedThisFrame = false;
        zombieRect = new Rectangle((int)zombiePosition.X, (int)zombiePosition.Y,
                                   zombieWidth, zombieHeight);
        foreach (Rectangle rectangle in _collisionRects)
        {
            if (!zombieRect.Intersects(rectangle))
                continue;

            if (zombieVelocityY >= 0)
            {
                zombiePosition.Y = rectangle.Top - zombieHeight;
                zombieVelocityY = 0f;
                landedThisFrame = true;
            }
            else
            {
                zombiePosition.Y = rectangle.Bottom;
                zombieVelocityY = 0f;
            }

            zombieRect = new Rectangle((int)zombiePosition.X, (int)zombiePosition.Y,
                                       zombieWidth, zombieHeight);
        }
        foreach (Rectangle rectangle in _platformRects)
        {
            if (!zombieRect.Intersects(rectangle))
                continue;

            if (zombieVelocityY >= 0)
            {
                zombiePosition.Y = rectangle.Top - zombieHeight;
                zombieVelocityY = 0f;
                landedThisFrame = true;
            }
            else
            {
                zombiePosition.Y = rectangle.Bottom;
                zombieVelocityY = 0f;
            }

            zombieRect = new Rectangle((int)zombiePosition.X, (int)zombiePosition.Y,
                                       zombieWidth, zombieHeight);
        }

        // Ground probe — 1px below feet to detect standing still or walking off an edge
        Rectangle groundProbe = new Rectangle((int)zombiePosition.X,
                                              (int)zombiePosition.Y + zombieHeight,
                                              zombieWidth, 1);
        bool groundBelow = false;
        foreach (Rectangle rectangle in _collisionRects)
        {
            if (groundProbe.Intersects(rectangle))
            {
                groundBelow = true;
                break;
            }
        }
        foreach (Rectangle rectangle in _platformRects)
        {
            if (groundProbe.Intersects(rectangle))
            {
                groundBelow = true;
                break;
            }
        }

        isGrounded = landedThisFrame || groundBelow;
    }
    public void TakeHit(Vector2 hitDirection, float force = 4f)
    {
        if (hitDirection.Y > 0)
        {
            knockbackVelocity.X = hitDirection.X * force;
        }
        else
        {
            knockbackVelocity = hitDirection * force;
        }
        zombieVelocityY = -4f;
        isGrounded = false;

        if (zombieHP > 0)
        {
            zombieHP -= 1;
        }
    }
    private bool HasLineOfSight(Vector2 from, Vector2 to)
    {
        Vector2 dir = to - from;
        float length = dir.Length();
        if (length == 0f) return true;
        dir /= length;

        int steps = (int)(length / 8f) + 1; // sample every ~8px
        for (int i = 1; i <= steps; i++)
        {
            Vector2 sample = from + dir * (length / steps * i);
            // Use a small point-sized rect for the probe
            Rectangle probe = new Rectangle((int)sample.X, (int)sample.Y, 2, 2);
            foreach (Rectangle rect in _collisionRects)
            {
                if (probe.Intersects(rect))
                    return false;
            }
        }
        return true;
    }
    public Rectangle GetFrameRect(int frame, Texture2D spriteSheet)
    {
        int columns = spriteSheet.Width / frameWidth;
        int column = frame % columns;
        int row = frame / columns;
        return new Rectangle(column * frameWidth, row * frameHeight, frameWidth, frameHeight);
    }
    void AnimateSprite(GameTime gameTime)
    {
        if (previousPosition == zombiePosition)
        {
            if (!zombieGas && zombieHP > 0)
            {
                if (zombieRect.Height == 52)
                {
                    currentSpriteSheet = idle;
                    float frameDuration = 1f / 16f;
                    frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (frameTimer >= frameDuration)
                    {
                        frameTimer = 0;
                        frameCounter++;
                        if (frameCounter > 8)
                            frameCounter = 0;
                    }
                    currentFrame = GetFrameRect(frameCounter, idle);
                }
                else if (zombieRect.Height == 32)
                {
                    currentSpriteSheet = crawlIdle;
                    float frameDuration = 1f / 16f;
                    frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (frameTimer >= frameDuration)
                    {
                        frameTimer = 0;
                        frameCounter++;
                        if (frameCounter > 3)
                            frameCounter = 0;
                    }
                    currentFrame = GetFrameRect(frameCounter, crawlIdle);
                }
            }
            else
            {
                if (currentSpriteSheet != gas)
                {
                    frameCounter = 0;
                }
                currentSpriteSheet = gas;
                float frameDuration = 1f / 9f;
                frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (frameTimer >= frameDuration)
                {
                    frameTimer = 0;
                    frameCounter++;
                    if (frameCounter > 26)
                        frameCounter = 0;
                }
                currentFrame = GetFrameRect(frameCounter, gas);
            }
        }
        else
        {
            if (zombieRect.Height == 52)
            {
                currentSpriteSheet = walk;
                float frameDuration = 1f / 16f;
                frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (frameTimer >= frameDuration)
                {
                    frameTimer = 0;
                    frameCounter++;
                    if (frameCounter > 7)
                        frameCounter = 1;
                }
                currentFrame = GetFrameRect(frameCounter, walk);
            }
            else if (zombieRect.Height == 32)
            {
                currentSpriteSheet = crawl;
                float frameDuration = 1f / 16f;
                frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (frameTimer >= frameDuration)
                {
                    frameTimer = 0;
                    frameCounter++;
                    if (frameCounter > 9)
                        frameCounter = 0;
                }
                currentFrame = GetFrameRect(frameCounter, crawl);
            }
        }
        previousPosition = zombiePosition;
    }
}