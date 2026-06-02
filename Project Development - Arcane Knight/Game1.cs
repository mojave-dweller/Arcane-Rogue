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
using static System.Net.Mime.MediaTypeNames;
using static VendorData;

namespace Project_Development___Arcane_Knight
{
    public class Game1 : Game
    {
        // Environment Variables
        private int screenHeight = 630;
        private int screenWidth = 1120;
        private GraphicsDeviceManager _graphics;
        private MouseState previousMouseState;
        private MouseState currentMouseState;
        private KeyboardState previousKeyboardState;
        private KeyboardState currentKeyboardState;
        private SpriteBatch _spriteBatch;
        private Texture2D _pixelTexture;
        private SpriteFont bigText;
        private SpriteFont smallText;
        private SpriteFont shoppingText;
        private string savePath;
        private Camera _camera;
        private Effect _lightingEffect;
        private List<LightSource> _lights;

        // Gameplay Variables
        private Texture2D map;
        private List<Rectangle> _collisionRects;
        private List<Rectangle> _platformRects;
        private List<Rectangle> _interactableRects;
        public List<Skeleton> _skeletons;
        public List<Zombie> _zombies;
        public List<Ghost> _ghosts;
        public List<KingBoss> _boss;
        private List<Door> _doors;
        private List<Key> _keys;
        private List<Scroll> _scrolls;
        private List<Chest> _chests;
        private Campfire spawn;
        Texture2D campfire;
        int campfireFrameCount = 0;
        float campfireFrameTimer = 0f;
        private Vendor shopkeep;
        private const float gravity = 0.5f;

        // Player and Player Related Variables
        private Player _wizard;
        Vector2 spawnLocation = new Vector2(60, 980);
        public bool bossDefeated = false;
        public bool shopping = false;
        public bool boughtSomething = false;
        public bool pickedUpWhip = false;
        public bool pickedUpMissile = false;
        public bool pickedUpTeleport = false;
        public bool pickedUpLightning = false;
        public bool pickedUpRegularKey = false;
        public bool pickedUpBossKey = false;
        public bool pickedUpPotion = false;

        // UI Variables
        private Texture2D whipIcon;
        private Texture2D missileIcon;
        private Texture2D teleportIcon;
        private Texture2D goldIcon;
        private Texture2D potionIcon;
        private Texture2D lightningIcon;
        private Texture2D vendorTexture;
        private Texture2D bossDoorTexture;
        private Texture2D regularDoorTexture;
        public bool paused = false;
        public bool showingControls = false;
        public bool introPrompt = true;
        public float introPromptTimer = 0f;

        Song backgroundMusic;
        public static SoundEffect whipCrack;
        public static SoundEffectInstance whipInstance;
        public static SoundEffect lightningCrackle;
        public static SoundEffectInstance lightningInstance;
        public static SoundEffect teleportSound;
        public static SoundEffectInstance teleportInstance;
        public static SoundEffect missileSound;
        public static SoundEffect footstep;
        public static SoundEffectInstance footstepInstance;
        public static SoundEffect playerHurt;
        public static SoundEffectInstance hurtInstance;
        public static SoundEffect potionSound;
        public static SoundEffectInstance potionInstance;
        public static SoundEffect zombieSnarl;
        public static SoundEffectInstance snarlInstance;
        public static SoundEffect snarlSound;
        public static SoundEffect zombieGasSound;
        public static SoundEffect skeletonBones;
        public static SoundEffectInstance bonesInstance;
        public static SoundEffect bowRelease;
        public static SoundEffect ghostScream;
        public static SoundEffectInstance screamInstance;

        // Gameplay Objects (Structs)
        public struct Key
        {
            public String Type;
            public Rectangle Rect;
            public Vector2 Position;
            public Color Color;
            public Texture2D Texture;
        }

        struct Scroll
        {
            public String Spell;
            public Rectangle Rect;
            public Vector2 Position;
            public Color Color;
            public Texture2D Texture;
        }

        struct Chest
        {
            public int Gold;
            public Rectangle Rect;
            public Vector2 Position;
            public Texture2D Texture;
        }

        struct Campfire
        {
            public Rectangle Rect;
            public Vector2 Position;
        }

        struct Vendor
        {
            public Rectangle Rect;
            public Vector2 Position;
            public int SpellPrice;
            public int TorchPrice;
            public int PotionUpgradePrice;
            public int SpellInventory;
            public int TorchInventory;
            public int PotionUpgradeInventory;
        }

        struct Door
        {
            public String Type;
            public Rectangle Rect;
            public Vector2 Position;
            public Color Color;
        }

        struct LightSource
        {
            public Vector2 Position;
            public Vector3 Color;
            public float Intensity;
            public float Radius;
        }

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = screenWidth;
            _graphics.PreferredBackBufferHeight = screenHeight;

            _graphics.ApplyChanges();

            _camera = new Camera(GraphicsDevice.Viewport);
            _camera.UpdateZoomForResolution();

            _platformRects = new List<Rectangle>
            {
                // Left Tower
                new Rectangle(464, 276, 64, 17),
                new Rectangle(591, 276, 64, 17),
                new Rectangle(519, 364, 81, 17),
                new Rectangle(465, 452, 112, 17),
                new Rectangle(543, 540, 112, 17),
                new Rectangle(465, 628, 112, 17),
                new Rectangle(543, 716, 112, 17),
                new Rectangle(465, 804, 112, 17),
                new Rectangle(543, 892, 112, 17),

                // First Garden
                new Rectangle(672, 644, 112, 17),
                new Rectangle(976, 644, 112, 17),
                new Rectangle(672, 819, 112, 17),
                new Rectangle(976, 819, 112, 17),
                new Rectangle(800, 731, 160, 17),
                new Rectangle(800, 971, 160, 17),

                // Room above First Garden
                new Rectangle(672, 404, 112, 17),
                new Rectangle(976, 404, 112, 17),

                // Center Room, First Floor
                new Rectangle(1105, 892, 112, 17),
                new Rectangle(1439, 892, 112, 17),
                new Rectangle(1271, 628, 112, 17),
                new Rectangle(1271, 804, 112, 17),
                new Rectangle(1143, 716, 65, 17),
                new Rectangle(1447, 716, 65, 17),

                // Center Room, Second Floor
                new Rectangle(1271, 452, 112, 17),

                // Second Garden
                new Rectangle(1696, 971, 160, 17),
                new Rectangle(1568, 716, 112, 17),
                new Rectangle(1568, 892, 112, 17),
                new Rectangle(1872, 892, 112, 17),

                // Room above Second Garden
                new Rectangle(1687, 404, 176, 17),

                // Second Tower
                new Rectangle(2000, 276, 64, 17),
                new Rectangle(2127, 276, 64, 17),
                new Rectangle(2055, 364, 81, 17),
                new Rectangle(2080, 452, 112, 17),
                new Rectangle(2000, 540, 112, 17),
                new Rectangle(2080, 628, 112, 17),
                new Rectangle(2000, 716, 112, 17),
                new Rectangle(2080, 804, 112, 17),
                new Rectangle(2000, 892, 112, 17),
                new Rectangle(2079, 980, 112, 9),
                new Rectangle(2000, 1060, 112, 9),
                new Rectangle(2079, 1140, 112, 9),
                new Rectangle(2000, 1220, 112, 9),

                // Left Dungeon Stairwell
                new Rectangle(616, 1292, 39, 17),
                new Rectangle(464, 1348, 112, 9),
                new Rectangle(543, 1420, 112, 9),

                // Right Dungeon Stairwell
                new Rectangle(1999, 1492, 65, 17),
                new Rectangle(2079, 1596, 112, 17),

                // Left Sewer Chest Room
                new Rectangle(64, 1364, 96, 17),
                new Rectangle(224, 1524, 96, 17),
                new Rectangle(64, 1612, 128, 17),

                // Center Sewer Room
                new Rectangle(1119, 1772, 96, 17),
                new Rectangle(1023, 1860, 96, 17),
                new Rectangle(927, 1948, 96, 17),

                // Sewer Boss Key Room
                new Rectangle(1887, 2180, 128, 17),

                // Boss Room
                new Rectangle (1271, 188, 113, 17),
                new Rectangle(1143, 100, 65, 17),
                new Rectangle(1447, 100, 65, 17)
            };
            _collisionRects = new List<Rectangle>();
            spawn = new Campfire();
            spawn.Rect = new Rectangle(30, 1028, 32, 32);
            spawn.Position = new Vector2(30, 1028);
            spawnLocation = new Vector2(spawnLocation.X, spawnLocation.Y+24);

            string myGames = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "My Games", "Arcane Rogue");
            Directory.CreateDirectory(myGames);
            savePath = Path.Combine(myGames, "save.json");

