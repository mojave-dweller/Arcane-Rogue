using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        public bool paused = false;
        public bool introPrompt = true;
        public float introPromptTimer = 0f;
        public float potionNotificationTimer = 0f;

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
            // TODO: Add your initialization logic here
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
                                         data.HasWhip, data.HasMissile, data.HasTeleport, data.HasLightning);
                    _wizard.gold = data.Gold;
                    _wizard.potions = data.Potions;
                    _wizard.playerHP = data.PlayerHP;
                    _wizard.hasTorch = data.HasTorch;
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

                    PlayerKeysList = _wizard.playerKeyInventory
                        .Select(k => k.Type)
                        .ToList(),

                    SpawnLocation = new Vector2Data
                    {
                        X = _wizard.playerPosition.X,
                        Y = _wizard.playerPosition.Y
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
                        Y = g.ghostPosition.Y,
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
            CreateKeys();
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
        }

        protected override void Update(GameTime gameTime)
        {
            previousKeyboardState = currentKeyboardState; // save last frame
            currentKeyboardState = Keyboard.GetState();
            previousMouseState = currentMouseState;       // save last frame first
            currentMouseState = Mouse.GetState();

            if (!introPrompt)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape))
                    paused = !paused;
                if (!bossDefeated)
                {
                    if (!paused)
                    {
                        int changeGold = 0;
                        int changePotions = 0;

                        if (_wizard.dead && Keyboard.GetState().IsKeyDown(Keys.P))
                        {
                            _wizard.playerHP = 20;
                            _wizard.dead = false;
                            _wizard.playerPosition = spawnLocation;
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

                        if (!pickedUpBossKey && !pickedUpRegularKey && !pickedUpWhip && !pickedUpMissile && !pickedUpLightning && !pickedUpTeleport)
                        {
                            _wizard.Update(gameTime, Keyboard.GetState(), Mouse.GetState(), _camera, _skeletons, _zombies, _ghosts, _boss, shopping);
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
                        if (_wizard.playerRect.Intersects(spawn.Rect) && Keyboard.GetState().IsKeyDown(Keys.F) &&
                            previousKeyboardState.IsKeyUp(Keys.F))
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
                                _zombies[i].canRespawn = true;
                            }
                            foreach (Ghost ghost in _ghosts)
                            {
                                ghost.ghostHP = 2;
                                ghost.ghostPosition = ghost.ghostSpawn;
                            }
                        }
                        if (_wizard.playerRect.Intersects(shopkeep.Rect) && Keyboard.GetState().IsKeyDown(Keys.F) && previousKeyboardState.IsKeyUp(Keys.F))
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
                                changeGold = -300;
                            }
                            else if (_wizard.gold >= 100 && !_wizard.hasTorch && shopkeep.TorchInventory > 0
                                && Keyboard.GetState().IsKeyDown(Keys.D2) && previousKeyboardState.IsKeyUp(Keys.D2))
                            {
                                boughtSomething = true;
                                changeGold = -100;
                                _wizard.hasTorch = true;

                            }
                            else if (_wizard.gold >= 430 && shopkeep.PotionUpgradeInventory > 0
                                && Keyboard.GetState().IsKeyDown(Keys.D3) && previousKeyboardState.IsKeyUp(Keys.D3))
                            {
                                boughtSomething = true;
                                changeGold = -430;
                                _wizard.potionMax += 1;
                                _wizard.potions += 1;
                            }
                        }
                        for (int i = 0; i < _keys.Count; i++)
                        {
                            if (_keys[i].Rect.Intersects(_wizard.playerRect))
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
                        for (int i = 0; i < _scrolls.Count; i++)
                        {
                            if (_scrolls[i].Rect.Intersects(_wizard.playerRect))
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
                        for (int i = 0; i < _doors.Count; i++)
                        {
                            foreach (Key key in _wizard.playerKeyInventory)
                            {
                                if (key.Type == _doors[i].Type && key.Type == "Regular" && Keyboard.GetState().IsKeyDown(Keys.F) &&
                                    _wizard.playerRect.Intersects(_doors[i].Rect))
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
                                    _wizard.playerRect.Intersects(_doors[i].Rect))
                                {
                                    _wizard.playerPosition = new Vector2(1311, 212);
                                    KingBoss boss = new KingBoss(new Vector2(1304, 36), _collisionRects, _platformRects, 1104, 1550, 275);
                                    _boss.Add(boss);
                                }
                            }
                        }
                        for (int i = 0; i < _chests.Count; i++)
                        {
                            int gold;
                            if (_chests[i].Rect.Intersects(_wizard.playerRect) && Keyboard.GetState().IsKeyDown(Keys.F))
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
                        if (!pickedUpBossKey && !pickedUpRegularKey && !pickedUpWhip && !pickedUpMissile && !pickedUpLightning && !pickedUpTeleport)
                        {
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
                        }
                        for (int i = 0; i < _boss.Count; i++)
                        {
                            _boss[i].Update(gameTime, _wizard, _zombies);
                            if (_boss[i].bossHP <= 0)
                            {
                                bossDefeated = true;
                                _boss.RemoveAt(i);
                            }
                        }

                        _camera.Follow(_wizard.playerPosition);

                        _wizard.gold += changeGold;
                        _wizard.potions += changePotions;
                        if (_wizard.potions > 0)
                        {
                            pickedUpPotion = true;
                        }
                        if (pickedUpPotion)
                        {
                            potionNotificationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                            if (potionNotificationTimer > 5f)
                            {
                                potionNotificationTimer = 5.01f;
                            }
                        }
                    }
                }
                else
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape))
                    {
                        SaveGame();
                        Exit();
                    }
                }
            }
            else
            {
                introPromptTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                _camera.Follow(_wizard.playerPosition);
                if (Keyboard.GetState().IsKeyDown(Keys.F) && previousKeyboardState.IsKeyUp(Keys.F))
                {
                    introPrompt = !introPrompt;
                }
            }
            
            
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0.02f, 0.04f, 0.1f));

            // Update light position to follow player center each frame
            // Build the full light list: world lights + player light
            var playerCenter = _wizard.playerAnchorPoint;

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

            _spriteBatch.Draw(map, new Vector2(0, 0), Color.White);
            foreach (Door door in _doors)
                _spriteBatch.Draw(_pixelTexture, door.Rect, door.Color);
            foreach (Scroll scroll in _scrolls)
                _spriteBatch.Draw(scroll.Texture, scroll.Rect, Color.White);
            foreach (Chest chest in _chests)
                _spriteBatch.Draw(chest.Texture, chest.Rect, Color.White);

            _spriteBatch.Draw(_pixelTexture, spawn.Rect, Color.Orange * 0.5f);
            _spriteBatch.Draw(_pixelTexture, shopkeep.Rect, Color.Gold * 0.5f);
            _wizard.Draw(_spriteBatch, _pixelTexture);
            foreach (Key key in _keys)
            {
                if (key.Texture == null)
                    _spriteBatch.Draw(_pixelTexture, key.Rect, key.Color);
                else
                    _spriteBatch.Draw(key.Texture, key.Rect, Color.White);
            }
            foreach (Skeleton enemy in _skeletons)
                enemy.Draw(_spriteBatch, _pixelTexture);
            foreach (Zombie enemy in _zombies)
                if (!enemy.zombieDead) enemy.Draw(_spriteBatch, _pixelTexture, gameTime);
            foreach (Ghost ghost in _ghosts)
                if (!(ghost.ghostHP <= 0)) ghost.Draw(_spriteBatch, _pixelTexture, gameTime);
            foreach (KingBoss boss in _boss)
                boss.Draw(_spriteBatch, _pixelTexture, gameTime);

            foreach (Rectangle rect in _interactableRects)
            {
                if (_wizard.playerRect.Intersects(rect))
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

            if (pickedUpPotion && potionNotificationTimer <= 5f)
            {

                String text = "Press R to use a Potion";
                float scale = 0.6f;
                Vector2 textSize = shoppingText.MeasureString(text) * scale;
                Vector2 textPosition = new Vector2(_wizard.playerAnchorPoint.X - textSize.X/2,
                    _wizard.playerAnchorPoint.Y - 42
                );
                _spriteBatch.DrawString(shoppingText, text, textPosition, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }

            _spriteBatch.End();

            _spriteBatch.Begin();
            DisplayUI(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
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
            bossDoor.Rect = new Rectangle(1303, 396, 49, 56);
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
                        vendorGreeting = "Welcome!\nI have plenty of potions\nfor sale!\n" +
                                         "(Press F to Continue)";
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
                    potionPrice = "3. Maximize Potions = 430 G";
                }
                else
                {
                    potionPrice = "3. Maximize Potions = SOLD OUT";
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

                String pauseText = "Pause";
                Vector2 pauseTextSize = bigText.MeasureString(pauseText);
                Vector2 pauseTextPosition = new Vector2(screenWidth / 2 - pauseTextSize.X / 2,
                        screenHeight / 3 - pauseTextSize.Y / 2
                    );

                float settingsTextScale;
                String settingsText = "Resolution (" + (_graphics.IsFullScreen ? "Full Screen):" : "Windowed):");
                Vector2 settingsTextSize = smallText.MeasureString(settingsText);
                Vector2 settingsTextPosition = new Vector2(screenWidth / 2 - settingsTextSize.X / 2,
                        screenHeight / 3 - settingsTextSize.Y / 2 + pauseTextSize.Y / 2 + 8
                    );
                Rectangle settingsButton = new Rectangle((int)settingsTextPosition.X, (int)settingsTextPosition.Y, 
                                                         (int)settingsTextSize.X, (int)settingsTextSize.Y);

                String resolutionText = screenWidth.ToString() + " x " + screenHeight.ToString();
                Vector2 resolutionTextSize = smallText.MeasureString(resolutionText);
                Vector2 resolutionTextPosition = new Vector2(screenWidth / 2 - resolutionTextSize.X / 2,
                        screenHeight / 3 + resolutionTextSize.Y / 6 + pauseTextSize.Y / 2 + settingsTextSize.Y / 2 + 8
                    );

                float exitTextScale;
                String exitText = "Save & Exit";
                Vector2 exitTextSize = smallText.MeasureString(exitText);
                Vector2 extiTextPosition = new Vector2(screenWidth / 2 - exitTextSize.X / 2,
                        resolutionTextPosition.Y + 50
                    );
                Rectangle exitButton = new Rectangle((int)extiTextPosition.X, (int)extiTextPosition.Y,
                                                         (int)exitTextSize.X, (int)exitTextSize.Y);

                screen.DrawString(bigText, pauseText, pauseTextPosition, Color.White);
                if (cursorRect.Intersects(settingsButton))
                {
                    settingsTextScale = 1.2f;
                    settingsTextSize = smallText.MeasureString(settingsText) * settingsTextScale;
                    settingsTextPosition = new Vector2(screenWidth / 2 - settingsTextSize.X / 2,
                            screenHeight / 3 - settingsTextSize.Y / 2 + pauseTextSize.Y / 2 + 8
                        );
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
                            screenHeight / 3 - settingsTextSize.Y / 2 + pauseTextSize.Y / 2 + 8
                        );
                }
                screen.DrawString(smallText, settingsText, settingsTextPosition, Color.White, 0f, Vector2.Zero, settingsTextScale, SpriteEffects.None, 0f);
                screen.DrawString(smallText, resolutionText, resolutionTextPosition, Color.White);
                if (cursorRect.Intersects(exitButton))
                {
                    exitTextScale = 1.2f;
                    exitTextSize = smallText.MeasureString(exitText) * exitTextScale;
                    extiTextPosition = new Vector2(screenWidth / 2 - exitTextSize.X / 2,
                        resolutionTextPosition.Y + 50
                    );
                    if (mouseCursor.LeftButton == ButtonState.Pressed)
                    {
                        SaveGame();
                        Exit();
                    }
                }
                else
                {
                    exitTextScale = 1.0f;
                    exitTextSize = smallText.MeasureString(exitText) * exitTextScale;
                    extiTextPosition = new Vector2(screenWidth / 2 - exitTextSize.X / 2,
                            resolutionTextPosition.Y + 50
                        );
                }
                screen.DrawString(smallText, exitText, extiTextPosition, Color.White, 0f, Vector2.Zero, exitTextScale, SpriteEffects.None, 0f);
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
    }

    public class Player
    {
        private readonly int playerWidth = 28;
        private readonly int playerHeight = 56;
        private const int playerSpeed = 3;
        private float playerGravity = 0.5f;
        private const float playerJumpForce = -11f;
        private float playerVelocityY = 0f;
        private bool isGrounded = false;
        private Vector2 cursorLocation;
        private Vector2 spawnPosition;
        public Rectangle playerRect;
        public float playerHP = 20;
        public int gold = 0;
        public int potionMax = 3;
        public int potions = 3;
        private KeyboardState currentKeyboardState, previousKeyboardState;


        public Vector2 playerAnchorPoint;
        private readonly int playerAnchorRadius = 32;

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

        public Vector2 playerPosition;
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
            playerPosition = startPosition;
            spawnPosition = startPosition;
            playerRect = new Rectangle((int)playerPosition.X,
                                               (int)playerPosition.Y,
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
            cursorLocation = new Vector2(0,0);
            playerAnchorPoint = new Vector2(playerPosition.X + playerWidth / 3, playerPosition.Y + playerHeight / 3);
            hasWhip = whip;
            hasMissile = missile;
            hasLightning = lightning;
            hasTeleport = teleport;

        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, MouseState mouseState, 
                           Camera camera, List<Skeleton> skeletons, List<Zombie> zombies, List<Ghost> ghosts, List<KingBoss> boss, bool shopping)
        {
            // Anchor Point is in the center of the player Rect
            playerAnchorPoint = new Vector2(playerPosition.X + playerWidth / 2, playerPosition.Y + playerHeight / 2);
            previousKeyboardState = currentKeyboardState; // save last frame
            currentKeyboardState = Keyboard.GetState();

            MouseState prevMouse = previousMouse;
            previousMouse = mouseState;

            if (playerHP <= 0f)
            {
                dead = true;
            }

            if (!dead)
            {
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
                        UpdateLightning(gameTime, keyboardState, prevMouse, mouseState, camera, skeletons, zombies, boss);
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
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture)
        {
            if (!dead)
            {
                bool visible = !iFrames || ((int)(iFramesTimer / 0.1f) % 2 == 0);

                if (visible)
                    spriteBatch.Draw(pixelTexture, playerRect, Color.Red);

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

            if (playerPosition.Y >= 2341 || playerPosition.Y <= 0 || playerPosition.X >= 2208 || playerPosition.X <= 0)
            {
                playerPosition = spawnPosition;
            }

            // --- playerGravity ---
            if (!isGrounded)
                playerVelocityY += playerGravity;


            // --- Knockback with collision ---
            if (knockbackVelocity != Vector2.Zero)
            {
                // Step X
                playerPosition.X += knockbackVelocity.X;
                Rectangle testRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, playerWidth, playerHeight);
                foreach (Rectangle rect in _collisionRects)
                {
                    if (testRect.Intersects(rect))
                    {
                        // Push back out and kill horizontal knockback
                        if (knockbackVelocity.X > 0)
                            playerPosition.X = rect.Left - playerWidth;
                        else
                            playerPosition.X = rect.Right;
                        knockbackVelocity.X = 0;
                        break;
                    }
                }

                // Step Y
                playerPosition.Y += knockbackVelocity.Y;
                testRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, playerWidth, playerHeight);
                foreach (Rectangle rect in _collisionRects)
                {
                    if (testRect.Intersects(rect))
                    {
                        if (knockbackVelocity.Y > 0)
                            playerPosition.Y = rect.Top - playerHeight;
                        else
                            playerPosition.Y = rect.Bottom;
                        knockbackVelocity.Y = 0;
                        break;
                    }
                }

                knockbackVelocity *= 0.8f;
                if (knockbackVelocity.LengthSquared() < 0.01f)
                    knockbackVelocity = Vector2.Zero;
            }

            // --- Horizontal move + collision ---
            if (keyboardState.IsKeyDown(Keys.A))
                playerPosition.X -= playerSpeed;
            if (keyboardState.IsKeyDown(Keys.D))
                playerPosition.X += playerSpeed;


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
                    Vector2 rayStart = playerPosition;
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
                    playerPosition = destination;
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
                Vector2 whipHandleLocation = playerAnchorPoint;
                // We invert the camera transformation matrix to get in world coordinates, not screen coordinates
                cursorLocation = Vector2.Transform(new Vector2(mouseState.X, mouseState.Y),
                                                           Matrix.Invert(camera.GetTransformationMatrix()));
                Vector2 whipDirection = cursorLocation - playerAnchorPoint;

                if (whipDirection != Vector2.Zero)
                {
                    // Normalize the whip direction to be a set distance away from the anchor point
                    whipDirection = Vector2.Normalize(whipDirection);
                    whipHandleLocation = playerAnchorPoint + whipDirection * playerAnchorRadius;
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

                Vector2 whipSegmentLocation = playerAnchorPoint;
                cursorLocation = Vector2.Transform(new Vector2(mouseState.X, mouseState.Y),
                                                           Matrix.Invert(camera.GetTransformationMatrix()));
                Vector2 prevCursorLocation = Vector2.Transform(new Vector2(prevMouse.Position.X,
                                                                           prevMouse.Position.Y),
                                                               Matrix.Invert(camera.GetTransformationMatrix()));
                Vector2 mouseDelta = (cursorLocation - prevCursorLocation);
                Vector2 whipDirection = cursorLocation - playerAnchorPoint;
                // set the whip handle location to be at a certain point within a circle
                // around the player anchor point
                if (whipDirection.Length() > playerAnchorRadius)
                {
                    whipDirection = Vector2.Normalize(whipDirection);
                    whipSegmentLocation = playerAnchorPoint + whipDirection * playerAnchorRadius;
                }
                else
                {
                    whipSegmentLocation = playerAnchorPoint + whipDirection;
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
                                    MouseState mouseState, Camera camera, List<Skeleton> skeletons, List<Zombie> zombies, List<KingBoss> boss)
        {
            if (keyboardState.IsKeyDown(Keys.E) && mouseState.RightButton == ButtonState.Pressed
                && prevMouse.RightButton == ButtonState.Released && whipSegmentList.Count == 0
                && committedLightningSegmentList.Count == 0)
            {
                // I want the lightning to draw while the mouse is being dragged, look at Draw method
                isDrawingLightning = true;
                cursorLocation = Vector2.Transform(new Vector2(mouseState.X, mouseState.Y),
                                                           Matrix.Invert(camera.GetTransformationMatrix()));
                LightningSegment segment = new LightningSegment
                {
                    PointA = playerAnchorPoint,
                    PointB = cursorLocation
                };
                lightningSegmentList.Add(segment);
            }
            if (keyboardState.IsKeyDown(Keys.E) && mouseState.RightButton == ButtonState.Pressed
                && prevMouse.RightButton == ButtonState.Pressed && whipSegmentList.Count == 0)
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
                        if (Math.Abs(Vector2.Dot(previewDirection, Vector2.Normalize(previousSegment.PointB - previousSegment.PointA))) > 0.35f
                            || Math.Abs(Vector2.Dot(previewDirection, Vector2.Normalize(previousSegment.PointB - previousSegment.PointA))) < -0.35f)
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

                if (lightningPersistTimer >= 1.5f)
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
                && missileList.Count == 0)
            {
                cursorLocation = Vector2.Transform(new Vector2(mouseState.X, mouseState.Y),
                                                           Matrix.Invert(camera.GetTransformationMatrix()));
                Vector2 missileDirection = cursorLocation - playerAnchorPoint;
                Vector2 missileLocation = playerAnchorPoint;
                if (missileDirection != Vector2.Zero)
                {
                    missileDirection.Normalize();
                    missileLocation = playerAnchorPoint + missileDirection * playerAnchorRadius;
                }
                Missile missile = new Missile();
                missile.Position = missileLocation;
                missile.Rect = new Rectangle((int)missile.Position.X - 5, (int)missile.Position.Y - 5, 10, 10);
                missile.Direction = missileDirection;

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
                    tempMissile.Direction += (newDirection*0.1f)*5f;
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
            playerRect.X = (int)playerPosition.X;
            playerRect.Y = (int)playerPosition.Y;
        }
        public void UpdateCollision()
        {
            Rectangle playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y,
                                                 playerWidth, playerHeight);
            foreach (Rectangle rectangle in _collisionRects)
            {
                if (!playerRect.Intersects(rectangle))
                    continue;


                if (playerPosition.X + playerWidth / 2 < rectangle.Center.X)
                    playerPosition.X = rectangle.Left - playerWidth;
                else
                    playerPosition.X = rectangle.Right;

                playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y,
                                           playerWidth, playerHeight);
            }
            foreach (Rectangle rectangle in _platformRects)
            {
                if (!playerRect.Intersects(rectangle))
                    continue;


                if (playerPosition.X + playerWidth / 2 < rectangle.Center.X)
                    playerPosition.X = rectangle.Left - playerWidth;
                else
                    playerPosition.X = rectangle.Right;

                playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y,
                                           playerWidth, playerHeight);
            }

            // --- Vertical move + collision ---
            playerPosition.Y += playerVelocityY;

            bool landedThisFrame = false;
            playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y,
                                       playerWidth, playerHeight);
            foreach (Rectangle rectangle in _collisionRects)
            {
                if (!playerRect.Intersects(rectangle))
                    continue;

                if (playerVelocityY >= 0)
                {
                    playerPosition.Y = rectangle.Top - playerHeight;
                    playerVelocityY = 0f;
                    landedThisFrame = true;
                }
                else
                {
                    playerPosition.Y = rectangle.Bottom;
                    playerVelocityY = 0f;
                }

                playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y,
                                           playerWidth, playerHeight);
            }
            foreach (Rectangle rectangle in _platformRects)
            {
                if (!playerRect.Intersects(rectangle))
                    continue;

                if (playerVelocityY >= 0)
                {
                    playerPosition.Y = rectangle.Top - playerHeight;
                    playerVelocityY = 0f;
                    landedThisFrame = true;
                }
                else
                {
                    playerPosition.Y = rectangle.Bottom;
                    playerVelocityY = 0f;
                }

                playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y,
                                           playerWidth, playerHeight);
            }

            // Ground probe — 1px below feet to detect standing still or walking off an edge
            Rectangle groundProbe = new Rectangle((int)playerPosition.X,
                                                  (int)playerPosition.Y + playerHeight,
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
            foreach (WhipSegment whipSegment in whipSegmentList)
            {
                spriteBatch.Draw(pixelTexture, whipSegment.Position, null,
                                 Color.SaddleBrown, whipSegment.Angle,
                                 new Vector2(0.5f, 0.5f),
                                 new Vector2(segmentWidth, segmentHeight),
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
                    : playerAnchorPoint;
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
                spriteBatch.Draw(pixelTexture, missile.Rect, Color.Gold);
            }
        }
    }
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
        public Vector2 skeletonSpawnPosition;
        public Vector2 skeletonAnchorPoint;
        private readonly int skeletonAnchorRadius = 24;
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

        }

        public void Update(GameTime gameTime, Player player, int mapWidth, int mapHeight)
        {
            this.skeletonRect = new Rectangle((int)skeletonPosition.X, (int)skeletonPosition.Y,
                                              this.skeletonWidth, this.skeletonHeight);
            skeletonAnchorPoint = new Vector2(skeletonPosition.X + skeletonWidth / 2, skeletonPosition.Y + skeletonHeight / 2);
            Vector2 direction = player.playerPosition - this.skeletonPosition;
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

        }
        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture)
        {
            spriteBatch.Draw(pixelTexture, skeletonRect, skeletonColor);
            
            DrawBowAndArrow(spriteBatch, pixelTexture);
            DrawDagger(spriteBatch, pixelTexture);
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

                if (arrowRect.Intersects(player.playerRect))
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
                isMelee = true;
                if (meleeTimer < 1f)
                {
                    float lungeProgress = (meleeTimer - 1.5f) / 0.125f;
                    dagger.Position = skeletonAnchorPoint + direction * (skeletonAnchorRadius + skeletonAnchorRadius * 2.5f * lungeProgress)/3;
                }
                else
                {
                    float pullProgress = (meleeTimer - 1.625f) / 0.125f;
                    dagger.Position = skeletonAnchorPoint + direction * (skeletonAnchorRadius + skeletonAnchorRadius * 2.5f * (1f - pullProgress))/3;
                }

                Rectangle daggerRect = new Rectangle(
                    (int)dagger.Position.X - dagger.Width / 2,
                    (int)dagger.Position.Y - dagger.Height / 2,
                    dagger.Width, dagger.Height);

                if (daggerRect.Intersects(player.playerRect))
                    player.TakeHit(direction, false, 1f);
            }

            dagger.Angle = MathF.Atan2(direction.Y, direction.X) + MathF.PI;
            dagger.Width = 20;
            dagger.Height = 5;

            if (meleeTimer >= 1.75f)
            {
                isMelee = false;
                meleeTimer = 0f;
            }
        }
        public void UpdatePlayerInteraction(GameTime gameTime, Vector2 direction, Player player)
        {
            if (!brokenBones)
            {
                bool canSee = HasLineOfSight(skeletonAnchorPoint, player.playerAnchorPoint);

                if (canSee)
                {
                    if (Vector2.DistanceSquared(player.playerPosition, skeletonAnchorPoint) <= 340 * 340 &&
                    Vector2.DistanceSquared(player.playerPosition, skeletonAnchorPoint) >= 60 * 60)
                    {
                        bowDrawn = true;
                        RangedAttack(gameTime, direction);
                    }
                    else if (Vector2.DistanceSquared(player.playerPosition, skeletonAnchorPoint) < 60 &&
                             Vector2.DistanceSquared(player.playerPosition, skeletonAnchorPoint) > 45)
                    {
                        bowDrawn = false;
                        this.skeletonPosition.X += direction.X * skeletonSpeed;
                    }
                    else if (Vector2.DistanceSquared(player.playerPosition, skeletonAnchorPoint) <= 45 * 45)
                    {
                        bowDrawn = false;
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
                if (skeletonRect.Intersects(player.playerRect))
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
                spriteBatch.Draw(pixelTexture, this.bow.Position, null,
                                Color.SaddleBrown, this.bow.Angle,
                                new Vector2(0.5f, 0.5f),
                                new Vector2(this.bow.Width, this.bow.Height),
                                SpriteEffects.None, 0f);

                if (arrowTimer >= 0.5f && arrowTimer <= 3f)
                {
                    spriteBatch.Draw(pixelTexture, nockedArrow.Position, null,
                               Color.Black, nockedArrow.Angle,
                               new Vector2(0.5f, 0.5f),
                               new Vector2(nockedArrow.Width, nockedArrow.Height),
                               SpriteEffects.None, 0f);
                }
                
            }

            foreach (Arrow arrow in arrowList)
            {
                spriteBatch.Draw(pixelTexture, arrow.Position, null,
                                Color.Black, arrow.Angle,
                                new Vector2(0.5f, 0.5f),
                                new Vector2(arrow.Width, arrow.Height),
                                SpriteEffects.None, 0f);
            }
        }
        public void DrawDagger(SpriteBatch spriteBatch, Texture2D pixelTexture)
        {
            if (isMelee && !brokenBones)
            {
                spriteBatch.Draw(pixelTexture, this.dagger.Position, null,
                                 Color.Gray, dagger.Angle,
                                 new Vector2(0.5f, 0.5f),
                                 new Vector2(dagger.Width, dagger.Height),
                                 SpriteEffects.None, 0f);
            }
        }
    }
    public class Zombie
    {
        private int zombieWidth = 32;
        private int zombieHeight = 64;
        private float zombieSpeed = 1;
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

        public Zombie(Vector2 position, List<Rectangle> collisionRects, List<Rectangle> platformRects, float gravity)
        {
            this.zombiePosition = position;
            this.zombieSpawn = position;
            this.zombieRect = new Rectangle((int)zombiePosition.X, (int)zombiePosition.Y,
                                              this.zombieWidth, this.zombieHeight);
            this._collisionRects = collisionRects;
            this._platformRects = platformRects;
            this.zombieGravity = gravity;
            this.zombieColor = Color.YellowGreen;

        }

        public void Update(GameTime gameTime, Player player)
        {
            if (!zombieDead)
            {
                this.zombieRect = new Rectangle((int)zombiePosition.X, (int)zombiePosition.Y,
                                              this.zombieWidth, this.zombieHeight);
                zombieAnchorPoint = new Vector2(zombieRect.X + zombieRect.Width / 2, zombieRect.Y + zombieRect.Height / 2);
                Vector2 direction = player.playerPosition - this.zombiePosition;
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
            }
            else
            {
                if (canRespawn)
                {
                    zombieGas = false;
                    zombieHP = 2;
                    zombieHeight = 64;
                    zombieWidth = 32;
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
            if (!zombieDead)
            {
                spriteBatch.Draw(pixelTexture, zombieRect, zombieColor);
            }
        }
        public void UpdatePlayerInteraction(GameTime gameTime, Vector2 direction, Player player)
        {
            bool canSee = HasLineOfSight(zombieAnchorPoint, player.playerAnchorPoint);

            if (canSee &&
                Vector2.DistanceSquared(player.playerPosition, zombieAnchorPoint) <= 300 * 300 &&
                Vector2.DistanceSquared(player.playerPosition, zombieAnchorPoint) >= 28 &&
                zombieHP > 0)
            {
                this.zombiePosition.X += direction.X * zombieSpeed;
            }
            if (zombieRect.Intersects(player.playerRect))
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
                zombieWidth = 64;
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
    }
    public class Ghost
    {
        private int ghostWidth = 70;
        private int ghostHeight = 70;
        private float ghostSpeed = 1.5f;
        public int ghostHP = 2;
        public float deltaTime = 0f;

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
            this.ghostRect = new Rectangle((int)ghostPosition.X, (int)ghostPosition.Y,
                                              this.ghostWidth, this.ghostHeight);
            ghostAnchorPoint = new Vector2(ghostPosition.X + ghostWidth/2, ghostPosition.Y +  ghostHeight/2);
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

                Vector2 direction = player.playerPosition - this.ghostPosition;
                if (direction != Vector2.Zero)
                {
                    direction.Normalize();
                }

                if (!player.dead)
                {
                    UpdatePlayerInteraction(gameTime, direction, player);
                }
                UpdateChair(player);
                UpdateWails(player);
                UpdateMovement(gameTime);
                UpdateGhostRectangle(gameTime);
            }
            else
            {
                ghostPosition = ghostSpawn;
            }
            

        }
        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, GameTime gameTime)
        {
            if (ghostHP > 0)
            {
                spriteBatch.Draw(pixelTexture, ghostRect, ghostColor);
                DrawChair(spriteBatch, pixelTexture);
                DrawWails(spriteBatch, pixelTexture);
            }
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

                if (chairRect.Intersects(player.playerRect))
                {
                    player.TakeHit(chair.Direction, false, 2f);
                    hit = true;
                }

                if (hit || chair.Position.X < -200 || chair.Position.X > 2000 ||
                    chair.Position.Y < -200 || chair.Position.Y > 1200)
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

                if (tempWailRect.Intersects(player.playerRect))
                {
                    player.TakeHit(tempWail.Direction, true, 0.5f);
                    hit = true;
                }

                if (hit || tempWail.Position.X < -200 || tempWail.Position.X > 2000 ||
                    tempWail.Position.Y < -200 || tempWail.Position.Y > 1200)
                    ghostlyWails.RemoveAt(i);
            }
        }
        public void UpdatePlayerInteraction(GameTime gameTime, Vector2 direction, Player player)
        {
            bool canSee = HasLineOfSight(ghostAnchorPoint, player.playerAnchorPoint);

            if (canSee)
            {
                if (Vector2.DistanceSquared(player.playerPosition, ghostAnchorPoint) <= 280 * 280 &&
                    Vector2.DistanceSquared(player.playerPosition, ghostAnchorPoint) >= 275 * 275 ||
                    Vector2.DistanceSquared(player.playerPosition, ghostAnchorPoint) <= 140 * 140 &&
                    Vector2.DistanceSquared(player.playerPosition, ghostAnchorPoint) >= 120 * 120)
                {
                    ghostPosition += direction * ghostSpeed;
                }
                if (Vector2.DistanceSquared(player.playerPosition, ghostAnchorPoint) <= 280 * 280 &&
                    Vector2.DistanceSquared(player.playerPosition, ghostAnchorPoint) >= 135 * 135)
                {
                    RangedAttack(gameTime, direction);
                    playerInRange = true;
                }
                else
                {
                    playerInRange = false;
                }
                if (Vector2.DistanceSquared(player.playerPosition, ghostAnchorPoint) <= 125 * 125 &&
                    Vector2.DistanceSquared(player.playerPosition, ghostAnchorPoint) <= 120 * 120)
                {
                    WailAttack(gameTime, direction);
                }
            }
            else
            {
                playerInRange = false;
            }

            if (ghostRect.Intersects(player.playerRect))
                player.TakeHit(direction, false, 3f);
        }
        public void UpdateMovement(GameTime gameTime)
        {
            ghostPosition += knockbackVelocity;
            knockbackVelocity *= 0.8f;
            if (knockbackVelocity.LengthSquared() < 0.01f)
                knockbackVelocity = Vector2.Zero;

            deltaTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            ghostPosition.Y += MathF.Sin(deltaTime * 0.5f * MathHelper.TwoPi);
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
                spriteBatch.Draw(pixelTexture, possessedChair.Position, null,
                               Color.SaddleBrown, 0,
                               new Vector2(0.5f, 0.5f),
                               new Vector2(possessedChair.Width, possessedChair.Height),
                               SpriteEffects.None, 0f);
            }
            foreach (Chair chair in _chairList)
            {
                spriteBatch.Draw(pixelTexture, chair.Position, null,
                                Color.SaddleBrown, 0,
                                new Vector2(0.5f, 0.5f),
                                new Vector2(chair.Width, chair.Height),
                                SpriteEffects.None, 0f);
            }
        }
        public void DrawWails(SpriteBatch spriteBatch, Texture2D pixelTexture)
        {
            foreach (Wail wail in ghostlyWails)
            {
                spriteBatch.Draw(pixelTexture, wail.Position, null,
                                Color.SaddleBrown, wail.Angle,
                                new Vector2(0.5f, 0.5f),
                                new Vector2(wail.Width, wail.Height),
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
    }
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

        public void Update(GameTime gameTime, Player player, List<Zombie> zombies)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            bossAnchorPoint = new Vector2(bossPosition.X + bossWidth / 2f, bossPosition.Y + bossHeight / 2f);

            Vector2 toPlayer = player.playerPosition - bossAnchorPoint;
            Vector2 dirToPlayer = toPlayer;
            if (dirToPlayer != Vector2.Zero) dirToPlayer.Normalize();

            if (bossRect.Intersects(player.playerRect))
                player.TakeHit(dirToPlayer, false, 0.5f);

            UpdateSlamCooldown(dt, player);
            UpdateSummonCooldown(dt);
            UpdateState(dt, dirToPlayer, player, zombies);
            UpdateOrbs(dt, dirToPlayer, player);
            UpdateShockwaves(dt, player);
            UpdateFloat(dt);
            UpdateKnockback();
            UpdateBossRect();
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

        private void UpdateState(float dt, Vector2 dirToPlayer, Player player, List<Zombie> zombies)
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
                        summoned.wasSummoned = true;
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

                    bool hitWall = chargeDirection.X > 0
                        ? bossPosition.X >= chargeTargetX
                        : bossPosition.X <= chargeTargetX;

                    if (hitWall || chargeDuration >= chargeDurationLimit)
                    {
                        bossPosition.X = chargeTargetX;
                        state = BossState.ChargeRecovery;
                        bossColor = Color.Blue;
                    }

                    if (bossRect.Intersects(player.playerRect))
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
                : MathHelper.Clamp(player.playerPosition.X - bossWidth / 2f, leftWall, rightWall - bossWidth);
            state = BossState.SlamSnappingToCenter;
            bossColor = Color.DarkViolet;
        }

        private void SpawnShockwaves()
        {
            _shockwaves.Add(new Shockwave
            {
                Position = new Vector2(bossAnchorPoint.X, bossAnchorPoint.Y+bossHeight/4),
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

                if (waveRect.Intersects(player.playerRect))
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
            bossPosition.Y = MathHelper.Clamp(player.playerPosition.Y, centerY, floor - bossHeight);
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

                if (orbRect.Intersects(player.playerRect))
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
            spriteBatch.Draw(pixelTexture, bossRect, bossColor);
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
                spriteBatch.Draw(pixelTexture, orbRect, Phase2 ? Color.Crimson : Color.Purple);
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
                spriteBatch.Draw(pixelTexture, waveRect, Color.Yellow);
            }
        }

        public void TakeHit(Vector2 hitDirection, float damage, float force = 8f)
        {
            knockbackVelocity = hitDirection * force;
            if (bossHP > 0)
                bossHP -= damage;
        }
    }
}