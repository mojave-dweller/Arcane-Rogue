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

public class Ghost
{
    private int ghostWidth = 32;
    private int ghostHeight = 64;
    private float ghostSpeed = 1.5f;
    public int ghostHP = 2;
    private float bobTime = 0f;
    private float bobAmplitude = 6f;
    private float bobSpeed = 0.5f;
    public float floatBaseY;

    public Rectangle ghostRect;
    public Vector2 ghostPosition;
    public Vector2 ghostSpawn;
    public Vector2 ghostAnchorPoint;
    private readonly int ghostAnchorRadius = 65;
    private readonly List<Rectangle> _collisionRects;
    private readonly List<Rectangle> _platformRects;
    private readonly List<Chair> _chairList;
    private bool playerInRange = false;
    private float chairTimer;
    private Chair possessedChair;
    private float chairFloatTimer;
    private float chairSpeed = 7.5f;
    private float wailTimer;
    private readonly List<Wail> ghostlyWails;
    public Color ghostColor;
    private Vector2 knockbackVelocity = Vector2.Zero;

    int frameWidth = 128;
    int frameHeight = 128;
    int frameCounter = 0;
    float frameTimer = 0f;
    String facingDirection = "left";
    public Rectangle currentFrame;
    public Texture2D currentSpriteSheet;
    public Texture2D floating;
    public Texture2D throwing;
    public Texture2D screaming;
    public Texture2D chairTexture;
    public Texture2D wailTexture;
    bool isScreaming = false;

    struct Chair
    {
        public Vector2 Position;
        public Vector2 Direction;
        //public float Angle;
        public int Width;
        public int Height;
    }

    struct Wail
    {
        public Vector2 Position;
        public Vector2 Direction;
        public float Angle;
        public int Width;
        public int Height;
    }

    public Ghost(Vector2 position, List<Rectangle> collisionRects, List<Rectangle> platformRects)
    {
        this.ghostPosition = position;
        this.ghostSpawn = position;
        this.floatBaseY = position.Y;
        this.ghostRect = new Rectangle((int)ghostPosition.X, (int)ghostPosition.Y,
                                          this.ghostWidth, this.ghostHeight);
        ghostAnchorPoint = new Vector2(ghostPosition.X + ghostWidth / 2, ghostPosition.Y + ghostHeight / 2);
        this._collisionRects = collisionRects;
        this._platformRects = platformRects;
        this._chairList = new List<Chair>();
        this.ghostlyWails = new List<Wail>();
        this.ghostColor = Color.White;

    }

