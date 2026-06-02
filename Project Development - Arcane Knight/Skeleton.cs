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


public class Skeleton
{
    public int skeletonWidth = 32;
    public int skeletonHeight = 64;
    private int skeletonSpeed = 2;
    private float skeletonGravity = 0.5f;
    private float skeletonVelocityY = 0f;
    private bool isGrounded = false;

    public Rectangle skeletonRect;
    public Vector2 skeletonPosition;
    public Vector2 previousPosition;
    public Vector2 skeletonSpawnPosition;
    public Vector2 skeletonAnchorPoint;
    private readonly int skeletonAnchorRadius = 20;
    private readonly List<Rectangle> _collisionRects;
    private readonly List<Rectangle> _platformRects;
    private Bow bow;
    private Arrow nockedArrow;
    private Dagger dagger;
    private bool bowDrawn = false;
    private List<Arrow> arrowList;
    private float arrowSpeed = 10f;
    private float arrowTimer = 0f;
    private float meleeTimer = 0f;
    private bool isMelee = false;
    public Color skeletonColor;
    public bool brokenBones = false;
    private Vector2 knockbackVelocity = Vector2.Zero;

    int frameWidth = 128;
    int frameHeight = 128;
    int frameCounter = 0;
    float frameTimer = 0f;
    float shotTimer = 0f;
    String facingDirection = "right";
    public Rectangle currentFrame;
    public Texture2D currentSpriteSheet;
    public Texture2D skeletonIdle;
    public Texture2D skeletonBow;
    public Texture2D skeletonStab;
    public Texture2D skeletonWalk;
    public Texture2D skeletonBones;
    public Texture2D arrowTexture;
    public Texture2D bowTexture;
    public Texture2D brokenBonesTexture;
    public Rectangle currentBowFrame;

    struct Bow
    {
        public Vector2 Position;
        public float Angle;
        public int Width;
        public int Height;
    }

    struct Arrow
    {
        public Vector2 Position;
        public float Angle;
        public Vector2 Direction;
        public int Width;
        public int Height;
    }

    struct Dagger
    {
        public Vector2 Position;
        public float Angle;
        public int Width;
        public int Height;
    }

    public Skeleton(Vector2 position, List<Rectangle> collisionRects, List<Rectangle> platformRects, float gravity)
    {
        this.skeletonPosition = position;
        this.skeletonSpawnPosition = position;
        this.skeletonRect = new Rectangle((int)skeletonPosition.X, (int)skeletonPosition.Y,
                                          this.skeletonWidth, this.skeletonHeight);
        this.skeletonAnchorPoint = new Vector2(skeletonPosition.X + skeletonWidth / 2, skeletonPosition.Y + skeletonHeight / 2);
        this._collisionRects = collisionRects;
        this._platformRects = platformRects;
        this.arrowList = new List<Arrow>();
        this.skeletonGravity = gravity;
        this.skeletonColor = Color.Gray;
        this.previousPosition = skeletonPosition;
    }