            // If the save file exists, we're gonna pull the game state from that. Otherwise, clean slate
            if (File.Exists(savePath))
            {
                try
                {
                    string json = File.ReadAllText(savePath);
                    SaveData data = JsonSerializer.Deserialize<SaveData>(json);

                    screenWidth = data.ScreenWidth;
                    screenHeight = data.ScreenHeight;
                    _graphics.PreferredBackBufferWidth = screenWidth;
                    _graphics.PreferredBackBufferHeight = screenHeight;
                    _graphics.IsFullScreen = data.IsFullScreen;
                    _graphics.ApplyChanges();
                    _camera.Viewport = GraphicsDevice.Viewport;
                    _camera.UpdateZoomForResolution();

                    // Restore collision and interactable rects
                    _collisionRects = data.CollisionRects
                        .Select(r => new Rectangle(r.X, r.Y, r.Width, r.Height))
                        .ToList();
                    _interactableRects = data.InteractableRects
                        .Select(r => new Rectangle(r.X, r.Y, r.Width, r.Height))
                        .ToList();                    

                    // Restore vendor
                    shopkeep = new Vendor();
                    shopkeep.Rect = new Rectangle(data.Vendor.Rect.X, data.Vendor.Rect.Y,
                                                  data.Vendor.Rect.Width, data.Vendor.Rect.Height);
                    shopkeep.Position = new Vector2(data.Vendor.Position.X, data.Vendor.Position.Y);
                    shopkeep.SpellPrice = data.Vendor.SpellPrice;
                    shopkeep.TorchPrice = data.Vendor.TorchPrice;
                    shopkeep.PotionUpgradePrice = data.Vendor.PotionUpgradePrice;
                    shopkeep.SpellInventory = data.Vendor.SpellInventory;
                    shopkeep.TorchInventory = data.Vendor.TorchInventory;
                    shopkeep.PotionUpgradeInventory = data.Vendor.PotionUpgradeInventory;

                    // Restore chests
                    _chests = data.Chests.Select(c => new Chest
                    {
                        Gold = c.Gold,
                        Rect = new Rectangle(c.Rect.X, c.Rect.Y, c.Rect.Width, c.Rect.Height),
                        Position = new Vector2(c.Position.X, c.Position.Y)
                    }).ToList();

                    // Restore player
                    _wizard = new Player(new Vector2(data.SpawnLocation.X, data.SpawnLocation.Y), _collisionRects, _platformRects, gravity,
                                         data.HasWhip, data.HasMissile, data.HasLightning, data.HasTeleport);
                    _wizard.gold = data.Gold;
                    _wizard.potions = data.Potions;
                    _wizard.playerHP = data.PlayerHP;
                    _wizard.hasTorch = data.HasTorch;
                    _wizard.potionMax = data.PotionMax;
                    introPrompt = data.IntroPrompt;

                    // Restore doors, scrolls, lights
                    _doors = data.Doors.Select(d => new Door
                    {
                        Type = d.Type,
                        Rect = new Rectangle(d.Rect.X, d.Rect.Y, d.Rect.Width, d.Rect.Height),
                        Position = new Vector2(d.Position.X, d.Position.Y),
                        Color = new Color(d.Color.R, d.Color.G, d.Color.B, d.Color.A)
                    }).ToList();

                    _scrolls = data.Scrolls.Select(s => new Scroll
                    {
                        Spell = s.Spell,
                        Rect = new Rectangle(s.Rect.X, s.Rect.Y, s.Rect.Width, s.Rect.Height),
                        Position = new Vector2(s.Position.X, s.Position.Y),
                        Color = new Color(s.Color.R, s.Color.G, s.Color.B, s.Color.A),
                        Texture = Content.Load<Texture2D>(s.Spell switch
                        {
                            "Whip" => @"Textures/whipscrollsprite",
                            "Missile" => @"Textures/missilescrollsprite",
                            "Teleport" => @"Textures/teleportscrollsprite",
                            "Lightning" => @"Textures/lightningscrollsprite",
                            _ => @"Textures/whipscrollsprite"
                        })
                    }).ToList();

                    _lights = new List<LightSource>();

                    // Call CreateKeys to get textures loaded, then remove any
                    // keys the player already has in their inventory
                    _keys = new List<Key>();
                    CreateKeys();

                    foreach (string keyType in data.PlayerKeysList)
                    {
                        Key k = _keys.First(k => k.Type == keyType);
                        _wizard.AddKey(k);
                        _keys.Remove(k);
                    }
                    for (int i = 0; i < _chests.Count; i++)
                    {
                        Chest temp = _chests[i];
                        temp.Texture = Content.Load<Texture2D>(@"Textures/chestsprite");
                        _chests[i] = temp;
                    }
                    // Restore skeletons
                    _skeletons = data.Skeletons.Select(s =>
                    {
                        var skeleton = new Skeleton(new Vector2(s.X, s.Y), _collisionRects, _platformRects, gravity);
                        skeleton.skeletonSpawnPosition = new Vector2(s.SpawnX, s.SpawnY);
                        skeleton.brokenBones = s.BrokenBones;
                        skeleton.skeletonPosition = new Vector2(s.X, s.Y);
                        return skeleton;
                    }).ToList();

                    // Restore zombies
                    _zombies = data.Zombies.Select(z =>
                    {
                        var zombie = new Zombie(new Vector2(z.X, z.Y), _collisionRects, _platformRects, gravity);
                        zombie.zombieSpawn = new Vector2(z.SpawnX, z.SpawnY);
                        zombie.zombieHP = (int)z.ZombieHP;
                        zombie.zombieDead = z.ZombieDead;
                        zombie.canRespawn = z.CanRespawn;
                        zombie.zombiePosition = new Vector2(z.X, z.Y);
                        return zombie;
                    }).ToList();

                    // Restore ghosts
                    _ghosts = data.Ghosts.Select(g =>
                    {
                        var ghost = new Ghost(new Vector2(g.X, g.Y), _collisionRects, _platformRects);
                        ghost.ghostSpawn = new Vector2(g.SpawnX, g.SpawnY);
                        ghost.ghostHP = (int)g.GhostHP;
                        ghost.ghostPosition = new Vector2(g.X, g.Y);
                        ghost.floatBaseY = g.Y;
                        return ghost;
                    }).ToList();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to load save file: " + e.Message);
                    LoadDefaults();
                }
            }
            else
            {
                LoadDefaults();
            }
            if (_skeletons == null)
            {
                _skeletons = new List<Skeleton>
                {
                    new Skeleton(new Vector2(705, 755), _collisionRects, _platformRects, gravity),
                    new Skeleton(new Vector2(1012, 755), _collisionRects, _platformRects, gravity),
                    new Skeleton(new Vector2(615, 651), _collisionRects, _platformRects, gravity),
                    new Skeleton(new Vector2(607, 475), _collisionRects, _platformRects, gravity),
                    new Skeleton(new Vector2(541, 299), _collisionRects, _platformRects, gravity),
                    new Skeleton(new Vector2(704, 339), _collisionRects, _platformRects, gravity),
                    new Skeleton(new Vector2(1018, 339), _collisionRects, _platformRects, gravity),
                    new Skeleton(new Vector2(1135, 827), _collisionRects, _platformRects, gravity),
                    new Skeleton(new Vector2(1479, 827), _collisionRects, _platformRects, gravity),
                    new Skeleton(new Vector2(1335, 995), _collisionRects, _platformRects, gravity),
                    new Skeleton(new Vector2(1752, 906), _collisionRects, _platformRects, gravity),
                    new Skeleton(new Vector2(2152, 915), _collisionRects, _platformRects, gravity),
                    new Skeleton(new Vector2(2152, 740), _collisionRects, _platformRects, gravity),
                    new Skeleton(new Vector2(2015, 475), _collisionRects, _platformRects, gravity),
                    new Skeleton(new Vector2(1754, 1227), _collisionRects, _platformRects, gravity),
                    new Skeleton(new Vector2(1179, 1227), _collisionRects, _platformRects, gravity),
                    new Skeleton(new Vector2(1450, 1227), _collisionRects, _platformRects, gravity),
                    new Skeleton(new Vector2(724, 1227), _collisionRects, _platformRects, gravity),
                    new Skeleton(new Vector2(1007, 1427), _collisionRects, _platformRects, gravity),
                    new Skeleton(new Vector2(1607, 1427), _collisionRects, _platformRects, gravity),
                    new Skeleton(new Vector2(1335, 1972), _collisionRects, _platformRects, gravity),
                };
            }
            if (_zombies == null)
            {
                _zombies = new List<Zombie>
                {
                    new Zombie(new Vector2(1647, 1227), _collisionRects, _platformRects, gravity),
                    new Zombie(new Vector2(962, 1227), _collisionRects, _platformRects, gravity),
                    new Zombie(new Vector2(857, 1427), _collisionRects, _platformRects, gravity),
                    new Zombie(new Vector2(1235, 1427), _collisionRects, _platformRects, gravity),
                    new Zombie(new Vector2(1402, 1427), _collisionRects, _platformRects, gravity),
                    new Zombie(new Vector2(1755, 1427), _collisionRects, _platformRects, gravity),
                    new Zombie(new Vector2(1716, 1627), _collisionRects, _platformRects, gravity),
                    new Zombie(new Vector2(774, 1532), _collisionRects, _platformRects, gravity),
                    new Zombie(new Vector2(803, 1620), _collisionRects, _platformRects, gravity),
                    new Zombie(new Vector2(377, 1708), _collisionRects, _platformRects, gravity),
                    new Zombie(new Vector2(571, 2059), _collisionRects, _platformRects, gravity),
                    new Zombie(new Vector2(805, 2059), _collisionRects, _platformRects, gravity),
                    new Zombie(new Vector2(1159, 1972), _collisionRects, _platformRects, gravity),
                    new Zombie(new Vector2(1645, 1972), _collisionRects, _platformRects, gravity),
                    new Zombie(new Vector2(1905, 1883), _collisionRects, _platformRects, gravity),
                    new Zombie(new Vector2(2020, 2220), _collisionRects, _platformRects, gravity),
                };
            }
            if (_ghosts == null)
            {
                _ghosts = new List<Ghost>
                {
                    new Ghost(new Vector2(842, 415), _collisionRects, _platformRects),
                    new Ghost(new Vector2(1292, 713), _collisionRects, _platformRects),
                    new Ghost(new Vector2(1603, 790), _collisionRects, _platformRects),
                    new Ghost(new Vector2(1884, 438), _collisionRects, _platformRects),
                    new Ghost(new Vector2(2062, 165), _collisionRects, _platformRects),
                    new Ghost(new Vector2(1593, 438), _collisionRects, _platformRects),
                    new Ghost(new Vector2(1304, 1171), _collisionRects, _platformRects),
                    new Ghost(new Vector2(1833, 1369), _collisionRects, _platformRects),
                    new Ghost(new Vector2(1514, 1573), _collisionRects, _platformRects),
                    new Ghost(new Vector2(948, 1724), _collisionRects, _platformRects),
                    new Ghost(new Vector2(212, 1921), _collisionRects, _platformRects),
                    new Ghost(new Vector2(2077, 2091), _collisionRects, _platformRects),
                };
            }
            _boss = new List<KingBoss>();
            base.Initialize();
        }

        // LoadDefaults and SaveGame relate to the save file
        private void LoadDefaults()
        {
            _collisionRects = new List<Rectangle>
            {
                new Rectangle(-5, 0, 5, 2341),
                new Rectangle(2191, 0, 17, 2284),
                new Rectangle(0, -5, 2191, 34),
                new Rectangle(-5, 2125, 1892, 216),
                new Rectangle(-5, 2037, 516, 88),
                new Rectangle(911, 2037, 816, 88),
                new Rectangle(1887, 2284, 321, 56),
                new Rectangle(0, 1060, 63, 978),
                new Rectangle(63, 1060, 608, 152),
                new Rectangle(63, 1212, 400, 80),
                new Rectangle(319, 1292, 17, 400),
                new Rectangle(336, 1292, 127, 320),
                new Rectangle(463, 1492, 1536, 33),
                new Rectangle(463, 1525, 88, 88),
                new Rectangle(448, 108, 17, 880),
                new Rectangle(655, 108, 17, 369),
                new Rectangle(655, 557, 17, 431),
                new Rectangle(135, 876, 313, 24),
                new Rectangle(672, 276, 1327, 17),
                new Rectangle(0, 29, 1104, 80),
                new Rectangle(0, 109, 448, 606),
                new Rectangle(672, 109, 431, 167),
                new Rectangle(1551, 29, 640, 80),
                new Rectangle(1551, 109, 449, 167),
                new Rectangle(1087, 293, 17, 184),
                new Rectangle(1551, 293, 17, 184),
                new Rectangle(1983, 293, 17, 184),
                new Rectangle(672, 556, 416, 17),
                new Rectangle(1568, 556, 416, 17),
                new Rectangle(1088, 556, 17, 433),
                new Rectangle(1551, 556, 17, 433),
                new Rectangle(1983, 556, 17, 433),
                new Rectangle(671, 1060, 1329, 49),
                new Rectangle(1087, 1109, 17, 120),
                new Rectangle(1551, 1109, 17, 120),
                new Rectangle(1983, 1109, 17, 120),
                new Rectangle(655, 1292, 1536, 17),
                new Rectangle(655, 1308, 17, 121),
                new Rectangle(1087, 1308, 17, 121),
                new Rectangle(1551, 1308, 17, 121),
                new Rectangle(1983, 1308, 17, 121),
                new Rectangle(2000, 1309, 191, 103),
                new Rectangle(551, 1596, 856, 17),
                new Rectangle(1983, 1524, 17, 104),
                new Rectangle(63, 1772, 704, 17),
                new Rectangle(495, 1789, 273, 176),
                new Rectangle(768, 1876, 159, 89),
                new Rectangle(767, 1684, 448, 17),
                new Rectangle(911, 1701, 17, 96),
                new Rectangle(1216, 1772, 191, 105),
                new Rectangle(1407, 1692, 321, 185),
                new Rectangle(1728, 1692, 463, 113),
                new Rectangle(2015, 1805, 176, 240),
                new Rectangle(1871, 1948, 144, 97),
                new Rectangle(1727, 1948, 144, 17),
                new Rectangle(1216, 1877, 16, 87),
                new Rectangle(655, 540, 1345, 17),
                new Rectangle(1983, 476, 17, 81),
            };

            _doors = new List<Door>();
            _keys = new List<Key>();
            _scrolls = new List<Scroll>();
            _chests = new List<Chest>();
            _lights = new List<LightSource>();
            _interactableRects = new List<Rectangle>();

            shopkeep = new Vendor();
            shopkeep.Rect = new Rectangle(295, 989, 49, 71);
            shopkeep.Position = new Vector2(295, 989);
            shopkeep.SpellPrice = 300;
            shopkeep.TorchPrice = 100;
            shopkeep.PotionUpgradePrice = 430;
            shopkeep.SpellInventory = 1;
            shopkeep.TorchInventory = 1;
            shopkeep.PotionUpgradeInventory = 5;

            CreateDoors();
            CreateScrolls();
            CreateChests();

            _wizard = new Player(spawnLocation, _collisionRects, _platformRects, gravity, false, false, false, false);

            CreateKeys();
        }

        private void SaveGame()
        {
            try
            {
                SaveData data = new SaveData
                {
                    Gold = _wizard.gold,
                    Potions = _wizard.potions,
                    PlayerHP = _wizard.playerHP,
                    HasWhip = _wizard.hasWhip,
                    HasMissile = _wizard.hasMissile,
                    HasLightning = _wizard.hasLightning,
                    HasTeleport = _wizard.hasTeleport,
                    HasTorch = _wizard.hasTorch,
                    IntroPrompt = introPrompt,
                    ScreenWidth = screenWidth,
                    ScreenHeight = screenHeight,
                    IsFullScreen = _graphics.IsFullScreen,
                    PotionMax = _wizard.potionMax,

                    PlayerKeysList = _wizard.playerKeyInventory
                        .Select(k => k.Type)
                        .ToList(),

                    SpawnLocation = new Vector2Data
                    {
                        X = _wizard.Position.X,
                        Y = _wizard.Position.Y
                    },

                    CollisionRects = _collisionRects
                        .Select(r => new RectData { X = r.X, Y = r.Y, Width = r.Width, Height = r.Height })
                        .ToList(),

                    InteractableRects = _interactableRects
                        .Select(r => new RectData { X = r.X, Y = r.Y, Width = r.Width, Height = r.Height })
                        .ToList(),

                    Chests = _chests.Select(c => new ChestData
                    {
                        Gold = c.Gold,
                        Rect = new RectData { X = c.Rect.X, Y = c.Rect.Y, Width = c.Rect.Width, Height = c.Rect.Height },
                        Position = new Vector2Data { X = c.Position.X, Y = c.Position.Y }

                    }).ToList(),

                    Skeletons = _skeletons.Select(s => new SkeletonData
                    {
                        X = s.skeletonPosition.X,
                        Y = s.skeletonPosition.Y,
                        SpawnX = s.skeletonSpawnPosition.X,
                        SpawnY = s.skeletonSpawnPosition.Y,
                        BrokenBones = s.brokenBones
                    }).ToList(),

                    Zombies = _zombies.Select(z => new ZombieData
                    {
                        X = z.zombiePosition.X,
                        Y = z.zombiePosition.Y,
                        SpawnX = z.zombieSpawn.X,
                        SpawnY = z.zombieSpawn.Y,
                        ZombieHP = z.zombieHP,
                        ZombieDead = z.zombieDead,
                        CanRespawn = z.canRespawn
                    }).ToList(),

                    Ghosts = _ghosts.Select(g => new GhostData
                    {
                        X = g.ghostPosition.X,
                        Y = g.floatBaseY,
                        SpawnX = g.ghostSpawn.X,
                        SpawnY = g.ghostSpawn.Y,
                        GhostHP = g.ghostHP
                    }).ToList(),

                    Vendor = new VendorData
                    {
                        Rect = new RectData { X = shopkeep.Rect.X, Y = shopkeep.Rect.Y, Width = shopkeep.Rect.Width, Height = shopkeep.Rect.Height },
                        Position = new Vector2Data { X = shopkeep.Position.X, Y = shopkeep.Position.Y },
                        SpellPrice = shopkeep.SpellPrice,
                        TorchPrice = shopkeep.TorchPrice,
                        PotionUpgradePrice = shopkeep.PotionUpgradePrice,
                        SpellInventory = shopkeep.SpellInventory,
                        TorchInventory = shopkeep.TorchInventory,
                        PotionUpgradeInventory = shopkeep.PotionUpgradeInventory,
                    },

                    Doors = _doors.Select(d => new DoorData
                    {
                        Type = d.Type,
                        Rect = new RectData { X = d.Rect.X, Y = d.Rect.Y, Width = d.Rect.Width, Height = d.Rect.Height },
                        Position = new Vector2Data { X = d.Position.X, Y = d.Position.Y },
                        Color = new ColorData { R = d.Color.R / 255f, G = d.Color.G / 255f, B = d.Color.B / 255f, A = d.Color.A / 255f }
                    }).ToList(),

                    Scrolls = _scrolls.Select(s => new ScrollData
                    {
                        Spell = s.Spell,
                        Rect = new RectData { X = s.Rect.X, Y = s.Rect.Y, Width = s.Rect.Width, Height = s.Rect.Height },
                        Position = new Vector2Data { X = s.Position.X, Y = s.Position.Y },
                        Color = new ColorData { R = s.Color.R / 255f, G = s.Color.G / 255f, B = s.Color.B / 255f, A = s.Color.A / 255f }
                    }).ToList(),

                    WorldKeys = _keys.Select(k => new WorldKeyData
                    {
                        Type = k.Type,
                        Rect = new RectData { X = k.Rect.X, Y = k.Rect.Y, Width = k.Rect.Width, Height = k.Rect.Height },
                        Position = new Vector2Data { X = k.Position.X, Y = k.Position.Y },
                        Color = new ColorData { R = k.Color.R / 255f, G = k.Color.G / 255f, B = k.Color.B / 255f, A = k.Color.A / 255f }
                    }).ToList()
                };


                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(savePath, json);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to save game: " + e.Message);
            }
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            map = Content.Load<Texture2D>(@"Textures/Map");
            bigText = Content.Load<SpriteFont>("BigText");
            smallText = Content.Load<SpriteFont>("SmallText");
            shoppingText = Content.Load<SpriteFont>("TinyText");
            campfire = Content.Load<Texture2D>(@"Textures/fireplace");
            vendorTexture = Content.Load<Texture2D>(@"Textures/vendor");
            bossDoorTexture = Content.Load<Texture2D>(@"Textures/bossdoor");
            regularDoorTexture = Content.Load<Texture2D>(@"Textures/regulardoor");

            _wizard.idle = (Content.Load<Texture2D>(@"Textures/Player/playeridle"));
            _wizard.run = (Content.Load<Texture2D>(@"Textures/Player/playerrun"));
            _wizard.jump = (Content.Load<Texture2D>(@"Textures/Player/playerjump"));
            _wizard.shootMissile = (Content.Load<Texture2D>(@"Textures/Player/playermissile"));
            _wizard.whipWalk = (Content.Load<Texture2D>(@"Textures/Player/playerwhip"));
            _wizard.whipArm = (Content.Load<Texture2D>(@"Textures/Player/whiparm"));
            _wizard.missile = Content.Load<Texture2D>(@"Textures/Player/missile");
            _wizard.whipHandle = Content.Load<Texture2D>(@"Textures/Player/whiphandle");
            _wizard.whipSegmentTexture = Content.Load<Texture2D>(@"Textures/Player/whipsegment");
            _wizard.whipTip = Content.Load<Texture2D>(@"Textures/Player/whiptip");
            foreach(Skeleton skeleton  in _skeletons)
            {
                skeleton.skeletonIdle = Content.Load<Texture2D>(@"Textures/Skeleton/skeletonidle");
                skeleton.skeletonWalk = Content.Load<Texture2D>(@"Textures/Skeleton/skeletonwalk");
                skeleton.skeletonBow = Content.Load<Texture2D>(@"Textures/Skeleton/skeletonbow");
                skeleton.arrowTexture = Content.Load<Texture2D>(@"Textures/Skeleton/arrow");
                skeleton.bowTexture = Content.Load<Texture2D>(@"Textures/Skeleton/bowsheet");
                skeleton.skeletonStab = Content.Load<Texture2D>(@"Textures/Skeleton/skeletonstab");
                skeleton.brokenBonesTexture = Content.Load<Texture2D>(@"Textures/Skeleton/brokenbones");
            }
            foreach (Zombie zombie in _zombies)
            {
                zombie.idle = Content.Load<Texture2D>(@"Textures/Zombie/zombieidle");
                zombie.walk = Content.Load<Texture2D>(@"Textures/Zombie/zombiewalk");
                zombie.crawl = Content.Load<Texture2D>(@"Textures/Zombie/zombiecrawl");
                zombie.crawlIdle = Content.Load<Texture2D>(@"Textures/Zombie/zombiecrawlidle");
                zombie.gas = Content.Load<Texture2D>(@"Textures/Zombie/zombiegas");
            }
            foreach (Ghost ghost in _ghosts)
            {
                ghost.floating = Content.Load<Texture2D>(@"Textures/Ghost/ghostfloat");
                ghost.throwing = Content.Load<Texture2D>(@"Textures/Ghost/ghostthrow");
                ghost.screaming = Content.Load<Texture2D>(@"Textures/Ghost/ghostscream");
                ghost.chairTexture = Content.Load<Texture2D>(@"Textures/Ghost/chair");
                ghost.wailTexture = Content.Load<Texture2D>(@"Textures/Ghost/wail");
            }
            //CreateKeys();
            CreateIcons();
            CreateLightSources();

            foreach(Door door in _doors)
            {
                _interactableRects.Add(door.Rect);
            }
            foreach(Chest chest in _chests)
            {
                _interactableRects.Add(chest.Rect);
            }
            _interactableRects.Add(spawn.Rect);
            _interactableRects.Add(shopkeep.Rect);

            _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });

            _lightingEffect = Content.Load<Effect>(@"Effects/AmbientDiffuseLight");

            _lightingEffect.Parameters["AmbientColor"].SetValue(new Vector3(1f, 1f, 1f));
            _lightingEffect.Parameters["AmbientIntensity"].SetValue(0.01f);

            // The sound effects are quite a bit messy. This got the least amount of work when finishing the project
            // for the semester. Everything else is well polished though!
            backgroundMusic = Content.Load<Song>(@"SFX/backgroundtrack");
            whipCrack = Content.Load<SoundEffect>(@"SFX/whipcrack");
            whipInstance = whipCrack.CreateInstance();
            lightningCrackle = Content.Load<SoundEffect>(@"SFX/lightningcrackle");
            lightningInstance = lightningCrackle.CreateInstance();
            teleportSound = Content.Load<SoundEffect>(@"SFX/teleport");
            teleportInstance = teleportSound.CreateInstance();
            missileSound = Content.Load<SoundEffect>(@"SFX/missile");
            footstep = Content.Load<SoundEffect>(@"SFX/footstep");
            footstepInstance = footstep.CreateInstance();
            playerHurt = Content.Load<SoundEffect>(@"SFX/hurt");
            hurtInstance = playerHurt.CreateInstance();
            potionSound = Content.Load<SoundEffect>(@"SFX/potion");
            potionInstance = potionSound.CreateInstance();
            zombieSnarl = Content.Load<SoundEffect>(@"SFX/zombieSnarl");
            snarlSound = zombieSnarl;
            zombieGasSound = Content.Load<SoundEffect>(@"SFX/gaspoof");
            skeletonBones = Content.Load<SoundEffect>(@"SFX/skeletonwalking");
            bonesInstance = skeletonBones.CreateInstance();
            bowRelease = Content.Load<SoundEffect>(@"SFX/bowrelease");
            ghostScream = Content.Load<SoundEffect>(@"SFX/ghostscream");
            screamInstance = ghostScream.CreateInstance();
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.3f;
            MediaPlayer.Play(backgroundMusic);

            foreach(Zombie zombie in _zombies)
            {
                zombie.Init();
            }
        }

        protected override void Update(GameTime gameTime)
        {
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            if (introPrompt)
            {
                introPromptTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                _camera.Follow(_wizard.Position);
                if (Keyboard.GetState().IsKeyDown(Keys.F) && previousKeyboardState.IsKeyUp(Keys.F))
                {
                    introPrompt = !introPrompt;
                }
            }
            else
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape))
                    paused = !paused;
                if (bossDefeated)
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape))
                        Exit();
                }
                else
                {
                    if (!paused)
                    {
                        // When the player dies, we reset the game state
                        if (_wizard.dead && Keyboard.GetState().IsKeyDown(Keys.P))
                            ResetGameStateUponPlayerDeath();

                        // This section handles player and enemy updates outside of picking up keys and scrolls, as doing so creates a UI message
                        if (!pickedUpBossKey && !pickedUpRegularKey && !pickedUpWhip && !pickedUpMissile && !pickedUpLightning && !pickedUpTeleport)
                        {
                            _wizard.Update(gameTime, Keyboard.GetState(), Mouse.GetState(), _camera, _skeletons, _zombies, _ghosts, _boss, shopping);
                            foreach (Skeleton enemy in _skeletons)
                            {
                                enemy.Update(gameTime, _wizard, map.Width, map.Height);
                            }
                            for (int i = 0; i < _zombies.Count; i++)
                            {
                                _zombies[i].Update(gameTime, _wizard);
                                if (_zombies[i].zombieHP == 0 && _zombies[i].zombieDeathTimer >= 3f)
                                {
                                    _zombies[i].zombieDead = true;
                                }
                            }
                            for (int i = 0; i < _ghosts.Count; i++)
                            {
                                _ghosts[i].Update(gameTime, _wizard);
                            }
                            for (int i = 0; i < _boss.Count; i++)
                            {
                                _boss[i].Update(gameTime, _wizard, _zombies, Content);
                                if (_boss[i].bossHP <= 0)
                                {
                                    bossDefeated = true;
                                    _boss.RemoveAt(i);
                                }
                            }
                        }
                        else
                        {
                            if (Keyboard.GetState().IsKeyDown(Keys.F) && previousKeyboardState.IsKeyUp(Keys.F))
                            {
                                pickedUpBossKey = false;
                                pickedUpRegularKey = false;
                                pickedUpWhip = false;
                                pickedUpMissile = false;
                                pickedUpLightning = false;
                                pickedUpTeleport = false;
                            }
                        }
                        // In addition to updating entities, update stationary objects
                        UpdateInteractableObjects();

                        _camera.Follow(_wizard.Position);
                    }
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0.02f, 0.04f, 0.1f));

            // Update light position to follow player center each frame
            // Build the full light list: world lights + player light
            var playerCenter = _wizard.AnchorPoint;

            var nearest = _lights
                .OrderBy(l => Vector2.DistanceSquared(l.Position, playerCenter))
                .Take(19)
                .ToList();

            if (_wizard.hasTorch)
            {
                nearest.Add(new LightSource
                {
                    Position = playerCenter,
                    Color = new Vector3(1f, 0.9f, 0.7f),
                    Intensity = 1.0f,
                    Radius = 300f
                });
            }

            var positions = new Vector2[20];
            var colors = new Vector3[20];
            var radiusIntensity = new Vector2[20];

            for (int i = 0; i < nearest.Count; i++)
            {
                positions[i] = nearest[i].Position;
                colors[i] = nearest[i].Color;
                radiusIntensity[i] = new Vector2(nearest[i].Radius, nearest[i].Intensity);
            }

            _lightingEffect.Parameters["LightPositions"].SetValue(positions);
            _lightingEffect.Parameters["LightColors"].SetValue(colors);
            _lightingEffect.Parameters["LightRadiusIntensity"].SetValue(radiusIntensity);

            Matrix wvp = _camera.GetTransformationMatrix() *
             Matrix.CreateOrthographicOffCenter(
                 0, GraphicsDevice.Viewport.Width,
                 GraphicsDevice.Viewport.Height, 0,
                 0, 1);
            _lightingEffect.Parameters["WorldViewProjection"].SetValue(wvp);

            
            _spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null, null,
                _lightingEffect,
                _camera.GetTransformationMatrix()
            );

            // Draw the world, doors, scrolls, chests, etc.
            _spriteBatch.Draw(map, new Vector2(0, 0), Color.White);
            foreach (Door door in _doors)
            {
                if (door.Type == "Regular")
                {
                    _spriteBatch.Draw(
                                regularDoorTexture,
                                new Vector2(door.Rect.X, door.Rect.Y),
                                null,
                                Color.White,
                                0f,
                                Vector2.Zero,
                                1f,
                                SpriteEffects.None,
                                0f
                            );
                }
                else
                {
                    _spriteBatch.Draw(
                                bossDoorTexture,
                                new Vector2(door.Rect.X, door.Rect.Y),
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
            foreach (Scroll scroll in _scrolls)
                _spriteBatch.Draw(scroll.Texture, scroll.Rect, Color.White);
            foreach (Chest chest in _chests)
                _spriteBatch.Draw(chest.Texture, chest.Rect, Color.White);

            campfireFrameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            float frameDuration = 0.125f;
            if (campfireFrameTimer >= frameDuration && !paused)
            {
                campfireFrameTimer = 0;
                campfireFrameCount++;
            }
            if (campfireFrameCount > 5)
            {
                campfireFrameCount = 0;
            }
            _spriteBatch.Draw(
                            campfire,
                            new Vector2(spawn.Position.X, spawn.Position.Y),
                            GetCampfireFrameRect(campfireFrameCount, campfire),
                            Color.White,
                            0f,
                            Vector2.Zero,
                            1,
                            SpriteEffects.None,  // mirrors left-right
                            0f
                        );
            _spriteBatch.Draw(
                            vendorTexture,
                            new Vector2(shopkeep.Position.X - 40, shopkeep.Position.Y - 57),
                            new Rectangle(0, 0, 128, 128),
                            Color.White,
                            0f,
                            Vector2.Zero,
                            1,
                            SpriteEffects.None,  // mirrors left-right
                            0f
                        );

            foreach (Key key in _keys)
            {
                if (key.Texture == null)
                    _spriteBatch.Draw(_pixelTexture, key.Rect, key.Color);
                else
                    _spriteBatch.Draw(key.Texture, key.Rect, Color.White);
            }

            // Draw wizard and enemies
            _wizard.Draw(_spriteBatch, _pixelTexture);

            foreach (Skeleton enemy in _skeletons)
                enemy.Draw(_spriteBatch, _pixelTexture);
            foreach (Zombie enemy in _zombies)
                if (!enemy.zombieDead) enemy.Draw(_spriteBatch, _pixelTexture, gameTime);
            foreach (Ghost ghost in _ghosts)
                if (!(ghost.ghostHP <= 0)) ghost.Draw(_spriteBatch, _pixelTexture, gameTime);
            foreach (KingBoss boss in _boss)
                boss.Draw(_spriteBatch, _pixelTexture, gameTime);

            // Draw in-game UI (using in-game world coordinates)
            foreach (Rectangle rect in _interactableRects)
            {
                if (_wizard.Rect.Intersects(rect) && !paused && !introPrompt)
                {
                    if (rect != _doors[0].Rect || _doors.Count >= 2 && rect != _doors[1].Rect)
                    {
                        String text = "Press F to Interact";
                        float scale = 0.6f; // Adjust this value to control size (e.g. 0.5f = half size)
                        Vector2 textSize = shoppingText.MeasureString(text) * scale;
                        Vector2 textPosition = new Vector2(rect.X + rect.Width / 2 - textSize.X / 2, rect.Y - 20);
                        if (rect == spawn.Rect)
                        {
                            textPosition.X += 10;
                        }
                        _spriteBatch.DrawString(shoppingText, text, textPosition, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                    }
                    else if (rect == _doors[0].Rect && _wizard.playerKeyInventory[0].Type == "Regular" ||
                        rect == _doors[1].Rect && _wizard.playerKeyInventory[1].Type == "Boss")
                    {
                        String text = "Press F to Interact";
                        float scale = 0.6f; // Adjust this value to control size (e.g. 0.5f = half size)
                        Vector2 textSize = shoppingText.MeasureString(text) * scale;
                        Vector2 textPosition = new Vector2(rect.X + rect.Width / 2 - textSize.X / 2, rect.Y - 20);
                        if (rect == spawn.Rect)
                        {
                            textPosition.X += 10;
                        }
                        _spriteBatch.DrawString(shoppingText, text, textPosition, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            _spriteBatch.End();

            _spriteBatch.Begin();
            DisplayUI(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        public Rectangle GetCampfireFrameRect(int frame, Texture2D spriteSheet)
        {
            int columns = spriteSheet.Width / 32;
            int column = frame % columns;
            int row = frame / columns;
            return new Rectangle(column * 32, row * 32, 32, 32);
        }
        void CreateDoors()
        {
            Door dungeonDoor = new Door();
            dungeonDoor.Type = "Regular";
            dungeonDoor.Rect = new Rectangle(1983, 1428, 17, 65);
            dungeonDoor.Position = new Vector2(1983, 1428);
            dungeonDoor.Color = Color.SaddleBrown;

            Door bossDoor = new Door();
            bossDoor.Type = "Boss";
            bossDoor.Rect = new Rectangle(1303, 382, 49, 70);
            bossDoor.Position = new Vector2(1303, 396);
            bossDoor.Color = Color.Blue;

            _doors.Add(dungeonDoor);
            _collisionRects.Add(dungeonDoor.Rect);
            _doors.Add(bossDoor);
        }
        void CreateKeys()
        {
            Key dungeonDoorKey = new Key();
            dungeonDoorKey.Type = "Regular";
            dungeonDoorKey.Rect = new Rectangle(471, 268, 17, 8);
            dungeonDoorKey.Position = new Vector2(471, 268);
            dungeonDoorKey.Color = Color.Gold;
            dungeonDoorKey.Texture = Content.Load<Texture2D>(@"Textures/regularkeysprite");


            Key bossRoomKey = new Key();
            bossRoomKey.Type = "Boss";
            bossRoomKey.Rect = new Rectangle(1895, 2276, 17, 8);
            bossRoomKey.Position = new Vector2(1895, 2276);
            bossRoomKey.Color = Color.Blue;
            bossRoomKey.Texture = Content.Load<Texture2D>(@"Textures/bosskeysprite");

            if (_wizard.playerKeyInventory.Count == 0)
            {
                _keys.Add(dungeonDoorKey);
                _keys.Add(bossRoomKey);
            }
            else if (_wizard.playerKeyInventory.Count == 1)
            {
                _keys.Add(bossRoomKey);
            }
            else if (_wizard.playerKeyInventory.Count == 2)
            {
                _keys.Clear();
            }
        }
        void CreateScrolls()
        {
            Scroll whipScroll = new Scroll();
            whipScroll.Spell = "Whip";
            whipScroll.Position = new Vector2(872, 963);
            whipScroll.Rect = new Rectangle(872, 963, 17, 8);
            whipScroll.Color = Color.SaddleBrown;
            whipScroll.Texture = Content.Load<Texture2D>(@"Textures/whipscrollsprite");

            Scroll missileScroll = new Scroll();
            missileScroll.Spell = "Missile";
            missileScroll.Position = new Vector2(1928, 884);
            missileScroll.Rect = new Rectangle(1928, 884, 17, 8);
            missileScroll.Color = Color.Magenta;
            missileScroll.Texture = Content.Load<Texture2D>(@"Textures/missilescrollsprite");

            Scroll teleportScroll = new Scroll();
            teleportScroll.Spell = "Teleport";
            teleportScroll.Position = new Vector2(1919, 484);
            teleportScroll.Rect = new Rectangle(1919, 1484, 17, 8);
            teleportScroll.Color = Color.Purple;
            teleportScroll.Texture = Content.Load<Texture2D>(@"Textures/teleportscrollsprite");

            _scrolls.Add(teleportScroll);
            _scrolls.Add(whipScroll);
            _scrolls.Add(missileScroll);
        }
        void CreateChests()
        {
            Chest chest1 = new Chest();
            Chest chest2 = new Chest();
            Chest chest3 = new Chest();
            Chest chest4 = new Chest();
            Chest chest5 = new Chest();
            Chest chest6 = new Chest();
            Chest chest7 = new Chest();
            Chest chest8 = new Chest();
            Chest chest9 = new Chest();
            Chest chest10 = new Chest();
            Chest chest11 = new Chest();
            Chest chest12 = new Chest();
            Chest chest13 = new Chest();
            Chest chest14 = new Chest();

            // First Garden
            chest1.Position = new Vector2(673, 628);
            chest1.Rect = new Rectangle(673, 628, 50, 16);
            chest1.Gold = 50;

            chest2.Position = new Vector2(728, 628);
            chest2.Rect = new Rectangle(728, 628, 50, 16);
            chest2.Gold = 50;

            chest3.Position = new Vector2(983, 628);
            chest3.Rect = new Rectangle(983, 628, 50, 16);
            chest3.Gold = 50;

            chest4.Position = new Vector2(1037, 628);
            chest4.Rect = new Rectangle(1037, 628, 50, 16);
            chest4.Gold = 50;

            // Second Garden
            chest5.Position = new Vector2(1592, 700);
            chest5.Rect = new Rectangle(1592, 700, 50, 16);
            chest5.Gold = 300;

            // Second Tower
            chest6.Position = new Vector2(2001, 260);
            chest6.Rect = new Rectangle(2001, 260, 50, 16);
            chest6.Gold = 150;

            chest7.Position = new Vector2(2140, 260);
            chest7.Rect = new Rectangle(2140, 260, 50, 16);
            chest7.Gold = 150;

            // Room above Second Garden
            chest8.Position = new Vector2(1695, 388);
            chest8.Rect = new Rectangle(1695, 388, 50, 16);
            chest8.Gold = 50;

            chest9.Position = new Vector2(1806, 388);
            chest9.Rect = new Rectangle(1806, 388, 50, 16);
            chest9.Gold = 50;

            // Sewer Chests
            chest10.Position = new Vector2(553, 1580);
            chest10.Rect = new Rectangle(553, 1580, 50, 16);
            chest10.Gold = 100;

            chest11.Position = new Vector2(80, 1348);
            chest11.Rect = new Rectangle(80, 1348, 50, 16);
            chest11.Gold = 300;

            chest12.Position = new Vector2(80, 2020);
            chest12.Rect = new Rectangle(80, 2020, 50, 16);
            chest12.Gold = 75;

            chest13.Position = new Vector2(1965, 1932);
            chest13.Rect = new Rectangle(1965, 1932, 50, 16);
            chest13.Gold = 75;

            chest14.Position = new Vector2(1303, 612);
            chest14.Rect = new Rectangle(1303, 612, 50, 16);
            chest14.Gold = 500;

            _chests.Add(chest1);
            _chests.Add(chest2);
            _chests.Add(chest3);
            _chests.Add(chest4);
            _chests.Add(chest5);
            _chests.Add(chest6);
            _chests.Add(chest7);
            _chests.Add(chest8);
            _chests.Add(chest9);
            _chests.Add(chest10);
            _chests.Add(chest11);
            _chests.Add(chest12);
            _chests.Add(chest13);
            _chests.Add(chest14);

            for (int i = 0; i < _chests.Count; i++)
            {
                Chest temp = _chests[i];
                temp.Texture = Content.Load<Texture2D>(@"Textures/chestsprite");
                _chests[i] = temp;
            }
        }
        void CreateIcons()
        {
            whipIcon = Content.Load<Texture2D>(@"Textures/whipuisprite");
            missileIcon = Content.Load<Texture2D>(@"Textures/missileuisprite");
            teleportIcon = Content.Load<Texture2D>(@"Textures/teleportuisprite");
            goldIcon = Content.Load<Texture2D>(@"Textures/golduisprite");
            potionIcon = Content.Load<Texture2D>(@"Textures/potionuisprite");
            lightningIcon = Content.Load<Texture2D>(@"Textures/lightninguisprite");
        }
        void CreateLightSources()
        {
            _lights = new List<LightSource>
            {
                new LightSource
                {
                    Position = new Vector2(46, 1044),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 600f
                },
                new LightSource
                {
                    Position = new Vector2(872, 970),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 300f
                },
                new LightSource
                {
                    Position = new Vector2(1776, 970),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 300f
                },
                new LightSource
                {
                    Position = new Vector2(551, 1364),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 300f
                },
                new LightSource
                {
                    Position = new Vector2(2095, 1172),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 300f
                },
                new LightSource
                {
                    Position = new Vector2(559, 980),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(559, 850),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(559, 765),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(559, 676),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(559, 589),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(559, 412),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(559, 204),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(887, 445),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(1767, 445),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(1327, 357),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(1175, 212),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(1480, 212),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(1327, 140),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(2103, 1020),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(2103, 844),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(2103, 676),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(2103, 492),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(2103, 204),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(1215, 996),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(1471, 996),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(1327, 740),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(1895, 1196),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(1671, 1196),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(1447, 1196),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(1223, 1196),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(999, 1196),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(791, 1196),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(1895, 1404),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(1671, 1404),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(1447, 1404),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(1223, 1404),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(999, 1404),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(791, 1404),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(880, 730),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
                new LightSource
                {
                    Position = new Vector2(1621, 715),
                    Color = new Vector3(1f, 0.5f, 0.1f),
                    Intensity = 1.0f,
                    Radius = 200f
                },
            };

            for(int i = 0; i < _lights.Count; i++)
            {
                if (i > 0)
                {
                    var light = _lights[i];
                    light.Radius = 300f;
                    light.Color = new Vector3(1, 1, 1);
                    _lights[i] = light;
                }
            }
        }
        void DisplayUI(SpriteBatch screen)
        {
            int leftUIBoundary = (int)(screenWidth / 15);
            int rightUIBoundary = (int)(screenWidth / 15 * 14);
            int centerUIX = (int)(screenWidth / 15 * 7.5);
            int topUIBoundary = (int)(screenHeight / 15);
            int bottomUIBoundary = (int)(screenHeight / 15 * 14);

            int shoppingWindowLeftBoundary = screenWidth / 7 * 4;
            int shoppingWindowRightBoundary = screenWidth / 7 * 4 + 300;
            int shoppingWindowTopBoundary = screenHeight / 3;
            int shoppingWindowBottomBoundary = screenHeight / 3 + 300;
            Rectangle shoppingWindowBackground = new Rectangle(shoppingWindowLeftBoundary, shoppingWindowTopBoundary, 300, 300);
            Rectangle topWindowBar = new Rectangle(shoppingWindowLeftBoundary, shoppingWindowTopBoundary, 300, 3);
            Rectangle leftWindowBar = new Rectangle(shoppingWindowLeftBoundary, shoppingWindowTopBoundary, 3, 300);
            Rectangle bottomWindowBar = new Rectangle(shoppingWindowLeftBoundary, shoppingWindowBottomBoundary, 300, 3);
            Rectangle rightWindowBar = new Rectangle(shoppingWindowRightBoundary, shoppingWindowTopBoundary, 3, 300);

            float healthLength = _wizard.playerHP / 20f * 297;
            Rectangle healthBarFill = new Rectangle(leftUIBoundary+3, topUIBoundary, (int)healthLength, 23);

            Rectangle healthBarTop = new Rectangle(leftUIBoundary, topUIBoundary, 300, 3);
            Rectangle healthBarLeft = new Rectangle(leftUIBoundary, topUIBoundary, 3, 20);
            Rectangle healthBarBottom = new Rectangle(leftUIBoundary, topUIBoundary+20, 300, 3);
            Rectangle healthBarRight = new Rectangle(leftUIBoundary+300, topUIBoundary, 3, 23);

            Rectangle healthBarBackground = new Rectangle(leftUIBoundary, topUIBoundary, 300, 23);
            screen.Draw(_pixelTexture, healthBarBackground, Color.Black);
            screen.Draw(_pixelTexture, healthBarFill, Color.Red);
            screen.Draw(_pixelTexture, healthBarTop, Color.White);
            screen.Draw(_pixelTexture, healthBarLeft, Color.White);
            screen.Draw(_pixelTexture, healthBarBottom, Color.White);
            screen.Draw(_pixelTexture, healthBarRight, Color.White);

            String playerHPString = "HP";
            Vector2 playerHPTextSize = smallText.MeasureString(playerHPString);
            Vector2 playerHPPosition = new Vector2(leftUIBoundary,
                    (topUIBoundary - playerHPTextSize.Y)
                );
            screen.DrawString(smallText, playerHPString, playerHPPosition, Color.White);

            screen.Draw(potionIcon, new Rectangle((int)(leftUIBoundary + playerHPTextSize.X+5), topUIBoundary - 35, 32, 32), Color.White);

            String potionCountText = _wizard.potions.ToString();
            Vector2 potionTextSize = smallText.MeasureString(potionCountText);
            Vector2 potionTextPosition = new Vector2(leftUIBoundary+100,
                    topUIBoundary - potionTextSize.Y
                );
            screen.DrawString(smallText, potionCountText, potionTextPosition, Color.White);

            screen.Draw(goldIcon, new Rectangle(leftUIBoundary + 160, topUIBoundary - 35, 32, 32), Color.White);

            String playerGold = _wizard.gold.ToString();
            Vector2 playerGoldTextSize = smallText.MeasureString(playerGold);
            Vector2 playerGoldPosition = new Vector2(leftUIBoundary + 200,
                    (topUIBoundary - playerGoldTextSize.Y)
                );
            screen.DrawString(smallText, playerGold, playerGoldPosition, Color.White);

            for (int i = 0; i < _wizard.playerKeyInventory.Count; i++)
            {
                screen.Draw(_wizard.playerKeyInventory[i].Texture, new Rectangle(leftUIBoundary + (i * 35), topUIBoundary+25, 32, 16), Color.White);
            }

            if (_wizard.hasWhip)
            {
                screen.Draw(whipIcon, new Rectangle(leftUIBoundary + 70, bottomUIBoundary - 20, 64, 64), Color.White);
            }
            if (_wizard.hasMissile)
            {
                screen.Draw(missileIcon, new Rectangle(leftUIBoundary, bottomUIBoundary - 84, 64, 64), Color.White);
            }
            if (_wizard.hasTeleport)
            {
                screen.Draw(teleportIcon, new Rectangle(leftUIBoundary + 70, bottomUIBoundary - 148, 64, 64), Color.White);
            }
            if (_wizard.hasLightning)
            {
                screen.Draw(lightningIcon, new Rectangle(leftUIBoundary + 140, bottomUIBoundary - 84, 64, 64), Color.White);
            }

            if (_boss.Count > 0)
            {
                float bossHealthLength = _boss[0].bossHP / _boss[0].bossMaxHP * 297;
                Rectangle bossHealthBarFill = new Rectangle(rightUIBoundary - 297, topUIBoundary, (int)bossHealthLength, 23);

                Rectangle bossHealthBarTop = new Rectangle(rightUIBoundary - 300, topUIBoundary, 300, 3);
                Rectangle bossHealthBarLeft = new Rectangle(rightUIBoundary-300, topUIBoundary, 3, 20);
                Rectangle bossHealthBarBottom = new Rectangle(rightUIBoundary-300, topUIBoundary + 20, 300, 3);
                Rectangle bossHealthBarRight = new Rectangle(rightUIBoundary, topUIBoundary, 3, 23);

                Rectangle bossHealthBarBackground = new Rectangle(rightUIBoundary-300, topUIBoundary, 300, 23);
                screen.Draw(_pixelTexture, bossHealthBarBackground, Color.Black);
                screen.Draw(_pixelTexture, bossHealthBarFill, Color.Red);
                screen.Draw(_pixelTexture, bossHealthBarTop, Color.White);
                screen.Draw(_pixelTexture, bossHealthBarLeft, Color.White);
                screen.Draw(_pixelTexture, bossHealthBarBottom, Color.White);
                screen.Draw(_pixelTexture, bossHealthBarRight, Color.White);

                String bossHPString = "Boss HP";
                Vector2 bossHPTextSize = smallText.MeasureString(playerHPString);
                Vector2 bossHPPosition = new Vector2(rightUIBoundary-300,
                        (topUIBoundary - playerHPTextSize.Y)
                    );
                screen.DrawString(smallText, bossHPString, bossHPPosition, Color.White);
            }
            if (shopping)
            {
                screen.Draw(_pixelTexture, shoppingWindowBackground, Color.Black * 0.5f);
                screen.Draw(_pixelTexture, topWindowBar, Color.White);
                screen.Draw(_pixelTexture, leftWindowBar, Color.White);
                screen.Draw(_pixelTexture, bottomWindowBar, Color.White);
                screen.Draw(_pixelTexture, rightWindowBar, Color.White);

                String vendorGreeting;
                if (!boughtSomething)
                {
                    if (shopkeep.SpellInventory > 0 && !_wizard.hasTorch)
                    {
                        vendorGreeting = "Greetings!\nHeading into the castle? Why\nnot buy my new spell?" +
                                         " \n\nI also recommend taking a\ntorch. You never know how \ndark a" +
                                         " new area could be...\n\nI also sell potions.\n" +
                                         "(Press F to Continue)";
                    }
                    else if (shopkeep.SpellInventory > 0 && _wizard.hasTorch)
                    {
                        vendorGreeting = "Hello there!\nI still have the\nlightning spell!\nPerfect for crowd control!\n" +
                                         "(Press F to Continue)";
                    }
                    else if (shopkeep.SpellInventory <= 0 && !_wizard.hasTorch)
                    {
                        vendorGreeting = "Good Evening!\nI still have a torch for sale!\n" +
                                         "(Press F to Continue)";
                    }
                    else
                    {
                        if (shopkeep.PotionUpgradeInventory > 0)
                        {
                            vendorGreeting = "Welcome!\nI have plenty of potions\nfor sale!\n" +
                                         "(Press F to Continue)";
                        }
                        else
                        {
                            vendorGreeting = "Touch luck! I'm all sold out!\n" +
                                         "(Press F to Continue)";
                        }
                    }
                }
                else
                {
                    vendorGreeting = "Thank you!\nSafe travels, friend\n" +
                                     "(Press F to Continue)";
                }
                Vector2 vendorGreetingPosition = new Vector2(shoppingWindowLeftBoundary + 20,
                        shoppingWindowTopBoundary + 20
                    );
                screen.DrawString(shoppingText, vendorGreeting, vendorGreetingPosition, Color.White);

                String lightningPrice;
                if (shopkeep.SpellInventory > 0)
                {
                    lightningPrice = "1. Chain Lightning = 300 G";
                }
                else
                {
                    lightningPrice = "Chain Lightning = SOLD OUT";
                }
                Vector2 lightningPriceTextPosition = new Vector2(shoppingWindowLeftBoundary + 20,
                        shoppingWindowBottomBoundary - 80
                    );
                screen.DrawString(shoppingText, lightningPrice, lightningPriceTextPosition, Color.White);

                String torchPrice;
                if (shopkeep.TorchInventory > 0)
                {
                    torchPrice = "2. Torch = 100 G";
                }
                else
                {
                    torchPrice = "Torch = SOLD OUT";
                }
                Vector2 torchPriceTextPosition = new Vector2(shoppingWindowLeftBoundary + 20,
                        shoppingWindowBottomBoundary - 60
                    );
                screen.DrawString(shoppingText, torchPrice, torchPriceTextPosition, Color.White);
                
                String potionPrice;
                if (shopkeep.PotionUpgradeInventory > 0)
                {
                    potionPrice = "3. Maximize Potions = 310 G";
                }
                else
                {
                    potionPrice = "Maximize Potions = SOLD OUT";
                }
                Vector2 potionPriceTextPosition = new Vector2(shoppingWindowLeftBoundary + 20,
                        shoppingWindowBottomBoundary - 40
                    );
                screen.DrawString(shoppingText, potionPrice, potionPriceTextPosition, Color.White);
            }
            if (pickedUpRegularKey || pickedUpBossKey || pickedUpWhip || pickedUpMissile || pickedUpLightning || pickedUpTeleport)
            {
                screen.Draw(_pixelTexture, shoppingWindowBackground, Color.Black * 0.5f);
                screen.Draw(_pixelTexture, topWindowBar, Color.White);
                screen.Draw(_pixelTexture, leftWindowBar, Color.White);
                screen.Draw(_pixelTexture, bottomWindowBar, Color.White);
                screen.Draw(_pixelTexture, rightWindowBar, Color.White);

                if (pickedUpRegularKey)
                {
                    String text = "You picked up a normal\nlooking key! But where does\nit go...?\n\n" +
                                  "Press F to continue.";
                    Vector2 textPosition = new Vector2(shoppingWindowLeftBoundary + 20,
                        shoppingWindowTopBoundary + 20
                    );
                    screen.DrawString(shoppingText, text, textPosition, Color.White);
                }
                if (pickedUpBossKey)
                {
                    String text = "You picked up a strange\nlooking key! But where does\nit go...?\n\n" +
                                  "Press F to continue.";
                    Vector2 textPosition = new Vector2(shoppingWindowLeftBoundary + 20,
                        shoppingWindowTopBoundary + 20
                    );
                    screen.DrawString(shoppingText, text, textPosition, Color.White);
                }
                if (pickedUpWhip)
                {
                    String text = "You picked up the whip!\nTo use it, simply hold down the\nleft mouse button.\n\n" +
                                  "Hock it back and flick it\nforward to successfully\nattack an enemy.\n\n" +
                                  "Press F to continue.";
                    Vector2 textPosition = new Vector2(shoppingWindowLeftBoundary + 20,
                        shoppingWindowTopBoundary + 20
                    );
                    screen.DrawString(shoppingText, text, textPosition, Color.White);
                }
                if (pickedUpMissile)
                {
                    String text = "You picked up a new spell!\n\nPrismatic Missile:\nHold Q and click the right \nmouse button\n\n" +
                                  "While it's flying, you can\nredirect it if you miss your\ntarget.\n\n" +
                                  "Press F to continue.";
                    Vector2 textPosition = new Vector2(shoppingWindowLeftBoundary + 20,
                        shoppingWindowTopBoundary + 20
                    );
                    screen.DrawString(shoppingText, text, textPosition, Color.White);
                }
                if (pickedUpLightning)
                {
                    String text = "You picked up a new spell!\n\nChain Lightning:\nHold E and hold the right\nmouse button" +
                                  " to begin casting.\nCast the spellin a zig-zag\nformation to accrue\nsuccessful chains.\n\n" +
                                  "Let go once you've built your\nchain to cast it.\n\n" +
                                  "Press F to continue";
                    Vector2 textPosition = new Vector2(shoppingWindowLeftBoundary + 20,
                        shoppingWindowTopBoundary + 20
                    );
                    screen.DrawString(shoppingText, text, textPosition, Color.White);
                }
                if (pickedUpTeleport)
                {
                    String text = "You picked up a new spell!\n\nTeleport:\nHold Left Alt and click the\nright mouse button" +
                                  " to\nteleport to where your\nmouse is.\n\nNote: You can't teleport\nthrough walls -- but you can\n" +
                                  "teleport through platforms.\nTry exploring more of the\ncastle now!\n\n" +
                                  "Press F to continue.";
                    Vector2 textPosition = new Vector2(shoppingWindowLeftBoundary + 20,
                        shoppingWindowTopBoundary + 20
                    );
                    screen.DrawString(shoppingText, text, textPosition, Color.White);
                }
            }

            if (_wizard.dead)
            {
                String gameOverText = "Game Over!";
                Vector2 gameOverTextSize = bigText.MeasureString(gameOverText);
                Vector2 gameOverTextPosition = new Vector2(screenWidth / 2 - gameOverTextSize.X / 2,
                        screenHeight / 2 - gameOverTextSize.Y / 2
                    );
                _spriteBatch.DrawString(bigText, gameOverText, gameOverTextPosition, Color.White);
                String gameOverTextSub = "Press P to respawn, or ESC to exit.";
                Vector2 gameOverTextSubSize = smallText.MeasureString(gameOverTextSub);
                Vector2 gameOverTextSubPosition = new Vector2(screenWidth / 2 - gameOverTextSubSize.X / 2,
                        screenHeight / 2 - gameOverTextSubSize.Y / 2 + 75
                    );
                _spriteBatch.DrawString(smallText, gameOverTextSub, gameOverTextSubPosition, Color.White);
            }
            if (bossDefeated && !paused)
            {
                String gameOverText = "You Win!";
                Vector2 gameOverTextSize = bigText.MeasureString(gameOverText);
                Vector2 gameOverTextPosition = new Vector2(screenWidth / 2 - gameOverTextSize.X / 2,
                        screenHeight / 2 - gameOverTextSize.Y / 2
                    );
                String gameOverTextSub = "Press Escape to exit the game.";
                Vector2 gameOverTextSubSize = smallText.MeasureString(gameOverTextSub);
                Vector2 gameOverTextSubPosition = new Vector2(screenWidth / 2 - gameOverTextSubSize.X / 2,
                        screenHeight / 2 - gameOverTextSubSize.Y / 2 + 75
                    );
                _spriteBatch.DrawString(smallText, gameOverTextSub, gameOverTextSubPosition, Color.White);
                _spriteBatch.DrawString(bigText, gameOverText, gameOverTextPosition, Color.White);
            }
            if (paused)
            {
                MouseState mouseCursor = currentMouseState;
                Rectangle cursorRect = new Rectangle(mouseCursor.X - 5, mouseCursor.Y - 5, 10, 10);

                // Shared data
                string[] controlActions = new string[]
                {
                    "Move", "Potion", "Whip", "Prismatic Missile", "Lightning", "Teleport",
                };
                string[] controlInputs = new string[]
                {
                    "A / D / Space",
                    "R",
                    "Left Click + Mouse Motion",
                    "Q + Right Click",
                    "E + Right Click and Drag",
                    "Left Alt + Right Click",
                };

                float maxActionWidth = 0f;
                foreach (string action in controlActions)
                    maxActionWidth = Math.Max(maxActionWidth, smallText.MeasureString(action).X);
                float maxInputWidth = 0f;
                foreach (string input in controlInputs)
                    maxInputWidth = Math.Max(maxInputWidth, smallText.MeasureString(input).X);
                float columnGap = 100f;
                float totalWidth = maxActionWidth + columnGap + maxInputWidth;
                float blockX = screenWidth / 2 - totalWidth / 2;
                Vector2 smallLineSize = smallText.MeasureString("A");
                float lineSpacing = smallLineSize.Y + 2;

                if (showingControls)
                {
                    // --- Controls screen ---
                    String controlsHeaderText = "Controls:";
                    Vector2 controlsHeaderSize = smallText.MeasureString(controlsHeaderText);
                    Vector2 controlsHeaderPosition = new Vector2(
                        screenWidth / 2 - controlsHeaderSize.X / 2,
                        screenHeight / 3
                    );
                    screen.DrawString(smallText, controlsHeaderText, controlsHeaderPosition, Color.White);

                    for (int i = 0; i < controlActions.Length; i++)
                    {
                        float lineY = controlsHeaderPosition.Y + controlsHeaderSize.Y + 4 + (i * lineSpacing);
                        screen.DrawString(smallText, controlActions[i], new Vector2(blockX, lineY), Color.LightGray);
                        screen.DrawString(smallText, controlInputs[i], new Vector2(blockX + maxActionWidth + columnGap, lineY), Color.LightGray);
                    }

                    float backTextScale;
                    String backText = "Back";
                    Vector2 backTextSize = smallText.MeasureString(backText);
                    Vector2 backTextPosition = new Vector2(
                        screenWidth / 2 - backTextSize.X / 2,
                        controlsHeaderPosition.Y + controlsHeaderSize.Y + 4 + (controlActions.Length * lineSpacing) + 20
                    );
                    Rectangle backButton = new Rectangle((int)backTextPosition.X, (int)backTextPosition.Y,
                                                         (int)backTextSize.X, (int)backTextSize.Y);

                    if (cursorRect.Intersects(backButton))
                    {
                        backTextScale = 1.2f;
                        backTextSize = smallText.MeasureString(backText) * backTextScale;
                        backTextPosition = new Vector2(screenWidth / 2 - backTextSize.X / 2, backTextPosition.Y);
                        if (mouseCursor.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                            showingControls = false;
                    }
                    else
                    {
                        backTextScale = 1.0f;
                    }
                    screen.DrawString(smallText, backText, backTextPosition, Color.White, 0f, Vector2.Zero, backTextScale, SpriteEffects.None, 0f);
                }
                else
                {
                    // --- Main pause menu ---
                    String pauseText = "Pause";
                    Vector2 pauseTextSize = bigText.MeasureString(pauseText);
                    Vector2 pauseTextPosition = new Vector2(screenWidth / 2 - pauseTextSize.X / 2,
                            screenHeight / 3 - pauseTextSize.Y / 2);

                    float settingsTextScale;
                    String settingsText = "Resolution (" + (_graphics.IsFullScreen ? "Full Screen):" : "Windowed):");
                    Vector2 settingsTextSize = smallText.MeasureString(settingsText);
                    Vector2 settingsTextPosition = new Vector2(screenWidth / 2 - settingsTextSize.X / 2,
                            screenHeight / 3 - settingsTextSize.Y / 2 + pauseTextSize.Y / 2 + 8);
                    Rectangle settingsButton = new Rectangle((int)settingsTextPosition.X, (int)settingsTextPosition.Y,
                                                             (int)settingsTextSize.X, (int)settingsTextSize.Y);

                    String resolutionText = screenWidth.ToString() + " x " + screenHeight.ToString();
                    Vector2 resolutionTextSize = smallText.MeasureString(resolutionText);
                    Vector2 resolutionTextPosition = new Vector2(screenWidth / 2 - resolutionTextSize.X / 2,
                            screenHeight / 3 + resolutionTextSize.Y / 6 + pauseTextSize.Y / 2 + settingsTextSize.Y / 2 + 8);

                    // Controls button
                    float controlsTextScale;
                    String controlsButtonText = "Controls";
                    Vector2 controlsButtonSize = smallText.MeasureString(controlsButtonText);
                    Vector2 controlsButtonPosition = new Vector2(screenWidth / 2 - controlsButtonSize.X / 2,
                            resolutionTextPosition.Y + 50);
                    Rectangle controlsButton = new Rectangle((int)controlsButtonPosition.X, (int)controlsButtonPosition.Y,
                                                             (int)controlsButtonSize.X, (int)controlsButtonSize.Y);

                    // Save & Exit button
                    float exitTextScale;
                    String exitText = "Save & Exit";
                    Vector2 exitTextSize = smallText.MeasureString(exitText);
                    Vector2 extiTextPosition = new Vector2(screenWidth / 2 - exitTextSize.X / 2,
                            controlsButtonPosition.Y + controlsButtonSize.Y + 16);
                    Rectangle exitButton = new Rectangle((int)extiTextPosition.X, (int)extiTextPosition.Y,
                                                         (int)exitTextSize.X, (int)exitTextSize.Y);

                    screen.DrawString(bigText, pauseText, pauseTextPosition, Color.White);

                    // Resolution button
                    if (cursorRect.Intersects(settingsButton))
                    {
                        settingsTextScale = 1.2f;
                        settingsTextSize = smallText.MeasureString(settingsText) * settingsTextScale;
                        settingsTextPosition = new Vector2(screenWidth / 2 - settingsTextSize.X / 2,
                                screenHeight / 3 - settingsTextSize.Y / 2 + pauseTextSize.Y / 2 + 8);
                        if (mouseCursor.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                        {
                            if (screenWidth == 1600 && screenHeight == 900)
                            {
                                screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                                screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                                _graphics.PreferredBackBufferWidth = screenWidth;
                                _graphics.PreferredBackBufferHeight = screenHeight;
                                _graphics.IsFullScreen = true;
                                _graphics.ApplyChanges();
                                _camera.Viewport = GraphicsDevice.Viewport;
                                _camera.UpdateZoomForResolution();
                            }
                            else if (screenWidth == GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width &&
                                     screenHeight == GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
                            {
                                if (1120 > GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width &&
                                    630 > GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
                                {
                                    screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                                    screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                                    _graphics.PreferredBackBufferWidth = screenWidth;
                                    _graphics.PreferredBackBufferHeight = screenHeight;
                                    _graphics.IsFullScreen = true;
                                    _graphics.ApplyChanges();
                                    _camera.Viewport = GraphicsDevice.Viewport;
                                    _camera.UpdateZoomForResolution();
                                }
                                else
                                {
                                    screenWidth = 1120;
                                    screenHeight = 630;
                                    _graphics.PreferredBackBufferWidth = screenWidth;
                                    _graphics.PreferredBackBufferHeight = screenHeight;
                                    _graphics.IsFullScreen = false;
                                    _graphics.ApplyChanges();
                                    _camera.Viewport = GraphicsDevice.Viewport;
                                    _camera.UpdateZoomForResolution();
                                }
                            }
                            else if (screenWidth == 1120 && screenHeight == 630)
                            {
                                if (1280 > GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width &&
                                    720 > GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
                                {
                                    screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                                    screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                                    _graphics.PreferredBackBufferWidth = screenWidth;
                                    _graphics.PreferredBackBufferHeight = screenHeight;
                                    _graphics.IsFullScreen = true;
                                    _graphics.ApplyChanges();
                                    _camera.Viewport = GraphicsDevice.Viewport;
                                    _camera.UpdateZoomForResolution();
                                }
                                else
                                {
                                    screenWidth = 1280;
                                    screenHeight = 720;
                                    _graphics.PreferredBackBufferWidth = screenWidth;
                                    _graphics.PreferredBackBufferHeight = screenHeight;
                                    _graphics.IsFullScreen = false;
                                    _graphics.ApplyChanges();
                                    _camera.Viewport = GraphicsDevice.Viewport;
                                    _camera.UpdateZoomForResolution();
                                }
                            }
                            else if (screenWidth == 1280 && screenHeight == 720)
                            {
                                if (1600 > GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width &&
                                    900 > GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
                                {
                                    screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                                    screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                                    _graphics.PreferredBackBufferWidth = screenWidth;
                                    _graphics.PreferredBackBufferHeight = screenHeight;
                                    _graphics.IsFullScreen = true;
                                    _graphics.ApplyChanges();
                                    _camera.Viewport = GraphicsDevice.Viewport;
                                    _camera.UpdateZoomForResolution();
                                }
                                else
                                {
                                    screenWidth = 1600;
                                    screenHeight = 900;
                                    _graphics.PreferredBackBufferWidth = screenWidth;
                                    _graphics.PreferredBackBufferHeight = screenHeight;
                                    _graphics.IsFullScreen = false;
                                    _graphics.ApplyChanges();
                                    _camera.Viewport = GraphicsDevice.Viewport;
                                    _camera.UpdateZoomForResolution();
                                }
                            }
                        }
                    }
                    else
                    {
                        settingsTextScale = 1.0f;
                        settingsTextSize = smallText.MeasureString(settingsText) * settingsTextScale;
                        settingsTextPosition = new Vector2(screenWidth / 2 - settingsTextSize.X / 2,
                                screenHeight / 3 - settingsTextSize.Y / 2 + pauseTextSize.Y / 2 + 8);
                    }
                    screen.DrawString(smallText, settingsText, settingsTextPosition, Color.White, 0f, Vector2.Zero, settingsTextScale, SpriteEffects.None, 0f);
                    screen.DrawString(smallText, resolutionText, resolutionTextPosition, Color.White);

                    // Controls button hover + click
                    if (cursorRect.Intersects(controlsButton))
                    {
                        controlsTextScale = 1.2f;
                        controlsButtonSize = smallText.MeasureString(controlsButtonText) * controlsTextScale;
                        controlsButtonPosition = new Vector2(screenWidth / 2 - controlsButtonSize.X / 2, controlsButtonPosition.Y);
                        if (mouseCursor.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                            showingControls = true;
                    }
                    else
                    {
                        controlsTextScale = 1.0f;
                    }
                    screen.DrawString(smallText, controlsButtonText, controlsButtonPosition, Color.White, 0f, Vector2.Zero, controlsTextScale, SpriteEffects.None, 0f);

                    // Save & Exit button hover + click
                    if (cursorRect.Intersects(exitButton))
                    {
                        exitTextScale = 1.2f;
                        exitTextSize = smallText.MeasureString(exitText) * exitTextScale;
                        extiTextPosition = new Vector2(screenWidth / 2 - exitTextSize.X / 2, extiTextPosition.Y);
                        if (mouseCursor.LeftButton == ButtonState.Pressed)
                        {
                            if (_boss.Count > 0) Exit();
                            else { SaveGame(); Exit(); }
                        }
                    }
                    else
                    {
                        exitTextScale = 1.0f;
                        exitTextSize = smallText.MeasureString(exitText) * exitTextScale;
                        extiTextPosition = new Vector2(screenWidth / 2 - exitTextSize.X / 2, extiTextPosition.Y);
                    }
                    screen.DrawString(smallText, exitText, extiTextPosition, Color.White, 0f, Vector2.Zero, exitTextScale, SpriteEffects.None, 0f);
                }
            }
            if (introPrompt)
            {
                screen.Draw(_pixelTexture, shoppingWindowBackground, Color.Black * 0.5f);
                screen.Draw(_pixelTexture, topWindowBar, Color.White);
                screen.Draw(_pixelTexture, leftWindowBar, Color.White);
                screen.Draw(_pixelTexture, bottomWindowBar, Color.White);
                screen.Draw(_pixelTexture, rightWindowBar, Color.White);

                String text;
                if (introPromptTimer >= 5f)
                {
                    text = "Rumors in local towns\nforetold great treasures" +
                              " in\nan old castle, haunted by\nan undead curse, " +
                              " and that\nthere was surely\ntreasure and spells " +
                              "to\nplunder.\n\nLooks like the rumors were\ntrue..." +
                              "\n\n (Press F to Continue)";
                }
                else
                {
                    text = "Rumors in local towns\nforetold great treasures" +
                              " in\nan old castle, haunted by\nan undead curse, " +
                              " and that\nthere was surely\ntreasure and spells " +
                              "to\nplunder.\n\nLooks like the rumors were\ntrue...";
                }
                Vector2 textPosition = new Vector2(shoppingWindowLeftBoundary + 20,
                    shoppingWindowTopBoundary + 20
                );
                screen.DrawString(shoppingText, text, textPosition, Color.White);
            }
        }
        void ResetGameStateUponPlayerDeath()
        {
            _wizard.playerHP = 20;
            _wizard.dead = false;
            _wizard.Position = spawnLocation;
            _wizard.potions = _wizard.potionMax;
            foreach (Skeleton skeleton in _skeletons)
            {
                skeleton.brokenBones = false;
                skeleton.skeletonPosition = skeleton.skeletonSpawnPosition;
                skeleton.skeletonWidth = 32;
                skeleton.skeletonHeight = 64;
            }
            for (int i = 0; i < _zombies.Count; i++)
            {
                if (_zombies[i].wasSummoned)
                {
                    _zombies.RemoveAt(i);
                    i--;
                    continue;
                }
                else
                {
                    _zombies[i].zombieDead = true;
                    _zombies[i].canRespawn = true;
                }
            }
            foreach (Ghost ghost in _ghosts)
            {
                ghost.ghostHP = 2;
            }
            _boss.Clear();
        }
        void UseCampfire()
        {
            _wizard.playerHP = 20;
            _wizard.potions = _wizard.potionMax;
            foreach (Skeleton skeleton in _skeletons)
            {
                skeleton.brokenBones = false;
                skeleton.skeletonPosition = skeleton.skeletonSpawnPosition;
                skeleton.skeletonWidth = 32;
                skeleton.skeletonHeight = 64;
            }
            for (int i = 0; i < _zombies.Count; i++)
            {
                if (_zombies[i].zombiePosition.X >= 1104 && _zombies[i].zombiePosition.X <= 1550 &&
                    _zombies[i].zombiePosition.Y <= 275)
                {
                    _zombies.RemoveAt(i);
                    i--;
                    continue;
                }
                _zombies[i].zombiePosition = _zombies[i].zombieSpawn;
                if (_zombies[i].zombieDead)
                {
                    _zombies[i].canRespawn = true;
                }
            }
            foreach (Ghost ghost in _ghosts)
            {
                ghost.ghostHP = 2;
                ghost.ghostPosition = ghost.ghostSpawn;
            }
        }
        void EnterBossRoom()
        {
            _wizard.Position = new Vector2(1311, 212);
            KingBoss boss = new KingBoss(new Vector2(1304, 36), _collisionRects, _platformRects, 1104, 1550, 275);
            boss.orbTexture = Content.Load<Texture2D>(@"Textures/Boss/missile");
            boss.idleFloat = Content.Load<Texture2D>(@"Textures/Boss/idle");
            boss.summoningTexture = Content.Load<Texture2D>(@"Textures/Boss/summoning");
            boss.slamTexture = Content.Load<Texture2D>(@"Textures/Boss/slamdownpose2");
            boss.slamWeapon = Content.Load<Texture2D>(@"Textures/Boss/slamdownweapon");
            boss.castingOrb = Content.Load<Texture2D>(@"Textures/Boss/castorb");
            boss.chargeTexture = Content.Load<Texture2D>(@"Textures/Boss/charge");
            boss.shockwaveTexture = Content.Load<Texture2D>(@"Textures/Boss/shockwave");
            _boss.Add(boss);
        }
        void UpdateInteractableObjects()
        {
            // Using the campfire is like resting at a bonfire in Dark Souls
            // Enemies respawn in their original positions, player health and potions get refilled
            if (_wizard.Rect.Intersects(spawn.Rect) && Keyboard.GetState().IsKeyDown(Keys.F) &&
                previousKeyboardState.IsKeyUp(Keys.F))
                UseCampfire();

            // This handles interactions with the shopkeep
            // Gold gets spent, shopkeeps inventory gets updated
            if (_wizard.Rect.Intersects(shopkeep.Rect) && Keyboard.GetState().IsKeyDown(Keys.F) && previousKeyboardState.IsKeyUp(Keys.F))
            {
                shopping = !shopping;
                if (!shopping)
                {
                    boughtSomething = false;
                }
            }
            if (shopping)
            {
                if (_wizard.gold >= 300 && shopkeep.SpellInventory > 0
                    && Keyboard.GetState().IsKeyDown(Keys.D1) && previousKeyboardState.IsKeyUp(Keys.D1))
                {
                    boughtSomething = true;
                    Scroll lightningScroll = new Scroll();
                    lightningScroll.Position = new Vector2(263, 1052);
                    lightningScroll.Rect = new Rectangle(263, 1052, 17, 8);
                    lightningScroll.Spell = "Lightning";
                    lightningScroll.Color = Color.Yellow;
                    lightningScroll.Texture = Content.Load<Texture2D>(@"Textures/lightningscrollsprite");

                    _scrolls.Add(lightningScroll);
                    shopkeep.SpellInventory = 0;
                    _wizard.gold -= 300;
                }
                else if (_wizard.gold >= 100 && !_wizard.hasTorch && shopkeep.TorchInventory > 0
                    && Keyboard.GetState().IsKeyDown(Keys.D2) && previousKeyboardState.IsKeyUp(Keys.D2))
                {
                    boughtSomething = true;
                    _wizard.gold -= 100;
                    shopkeep.TorchInventory = 0;
                    _wizard.hasTorch = true;

                }
                else if (_wizard.gold >= 310 && shopkeep.PotionUpgradeInventory > 0
                    && Keyboard.GetState().IsKeyDown(Keys.D3) && previousKeyboardState.IsKeyUp(Keys.D3))
                {
                    boughtSomething = true;
                    _wizard.gold -= 310;
                    shopkeep.PotionUpgradeInventory -= 1;
                    _wizard.potionMax += 1;
                    _wizard.potions += 1;
                }
            }

            // This handles whether or not the player picked up a key
            for (int i = 0; i < _keys.Count; i++)
            {
                if (_keys[i].Rect.Intersects(_wizard.Rect))
                {
                    if (_keys[i].Type == "Regular")
                    {
                        pickedUpRegularKey = true;
                    }
                    else
                    {
                        pickedUpBossKey = true;
                    }
                    _wizard.AddKey(_keys[i]);
                    _keys.RemoveAt(i);
                }
            }

            // This handles whether or not the player picked up a scroll. The players spell inventory will update when they do
            for (int i = 0; i < _scrolls.Count; i++)
            {
                if (_scrolls[i].Rect.Intersects(_wizard.Rect))
                {
                    if (_scrolls[i].Spell == "Teleport")
                    {
                        _wizard.hasTeleport = true;
                        pickedUpTeleport = true;
                    }
                    if (_scrolls[i].Spell == "Whip")
                    {
                        _wizard.hasWhip = true;
                        pickedUpWhip = true;
                    }
                    if (_scrolls[i].Spell == "Missile")
                    {
                        _wizard.hasMissile = true;
                        pickedUpMissile = true;
                    }
                    if (_scrolls[i].Spell == "Lightning")
                    {
                        _wizard.hasLightning = true;
                        pickedUpLightning = true;
                    }
                    _scrolls.RemoveAt(i);
                }
            }

            // This handles interactions with doors
            // One door is a simple boundary. Unlocking it removes it
            // The other door leads to the boss room, which moves the player to the boss room and instantiates the boss
            for (int i = 0; i < _doors.Count; i++)
            {
                foreach (Key key in _wizard.playerKeyInventory)
                {
                    if (key.Type == _doors[i].Type && key.Type == "Regular" && Keyboard.GetState().IsKeyDown(Keys.F) &&
                        _wizard.Rect.Intersects(_doors[i].Rect))
                    {
                        for (int j = 0; j < _interactableRects.Count; j++)
                        {
                            if (_interactableRects[j] == _doors[i].Rect)
                            {
                                _interactableRects.RemoveAt(j);
                            }
                        }
                        _doors.Remove(_doors[i]);
                        _collisionRects.Remove(_collisionRects[_collisionRects.Count - 1]);
                    }
                    if (key.Type == _doors[i].Type && key.Type == "Boss" && Keyboard.GetState().IsKeyDown(Keys.F) &&
                        _wizard.Rect.Intersects(_doors[i].Rect))
                        EnterBossRoom();
                }
            }

            // This handles chest interactions
            for (int i = 0; i < _chests.Count; i++)
            {
                int gold;
                if (_chests[i].Rect.Intersects(_wizard.Rect) && Keyboard.GetState().IsKeyDown(Keys.F))
                {
                    gold = _chests[i].Gold;
                    for (int j = 0; j < _interactableRects.Count; j++)
                    {
                        if (_chests[i].Rect == _interactableRects[j])
                        {
                            _interactableRects.RemoveAt(j);
                        }
                    }
                    _chests.RemoveAt(i);
                }
                else
                {
                    gold = 0;
                }
                _wizard.gold += gold;
            }
        }
    }
}