    public void Update(GameTime gameTime, Player player)
    {
        if (ghostHP > 0)
        {
            this.ghostRect = new Rectangle((int)ghostPosition.X, (int)ghostPosition.Y,
                                          this.ghostWidth, this.ghostHeight);

            ghostAnchorPoint = new Vector2(ghostPosition.X + ghostWidth / 2, ghostPosition.Y + ghostHeight / 2);

            Vector2 direction = player.AnchorPoint - this.ghostAnchorPoint;
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
            }

            if (!player.dead)
            {
                UpdatePlayerInteraction(gameTime, direction, player);
            }
            UpdateMovement(gameTime);
            UpdateGhostRectangle(gameTime);
            AnimateSprite(gameTime);
        }
        else
        {
            ghostPosition = ghostSpawn;
        }
        UpdateChair(player);
        UpdateWails(player);

    }
    public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, GameTime gameTime)
    {
        if (currentSpriteSheet == null)
        {
            currentSpriteSheet = floating;
        }
        if (ghostHP > 0)
        {
            if (facingDirection == "right")
            {
                spriteBatch.Draw(
                                    currentSpriteSheet,
                                    new Vector2(ghostPosition.X - 48, ghostPosition.Y - 56),
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
                                    new Vector2(ghostPosition.X - 48, ghostPosition.Y - 56),
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
        DrawChair(spriteBatch, pixelTexture);
        DrawWails(spriteBatch, pixelTexture);
    }
    public void RangedAttack(GameTime gameTime, Vector2 direction)
    {
        chairTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        possessedChair = new Chair();
        possessedChair.Direction = direction;
        possessedChair.Position = ghostAnchorPoint + direction * ghostAnchorRadius;
        //possessedChair.Angle = MathF.Atan2(direction.Y, direction.X) + MathF.PI;
        possessedChair.Width = 25;
        possessedChair.Height = 40;

        if (chairTimer <= 2f)
        {
            chairFloatTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            possessedChair.Position.Y += MathF.Sin(chairFloatTimer * 0.5f * MathHelper.TwoPi);
        }

        if (chairTimer >= 2f)
        {
            _chairList.Add(possessedChair);
            chairTimer = 0f;
            chairFloatTimer = 0f;
        }
    }
    public void WailAttack(GameTime gameTime, Vector2 direction)
    {
        wailTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        Wail ghostlyWail = new Wail();
        ghostlyWail.Direction = direction;
        ghostlyWail.Position = ghostAnchorPoint + direction;
        ghostlyWail.Angle = MathF.Atan2(direction.Y, direction.X) + MathF.PI;
        ghostlyWail.Width = 10;
        ghostlyWail.Height = 40;

        if (wailTimer >= 0.37f)
        {
            ghostlyWails.Add(ghostlyWail);
            wailTimer = 0f;
        }

    }
    public void UpdateChair(Player player)
    {
        for (int i = _chairList.Count - 1; i >= 0; i--)
        {
            Chair chair = _chairList[i];
            chair.Position += chair.Direction * chairSpeed;
            _chairList[i] = chair;

            Rectangle chairRect = new Rectangle(
                (int)chair.Position.X - chair.Width / 2,
                (int)chair.Position.Y - chair.Height / 2,
                chair.Width, chair.Height);

            bool hit = false;
            foreach (Rectangle rect in _collisionRects)
            {
                if (chairRect.Intersects(rect))
                {
                    hit = true;
                    break;
                }
            }

            if (chairRect.Intersects(player.Rect))
            {
                player.TakeHit(chair.Direction, false, 2f);
                hit = true;
            }

            if (hit || chair.Position.X < -200 || chair.Position.X > 2400 ||
                chair.Position.Y < -200 || chair.Position.Y > 2400)
                _chairList.RemoveAt(i);
        }
    }
    public void UpdateWails(Player player)
    {
        for (int i = ghostlyWails.Count - 1; i >= 0; i--)
        {
            Wail tempWail = ghostlyWails[i];
            tempWail.Position += tempWail.Direction * chairSpeed;
            ghostlyWails[i] = tempWail;

            Rectangle tempWailRect = new Rectangle(
                (int)tempWail.Position.X - tempWail.Width / 2,
                (int)tempWail.Position.Y - tempWail.Height / 2,
                tempWail.Width, tempWail.Height);

            bool hit = false;
            foreach (Rectangle rect in _collisionRects)
            {
                if (tempWailRect.Intersects(rect))
                {
                    hit = true;
                    break;
                }
            }

            if (tempWailRect.Intersects(player.Rect))
            {
                player.TakeHit(tempWail.Direction, true, 0.5f);
                hit = true;
            }

            if (hit || tempWail.Position.X < -200 || tempWail.Position.X > 2400 ||
                tempWail.Position.Y < -200 || tempWail.Position.Y > 2400)
                ghostlyWails.RemoveAt(i);
        }
    }
    public void UpdatePlayerInteraction(GameTime gameTime, Vector2 direction, Player player)
    {
        bool canSee = HasLineOfSight(ghostAnchorPoint, player.AnchorPoint);
        if (direction.X >= 0)
        {
            facingDirection = "right";
        }
        else
        {
            facingDirection = "left";
        }
        if (canSee)
        {
            if (Vector2.DistanceSquared(player.Position, ghostAnchorPoint) <= 280 * 280 &&
                Vector2.DistanceSquared(player.Position, ghostAnchorPoint) >= 275 * 275 ||
                Vector2.DistanceSquared(player.Position, ghostAnchorPoint) <= 140 * 140 &&
                Vector2.DistanceSquared(player.Position, ghostAnchorPoint) >= 120 * 120)
            {
                ghostPosition += direction * ghostSpeed;
                floatBaseY += direction.Y * ghostSpeed;
            }
            if (Vector2.DistanceSquared(player.Position, ghostAnchorPoint) <= 280 * 280 &&
                Vector2.DistanceSquared(player.Position, ghostAnchorPoint) >= 135 * 135)
            {
                RangedAttack(gameTime, direction);
                playerInRange = true;
            }
            else
            {
                playerInRange = false;
            }
            if (Vector2.DistanceSquared(player.Position, ghostAnchorPoint) <= 125 * 125)
            {
                isScreaming = true;
                if (Game1.screamInstance.State != SoundState.Playing)
                {
                    Game1.screamInstance.Play();
                }
                WailAttack(gameTime, direction);
            }
            else
            {
                isScreaming = false;
                if (Game1.screamInstance.State == SoundState.Playing)
                {
                    Game1.screamInstance.Stop();
                }
            }
        }
        else
        {
            playerInRange = false;
        }

        if (ghostRect.Intersects(player.Rect))
            player.TakeHit(direction, false, 3f);
    }
    public void UpdateMovement(GameTime gameTime)
    {
        ghostPosition += knockbackVelocity;
        knockbackVelocity *= 0.8f;
        if (knockbackVelocity.LengthSquared() < 0.01f)
            knockbackVelocity = Vector2.Zero;

        bobTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        ghostPosition.Y = floatBaseY + MathF.Sin(bobTime * bobSpeed * MathHelper.TwoPi) * bobAmplitude;
    }
    public void UpdateGhostRectangle(GameTime gameTime)
    {
        ghostRect.X = (int)ghostPosition.X;
        ghostRect.Y = (int)ghostPosition.Y;
    }
    public void DrawChair(SpriteBatch spriteBatch, Texture2D pixelTexture)
    {
        if (chairTimer >= 0.5f && chairTimer <= 3f && playerInRange)
        {
            spriteBatch.Draw(chairTexture, possessedChair.Position, null,
                           Color.White, 0,
                           new Vector2(0.5f, 0.5f),
                           new Vector2(1, 1),
                           SpriteEffects.None, 0f);
        }
        foreach (Chair chair in _chairList)
        {
            spriteBatch.Draw(chairTexture, chair.Position, null,
                            Color.White, 0,
                            new Vector2(0.5f, 0.5f),
                            new Vector2(1, 1),
                            SpriteEffects.None, 0f);
        }
    }
    public void DrawWails(SpriteBatch spriteBatch, Texture2D pixelTexture)
    {
        foreach (Wail wail in ghostlyWails)
        {
            spriteBatch.Draw(wailTexture, wail.Position, null,
                            Color.White, wail.Angle * MathHelper.Pi,
                            new Vector2(wailTexture.Width / 2f, wailTexture.Height / 2f),
                            new Vector2(1, 1),
                            SpriteEffects.None, 0f);
        }
    }
    public void TakeHit(Vector2 hitDirection, float force = 8f)
    {

        knockbackVelocity = hitDirection * force;

        if (ghostHP > 0)
        {
            ghostHP -= 1;
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
        if (playerInRange && chairFloatTimer >= 1.6f)
        {
            if (currentSpriteSheet != throwing)
            {
                frameCounter = 0;
            }
            currentSpriteSheet = throwing;
            float frameDuration = 0.125f;
            frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (frameTimer >= frameDuration)
            {
                frameTimer = 0;
                frameCounter++;
                if (frameCounter > 3)
                    frameCounter = 0;
            }
            currentFrame = GetFrameRect(frameCounter, throwing);
        }
        else if (isScreaming)
        {
            if (currentSpriteSheet != screaming)
            {
                frameCounter = 0;
            }
            currentSpriteSheet = screaming;
            float frameDuration = 1f / 16f;
            frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (frameTimer >= frameDuration)
            {
                frameTimer = 0;
                frameCounter++;
                if (frameCounter > 3)
                    frameCounter = 3;
            }
            currentFrame = GetFrameRect(frameCounter, screaming);
        }
        else
        {
            currentSpriteSheet = floating;
            float frameDuration = 1f / 16f;
            frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (frameTimer >= frameDuration)
            {
                frameTimer = 0;
                frameCounter++;
                if (frameCounter > 4)
                    frameCounter = 0;
            }
            currentFrame = GetFrameRect(frameCounter, floating);
        }
    }
}