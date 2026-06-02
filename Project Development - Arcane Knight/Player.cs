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

public class Player
{
    private readonly int playerWidth = 28;
    private readonly int playerHeight = 56;
    private int playerSpeed = 3;
    private float playerGravity = 0.5f;
    private const float playerJumpForce = -11f;
    private float playerVelocityY = 0f;
    private bool isGrounded = false;
    private Vector2 cursorLocation;
    private Vector2 spawnPosition;
    public Rectangle Rect;
    public float playerHP = 20;
    public int gold = 0;
    public int potionMax = 3;
    public int potions = 3;
    private KeyboardState currentKeyboardState, previousKeyboardState;


    public Vector2 AnchorPoint;
    private readonly int playerAnchorRadius = 20;

    private readonly int whipSegments = 7;
    private readonly int segmentHeight = 20;
    private readonly int segmentWidth = 5;
    private Vector2 _whipTipVelocity = Vector2.Zero;
    private List<WhipSegment> whipSegmentList;

    private float lightningFlickerTimer;
    private float lightningPersistTimer;
    private float lightningFlickerInterval = 0f;
    private bool flickerVisibility;
    private bool isDrawingLightning;
    private List<LightningSegment> lightningSegmentList;
    private List<LightningSegment> committedLightningSegmentList;

    private float missileSpeed = 0.45f;
    private List<Missile> missileList;

    public Vector2 Position;
    private Vector2 knockbackVelocity = Vector2.Zero;
    private MouseState previousMouse;
    private readonly List<Rectangle> _collisionRects;
    private readonly List<Rectangle> _platformRects;
    private float iFramesTimer;
    private bool iFrames = false;
    private float teleportCooldownTimer;
    private bool teleportCooldown = false;
    public bool dead = false;

    public bool hasWhip;
    public bool hasMissile;
    public bool hasLightning;
    public bool hasTeleport;
    public bool hasTorch = false;
    public Texture2D idle;
    public Texture2D run;
    public Texture2D jump;
    public Texture2D shootMissile;
    public Texture2D currentSpriteSheet;
    public Texture2D whipWalk;
    public Texture2D whipArm;
    public Texture2D missile;
    public Texture2D whipHandle;
    public Texture2D whipSegmentTexture;
    public Texture2D whipTip;
    Color missileColor;
    Rectangle currentFrame;
    int frameWidth = 128;
    int frameHeight = 128;
    Vector2 previousPosition;
    float frameTimer = 0;
    int frameCounter = 0;
    String direction = "left";
    bool shootingMissile = false;

    public List<Game1.Key> playerKeyInventory;

    private struct WhipSegment
    {
        public Vector2 Position;
        public float Angle;
        public Vector2 PreviousLocation;
        public bool _whipGrounded;
    }

    private struct LightningSegment
    {
        public Vector2 PointA;
        public Vector2 PointB;
    }

    private struct Missile
    {
        public Vector2 Position;
        public Rectangle Rect;
        public Vector2 Direction;
    }

    public Player(Vector2 startPosition, List<Rectangle> collisionRects, List<Rectangle> platformRects,
                  float gravity, bool whip, bool missile, bool lightning, bool teleport)
    {
        Position = startPosition;
        spawnPosition = startPosition;
        Rect = new Rectangle((int)Position.X,
                                           (int)Position.Y,
                                           playerWidth,
                                           playerHeight);
        _collisionRects = collisionRects;
        _platformRects = platformRects;
        playerGravity = gravity;
        whipSegmentList = new List<WhipSegment>();
        lightningSegmentList = new List<LightningSegment>();
        committedLightningSegmentList = new List<LightningSegment>();
        missileList = new List<Missile>();
        playerKeyInventory = new List<Game1.Key>();
        cursorLocation = new Vector2(0, 0);
        AnchorPoint = new Vector2(Position.X + playerWidth / 3, Position.Y + playerHeight / 3);
        hasWhip = whip;
        hasMissile = missile;
        hasLightning = lightning;
        hasTeleport = teleport;
        previousPosition = startPosition;
    }

