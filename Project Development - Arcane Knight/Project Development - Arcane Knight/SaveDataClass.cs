using System.Collections.Generic;

public class SaveData
{
    public int Gold { get; set; }
    public int Potions { get; set; }
    public bool HasWhip { get; set; }
    public bool HasMissile { get; set; }
    public bool HasLightning { get; set; }
    public bool HasTeleport { get; set; }
    public bool HasTorch { get; set; }
    public List<DoorData> Doors { get; set; }
    public List<ScrollData> Scrolls { get; set; }
    public List<WorldKeyData> WorldKeys { get; set; }
    public List<string> PlayerKeysList { get; set; }
    public List<RectData> CollisionRects { get; set; }
    public Vector2Data SpawnLocation { get; set; }
    public List<RectData> InteractableRects { get; set; }
    public bool IntroPrompt { get; set; }
    public List<ChestData> Chests { get; set; }
    public VendorData Vendor { get; set; }
    public int ScreenWidth { get; set; }
    public int ScreenHeight { get; set; }
    public bool IsFullScreen { get; set; }
    public List<SkeletonData> Skeletons { get; set; }
    public List<ZombieData> Zombies { get; set; }
    public List<GhostData> Ghosts { get; set; }
    public float PlayerHP { get; set; }
}

public class SkeletonData
{
    public float X { get; set; }
    public float Y { get; set; }
    public float SpawnX { get; set; }
    public float SpawnY { get; set; }
    public bool BrokenBones { get; set; }
}

public class ZombieData
{
    public float X { get; set; }
    public float Y { get; set; }
    public float SpawnX { get; set; }
    public float SpawnY { get; set; }
    public float ZombieHP { get; set; }
    public bool ZombieDead { get; set; }
    public bool CanRespawn { get; set; }
}

public class GhostData
{
    public float X { get; set; }
    public float Y { get; set; }
    public float SpawnX { get; set; }
    public float SpawnY { get; set; }
    public float GhostHP { get; set; }
}

public class RectData
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public class Vector2Data
{
    public float X { get; set; }
    public float Y { get; set; }
}

public class ChestData
{
    public int Gold { get; set; }
    public int Potions { get; set; }
    public RectData Rect { get; set; }
    public Vector2Data Position { get; set; }
}

public class VendorData
{
    public RectData Rect { get; set; }
    public Vector2Data Position { get; set; }
    public int SpellPrice { get; set; }
    public int TorchPrice { get; set; }
    public int PotionUpgradePrice { get; set; }
    public int SpellInventory { get; set; }
    public int TorchInventory { get; set; }
    public int PotionUpgradeInventory { get; set; }
}

public class DoorData
{
    public string Type { get; set; }
    public RectData Rect { get; set; }
    public Vector2Data Position { get; set; }
    public ColorData Color { get; set; }
}

public class ScrollData
{
    public string Spell { get; set; }
    public RectData Rect { get; set; }
    public Vector2Data Position { get; set; }
    public ColorData Color { get; set; }
}

public class WorldKeyData
{
    public string Type { get; set; }
    public RectData Rect { get; set; }
    public Vector2Data Position { get; set; }
    public ColorData Color { get; set; }
}

public class ColorData
{
    public float R { get; set; }
    public float G { get; set; }
    public float B { get; set; }
    public float A { get; set; }
}
