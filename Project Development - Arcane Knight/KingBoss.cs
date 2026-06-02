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

public class KingBoss
{
    private int bossWidth = 48;
    private int bossHeight = 96;
    public float bossHP = 80;
    public float bossMaxHP = 80;
    public Color bossColor;

    public Rectangle bossRect;
    public Vector2 startingPosition;
    public Vector2 bossPosition;
    public Vector2 bossAnchorPoint;
    private readonly int bossAnchorRadius = 65;

    private readonly int leftWall;
    private readonly int rightWall;
    private readonly int floor;

    private readonly List<Rectangle> _collisionRects;
    private readonly List<Rectangle> _platformRects;

    private Vector2 knockbackVelocity = Vector2.Zero;

    private float bobTime = 0f;
    private float floatBaseY;
    private float centerY;
    private float centerX;
    private const float bobAmplitude = 8f;
    private const float bobSpeed = 1.5f;

    private float idleX;
    private const float idleDriftSpeed = 4f;
    private const float snapSpeed = 22f;
    private const float centerSnapThreshold = 4f;

    private bool Phase2 => bossHP <= bossMaxHP / 2;

    private enum BossState
    {
        Idle,
        ChargingUp, Charging, ChargeRecovery, ReturningToCenter,
        SlamSnappingToCenter, SlamSnappingToTarget, SlamDiving, SlamImpact, SlamReturning,
        Summoning
    }
    private BossState state = BossState.Idle;

    private float attackCooldown = 0f;
    private const float attackCooldownP1 = 3.0f;
    private const float attackCooldownP2 = 2.25f;
    private bool nextAttackIsCharge = true;

    private float slamCooldown = 0f;
    private const float slamCooldownP1 = 10.0f;
    private const float slamCooldownP2 = 12.0f;
    private bool slamQueued = false;

    private float summonCooldown = 0f;
    private const float summonCooldownP1 = 15.0f;
    private const float summonCooldownP2 = 10.0f;
    private bool summonQueued = false;
    private float summonTimer = 0f;
    private const float summonDuration = 1.2f;
    private int summonCount = 0;
    private const int summonAmountP1 = 1;
    private const int summonAmountP2 = 2;
    private Random _random = new Random();

    private float chargeWindupTimer = 0f;
    private float chargeDuration = 0f;
    private Vector2 chargeDirection;
    private float chargeTargetX;
    private const float chargeWindup = 0.5f;
    private const float chargeDurationLimit = 0.45f;
    private const float chargeSpeed = 18f;

    private float slamTargetX;
    private float slamDiveSpeed = 28f;
    private float slamImpactTimer = 0f;
    private const float slamImpactDuration = 0.4f;


    private List<Orb> _orbs = new List<Orb>();
    private const float orbCooldownP2Limit = 5.0f;
    private float orbCooldownP2 = 0f;
    private const float orbSpeed = 4f;
    private const int orbSize = 12;


    private List<Shockwave> _shockwaves = new List<Shockwave>();
    private const float shockwaveSpeed = 5f;
    private const int shockwaveWidth = 24;
    private const int shockwaveHeight = 96;

    int frameWidth = 128;
    int frameHeight = 128;
    int frameCounter = 0;
    float frameTimer = 0f;
    String facingDirection = "left";
    public Texture2D orbTexture;
    public Texture2D idleFloat;
    public Texture2D chargeTexture;
    public Texture2D summoningTexture;
    public Texture2D slamTexture;
    public Texture2D slamWeapon;
    public Texture2D castingOrb;
    public Texture2D shockwaveTexture;
    public Texture2D currentSpriteSheet;
    public Rectangle currentFrame;

    Color orbColor;

    private struct Orb
    {
        public Vector2 Position;
        public Vector2 Direction;
        public int Width;
        public int Height;
    }

    private struct Shockwave
    {
        public Vector2 Position;
        public Vector2 Direction;
        public int Width;
        public int Height;
    }

    public KingBoss(Vector2 position, List<Rectangle> collisionRects, List<Rectangle> platformRects,
                    int roomLeftBoundary, int roomRightBoundary, int floorBoundary)
    {
        _collisionRects = collisionRects;
        _platformRects = platformRects;
        leftWall = roomLeftBoundary;
        rightWall = roomRightBoundary;
        floor = floorBoundary;

        centerX = (leftWall + rightWall) / 2f - bossWidth / 2f;
        centerY = floor - bossHeight * 2.5f;

        bossPosition = new Vector2(centerX, centerY);
        startingPosition = bossPosition;
        floatBaseY = centerY;
        idleX = centerX;

        bossRect = new Rectangle((int)bossPosition.X, (int)bossPosition.Y, bossWidth, bossHeight);
        bossAnchorPoint = new Vector2(bossPosition.X + bossWidth / 2f, bossPosition.Y + bossHeight / 2f);
        bossColor = Color.Blue;
    }