    public void Update(GameTime gameTime, KeyboardState keyboardState, MouseState mouseState,
                       Camera camera, List<Skeleton> skeletons, List<Zombie> zombies, List<Ghost> ghosts, List<KingBoss> boss, bool shopping)
    {
        cursorLocation = Vector2.Transform(new Vector2(mouseState.X, mouseState.Y),
                                                       Matrix.Invert(camera.GetTransformationMatrix()));

        Vector2 mouseDirection = cursorLocation - AnchorPoint;
        if (mouseDirection.X >= 0 && whipSegmentList.Count <= 0)
        {
            direction = "right";
        }
        else if (mouseDirection.X < 0 && whipSegmentList.Count <= 0)
        {
            direction = "left";
        }
        // Anchor Point is in the center of the player Rect
        AnchorPoint = new Vector2(Position.X + playerWidth / 2, Position.Y + playerHeight / 3);
        if (whipSegmentList.Count > 0)
        {
            if (direction == "left")
            {
                AnchorPoint.X = AnchorPoint.X + 7;
            }
            else if (direction == "right")
            {
                AnchorPoint.X = AnchorPoint.X - 7;
            }
            AnchorPoint.Y = AnchorPoint.Y + 5;
        }
        previousKeyboardState = currentKeyboardState; // save last frame
        currentKeyboardState = Keyboard.GetState();

        MouseState prevMouse = previousMouse;
        previousMouse = mouseState;

        if (playerHP <= 0f)
        {
            dead = true;
        }
        if (whipSegmentList.Count > 0)
        {
            playerSpeed = 2;
        }
        else
        {
            playerSpeed = 3;
        }

        if (!dead)
        {
            if (Game1.lightningInstance.State != SoundState.Playing && committedLightningSegmentList.Count > 0)
                Game1.lightningInstance.Play();
            int addHealth;
            if (iFrames)
            {
                iFramesTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (iFramesTimer >= 1f)
                {
                    iFrames = false;
                    iFramesTimer = 0f;
                }
            }
            if (teleportCooldown)
            {
                teleportCooldownTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (teleportCooldownTimer >= 1f)
                {
                    teleportCooldown = false;
                    teleportCooldownTimer = 0f;
                }
            }

            if (potions > 0 && playerHP < 20 && currentKeyboardState.IsKeyDown(Keys.R) && previousKeyboardState.IsKeyUp(Keys.R))
            {
                if (Game1.potionInstance.State != SoundState.Playing)
                    Game1.potionInstance.Play();
                potions -= 1;
                if (playerHP > 15)
                {
                    addHealth = 20 - (int)playerHP;
                }
                else
                {
                    addHealth = 5;
                }
            }
            else
            {
                addHealth = 0;
            }

            if (!shopping)
            {
                UpdateMovement(keyboardState, previousKeyboardState, mouseState, camera);
                if (hasWhip)
                {
                    UpdateWhip(keyboardState, mouseState, prevMouse, camera, skeletons, zombies, boss);
                }
                if (hasLightning)
                {
                    UpdateLightning(gameTime, keyboardState, prevMouse, mouseState, camera, skeletons, zombies, ghosts, boss);
                }
                if (hasMissile)
                {
                    UpdateMissile(keyboardState, mouseState, prevMouse, camera, skeletons, zombies, ghosts, boss);
                }
                UpdatePlayerRectangle();
                UpdateCollision();

                playerHP += addHealth;
                if (playerHP > 20)
                {
                    playerHP = 20;
                }
                AnimateSprite(gameTime);
            }
        }
        float cycleDuration = 3f; // seconds for one full rainbow loop
        float t = (float)(gameTime.TotalGameTime.TotalSeconds % cycleDuration) / cycleDuration;
        missileColor = GetRainbowColor(t);
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture)
    {
        if (currentSpriteSheet == null)
        {
            currentSpriteSheet = idle;
        }
        if (!dead)
        {

            bool visible = !iFrames || ((int)(iFramesTimer / 0.1f) % 2 == 0);

            if (visible)
            {
                if (direction == "left")
                {
                    spriteBatch.Draw(
                                currentSpriteSheet,
                                new Vector2(Position.X - 50, Position.Y - 70),
                                currentFrame,
                                Color.White,
                                0f,
                                Vector2.Zero,
                                1,
                                SpriteEffects.FlipHorizontally,  // mirrors left-right
                                0f
                            );
                }
                else
                {
                    spriteBatch.Draw(
                                currentSpriteSheet,
                                new Vector2(Position.X - 50, Position.Y - 70),
                                currentFrame,
                                Color.White,
                                0f,
                                Vector2.Zero,
                                1,
                                SpriteEffects.None,  // mirrors left-right
                                0f
                            );
                }
            }

            DrawWhip(spriteBatch, pixelTexture);
            DrawLightning(spriteBatch, pixelTexture);
            DrawMissile(spriteBatch, pixelTexture);
        }
    }

