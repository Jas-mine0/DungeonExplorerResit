
namespace Dungeon_Explorer.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;


    [TestClass]
    public class GameTests
    {
        private static StreamWriter _logWriter;
        private static string _logPath;

       [ClassInitialize]
        public static void ClassInitialization(TestContext context)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _logPath = $"GameTest_Results_{timestamp}.log";
            _logWriter = new StreamWriter(_logPath, true);
            LogMessage("Game system tests have begun.");
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            LogMessage("Game system tests are complete.");
            _logWriter.Flush();
            _logWriter.Close();

            Console.WriteLine($"Test log file created at: {Path.GetFullPath(_logPath)}");
        }

        [TestInitialize]
        public void TestInitialization()
        {
            LogMessage($"Starting test: {TestContext.TestName}");
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            LogMessage($"Completed test: {TestContext.TestName}");
        }

        private static void LogMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string formattedMessage = $"[{timestamp}] {message}";
            _logWriter.WriteLine(formattedMessage);
            _logWriter.Flush();

            // Also output to console for real-time monitoring
            Console.WriteLine(formattedMessage);
        }

        // Test player creation and basic stats
        [TestMethod]
        public void Player_Creation_Test()
        {
            try
            {
                // Arrange
                LogMessage("Creating test player");
                string name = "TestHero";
                int health = 100;
                int attack = 15;
                int defense = 8;

                // Act
                var player = new Dungeon_Explorer.Player(name, health, attack, defense);

                // Assert with Debug.Assert for code verification
                Debug.Assert(player.Name == name, "Debug: Player name should match input");
                Debug.Assert(player.MaxHealth == health, "Debug: Player max health should match input");
                Debug.Assert(player.AttackPower == attack, "Debug: Player attack should match input");
                Debug.Assert(player.Defense == defense, "Debug: Player defense should match input");
                Debug.Assert(player.IsAlive, "Debug: Player should be alive when created");

                Assert.AreEqual(name, player.Name, "Player name should match input");
                Assert.AreEqual(health, player.MaxHealth, "Player max health should match input");
                Assert.AreEqual(health, player.CurrentHealth, "Player current health should equal max health");
                Assert.AreEqual(attack, player.AttackPower, "Player attack should match input");
                Assert.AreEqual(defense, player.Defense, "Player defense should match input");
                Assert.IsTrue(player.IsAlive, "Player should be alive when created");

                LogMessage("Player_Creation_Test: PASSED");
            }
            catch (Exception ex)
            {
                LogMessage($"Player_Creation_Test: FAILED. Exception: {ex.Message}");
                LogMessage($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        // Test player inventory management
        [TestMethod]
        public void Player_Inventory_Management_Test()
        {
            try
            {
                // Arrange
                LogMessage("Testing inventory management");
                var player = new Dungeon_Explorer.Player("InventoryTester", 100, 10, 5);
                var sword = new Dungeon_Explorer.Weapons("Test Sword", "A test weapon", 10);
                var potion = new Dungeon_Explorer.Potions("Test Potion", "A test healing item", 20);

                // Act & Assert: Add items to inventory
                player.AddToInventory(sword);
                player.AddToInventory(potion);

                Debug.Assert(player.Inventory.Count == 2, "Debug: Inventory should contain two items");
                Assert.AreEqual(2, player.Inventory.Count, "Inventory should contain two items");

                // Act & Assert: Equip weapon
                player.EquipWeapon(sword);

                Debug.Assert(player.EquippedWeapon == sword, "Debug: Player should have the sword equipped");
                Assert.AreEqual(sword, player.EquippedWeapon, "Player should have the sword equipped");

                // Create console output capture
                var consoleOutput = new StringWriter();
                var originalOutput = Console.Out;
                Console.SetOut(consoleOutput);

                // Act & Assert: Use potion
                int initialHealth = player.CurrentHealth;
                player.CurrentHealth = 50; // Set health lower to test potion
                int potionIndex = player.Inventory.IndexOf(potion);
                player.UsePotion(potionIndex);

                Console.SetOut(originalOutput);

                Debug.Assert(player.Inventory.Count == 1, "Debug: Inventory should have one item after using potion");
                Assert.AreEqual(1, player.Inventory.Count, "Inventory should have one item after using potion");

                string output = consoleOutput.ToString();
                Debug.Assert(output.Contains("heals for"), "Debug: Output should indicate healing occurred");
                Assert.IsTrue(output.Contains("heals for"), "Output should indicate healing occurred");

                LogMessage("Player_Inventory_Management_Test: PASSED");
            }
            catch (Exception ex)
            {
                LogMessage($"Player_Inventory_Management_Test: FAILED. Exception: {ex.Message}");
                LogMessage($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        // Test combat system
        [TestMethod]
        public void Combat_System_Test()
        {
            try
            {
                // Arrange
                LogMessage("Testing combat system");
                var player = new Dungeon_Explorer.Player("CombatTester", 100, 20, 5);
                var monster = new Dungeon_Explorer.FrogMonster();
                int initialMonsterHealth = monster.CurrentHealth;

                // Create console output capture
                var consoleOutput = new StringWriter();
                var originalOutput = Console.Out;
                Console.SetOut(consoleOutput);

                // Act: Player attacks monster
                player.Attack(monster);

                // Assert: Monster health should decrease
                Console.SetOut(originalOutput);
                string output = consoleOutput.ToString();

                Debug.Assert(monster.CurrentHealth < initialMonsterHealth, "Debug: Monster health should decrease after attack");
                Assert.IsTrue(monster.CurrentHealth < initialMonsterHealth, "Monster health should decrease after attack");

                Debug.Assert(output.Contains("attacks"), "Debug: Output should describe the attack");
                Assert.IsTrue(output.Contains("attacks"), "Output should describe the attack");

                // Reset console output capture for monster attack
                consoleOutput = new StringWriter();
                Console.SetOut(consoleOutput);

                // Act: Monster attacks player
                int initialPlayerHealth = player.CurrentHealth;
                monster.Attack(player);

                // Assert: Player health should decrease
                Console.SetOut(originalOutput);
                output = consoleOutput.ToString();

                Debug.Assert(player.CurrentHealth < initialPlayerHealth, "Debug: Player health should decrease after monster attack");
                Assert.IsTrue(player.CurrentHealth < initialPlayerHealth, "Player health should decrease after monster attack");

                Debug.Assert(output.Contains("attacks"), "Debug: Output should describe the monster attack");
                Assert.IsTrue(output.Contains("attacks"), "Output should describe the monster attack");

                LogMessage("Combat_System_Test: PASSED");
            }
            catch (Exception ex)
            {
                LogMessage($"Combat_System_Test: FAILED. Exception: {ex.Message}");
                LogMessage($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        // Test room navigation and interconnection
        [TestMethod]
        public void Room_Navigation_Test()
        {
            try
            {
                // Arrange
                LogMessage("Testing room navigation");
                var gameMap = new Dungeon_Explorer.GameMap();

                // Create test rooms
                Dungeon_Explorer.Room room1 = new Dungeon_Explorer.Room(1, "Test Room 1", "A test room");
                Dungeon_Explorer.Room room2 = new Dungeon_Explorer.Room(2, "Test Room 2", "Another test room");
                Dungeon_Explorer.Room room3 = new Dungeon_Explorer.Room(3, "Test Room 3", "A third test room");

                // Connect rooms
                room1.ConnectRoom(2);
                room2.ConnectRoom(1);
                room2.ConnectRoom(3);
                room3.ConnectRoom(2);

                // Add rooms to map
                gameMap.AddRoom(room1);
                gameMap.AddRoom(room2);
                gameMap.AddRoom(room3);

                // Act & Assert: Check connections
                var room1Connections = room1.GetConnectedRooms();
                var room2Connections = room2.GetConnectedRooms();
                var room3Connections = room3.GetConnectedRooms();

                Debug.Assert(room1Connections.Contains(2), "Debug: Room 1 should connect to Room 2");
                Debug.Assert(room2Connections.Contains(1), "Debug: Room 2 should connect to Room 1");
                Debug.Assert(room2Connections.Contains(3), "Debug: Room 2 should connect to Room 3");
                Debug.Assert(room3Connections.Contains(2), "Debug: Room 3 should connect to Room 2");

                Assert.IsTrue(room1Connections.Contains(2), "Room 1 should connect to Room 2");
                Assert.IsTrue(room2Connections.Contains(1), "Room 2 should connect to Room 1");
                Assert.IsTrue(room2Connections.Contains(3), "Room 2 should connect to Room 3");
                Assert.IsTrue(room3Connections.Contains(2), "Room 3 should connect to Room 2");

                // Test locked room
                room1.ConnectLockedRoom(3, Dungeon_Explorer.KeyType.Silver);
                Assert.IsTrue(room1.IsConnectionLocked(3), "Connection from Room 1 to Room 3 should be locked");
                Assert.AreEqual(Dungeon_Explorer.KeyType.Silver, room1.GetRequiredKeyType(3), "Room 1 to Room 3 should require a Silver key");

                LogMessage("Room_Navigation_Test: PASSED");
            }
            catch (Exception ex)
            {
                LogMessage($"Room_Navigation_Test: FAILED. Exception: {ex.Message}");
                LogMessage($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        // Test monster behaviour AI
        [TestMethod]
        public void Monster_Behaviour_Test()
        {
            try
            {
                // Arrange
                LogMessage("Testing monster behaviour AI");
                var monster = new Dungeon_Explorer.FrogMonster();

                // Test aggressive behaviour (default)
                Dungeon_Explorer.MonsterBehaviour behaviour = monster.Behaviour;
                Assert.AreEqual(Dungeon_Explorer.MonsterBehaviour.Aggressive, behaviour, "Monster should start with aggressive behaviour");

                // Simulate damage to trigger defensive behaviour
                int damage = (int)(monster.MaxHealth * 0.6); // Reduce to 40% health
                monster.DamageTaken(damage);

                behaviour = monster.DecideBehaviour();
                Assert.AreEqual(Dungeon_Explorer.MonsterBehaviour.Defensive, behaviour, "Monster should become defensive at low health");

                // Simulate more damage to trigger fleeing behaviour
                monster.DamageTaken(monster.MaxHealth / 4); // Reduce to less than 20% health

                behaviour = monster.DecideBehaviour();
                Assert.AreEqual(Dungeon_Explorer.MonsterBehaviour.Fleeing, behaviour, "Monster should try to flee at very low health");

                // Test boss monster behaviour
                var bossMonster = new Dungeon_Explorer.BossMonster();
                behaviour = bossMonster.DecideBehaviour();
                Assert.AreEqual(Dungeon_Explorer.MonsterBehaviour.Aggressive, behaviour, "Boss should always be aggressive");

                // Boss activation test
                Assert.IsFalse(bossMonster.IsActive, "Boss should start inactive");
                bossMonster.Activate();
                Assert.IsTrue(bossMonster.IsActive, "Boss should be active after activation");

                LogMessage("Monster_Behaviour_Test: PASSED");
            }
            catch (Exception ex)
            {
                LogMessage($"Monster_Behaviour_Test: FAILED. Exception: {ex.Message}");
                LogMessage($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        // Test LINQ functionality for inventory
        [TestMethod]
        public void LINQ_Inventory_Test()
        {
            try
            {
                // Arrange
                LogMessage("Testing LINQ functionality for inventory");
                var player = new Dungeon_Explorer.Player("LINQTester", 100, 10, 5);

                // Add varied items
                player.AddToInventory(new Dungeon_Explorer.Weapons("Weak Sword", "A weak sword", 5));
                player.AddToInventory(new Dungeon_Explorer.Potions("Small Potion", "A minor healing potion", 10));
                player.AddToInventory(new Dungeon_Explorer.Weapons("Strong Sword", "A powerful sword", 15));
                player.AddToInventory(new Dungeon_Explorer.Potions("Large Potion", "A major healing potion", 25));
                player.AddToInventory(new Dungeon_Explorer.Key("Bronze Key", "A simple key", Dungeon_Explorer.KeyType.Bronze));

                // Act & Assert

                // Test getting all healing items
                var healingItems = player.GetAllHealingItems();

                Debug.Assert(healingItems.Count == 2, "Debug: Should find 2 healing items");
                Debug.Assert(healingItems[0].HealAmount > healingItems[1].HealAmount, "Debug: Healing items should be sorted by heal amount");

                Assert.AreEqual(2, healingItems.Count, "Should find 2 healing items");
                Assert.IsTrue(healingItems[0].HealAmount > healingItems[1].HealAmount, "Healing items should be sorted by heal amount");

                // Test key type check
                bool hasBronzeKey = player.HasKeyOfType(Dungeon_Explorer.KeyType.Bronze);
                bool hasSilverKey = player.HasKeyOfType(Dungeon_Explorer.KeyType.Silver);

                Assert.IsTrue(hasBronzeKey, "Player should have a bronze key");
                Assert.IsFalse(hasSilverKey, "Player should not have a silver key");

                // Test equip strongest weapon
                player.EquipStrongestWeapon();

                Debug.Assert(player.EquippedWeapon.Name == "Strong Sword", "Debug: Strongest weapon should be equipped");
                Debug.Assert(player.EquippedWeapon.Damage == 15, "Debug: Strongest weapon should have 15 damage");

                Assert.AreEqual("Strong Sword", player.EquippedWeapon.Name, "Strongest weapon should be equipped");
                Assert.AreEqual(15, player.EquippedWeapon.Damage, "Strongest weapon should have 15 damage");

                LogMessage("LINQ_Inventory_Test: PASSED");
            }
            catch (Exception ex)
            {
                LogMessage($"LINQ_Inventory_Test: FAILED. Exception: {ex.Message}");
                LogMessage($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        // Test Statistics tracking
        [TestMethod]
        public void Statistics_Tracking_Test()
        {
            try
            {
                // Arrange
                LogMessage("Testing statistics tracking");
                var stats = new Dungeon_Explorer.Statistics();
                var player = new Dungeon_Explorer.Player("StatsTester", 100, 10, 5);

                stats.TrackPlayer(player);

                // Act
                stats.MonsterDefeated(10); // Defeat a monster worth 10 XP
                stats.RoomVisited(); // Visit one room
                stats.ItemCollected(); // Collect one item
                stats.ItemCollected(); // Collect another item

                // Create console output capture
                var consoleOutput = new StringWriter();
                var originalOutput = Console.Out;
                Console.SetOut(consoleOutput);

                // Display stats
                stats.DisplayStatistics();

                // Assert
                Console.SetOut(originalOutput);
                string output = consoleOutput.ToString();

                Debug.Assert(output.Contains("Monsters Defeated: 1"), "Debug: Should show 1 monster defeated");
                Debug.Assert(output.Contains("Rooms Visited: 1"), "Debug: Should show 1 room visited");
                Debug.Assert(output.Contains("Items Collected: 2"), "Debug: Should show 2 items collected");

                Assert.IsTrue(output.Contains("Monsters Defeated: 1"), "Should show 1 monster defeated");
                Assert.IsTrue(output.Contains("Rooms Visited: 1"), "Should show 1 room visited");
                Assert.IsTrue(output.Contains("Items Collected: 2"), "Should show 2 items collected");

                LogMessage("Statistics_Tracking_Test: PASSED");
            }
            catch (Exception ex)
            {
                LogMessage($"Statistics_Tracking_Test: FAILED. Exception: {ex.Message}");
                LogMessage($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        // Test Special Room puzzle functionality
        [TestMethod]
        public void SpecialRoom_Puzzle_Test()
        {
            try
            {
                // Arrange
                LogMessage("Testing special room puzzle functionality");
                var player = new Dungeon_Explorer.Player("PuzzleTester", 100, 10, 5);

                // Test Riddle Room
                var riddleRoom = new Dungeon_Explorer.RiddleRoom(1, "Test Riddle Room", "A room with a riddle");
                var rewardWeapon = new Dungeon_Explorer.Weapons("Puzzle Sword", "A reward for solving the riddle", 15);
                riddleRoom.SetReward(rewardWeapon);

                // Access the private fields through reflection to set a known riddle/answer for testing
                var riddleType = typeof(Dungeon_Explorer.RiddleRoom);
                var riddleField = riddleType.GetField("_currentRiddle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var answerField = riddleType.GetField("_currentAnswer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (riddleField != null && answerField != null)
                {
                    riddleField.SetValue(riddleRoom, "Test riddle");
                    answerField.SetValue(riddleRoom, "test");
                }

                // Create console output capture
                var consoleOutput = new StringWriter();
                var originalOutput = Console.Out;
                Console.SetOut(consoleOutput);

                // Act & Assert: Attempt puzzle with correct answer
                bool result = riddleRoom.AttemptPuzzle(player, "test");

                Console.SetOut(originalOutput);
                string output = consoleOutput.ToString();

                Assert.IsTrue(result, "Puzzle should be solved with correct answer");
                Assert.IsTrue(output.Contains("Congratulations"), "Output should congratulate player");
                Assert.IsTrue(player.Inventory.Contains(rewardWeapon), "Player should receive reward");

                LogMessage("SpecialRoom_Puzzle_Test: PASSED");
            }
            catch (Exception ex)
            {
                LogMessage($"SpecialRoom_Puzzle_Test: FAILED. Exception: {ex.Message}");
                LogMessage($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        public TestContext TestContext { get; set; }
    }
}