    public void Update(GameTime gameTime, Player player, List<Zombie> zombies, ContentManager Content)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float cycleDuration = 3f; // seconds for one full rainbow loop
        float t = (float)(gameTime.TotalGameTime.TotalSeconds % cycleDuration) / cycleDuration;
        orbColor = GetRainbowColor(t);

        bossAnchorPoint = new Vector2(bossPosition.X + bossWidth / 2f, bossPosition.Y + bossHeight / 2f);

        Vector2 toPlayer = player.Position - bossAnchorPoint;
        Vector2 dirToPlayer = toPlayer;
        if (dirToPlayer != Vector2.Zero) dirToPlayer.Normalize();

        if (dirToPlayer.X >= 0)
        {
            facingDirection = "right";
        }
        else
        {
            facingDirection = "left";
        }
        if (bossRect.Intersects(player.Rect))
            player.TakeHit(dirToPlayer, false, 0.5f);

        UpdateSlamCooldown(dt, player);
        UpdateSummonCooldown(dt);
        UpdateState(dt, dirToPlayer, player, zombies, Content);
        UpdateOrbs(dt, dirToPlayer, player);
        UpdateShockwaves(dt, player);
        UpdateFloat(dt);
        UpdateKnockback();
        UpdateBossRect();
        AnimateSprite(gameTime);
    }

    private void UpdateSlamCooldown(float dt, Player player)
    {
        if (state == BossState.Idle || state == BossState.ReturningToCenter)
        {
            slamCooldown -= dt;
            if (slamCooldown <= 0f && !slamQueued)
                slamQueued = true;
        }
    }

    private void UpdateSummonCooldown(float dt)
    {
        if (state == BossState.Idle || state == BossState.ReturningToCenter)
        {
            summonCooldown -= dt;
            if (summonCooldown <= 0f && !summonQueued)
                summonQueued = true;
        }
    }

    private void UpdateState(float dt, Vector2 dirToPlayer, Player player, List<Zombie> zombies, ContentManager Content)
    {
        switch (state)
        {
            case BossState.Idle:
                bossPosition.X += (idleX - bossPosition.X) * idleDriftSpeed * dt;
                attackCooldown -= dt;

                if (slamQueued)
                {
                    slamQueued = false;
                    slamCooldown = Phase2 ? slamCooldownP2 : slamCooldownP1;
                    BeginSlam(player);
                    break;
                }

                if (summonQueued)
                {
                    summonQueued = false;
                    summonCooldown = Phase2 ? summonCooldownP2 : summonCooldownP1;
                    summonTimer = 0f;
                    summonCount = 0;
                    state = BossState.Summoning;
                    bossColor = Color.LimeGreen;
                    break;
                }

                if (attackCooldown <= 0f)
                {
                    if (!Phase2)
                    {
                        if (nextAttackIsCharge)
                            BeginChargeWindup(dirToPlayer, player);
                        else
                            FireSingleOrb(dirToPlayer);
                        nextAttackIsCharge = !nextAttackIsCharge;
                    }
                    else
                    {
                        BeginChargeWindup(dirToPlayer, player);
                    }
                    attackCooldown = Phase2 ? attackCooldownP2 : attackCooldownP1;
                }
                break;

            case BossState.Summoning:
                summonTimer += dt;
                int targetCount = Phase2 ? summonAmountP2 : summonAmountP1;
                float interval = summonDuration / targetCount;
                int shouldHaveSpawned = (int)(summonTimer / interval);
                while (summonCount < shouldHaveSpawned && summonCount < targetCount)
                {
                    float spawnX = leftWall + (float)_random.NextDouble() * (rightWall - leftWall - 32);
                    Zombie summoned = new Zombie(new Vector2(spawnX, floor - 64),
                                           _collisionRects, _platformRects, 0.5f);
                    summoned.idle = Content.Load<Texture2D>(@"Textures/Zombie/zombieidle");
                    summoned.walk = Content.Load<Texture2D>(@"Textures/Zombie/zombiewalk");
                    summoned.crawl = Content.Load<Texture2D>(@"Textures/Zombie/zombiecrawl");
                    summoned.crawlIdle = Content.Load<Texture2D>(@"Textures/Zombie/zombiecrawlidle");
                    summoned.gas = Content.Load<Texture2D>(@"Textures/Zombie/zombiegas");
                    summoned.wasSummoned = true;
                    summoned.Init();
                    zombies.Add(summoned);
                    summonCount++;
                }
                if (summonCount >= targetCount && summonTimer >= summonDuration)
                {
                    bossColor = Color.Blue;
                    state = BossState.Idle;
                }
                break;

            case BossState.ChargingUp:
                chargeWindupTimer += dt;
                if (chargeWindupTimer >= chargeWindup)
                {
                    chargeDuration = 0f;
                    state = BossState.Charging;
                }
                break;

            case BossState.Charging:
                chargeDuration += dt;
                bossPosition.X += chargeDirection.X * chargeSpeed;
                if (chargeDirection.X >= 0)
                {
                    facingDirection = "right";
                }
                else
                {
                    facingDirection = "left";
                }

                bool hitWall = chargeDirection.X > 0
                    ? bossPosition.X >= chargeTargetX
                    : bossPosition.X <= chargeTargetX;

                if (hitWall || chargeDuration >= chargeDurationLimit)
                {
                    bossPosition.X = chargeTargetX;
                    state = BossState.ChargeRecovery;
                    bossColor = Color.Blue;
                }

                if (bossRect.Intersects(player.Rect))
                    player.TakeHit(chargeDirection, false, 3f);
                break;

            case BossState.ChargeRecovery:
                if (knockbackVelocity.LengthSquared() < 0.01f)
                {
                    idleX = centerX;
                    state = BossState.ReturningToCenter;
                }
                break;

            case BossState.ReturningToCenter:
                bossPosition.X += (centerX - bossPosition.X) * snapSpeed * dt;
                bossPosition.Y += (centerY - bossPosition.Y) * snapSpeed * dt;
                floatBaseY += (centerY - floatBaseY) * snapSpeed * dt;
                if (MathF.Abs(bossPosition.X - centerX) < centerSnapThreshold)
                {
                    bossPosition.X = centerX;
                    bossPosition.Y = centerY;
                    floatBaseY = centerY;
                    state = BossState.Idle;
                }
                break;

            case BossState.SlamSnappingToCenter:
                bossPosition.X += (centerX - bossPosition.X) * snapSpeed * dt;
                bossPosition.Y += (centerY - bossPosition.Y) * snapSpeed * dt;
                floatBaseY += (centerY - floatBaseY) * snapSpeed * dt;
                if (MathF.Abs(bossPosition.X - centerX) < centerSnapThreshold)
                {
                    bossPosition.X = centerX;
                    bossPosition.Y = centerY;
                    floatBaseY = centerY;
                    state = BossState.SlamSnappingToTarget;
                }
                break;

            case BossState.SlamSnappingToTarget:
                bossPosition.X += (slamTargetX - bossPosition.X) * snapSpeed * dt;
                if (MathF.Abs(bossPosition.X - slamTargetX) < centerSnapThreshold)
                {
                    bossPosition.X = slamTargetX;
                    state = BossState.SlamDiving;
                    bossColor = Color.DarkViolet;
                }
                break;

            case BossState.SlamDiving:
                bossPosition.Y += slamDiveSpeed;
                floatBaseY = bossPosition.Y;
                if (bossPosition.Y >= floor - bossHeight)
                {
                    bossPosition.Y = floor - bossHeight;
                    floatBaseY = floor - bossHeight;
                    SpawnShockwaves();
                    slamImpactTimer = 0f;
                    state = BossState.SlamImpact;
                    bossColor = Color.Blue;
                }
                break;

            case BossState.SlamImpact:
                slamImpactTimer += dt;
                if (slamImpactTimer >= slamImpactDuration)
                    state = BossState.SlamReturning;
                break;

            case BossState.SlamReturning:
                bossPosition.X += (centerX - bossPosition.X) * snapSpeed * dt;
                bossPosition.Y += (centerY - bossPosition.Y) * snapSpeed * dt;
                floatBaseY += (centerY - floatBaseY) * snapSpeed * dt;
                if (MathF.Abs(bossPosition.Y - centerY) < centerSnapThreshold)
                {
                    bossPosition.X = centerX;
                    bossPosition.Y = centerY;
                    floatBaseY = centerY;
                    idleX = centerX;
                    state = BossState.Idle;
                }
                break;
        }
    }

    private void BeginSlam(Player player)
    {
        slamTargetX = !Phase2
            ? centerX
            : MathHelper.Clamp(player.Position.X - bossWidth / 2f, leftWall, rightWall - bossWidth);
        state = BossState.SlamSnappingToCenter;
        bossColor = Color.DarkViolet;
    }

    private void SpawnShockwaves()
    {
        _shockwaves.Add(new Shockwave
        {
            Position = new Vector2(bossAnchorPoint.X, bossAnchorPoint.Y + bossHeight / 4),
            Direction = new Vector2(-1f, 0f),
            Width = shockwaveWidth,
            Height = shockwaveHeight
        });
        _shockwaves.Add(new Shockwave
        {
            Position = new Vector2(bossAnchorPoint.X, bossAnchorPoint.Y + bossHeight / 4),
            Direction = new Vector2(1f, 0f),
            Width = shockwaveWidth,
            Height = shockwaveHeight
        });
    }

    private void UpdateShockwaves(float dt, Player player)
    {
        for (int i = _shockwaves.Count - 1; i >= 0; i--)
        {
            Shockwave wave = _shockwaves[i];
            wave.Position += wave.Direction * shockwaveSpeed;
            _shockwaves[i] = wave;

            Rectangle waveRect = new Rectangle(
                (int)wave.Position.X - wave.Width / 2,
                (int)wave.Position.Y - wave.Height / 2,
                wave.Width, wave.Height);

            if (waveRect.Intersects(player.Rect))
                player.TakeHit(wave.Direction, false, 2f);

            if (wave.Position.X < leftWall || wave.Position.X > rightWall)
                _shockwaves.RemoveAt(i);
        }
    }

    private void BeginChargeWindup(Vector2 dirToPlayer, Player player)
    {
        chargeDirection = new Vector2(dirToPlayer.X >= 0 ? 1f : -1f, 0f);
        chargeTargetX = chargeDirection.X > 0 ? rightWall - bossWidth : leftWall;
        bossPosition.X = chargeDirection.X > 0 ? leftWall : rightWall - bossWidth;
        bossPosition.Y = MathHelper.Clamp(player.Position.Y, centerY, floor - bossHeight);
        floatBaseY = bossPosition.Y;
        idleX = bossPosition.X;
        chargeWindupTimer = 0f;
        state = BossState.ChargingUp;
        bossColor = Color.OrangeRed;
    }

    private void FireSingleOrb(Vector2 dirToPlayer)
    {
        _orbs.Add(new Orb
        {
            Position = bossAnchorPoint,
            Direction = dirToPlayer,
            Width = orbSize,
            Height = orbSize
        });
        Game1.missileSound.Play();
    }

    private void UpdateOrbs(float dt, Vector2 dirToPlayer, Player player)
    {
        if (Phase2 && state == BossState.Idle && attackCooldown < attackCooldownP2 - 1.5f)
        {
            orbCooldownP2 -= dt;
            if (orbCooldownP2 <= 0f)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * MathHelper.TwoPi / 8f;
                    _orbs.Add(new Orb
                    {
                        Position = bossAnchorPoint,
                        Direction = new Vector2(MathF.Cos(angle), MathF.Sin(angle)),
                        Width = orbSize,
                        Height = orbSize
                    });
                    Game1.missileSound.Play();
                }
                orbCooldownP2 = orbCooldownP2Limit;
            }
        }

        for (int i = _orbs.Count - 1; i >= 0; i--)
        {
            Orb orb = _orbs[i];
            orb.Position += orb.Direction * orbSpeed;
            _orbs[i] = orb;

            Rectangle orbRect = new Rectangle(
                (int)orb.Position.X - orb.Width / 2,
                (int)orb.Position.Y - orb.Height / 2,
                orb.Width, orb.Height);

            bool hit = false;
            foreach (Rectangle rect in _collisionRects)
            {
                if (orbRect.Intersects(rect))
                {
                    hit = true;
                    break;
                }
            }

            if (orbRect.Intersects(player.Rect))
            {
                player.TakeHit(orb.Direction, false, 1.5f);
                hit = true;
            }

            if (hit || orb.Position.X < -200 || orb.Position.X > 2000 ||
                orb.Position.Y < -200 || orb.Position.Y > 1200)
                _orbs.RemoveAt(i);
        }
    }

    private void UpdateFloat(float dt)
    {
        bool suppressBob = state == BossState.SlamDiving
                        || state == BossState.SlamImpact
                        || state == BossState.Charging;
        if (suppressBob) return;

        bobTime += dt;
        bossPosition.Y = floatBaseY + MathF.Sin(bobTime * bobSpeed * MathHelper.TwoPi) * bobAmplitude;
    }

    private void UpdateKnockback()
    {
        bossPosition += knockbackVelocity;
        knockbackVelocity *= 0.8f;
        if (knockbackVelocity.LengthSquared() < 0.01f)
            knockbackVelocity = Vector2.Zero;
    }

    private void UpdateBossRect()
    {
        bossRect.X = (int)bossPosition.X;
        bossRect.Y = (int)bossPosition.Y;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, GameTime gameTime)
    {
        if (currentSpriteSheet == null)
        {
            currentSpriteSheet = idleFloat;
            currentFrame = GetFrameRect(0, idleFloat);
        }
        if (facingDirection == "left")
        {
            spriteBatch.Draw(
                        currentSpriteSheet,
                        new Vector2(bossPosition.X - 72, bossPosition.Y - 72),
                        currentFrame,
                        Color.White,
                        0f,
                        Vector2.Zero,
                        1.5f,
                        SpriteEffects.FlipHorizontally,  // mirrors left-right
                        0f
                    );
        }
        else
        {
            spriteBatch.Draw(
                        currentSpriteSheet,
                        new Vector2(bossPosition.X - 72, bossPosition.Y - 72),
                        currentFrame,
                        Color.White,
                        0f,
                        Vector2.Zero,
                        1.5f,
                        SpriteEffects.None,  // mirrors left-right
                        0f
                    );
        }
        DrawOrbs(spriteBatch, pixelTexture);
        DrawShockwaves(spriteBatch, pixelTexture);
    }

    private void DrawOrbs(SpriteBatch spriteBatch, Texture2D pixelTexture)
    {
        foreach (Orb orb in _orbs)
        {
            Rectangle orbRect = new Rectangle(
                (int)orb.Position.X - orb.Width / 2,
                (int)orb.Position.Y - orb.Height / 2,
                orb.Width, orb.Height);
            spriteBatch.Draw(orbTexture, orbRect, orbColor);
        }
    }

    private void DrawShockwaves(SpriteBatch spriteBatch, Texture2D pixelTexture)
    {
        foreach (Shockwave wave in _shockwaves)
        {
            Rectangle waveRect = new Rectangle(
                (int)wave.Position.X - wave.Width / 2,
                (int)wave.Position.Y - wave.Height / 2,
                wave.Width, wave.Height);
            if (wave.Direction.X >= 0)
            {
                spriteBatch.Draw(
                            shockwaveTexture,
                            new Vector2(waveRect.X, waveRect.Y),
                            null,
                            Color.White,
                            0f,
                            Vector2.Zero,
                            1f,
                            SpriteEffects.FlipHorizontally,  // mirrors left-right
                            0f
                        );
            }
            else
            {
                spriteBatch.Draw(
                            shockwaveTexture,
                            new Vector2(waveRect.X, waveRect.Y),
                            null,
                            Color.White,
                            0f,
                            Vector2.Zero,
                            1f,
                            SpriteEffects.None,
                            0f
                        );
            }
        }
    }

    public void TakeHit(Vector2 hitDirection, float damage, float force = 8f)
    {
        knockbackVelocity = hitDirection * force;
        if (bossHP > 0)
            bossHP -= damage;
    }
    void AnimateSprite(GameTime gameTime)
    {
        bool aboutToFireOrb = state == BossState.Idle
                   && !Phase2
                   && !nextAttackIsCharge
                   && attackCooldown <= 0.5f;
        if (state == BossState.Idle)
        {
            if (!Phase2 && !nextAttackIsCharge && attackCooldown <= 0.5f)
            {
                currentSpriteSheet = castingOrb;
                currentFrame = GetFrameRect(0, castingOrb);
            }
            else
            {
                currentSpriteSheet = idleFloat;
                currentFrame = GetFrameRect(0, idleFloat);
            }
        }
        else if (state == BossState.Summoning)
        {
            currentSpriteSheet = summoningTexture;
            float frameDuration = 1f / 8f;
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
            currentFrame = GetFrameRect(frameCounter, summoningTexture);
        }
        else if (state == BossState.SlamDiving)
        {
            currentSpriteSheet = slamTexture;
            currentFrame = GetFrameRect(0, slamTexture);
        }
        else if (state == BossState.Charging)
        {
            currentSpriteSheet = chargeTexture;
            currentFrame = GetFrameRect(0, chargeTexture);
        }

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