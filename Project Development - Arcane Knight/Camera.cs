using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Project_Development___Arcane_Knight;

public class Camera
{
    public Vector2 Position { get; set; }
    public float Zoom { get; set; }
    public float Rotation { get; set; }

    public Viewport Viewport;

    // Full map dimensions in world pixels
    private const int MapWidth = 2208;
    private const int MapHeight = 2336;
    private const float TargetVisibleWorldHeight = 360f;

    // Boss room bounds in world pixels (the room the player enters via the boss door)
    private static readonly Rectangle BossRoom = new Rectangle(1100, 0, 448, 276);

    // In your Camera class, if not already present

    public Vector2 GetTopLeftPosition()
    {
        // Inverse of your transform matrix's translation, adjusted for zoom
        Matrix transform = GetTransformationMatrix();
        return new Vector2(-transform.Translation.X / Zoom,
                           -transform.Translation.Y / Zoom);
    }

    public Camera(Viewport viewport)
    {
        Viewport = viewport;
        Zoom = 2.5f;
        Rotation = 0.0f;
        Position = new Vector2(216, 600);
    }

    /// <summary>
    /// How many world units fit on screen at the current zoom.
    /// </summary>
    private float VisibleWidth => Viewport.Width / Zoom;
    private float VisibleHeight => Viewport.Height / Zoom;

    /// <summary>
    /// Clamps a desired camera center so the viewport never shows
    /// outside the given world rectangle.
    /// If the room is narrower/shorter than the viewport, the camera
    /// is centered on that axis instead of clamped.
    /// </summary>
    private Vector2 ClampToRegion(Vector2 desired, Rectangle region)
    {
        float halfW = VisibleWidth / 2f;
        float halfH = VisibleHeight / 2f;

        float minX = region.Left + halfW;
        float maxX = region.Right - halfW;
        float minY = region.Top + halfH;
        float maxY = region.Bottom - halfH;

        float x = (minX > maxX) ? region.Left + region.Width / 2f
                                 : MathHelper.Clamp(desired.X, minX, maxX);
        float y = (minY > maxY) ? region.Top + region.Height / 2f
                                 : MathHelper.Clamp(desired.Y, minY, maxY);

        return new Vector2(x, y);
    }

    /// <summary>
    /// Call every frame. Pass the player's world position.
    /// Automatically switches between normal map clamping and
    /// boss-room clamping based on where the player is.
    /// </summary>
    public void Follow(Vector2 position)
    {
        Vector2 desired = new Vector2(position.X, position.Y - 40);

        bool inBossRoom = BossRoom.Contains((int)position.X, (int)position.Y);

        if (inBossRoom)
        {
            // Clamp to boss room first, then to map as a safety net
            Vector2 clamped = ClampToRegion(desired, BossRoom);
            Position = ClampToRegion(clamped, new Rectangle(0, 0, MapWidth, MapHeight));
        }
        else
        {
            Position = ClampToRegion(desired, new Rectangle(0, 0, MapWidth, MapHeight));
        }
    }

    public void UpdateZoomForResolution()
    {
        Zoom = Viewport.Height / TargetVisibleWorldHeight;
    }

    public Matrix GetTransformationMatrix()
    {
        return Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
               Matrix.CreateRotationZ(Rotation) *
               Matrix.CreateScale(Zoom, Zoom, 1) *
               Matrix.CreateTranslation(new Vector3(Viewport.Width / 2f, Viewport.Height / 2f, 0));
    }
}