    public void Update(GameTime gameTime, Player player, int mapWidth, int mapHeight)
    {
        this.skeletonRect = new Rectangle((int)skeletonPosition.X, (int)skeletonPosition.Y,
                                          this.skeletonWidth, this.skeletonHeight);
        skeletonAnchorPoint = new Vector2(skeletonPosition.X + skeletonWidth / 2, skeletonPosition.Y + skeletonHeight / 3);
        Vector2 direction = player.AnchorPoint - this.skeletonAnchorPoint;
        if (direction != Vector2.Zero)
        {
            direction.Normalize();
        }

        if (!brokenBones)
        {
            if (!player.dead)
                UpdatePlayerInteraction(gameTime, direction, player);

        }
        UpdateArrow(player, mapWidth, mapHeight);
        UpdateMovement();
        UpdateSkeletonRectangle();
        UpdateCollision();
        AnimateSprite(gameTime);

    }
    public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture)
    {
        if (currentSpriteSheet == null)
        {
            currentSpriteSheet = skeletonIdle;
        }
        if (facingDirection == "right")
        {
            spriteBatch.Draw(
                                currentSpriteSheet,
                                new Vector2(skeletonPosition.X - 50, skeletonPosition.Y - 64),
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
                                new Vector2(skeletonPosition.X - 50, skeletonPosition.Y - 64),
                                currentFrame,
                                Color.White,
                                0f,
                                Vector2.Zero,
                                1,
                                SpriteEffects.FlipHorizontally,  // mirrors left-right
                                0f
                            );
        }
        DrawBowAndArrow(spriteBatch, pixelTexture);
    }
    public void RangedAttack(GameTime gameTime, Vector2 direction)
    {
        Vector2 bowLocation = skeletonAnchorPoint;

        Bow bow = new Bow();
        bow.Position = skeletonAnchorPoint + direction * skeletonAnchorRadius;
        bow.Angle = MathF.Atan2(direction.Y, direction.X) + MathF.PI;
        bow.Width = 10;
        bow.Height = 32;
        this.bow = bow;

        arrowTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        nockedArrow = new Arrow();
        nockedArrow.Direction = direction;
        nockedArrow.Position = bow.Position;
        nockedArrow.Angle = bow.Angle;
        nockedArrow.Width = 20;
        nockedArrow.Height = 5;

        if (arrowTimer >= 3f)
        {
            Game1.bowRelease.Play();
            arrowList.Add(nockedArrow);
            arrowTimer = 0f;
        }
    }
    public void UpdateArrow(Player player, int mapWidth, int mapHeight)
    {
        for (int i = arrowList.Count - 1; i >= 0; i--)
        {
            Arrow arrow = arrowList[i];
            arrow.Position += arrow.Direction * arrowSpeed;
            arrowList[i] = arrow;

            Rectangle arrowRect = new Rectangle(
                (int)arrow.Position.X - arrow.Width / 2,
                (int)arrow.Position.Y - arrow.Height / 2,
                arrow.Width, arrow.Height);

            bool hit = false;
            foreach (Rectangle rect in _collisionRects)
            {
                if (arrowRect.Intersects(rect))
                {
                    hit = true;
                    break;
                }
            }

            if (arrowRect.Intersects(player.Rect))
            {
                player.TakeHit(arrow.Direction, false, 1f);
                hit = true;
            }

            if (hit || arrow.Position.X < 0 || arrow.Position.X > mapWidth ||
                arrow.Position.Y < 0 || arrow.Position.Y > mapHeight)
                arrowList.RemoveAt(i);
        }
    }
    public void MeleeAttack(GameTime gameTime, Vector2 direction, Player player)
    {
        meleeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (meleeTimer >= 1.5f && meleeTimer <= 1.75f)
        {
            if (!isMelee) // first frame of the stab window
            {
                frameCounter = 2;
                frameTimer = 0f;
            }
            isMelee = true;
            if (meleeTimer < 1f)
            {
                float lungeProgress = (meleeTimer - 1.5f) / 0.125f;
                dagger.Position = skeletonAnchorPoint + direction * (skeletonAnchorRadius + skeletonAnchorRadius * 2.5f * lungeProgress) / 3;
            }
            else
            {
                float pullProgress = (meleeTimer - 1.625f) / 0.125f;
                dagger.Position = skeletonAnchorPoint + direction * (skeletonAnchorRadius + skeletonAnchorRadius * 2.5f * (1f - pullProgress)) / 3;
            }

            Rectangle daggerRect = new Rectangle(
                (int)dagger.Position.X - dagger.Width / 2,
                (int)dagger.Position.Y - dagger.Height / 2,
                dagger.Width, dagger.Height);

            if (daggerRect.Intersects(player.Rect))
                player.TakeHit(direction, false, 1f);
        }

        dagger.Angle = MathF.Atan2(direction.Y, direction.X) + MathF.PI;
        dagger.Width = 20;
        dagger.Height = 5;
    }
    public void UpdatePlayerInteraction(GameTime gameTime, Vector2 direction, Player player)
    {
        if (!brokenBones)
        {
            if (meleeTimer > 0f)
            {
                meleeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (meleeTimer >= 1.75f)
                {
                    isMelee = false;
                    meleeTimer = 0f;
                }
            }

            bool canSee = HasLineOfSight(skeletonAnchorPoint, player.AnchorPoint);
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
                if (Vector2.DistanceSquared(player.Position, skeletonAnchorPoint) <= 340 * 340 &&
                Vector2.DistanceSquared(player.Position, skeletonAnchorPoint) >= 95 * 95)
                {
                    isMelee = false;
                    bowDrawn = true;
                    RangedAttack(gameTime, direction);
                }
                else if (Vector2.DistanceSquared(player.Position, skeletonAnchorPoint) < 95 * 95 &&
                         Vector2.DistanceSquared(player.Position, skeletonAnchorPoint) >= 50 * 50)
                {
                    isMelee = false;
                    bowDrawn = false;
                    this.skeletonPosition.X += direction.X * skeletonSpeed;
                    if (Game1.bonesInstance.State != SoundState.Playing)
                        Game1.bonesInstance.Play();
                }
                else if (Vector2.DistanceSquared(player.Position, skeletonAnchorPoint) <= 50 * 50)
                {
                    MeleeAttack(gameTime, direction, player);
                }
                else
                {
                    bowDrawn = false;
                }
            }
            else
            {
                bowDrawn = false;
            }
            if (skeletonRect.Intersects(player.Rect))
                player.TakeHit(direction, false, 0.5f);
        }
    }
    public void UpdateMovement()
    {
        // --- Knockback with collision ---
        if (knockbackVelocity != Vector2.Zero)
        {
            // Step X
            skeletonPosition.X += knockbackVelocity.X;
            Rectangle testRect = new Rectangle((int)skeletonPosition.X, (int)skeletonPosition.Y, skeletonWidth, skeletonHeight);
            foreach (Rectangle rect in _collisionRects)
            {
                if (testRect.Intersects(rect))
                {
                    // Push back out and kill horizontal knockback
                    if (knockbackVelocity.X > 0)
                        skeletonPosition.X = rect.Left - skeletonWidth;
                    else
                        skeletonPosition.X = rect.Right;
                    knockbackVelocity.X = 0;
                    break;
                }
            }

            // Step Y
            skeletonPosition.Y += knockbackVelocity.Y;
            testRect = new Rectangle((int)skeletonPosition.X, (int)skeletonPosition.Y, skeletonWidth, skeletonHeight);
            foreach (Rectangle rect in _collisionRects)
            {
                if (testRect.Intersects(rect))
                {
                    if (knockbackVelocity.Y > 0)
                        skeletonPosition.Y = rect.Top - skeletonHeight;
                    else
                        skeletonPosition.Y = rect.Bottom;
                    knockbackVelocity.Y = 0;
                    break;
                }
            }

            knockbackVelocity *= 0.8f;
            if (knockbackVelocity.LengthSquared() < 0.01f)
                knockbackVelocity = Vector2.Zero;
        }

        // --- skeletonGravity ---
        if (!isGrounded)
            skeletonVelocityY += skeletonGravity;
    }
    public void UpdateSkeletonRectangle()
    {
        if (brokenBones)
        {
            Rectangle brokenBonesRect = skeletonRect;
            skeletonHeight = 32;
            skeletonWidth = 64;
        }
        skeletonRect.X = (int)skeletonPosition.X;
        skeletonRect.Y = (int)skeletonPosition.Y;
    }
    public void UpdateCollision()
    {
        Rectangle skeletonRect = new Rectangle((int)skeletonPosition.X, (int)skeletonPosition.Y,
                                             skeletonWidth, skeletonHeight);
        foreach (Rectangle rectangle in _collisionRects)
        {
            if (!skeletonRect.Intersects(rectangle))
                continue;


            if (skeletonPosition.X + skeletonWidth / 2 < rectangle.Center.X)
                skeletonPosition.X = rectangle.Left - skeletonWidth;
            else
                skeletonPosition.X = rectangle.Right;

            skeletonRect = new Rectangle((int)skeletonPosition.X, (int)skeletonPosition.Y,
                                       skeletonWidth, skeletonHeight);
        }
        foreach (Rectangle rectangle in _platformRects)
        {
            if (!skeletonRect.Intersects(rectangle))
                continue;


            if (skeletonPosition.X + skeletonWidth / 2 < rectangle.Center.X)
                skeletonPosition.X = rectangle.Left - skeletonWidth;
            else
                skeletonPosition.X = rectangle.Right;

            skeletonRect = new Rectangle((int)skeletonPosition.X, (int)skeletonPosition.Y,
                                       skeletonWidth, skeletonHeight);
        }

        // --- Vertical move + collision ---
        skeletonPosition.Y += skeletonVelocityY;

        bool landedThisFrame = false;
        skeletonRect = new Rectangle((int)skeletonPosition.X, (int)skeletonPosition.Y,
                                   skeletonWidth, skeletonHeight);
        foreach (Rectangle rectangle in _collisionRects)
        {
            if (!skeletonRect.Intersects(rectangle))
                continue;

            if (skeletonVelocityY >= 0)
            {
                skeletonPosition.Y = rectangle.Top - skeletonHeight;
                skeletonVelocityY = 0f;
                landedThisFrame = true;
            }
            else
            {
                skeletonPosition.Y = rectangle.Bottom;
                skeletonVelocityY = 0f;
            }

            skeletonRect = new Rectangle((int)skeletonPosition.X, (int)skeletonPosition.Y,
                                       skeletonWidth, skeletonHeight);
        }
        foreach (Rectangle rectangle in _platformRects)
        {
            if (!skeletonRect.Intersects(rectangle))
                continue;

            if (skeletonVelocityY >= 0)
            {
                skeletonPosition.Y = rectangle.Top - skeletonHeight;
                skeletonVelocityY = 0f;
                landedThisFrame = true;
            }
            else
            {
                skeletonPosition.Y = rectangle.Bottom;
                skeletonVelocityY = 0f;
            }

            skeletonRect = new Rectangle((int)skeletonPosition.X, (int)skeletonPosition.Y,
                                       skeletonWidth, skeletonHeight);
        }

        // Ground probe — 1px below feet to detect standing still or walking off an edge
        Rectangle groundProbe = new Rectangle((int)skeletonPosition.X,
                                              (int)skeletonPosition.Y + skeletonHeight,
                                              skeletonWidth, 1);
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
    public void TakeHit(Vector2 hitDirection, float force = 8f)
    {
        if (hitDirection.Y > 0)
        {
            knockbackVelocity.X = hitDirection.X * force;
        }
        else
        {
            knockbackVelocity = hitDirection * force;
        }
        skeletonVelocityY = -4f;
        isGrounded = false;
        brokenBones = true;
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
    public void DrawBowAndArrow(SpriteBatch spriteBatch, Texture2D pixelTexture)
    {
        if (bowDrawn && !brokenBones)
        {
            float radiusDirection = facingDirection == "left" ? -skeletonAnchorRadius : skeletonAnchorRadius;

            Vector2 originOffset = new Vector2(currentBowFrame.Width / 2f, currentBowFrame.Height / 2f)
                                 + new Vector2(0, 0);

            spriteBatch.Draw(
                bowTexture,
                skeletonAnchorPoint,
                currentBowFrame,
                Color.White,
                this.bow.Angle,
                originOffset,
                1f,
                facingDirection == "left"
                                        ? SpriteEffects.FlipHorizontally
                                        : SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically,
                0f
            );

            //if (arrowTimer >= 0.5f && arrowTimer <= 3f)
            //{
            //    spriteBatch.Draw(arrowTexture, nockedArrow.Position, null,
            //               Color.White, nockedArrow.Angle,
            //               new Vector2(arrowTexture.Width / 2f, arrowTexture.Height / 2f),
            //               new Vector2(1, 1),
            //               SpriteEffects.None, 0f);
            //}

        }

        foreach (Arrow arrow in arrowList)
        {
            spriteBatch.Draw(arrowTexture, arrow.Position, null,
                            Color.White, arrow.Angle,
                            new Vector2(arrowTexture.Width / 2f, arrowTexture.Height / 2f),
                            new Vector2(1, 1),
                            SpriteEffects.None, 0f);
        }
    }
    void AnimateSprite(GameTime gameTime)
    {
        if (!brokenBones)
        {
            if (previousPosition == skeletonPosition)
            {
                if (isMelee)
                {
                    currentSpriteSheet = skeletonStab;
                    float frameDuration = 1f / 16f;
                    frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (frameTimer >= frameDuration)
                    {
                        frameTimer = 0;
                        frameCounter++;
                        // Clamp at last frame so it holds until isMelee goes false
                        int stabFrameCount = (skeletonStab.Width / frameWidth) - 1;
                        if (frameCounter > stabFrameCount)
                            frameCounter = stabFrameCount;
                    }
                    currentFrame = GetFrameRect(frameCounter, skeletonStab);
                }
                else if (bowDrawn)
                {
                    currentSpriteSheet = skeletonBow;
                    float frameDuration = 1f / 16f;
                    frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    shotTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (frameTimer >= frameDuration && frameCounter < 9)
                    {
                        frameTimer = 0;
                        frameCounter++;
                    }
                    if (frameCounter >= 9 && shotTimer >= arrowTimer)
                    {
                        frameCounter++;
                        if (frameCounter > 12)
                        {
                            frameCounter = 0;
                            frameTimer = 0f;
                            shotTimer = 0f;
                        }
                    }
                    currentFrame = GetFrameRect(frameCounter, skeletonBow);
                    currentBowFrame = GetFrameRect(frameCounter, bowTexture);
                }
                else
                {
                    currentSpriteSheet = skeletonIdle;
                    float frameDuration = 1f / 16f;
                    frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (frameTimer >= frameDuration)
                    {
                        frameTimer = 0;
                        frameCounter++;
                        if (frameCounter > 6)
                            frameCounter = 0;
                    }
                    currentFrame = GetFrameRect(frameCounter, skeletonIdle);
                }
            }
            else
            {
                currentSpriteSheet = skeletonWalk;
                float frameDuration = (float)1 / 16;
                frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (frameTimer >= frameDuration)
                {
                    frameTimer = 0;
                    frameCounter++;
                    if (frameCounter > 6)
                    {
                        frameCounter = 0;
                    }
                }
                currentFrame = GetFrameRect(frameCounter, skeletonWalk);
            }
        }
        else
        {
            currentSpriteSheet = brokenBonesTexture;
            currentFrame = GetFrameRect(0, brokenBonesTexture);
        }
        previousPosition = skeletonPosition;
    }
    public Rectangle GetFrameRect(int frame, Texture2D spriteSheet)
    {
        int columns = spriteSheet.Width / frameWidth;
        int column = frame % columns;
        int row = frame / columns;
        return new Rectangle(column * frameWidth, row * frameHeight, frameWidth, frameHeight);
    }
}