    public void UpdateMovement(KeyboardState keyboardState, KeyboardState prevKey, MouseState mouseState, Camera camera)
    {
        // --- Jump ---
        if (isGrounded && keyboardState.IsKeyDown(Keys.Space)
            && !prevKey.IsKeyDown(Keys.Space))
        {
            playerVelocityY = playerJumpForce;
            isGrounded = false;
        }

        if (Position.Y >= 2341 || Position.Y <= 0 || Position.X >= 2208 || Position.X <= 0)
        {
            Position = spawnPosition;
        }

        // --- playerGravity ---
        if (!isGrounded)
            playerVelocityY += playerGravity;


        // --- Knockback with collision ---
        if (knockbackVelocity != Vector2.Zero)
        {
            // Step X
            Position.X += knockbackVelocity.X;
            Rectangle testRect = new Rectangle((int)Position.X, (int)Position.Y, playerWidth, playerHeight);
            foreach (Rectangle rect in _collisionRects)
            {
                if (testRect.Intersects(rect))
                {
                    // Push back out and kill horizontal knockback
                    if (knockbackVelocity.X > 0)
                        Position.X = rect.Left - playerWidth;
                    else
                        Position.X = rect.Right;
                    knockbackVelocity.X = 0;
                    break;
                }
            }

            // Step Y
            Position.Y += knockbackVelocity.Y;
            testRect = new Rectangle((int)Position.X, (int)Position.Y, playerWidth, playerHeight);
            foreach (Rectangle rect in _collisionRects)
            {
                if (testRect.Intersects(rect))
                {
                    if (knockbackVelocity.Y > 0)
                        Position.Y = rect.Top - playerHeight;
                    else
                        Position.Y = rect.Bottom;
                    knockbackVelocity.Y = 0;
                    break;
                }
            }

            knockbackVelocity *= 0.8f;
            if (knockbackVelocity.LengthSquared() < 0.01f)
                knockbackVelocity = Vector2.Zero;

            if (Game1.hurtInstance.State != SoundState.Playing)
                Game1.hurtInstance.Play();
        }

        // --- Horizontal move + collision ---
        if (keyboardState.IsKeyDown(Keys.A))
        {
            Position.X -= playerSpeed;
            direction = "left";
            if (Game1.footstepInstance.State != SoundState.Playing && isGrounded)
                Game1.footstepInstance.Play();
        }
        if (keyboardState.IsKeyDown(Keys.D))
        {
            Position.X += playerSpeed;
            direction = "right";
            if (Game1.footstepInstance.State != SoundState.Playing && isGrounded)
                Game1.footstepInstance.Play();
        }


        // --- Teleport ---
        if (keyboardState.IsKeyDown(Keys.LeftAlt) && mouseState.RightButton == ButtonState.Pressed && !teleportCooldown
            && hasTeleport)
        {
            cursorLocation = Vector2.Transform(new Vector2(mouseState.X, mouseState.Y),
                                                       Matrix.Invert(camera.GetTransformationMatrix()));

            Vector2 destination = new Vector2(cursorLocation.X - playerWidth / 2, cursorLocation.Y - playerHeight / 2);
            Rectangle destinationRect = new Rectangle((int)destination.X, (int)destination.Y, playerWidth, playerHeight);

            // Check destination doesn't overlap a collision rect
            bool destinationBlocked = false;
            foreach (Rectangle rect in _collisionRects)
            {
                if (destinationRect.Intersects(rect))
                {
                    destinationBlocked = true;
                    break;
                }
            }

            // Raycast: step along the path and check for collision rects
            if (!destinationBlocked)
            {
                Vector2 rayStart = Position;
                Vector2 rayEnd = destination;
                Vector2 rayDir = rayEnd - rayStart;
                float rayLength = rayDir.Length();

                if (rayLength > 0f)
                {
                    rayDir /= rayLength;
                    int steps = (int)(rayLength / (playerWidth / 2f)) + 1;

                    for (int s = 1; s <= steps; s++)
                    {
                        float t = (rayLength / steps) * s;
                        Vector2 samplePos = rayStart + rayDir * t;
                        Rectangle sampleRect = new Rectangle((int)samplePos.X, (int)samplePos.Y, playerWidth, playerHeight);

                        foreach (Rectangle rect in _collisionRects)
                        {
                            if (sampleRect.Intersects(rect))
                            {
                                destinationBlocked = true;
                                break;
                            }
                        }
                        if (destinationBlocked) break;
                    }
                }
            }

            if (!destinationBlocked)
            {
                if (Game1.teleportInstance.State != SoundState.Playing)
                    Game1.teleportInstance.Play();
                Position = destination;
                teleportCooldown = true;
            }
        }
    }
    public void UpdateWhip(KeyboardState keyboardState, MouseState mouseState, MouseState prevMouse,
                           Camera camera, List<Skeleton> skeletons, List<Zombie> zombies, List<KingBoss> boss)
    {
        // This section uses Verlet Integration (which I barely understand) to handle the whip
        if (mouseState.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released
            && !isDrawingLightning)
        {
            Vector2 whipHandleLocation = AnchorPoint;
            // We invert the camera transformation matrix to get in world coordinates, not screen coordinates
            cursorLocation = Vector2.Transform(new Vector2(mouseState.X, mouseState.Y),
                                                       Matrix.Invert(camera.GetTransformationMatrix()));
            Vector2 whipDirection = cursorLocation - AnchorPoint;
            if (whipDirection.X > 0)
            {
                direction = "left";
            }
            else if (whipDirection.X < 0)
            {
                direction = "right";
            }
            if (whipDirection != Vector2.Zero)
            {
                // Normalize the whip direction to be a set distance away from the anchor point
                whipDirection = Vector2.Normalize(whipDirection);
                whipHandleLocation = AnchorPoint + whipDirection * playerAnchorRadius;
                for (int i = 0; i < whipSegments; i++)
                {
                    WhipSegment whipSegment = new WhipSegment();
                    whipSegment.Position = whipHandleLocation;
                    // Angle helps the whip segment face the direction the mouse is
                    // Have to divide by 2 to get the slope of the tangent to the right side (towards the mouse,
                    // not in relation to it)
                    whipSegment.Angle = MathF.Atan2(whipDirection.Y, whipDirection.X) + MathF.PI / 2;
                    // Each segment relies on a previous location
                    whipSegment.PreviousLocation = whipHandleLocation;
                    whipSegmentList.Add(whipSegment);
                }
            }
        }
        else if (mouseState.LeftButton == ButtonState.Pressed
                && prevMouse.LeftButton == ButtonState.Pressed
                && !isDrawingLightning)
        {

            Vector2 whipSegmentLocation = AnchorPoint;
            cursorLocation = Vector2.Transform(new Vector2(mouseState.X, mouseState.Y),
                                                       Matrix.Invert(camera.GetTransformationMatrix()));
            Vector2 prevCursorLocation = Vector2.Transform(new Vector2(prevMouse.Position.X,
                                                                       prevMouse.Position.Y),
                                                           Matrix.Invert(camera.GetTransformationMatrix()));
            Vector2 mouseDelta = (cursorLocation - prevCursorLocation);
            Vector2 whipDirection = cursorLocation - AnchorPoint;

            // set the whip handle location to be at a certain point within a circle
            // around the player anchor point
            if (whipDirection.Length() > playerAnchorRadius)
            {
                whipDirection = Vector2.Normalize(whipDirection);
                whipSegmentLocation = AnchorPoint + whipDirection * playerAnchorRadius;
            }
            else
            {
                whipSegmentLocation = AnchorPoint + whipDirection;
            }
            // First loop - Verlet only, runs once
            for (int i = 0; i < whipSegments; i++)
            {
                // Calculating collisions before everything helps the whip stutter less
                WhipSegment segment = whipSegmentList[i];
                segment._whipGrounded = false;

                Rectangle segmentGroundProbe = new Rectangle(
                    (int)segment.Position.X - segmentWidth / 2,
                    (int)segment.Position.Y + segmentHeight / 2,
                    segmentWidth, 1);

                foreach (Rectangle rect in _collisionRects)
                {
                    if (segmentGroundProbe.Intersects(rect))
                    {
                        segment._whipGrounded = true;
                        break;
                    }
                }
                if (i != 0)
                {
                    // Set each segment position after the whip handle 
                    Vector2 previousPosition = segment.Position;
                    segment.Position = segment.Position + (segment.Position - segment.PreviousLocation)
                                                        * 0.75f
                                                        + (segment._whipGrounded ? Vector2.Zero : new Vector2(0, playerGravity));
                    segment.PreviousLocation = previousPosition;

                    if (i == whipSegments - 1)
                        _whipTipVelocity = segment.Position - previousPosition;
                }
                else
                {
                    segment.Position = whipSegmentLocation;
                    segment.Angle = MathF.Atan2(whipDirection.Y, whipDirection.X) + MathF.PI / 2;
                    segment.PreviousLocation = whipSegmentLocation - mouseDelta;
                }
                whipSegmentList[i] = segment;
            }

            // Second loop - constraint solving, runs multiple times
            for (int iteration = 0; iteration < 1; iteration++)
            {
                for (int i = 0; i < whipSegments; i++)
                {
                    // new Anchor point for each segment
                    Vector2 anchorPoint;
                    Vector2 segmentDirection;
                    WhipSegment segment = whipSegmentList[i];
                    if (i == 0)
                        anchorPoint = whipSegmentLocation;
                    else
                    {
                        // Determine the direction using the angle of the previous segment
                        // This mimics the tension of whip as in reality it's connected to itself
                        Vector2 direction = new Vector2(MathF.Sin(whipSegmentList[i - 1].Angle),
                                                        -MathF.Cos(whipSegmentList[i - 1].Angle));
                        anchorPoint = whipSegmentList[i - 1].Position + direction * (segmentHeight / 2);
                    }

                    // All the same math we've been doing so far
                    segmentDirection = segment.Position - anchorPoint;
                    if (segmentDirection != Vector2.Zero)
                    {
                        segmentDirection = Vector2.Normalize(segmentDirection);
                        segment.Angle = MathF.Atan2(segmentDirection.Y, segmentDirection.X) + MathF.PI / 2;
                    }
                    // The dividing number helps get rid of the gap between the whip segments
                    segment.Position = anchorPoint + segmentDirection * segmentHeight / 2f;

                    // Now collision handling for the platform rects
                    foreach (Rectangle rect in _collisionRects)
                    {
                        Rectangle segmentRect = new Rectangle(
                            (int)segment.Position.X - segmentWidth / 2,
                            (int)segment.Position.Y - segmentHeight / 2,
                            segmentWidth,
                            segmentHeight);

                        if (!segmentRect.Intersects(rect))
                            continue;

                        float overlapLeft = segmentRect.Right - rect.Left;
                        float overlapRight = rect.Right - segmentRect.Left;
                        float overlapTop = segmentRect.Bottom - rect.Top;
                        float overlapBottom = rect.Bottom - segmentRect.Top;

                        float minOverlapX = Math.Min(overlapLeft, overlapRight);
                        float minOverlapY = Math.Min(overlapTop, overlapBottom);

                        if (minOverlapX < minOverlapY)
                        {
                            if (overlapLeft < overlapRight)
                            {
                                segment.Position.X = rect.Left - segmentWidth / 2;
                                segment.PreviousLocation.X = segment.Position.X;
                                segment._whipGrounded = true;
                            }

                            else
                            {
                                segment.Position.X = rect.Right + segmentWidth / 2;
                                segment.PreviousLocation.X = segment.Position.X;
                                segment._whipGrounded = true;
                            }
                        }
                        else
                        {
                            if (overlapTop < overlapBottom)
                            {
                                segment.Position.Y = rect.Top - segmentHeight / 2;
                                segment.PreviousLocation.Y = segment.Position.Y;
                                segment._whipGrounded = true;
                            }
                            else
                            {
                                segment.Position.Y = rect.Bottom + segmentHeight / 2;
                                segment.PreviousLocation.Y = segment.Position.Y;
                                segment._whipGrounded = true;
                            }
                        }

                        segment.PreviousLocation.Y = segment.Position.Y;
                    }

                    whipSegmentList[i] = segment;
                }
            }
            if (whipSegmentList.Count == whipSegments)
            {
                WhipSegment tip = whipSegmentList[whipSegments - 1];
                Rectangle tipRect = new Rectangle(
                    (int)tip.Position.X - segmentWidth / 2,
                    (int)tip.Position.Y - segmentHeight / 2,
                    segmentWidth, segmentHeight);

                float tipSpeed = _whipTipVelocity.Length();
                // Minimum speed threshold so a barely-moving whip does nothing
                if (tipSpeed > 24f)
                {
                    if (Game1.whipInstance.State != SoundState.Playing)
                        Game1.whipInstance.Play();

                    //Debug.WriteLine(tipSpeed);
                    // Scale force: tipSpeed 3 → force 4, tipSpeed 12+ → force 14 (capped)
                    float hitForce = MathHelper.Clamp(tipSpeed * 0.166f, 4f, 14f);
                    foreach (Skeleton skeleton in skeletons)
                    {
                        if (tipRect.Intersects(skeleton.skeletonRect))
                        {
                            Vector2 hitDir = skeleton.skeletonPosition - tip.Position;
                            if (hitDir != Vector2.Zero) hitDir.Normalize();
                            if (!skeleton.brokenBones)
                                skeleton.TakeHit(hitDir, hitForce);
                        }
                    }
                    foreach (Zombie zombie in zombies)
                    {
                        if (tipRect.Intersects(zombie.zombieRect))
                        {
                            if (!zombie.zombieGas)
                            {
                                Vector2 hitDir = zombie.zombiePosition - tip.Position;
                                if (hitDir != Vector2.Zero) hitDir.Normalize();
                                zombie.TakeHit(hitDir, hitForce);
                            }
                        }
                    }
                    foreach (KingBoss enemy in boss)
                    {
                        if (tipRect.Intersects(enemy.bossRect))
                        {
                            Vector2 hitDir = enemy.bossPosition - tip.Position;
                            if (hitDir != Vector2.Zero) hitDir.Normalize();

                            enemy.TakeHit(hitDir, 1, hitForce);
                        }
                    }
                }
            }
        }
        else if (mouseState.LeftButton == ButtonState.Released)
        {
            whipSegmentList.Clear();
        }
    }
    public void UpdateLightning(GameTime gameTime, KeyboardState keyboardState, MouseState prevMouse,
                                MouseState mouseState, Camera camera, List<Skeleton> skeletons, List<Zombie> zombies, List<Ghost> ghosts, List<KingBoss> boss)
    {
        if (keyboardState.IsKeyDown(Keys.E) && mouseState.RightButton == ButtonState.Pressed
            && prevMouse.RightButton == ButtonState.Released && whipSegmentList.Count == 0
            && committedLightningSegmentList.Count == 0 && previousPosition == Position)
        {
            // I want the lightning to draw while the mouse is being dragged, look at Draw method
            isDrawingLightning = true;
            cursorLocation = Vector2.Transform(new Vector2(mouseState.X, mouseState.Y),
                                                       Matrix.Invert(camera.GetTransformationMatrix()));
            LightningSegment segment = new LightningSegment
            {
                PointA = AnchorPoint,
                PointB = cursorLocation
            };
            if (!SegmentBlockedByGeometry(AnchorPoint, cursorLocation))
                lightningSegmentList.Add(segment);
        }
        if (keyboardState.IsKeyDown(Keys.E) && mouseState.RightButton == ButtonState.Pressed
            && prevMouse.RightButton == ButtonState.Pressed && whipSegmentList.Count == 0
            && previousPosition == Position)
        {
            cursorLocation = Vector2.Transform(new Vector2(mouseState.X, mouseState.Y),
                                                       Matrix.Invert(camera.GetTransformationMatrix()));

            if (lightningSegmentList.Count > 0)
            {
                Vector2 lastPoint = lightningSegmentList[lightningSegmentList.Count - 1].PointB;

                if (Vector2.Distance(cursorLocation, lastPoint) > playerAnchorRadius + 16)
                {
                    LightningSegment segment = new LightningSegment
                    {
                        PointA = lastPoint,
                        PointB = cursorLocation
                    };
                    // These are the conditions for checking the zig-zag formation
                    LightningSegment previousSegment = lightningSegmentList[lightningSegmentList.Count - 1];
                    Vector2 previewDirection = Vector2.Normalize(cursorLocation - previousSegment.PointB);
                    if ((Math.Abs(Vector2.Dot(previewDirection, Vector2.Normalize(previousSegment.PointB - previousSegment.PointA))) > 0.35f
                        || Math.Abs(Vector2.Dot(previewDirection, Vector2.Normalize(previousSegment.PointB - previousSegment.PointA))) < -0.35f)
                        && !SegmentBlockedByGeometry(lastPoint, cursorLocation))
                    {
                        lightningSegmentList.Add(segment);
                    }
                }
            }
        }
        if (mouseState.RightButton == ButtonState.Released && prevMouse.RightButton == ButtonState.Pressed)
        {
            committedLightningSegmentList = new List<LightningSegment>(lightningSegmentList);
            lightningSegmentList.Clear();
            isDrawingLightning = false;
            lightningFlickerTimer = 0f;
            flickerVisibility = true;
        }

        if (committedLightningSegmentList.Count > 0 && !isDrawingLightning)
        {
            lightningFlickerTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            lightningPersistTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (lightningFlickerTimer >= lightningFlickerInterval)
            {
                flickerVisibility = !flickerVisibility;
                lightningFlickerTimer = 0f;
            }

            if (flickerVisibility)
            {
                foreach (Skeleton skeleton in skeletons)
                {
                    foreach (LightningSegment seg in committedLightningSegmentList)
                    {
                        if (LineIntersectsRect(seg.PointA, seg.PointB, skeleton.skeletonRect))
                        {
                            Vector2 midpoint = (seg.PointA + seg.PointB) * 0.5f;
                            Vector2 hitDir = skeleton.skeletonPosition - midpoint;
                            if (hitDir != Vector2.Zero) hitDir.Normalize();
                            if (!skeleton.brokenBones)
                                skeleton.TakeHit(hitDir);
                            break; // one hit per skeleton per flicker
                        }
                    }
                }
                foreach (Zombie zombie in zombies)
                {
                    foreach (LightningSegment seg in committedLightningSegmentList)
                    {
                        if (LineIntersectsRect(seg.PointA, seg.PointB, zombie.zombieRect))
                        {
                            if (!zombie.zombieGas)
                            {
                                Vector2 midpoint = (seg.PointA + seg.PointB) * 0.5f;
                                Vector2 hitDir = zombie.zombiePosition - midpoint;
                                if (hitDir != Vector2.Zero) hitDir.Normalize();
                                zombie.TakeHit(hitDir);
                                break; // one hit per skeleton per flicker
                            }
                        }
                    }
                }
                foreach (Ghost ghost in ghosts)
                {
                    foreach (LightningSegment seg in committedLightningSegmentList)
                    {
                        if (LineIntersectsRect(seg.PointA, seg.PointB, ghost.ghostRect))
                        {
                            if (!(ghost.ghostHP <= 0))
                            {
                                Vector2 midpoint = (seg.PointA + seg.PointB) * 0.5f;
                                Vector2 hitDir = ghost.ghostPosition - midpoint;
                                if (hitDir != Vector2.Zero) hitDir.Normalize();
                                ghost.TakeHit(hitDir);
                                break; // one hit per skeleton per flicker
                            }
                        }
                    }
                }
                foreach (KingBoss enemy in boss)
                {
                    foreach (LightningSegment seg in committedLightningSegmentList)
                    {
                        if (LineIntersectsRect(seg.PointA, seg.PointB, enemy.bossRect))
                        {
                            Vector2 midpoint = (seg.PointA + seg.PointB) * 0.5f;
                            Vector2 hitDir = enemy.bossPosition - midpoint;
                            if (hitDir != Vector2.Zero) hitDir.Normalize();
                            enemy.TakeHit(hitDir, 0.025f);
                            break; // one hit per skeleton per flicker
                        }
                    }
                }
            }

            if (lightningPersistTimer >= 3f)
            {
                committedLightningSegmentList.Clear();
                flickerVisibility = false;
                lightningPersistTimer = 0f;
            }
        }
    }
    public void UpdateMissile(KeyboardState keyboardState, MouseState mouseState, MouseState prevMouse,
                              Camera camera, List<Skeleton> skeletons, List<Zombie> zombies, List<Ghost> ghosts, List<KingBoss> boss)
    {
        if (keyboardState.IsKeyDown(Keys.Q) && mouseState.RightButton == ButtonState.Pressed
            && prevMouse.RightButton == ButtonState.Released && whipSegmentList.Count == 0
            && missileList.Count == 0 && previousPosition == Position)
        {
            shootingMissile = true;
            cursorLocation = Vector2.Transform(new Vector2(mouseState.X, mouseState.Y),
                                                       Matrix.Invert(camera.GetTransformationMatrix()));
            Vector2 missileDirection = cursorLocation - AnchorPoint;
            if (missileDirection.X > 0)
            {
                direction = "right";
            }
            else if (missileDirection.X < 0)
            {
                direction = "left";
            }
            Vector2 missileLocation = AnchorPoint;
            if (missileDirection != Vector2.Zero)
            {
                missileDirection.Normalize();
                missileLocation = AnchorPoint + missileDirection * playerAnchorRadius;
            }
            Missile missile = new Missile();
            missile.Position = missileLocation;
            missile.Rect = new Rectangle((int)missile.Position.X - 5, (int)missile.Position.Y - 5, 10, 10);
            missile.Direction = missileDirection;

            Game1.missileSound.Play();
            missileList.Add(missile);
        }
        for (int i = 0; i < missileList.Count; i++)
        {
            bool clearList = false;
            cursorLocation = Vector2.Transform(new Vector2(mouseState.X, mouseState.Y),
                                                       Matrix.Invert(camera.GetTransformationMatrix()));
            Missile tempMissile = missileList[i];
            Vector2 newDirection = cursorLocation - tempMissile.Position;
            if (newDirection != Vector2.Zero)
            {
                newDirection.Normalize();
            }
            if (Math.Abs(newDirection.X) >= 0 && Math.Abs(newDirection.Y) >= 0)
            {
                tempMissile.Direction += (newDirection * 0.1f) * 5f;
            }
            missileSpeed += 0.07f;
            tempMissile.Position += tempMissile.Direction * missileSpeed;
            tempMissile.Rect.X = (int)tempMissile.Position.X;
            tempMissile.Rect.Y = (int)tempMissile.Position.Y;
            missileList[i] = tempMissile;

            foreach (Rectangle rect in _collisionRects)
            {
                if (missileList[i].Rect.Intersects(rect))
                {
                    clearList = true;
                }
            }

            foreach (Skeleton skeleton in skeletons)
            {
                if (missileList[i].Rect.Intersects(skeleton.skeletonRect))
                {
                    Vector2 hitDir = tempMissile.Direction;
                    if (hitDir != Vector2.Zero) hitDir.Normalize();
                    if (!skeleton.brokenBones)
                    {
                        skeleton.TakeHit(hitDir);
                        clearList = true;
                    }

                }
            }
            foreach (Zombie zombie in zombies)
            {
                if (missileList[i].Rect.Intersects(zombie.zombieRect))
                {
                    if (!zombie.zombieGas && !zombie.zombieDead)
                    {
                        Vector2 hitDir = tempMissile.Direction;
                        if (hitDir != Vector2.Zero) hitDir.Normalize();
                        zombie.TakeHit(hitDir);
                        clearList = true;
                    }
                }
            }
            foreach (Ghost ghost in ghosts)
            {
                if (missileList[i].Rect.Intersects(ghost.ghostRect) && ghost.ghostHP > 0)
                {
                    Vector2 hitDir = tempMissile.Direction;
                    if (hitDir != Vector2.Zero) hitDir.Normalize();
                    ghost.TakeHit(hitDir);
                    clearList = true;
                }
            }
            foreach (KingBoss enemy in boss)
            {
                if (missileList[i].Rect.Intersects(enemy.bossRect))
                {
                    Vector2 hitDir = tempMissile.Direction;
                    if (hitDir != Vector2.Zero) hitDir.Normalize();
                    enemy.TakeHit(hitDir, 0.5f);
                    clearList = true;

                }
            }
            if (clearList)
            {
                missileList.Clear();
                missileSpeed = 1;
            }
        }
    }
    public void UpdatePlayerRectangle()
    {
        Rect.X = (int)Position.X;
        Rect.Y = (int)Position.Y;
    }
    public void UpdateCollision()
    {
        Rectangle Rect = new Rectangle((int)Position.X, (int)Position.Y,
                                             playerWidth, playerHeight);
        foreach (Rectangle rectangle in _collisionRects)
        {
            if (!Rect.Intersects(rectangle))
                continue;


            if (Position.X + playerWidth / 2 < rectangle.Center.X)
                Position.X = rectangle.Left - playerWidth;
            else
                Position.X = rectangle.Right;

            Rect = new Rectangle((int)Position.X, (int)Position.Y,
                                       playerWidth, playerHeight);
        }
        foreach (Rectangle rectangle in _platformRects)
        {
            if (!Rect.Intersects(rectangle))
                continue;


            if (Position.X + playerWidth / 2 < rectangle.Center.X)
                Position.X = rectangle.Left - playerWidth;
            else
                Position.X = rectangle.Right;

            Rect = new Rectangle((int)Position.X, (int)Position.Y,
                                       playerWidth, playerHeight);
        }

        // --- Vertical move + collision ---
        Position.Y += playerVelocityY;

        bool landedThisFrame = false;
        Rect = new Rectangle((int)Position.X, (int)Position.Y,
                                   playerWidth, playerHeight);
        foreach (Rectangle rectangle in _collisionRects)
        {
            if (!Rect.Intersects(rectangle))
                continue;

            if (playerVelocityY >= 0)
            {
                Position.Y = rectangle.Top - playerHeight;
                playerVelocityY = 0f;
                landedThisFrame = true;
            }
            else
            {
                Position.Y = rectangle.Bottom;
                playerVelocityY = 0f;
            }

            Rect = new Rectangle((int)Position.X, (int)Position.Y,
                                       playerWidth, playerHeight);
        }
        foreach (Rectangle rectangle in _platformRects)
        {
            if (!Rect.Intersects(rectangle))
                continue;

            if (playerVelocityY >= 0)
            {
                Position.Y = rectangle.Top - playerHeight;
                playerVelocityY = 0f;
                landedThisFrame = true;
            }
            else
            {
                Position.Y = rectangle.Bottom;
                playerVelocityY = 0f;
            }

            Rect = new Rectangle((int)Position.X, (int)Position.Y,
                                       playerWidth, playerHeight);
        }

        // Ground probe — 1px below feet to detect standing still or walking off an edge
        Rectangle groundProbe = new Rectangle((int)Position.X,
                                              (int)Position.Y + playerHeight,
                                              playerWidth, 1);
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
    private static bool LineIntersectsRect(Vector2 a, Vector2 b, Rectangle rect)
    {
        if (rect.Contains((int)a.X, (int)a.Y) || rect.Contains((int)b.X, (int)b.Y))
            return true;
        Vector2 tl = new Vector2(rect.Left, rect.Top);
        Vector2 tr = new Vector2(rect.Right, rect.Top);
        Vector2 bl = new Vector2(rect.Left, rect.Bottom);
        Vector2 br = new Vector2(rect.Right, rect.Bottom);
        return SegmentsIntersect(a, b, tl, tr)
            || SegmentsIntersect(a, b, tr, br)
            || SegmentsIntersect(a, b, br, bl)
            || SegmentsIntersect(a, b, bl, tl);
    }
    private static bool SegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        float d1x = p2.X - p1.X, d1y = p2.Y - p1.Y;
        float d2x = p4.X - p3.X, d2y = p4.Y - p3.Y;
        float cross = d1x * d2y - d1y * d2x;
        if (Math.Abs(cross) < 1e-6f) return false;
        float t = ((p3.X - p1.X) * d2y - (p3.Y - p1.Y) * d2x) / cross;
        float u = ((p3.X - p1.X) * d1y - (p3.Y - p1.Y) * d1x) / cross;
        return t >= 0f && t <= 1f && u >= 0f && u <= 1f;
    }
    private bool SegmentBlockedByGeometry(Vector2 a, Vector2 b)
    {
        foreach (Rectangle rect in _collisionRects)
            if (LineIntersectsRect(a, b, rect)) return true;
        foreach (Rectangle rect in _platformRects)
            if (LineIntersectsRect(a, b, rect)) return true;
        return false;
    }
    public void TakeHit(Vector2 hitDirection, bool IgnoreIFrames, float damage, float force = 8f)
    {
        if (!iFrames && !IgnoreIFrames)
        {
            if (hitDirection.Y > 0)
            {
                knockbackVelocity.X = hitDirection.X * force;
            }
            else
            {
                knockbackVelocity = hitDirection * force;
            }
            playerVelocityY = -4f;
            playerHP -= damage;
            isGrounded = false;
            iFrames = true;
        }
        if (IgnoreIFrames)
        {
            playerHP -= damage;
        }
    }
    public void AddKey(Game1.Key key)
    {
        playerKeyInventory.Add(key);
    }
    public void DrawWhip(SpriteBatch spriteBatch, Texture2D pixelTexture)
    {
        if (whipSegmentList.Count > 0)
        {
            Vector2 armDirection = whipSegmentList[0].Position - AnchorPoint;
            if (armDirection != Vector2.Zero)
            {
                armDirection.Normalize();
            }
            float angle = (float)Math.Atan2(armDirection.Y, armDirection.X) - (float)Math.PI / 2f;
            spriteBatch.Draw(
                            whipArm,
                            AnchorPoint,
                            null,
                            Color.White,
                            angle,
                            new Vector2(whipArm.Width / 2f, 0),
                            new Vector2(1, 1),
                            SpriteEffects.None,
                            0f
                        );
        }
        for (int i = 0; i < whipSegmentList.Count; i++)
        {
            Texture2D texture = i == 0 ? whipHandle
                      : i == whipSegmentList.Count - 1 ? whipTip
                      : whipSegmentTexture;

            spriteBatch.Draw(texture, whipSegmentList[i].Position, null,
                             Color.White, whipSegmentList[i].Angle,
                             new Vector2(texture.Width / 2f, texture.Height / 2f),
                             new Vector2(1, 1),
                             SpriteEffects.None, 0f);
        }
    }
    public void DrawLightning(SpriteBatch spriteBatch, Texture2D pixelTexture)
    {
        if (isDrawingLightning)
        {
            foreach (LightningSegment segment in lightningSegmentList)
            {
                Vector2 diff = segment.PointB - segment.PointA;
                float angle = MathF.Atan2(diff.Y, diff.X);
                float length = diff.Length();

                spriteBatch.Draw(pixelTexture, segment.PointA, null,
                                 Color.Yellow, angle,
                                 new Vector2(0f, 0.5f),
                                 new Vector2(length, 2f),
                                 SpriteEffects.None, 0f);
            }

            Vector2 previewStart = lightningSegmentList.Count > 0
                ? lightningSegmentList[lightningSegmentList.Count - 1].PointB
                : AnchorPoint;
            Vector2 previewDiff = cursorLocation - previewStart;
            float previewAngle = MathF.Atan2(previewDiff.Y, previewDiff.X);
            float previewLength = previewDiff.Length();

            spriteBatch.Draw(pixelTexture, previewStart, null,
                             Color.Yellow, previewAngle,
                             new Vector2(0f, 0.5f),
                             new Vector2(previewLength, 2f),
                             SpriteEffects.None, 0f);
        }

        if (flickerVisibility)
        {
            foreach (LightningSegment segment in committedLightningSegmentList)
            {
                Vector2 diff = segment.PointB - segment.PointA;
                float angle = MathF.Atan2(diff.Y, diff.X);
                float length = diff.Length();

                spriteBatch.Draw(pixelTexture, segment.PointA, null,
                                 Color.Yellow, angle,
                                 new Vector2(0f, 0.5f),
                                 new Vector2(length, 2f),
                                 SpriteEffects.None, 0f);
            }
        }
    }
    public void DrawMissile(SpriteBatch spriteBatch, Texture2D pixelTexture)
    {
        foreach (Missile missile in missileList)
        {
            spriteBatch.Draw(this.missile, missile.Rect, missileColor);
        }
    }
    void AnimateSprite(GameTime gameTime)
    {
        if (previousPosition == Position)
        {
            if (shootingMissile || isDrawingLightning)
            {
                frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                frameCounter = 0;
                currentSpriteSheet = shootMissile;
                currentFrame = GetFrameRect(frameCounter, shootMissile);
                if (frameTimer >= 0.5f)
                {
                    frameTimer = 0;
                    shootingMissile = false;
                }
            }
            else if (whipSegmentList.Count > 0)
            {
                frameCounter = 0;
                currentSpriteSheet = whipWalk;
                currentFrame = GetFrameRect(frameCounter, whipWalk);
            }
            else
            {
                frameCounter = 0;
                currentSpriteSheet = idle;
                currentFrame = GetFrameRect(frameCounter, idle);
            }
        }
        else
        {
            if (isGrounded && !shootingMissile && !isDrawingLightning && whipSegmentList.Count <= 0)
            {
                currentSpriteSheet = run;
                float frameDuration = (float)1 / 16;
                frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (frameTimer >= frameDuration)
                {
                    frameTimer = 0;
                    frameCounter++;
                    if (frameCounter > 7)
                    {
                        frameCounter = 0;
                    }
                }
                currentFrame = GetFrameRect(frameCounter, run);
            }
            else if (isGrounded && shootingMissile || isGrounded && isDrawingLightning)
            {
                frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                frameCounter = 0;
                currentSpriteSheet = shootMissile;
                currentFrame = GetFrameRect(frameCounter, shootMissile);
                if (frameTimer >= 0.5f)
                {
                    frameTimer = 0;
                    shootingMissile = false;
                }
            }
            else if (whipSegmentList.Count > 0)
            {
                currentSpriteSheet = whipWalk;
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
                currentFrame = GetFrameRect(frameCounter, whipWalk);
            }
            else
            {
                if (shootingMissile || isDrawingLightning)
                {
                    frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    frameCounter = 0;
                    currentSpriteSheet = shootMissile;
                    currentFrame = GetFrameRect(frameCounter, shootMissile);
                    if (frameTimer >= 0.5f)
                    {
                        frameTimer = 0;
                        shootingMissile = false;
                    }
                }
                else
                {
                    frameCounter = 0;
                    currentSpriteSheet = jump;
                    currentFrame = GetFrameRect(frameCounter, jump);
                }
            }
        }
        previousPosition = Position;
    }
    public Rectangle GetFrameRect(int frame, Texture2D spriteSheet)
    {
        int columns = spriteSheet.Width / frameWidth;
        int column = frame % columns;
        int row = frame / columns;
        return new Rectangle(column * frameWidth, row * frameHeight, frameWidth, frameHeight);
    }
    Color GetRainbowColor(float t)
    {
        t = t % 1f;
        float hue = t * 6f;
        float x = 1f - Math.Abs(hue % 2f - 1f);

        return (int)hue switch
        {
            0 => new Color(1f, x, 0f),
            1 => new Color(x, 1f, 0f),
            2 => new Color(0f, 1f, x),
            3 => new Color(0f, x, 1f),
            4 => new Color(x, 0f, 1f),
            5 => new Color(1f, 0f, x),
            _ => new Color(1f, 0f, 0f)
        };
    }
}
