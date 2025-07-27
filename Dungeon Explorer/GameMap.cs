using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading;

namespace Dungeon_Explorer
{
    public class GameMap
    {
        private Dictionary<int, Room> _rooms;
        private int _currentRoomId;
        private Statistics _stats;

        public GameMap()
        {
            _rooms = new Dictionary<int, Room>();
            _currentRoomId = 1;
            _stats = null;
        }

        public void DebugPrintRoomConnections()
        {
            Console.WriteLine("===== DEBUG: ROOM CONNECTIONS =====");
            foreach (var roomEntry in _rooms)
            {
                Room room = roomEntry.Value;
                Console.WriteLine($"Room {room.ID} ({room.Name}) connects to:");
                List<int> connections = room.GetConnectedRooms();
                if (connections.Count == 0)
                {
                    Console.WriteLine("  - NO CONNECTIONS");
                }
                else
                {
                    foreach (int connectedRoomId in connections)
                    {
                        Room connectedRoom = GetRoom(connectedRoomId);
                        string roomName = connectedRoom != null ? connectedRoom.Name : "Unknown Room";
                        Console.WriteLine($"  - Room {connectedRoomId} ({roomName})" +
                            (room.IsConnectionLocked(connectedRoomId) ? " (Locked)" : ""));
                    }
                }
            }
            Console.WriteLine("=================================");
        }

        public void SetStatistics(Statistics stats)
        {
            _stats = stats;
        }

        public void AddRoom(Room room)
        {
            _rooms[room.ID] = room;
        }

        public Room GetRoom(int roomId)
        {
            return _rooms.ContainsKey(roomId) ? _rooms[roomId] : null;
        }

        public Room GetCurrentRoom()
        {
            return GetRoom(_currentRoomId);
        }

        public List<int> GetAvailableRooms()
        {
            Room currentRoom = GetCurrentRoom();
            if (currentRoom == null)
            {
                return new List<int>();
            }

            return currentRoom.GetConnectedRooms() ?? new List<int>();
        }

        // Method to prevent automatic combat in special rooms
        public void MoveToAnotherRoom(int roomId, Player player)
        {
            Room currentRoom = GetCurrentRoom();

            if (currentRoom == null || !currentRoom.GetConnectedRooms().Contains(roomId))
            {
                Console.WriteLine("You cannot go there from here.");
                return;
            }

            if (currentRoom.IsConnectionLocked(roomId))
            {
                KeyType requiredKeyType = currentRoom.GetRequiredKeyType(roomId);

                if (player.HasKeyOfType(requiredKeyType))
                {
                    Console.WriteLine($"This door is locked and requires a {requiredKeyType} Key.");
                    Console.WriteLine("You have the required key. Use it to unlock the door? (y/n)");

                    if (Console.ReadLine().ToLower().StartsWith("y"))
                    {
                        player.UseKey(requiredKeyType);
                        Console.WriteLine("The door is now unlocked.");
                    }
                    else
                    {
                        Console.WriteLine("You decide not to use the key.");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine($"This door is locked and requires a {requiredKeyType} Key.");
                    Console.WriteLine("You don't have the required key.");
                    return;
                }
            }

            _currentRoomId = roomId;
            Room newRoom = GetCurrentRoom();

            if (newRoom != null)
            {
                if (_stats != null && !newRoom.HasBeenVisited)
                {
                    _stats.RoomVisited();
                    newRoom.HasBeenVisited = true;
                }

                newRoom.GenerateRandomEncounter();

                Console.Clear();
                newRoom.DisplayRoom();

                // Only initiate combat if not a special room with unsolved puzzle.
                if (newRoom.Monster != null && newRoom.Monster.IsAlive)
                {
                    if (newRoom.Monster is BossMonster bossMonster && !bossMonster.IsActive)
                    {
                        bossMonster.Activate();
                    }

                    if (!(newRoom is SpecialRoom specialRoom) || specialRoom.PuzzleSolved)
                    {
                        InitiateCombat(player, newRoom.Monster);
                    }
                    else
                    {
                        Console.WriteLine($"The {newRoom.Monster.Name} is in the room but seems distracted by the puzzle mechanism.");
                        Console.WriteLine("You might have time to solve the puzzle before engaging it.");
                    }
                }

                PromptForAction(player);
            }
            else
            {
                Console.WriteLine("Error: The room does not exist.");
            }
        }

        private void InitiateCombat(Player player, Monster monster)
        {
            if (monster == null || !monster.IsAlive) return;

            Console.WriteLine($"\nCombat begins: {player.Name} vs {monster.Name}");
            bool combatEnded = false;
            bool playerEscaped = false;

            while (player.IsAlive && monster.IsAlive && !combatEnded)
            {
                MonsterBehaviour behaviour = monster.DecideBehaviour();

                if (behaviour == MonsterBehaviour.Fleeing)
                {
                    Random random = new Random();
                    int escapeChance = random.Next(100);

                    if (escapeChance < 40)
                    {
                        Console.WriteLine($"{monster.Name} flees from the battle!");
                        combatEnded = true;
                        continue;
                    }
                    else
                    {
                        Console.WriteLine($"{monster.Name} tries to escape but fails!");
                    }
                }

                Console.WriteLine($"\n{player.Name}'s turn");
                DisplayCombatOptions();

                string input = Console.ReadLine();
                switch (input.ToLower())
                {
                    case "1":
                    case "attack":
                        player.Attack(monster);
                        break;

                    case "2":
                    case "potion":
                        HandlePotionUsage(player);
                        break;

                    case "3":
                    case "run":
                        if (TryToEscape())
                        {
                            playerEscaped = true;
                            combatEnded = true;
                        }
                        break;

                    case "4":
                    case "equip":
                        player.EquipStrongestWeapon();
                        Console.WriteLine("You quickly switch weapons!");
                        break;

                    default:
                        Console.WriteLine("Invalid choice. You hesitate and lose your opportunity.");
                        break;
                }

                if (combatEnded || !monster.IsAlive) break;

                if (behaviour != MonsterBehaviour.Fleeing)
                {
                    Console.WriteLine($"\n{monster.Name}'s turn");

                    if (behaviour == MonsterBehaviour.Defensive)
                    {
                        Console.WriteLine($"{monster.Name} takes a defensive stance!");
                        monster.Heal(monster.MaxHealth / 10);

                        int originalPower = monster.AttackPower;
                        monster.AttackPower = originalPower / 2;
                        monster.Attack(player);
                        monster.AttackPower = originalPower;
                    }
                    else
                    {
                        monster.Attack(player);
                    }
                }

                Console.WriteLine($"\n{player.Name}: {player.CurrentHealth}/{player.MaxHealth} HP | {monster.Name}: {monster.CurrentHealth}/{monster.MaxHealth} HP");
            }

            if (!player.IsAlive)
            {
                Console.WriteLine($"\n{player.Name} has been defeated by {monster.Name}!");
                Console.WriteLine("GAME OVER");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            else if (!monster.IsAlive)
            {
                Console.WriteLine($"\n{monster.Name} has been defeated!");

                if (_stats != null)
                {
                    _stats.MonsterDefeated(monster.ExperienceValue);
                }

                player.Experience += monster.ExperienceValue;
                Console.WriteLine($"You gained {monster.ExperienceValue} experience points.");

                List<Items> loot = monster.DropLoot();
                foreach (var item in loot)
                {
                    Console.WriteLine($"{monster.Name} dropped {item.Name}.");
                    if (player.AddToInventory(item) && _stats != null)
                    {
                        _stats.ItemCollected();
                    }
                }
            }
            else if (playerEscaped)
            {
                Console.WriteLine("You managed to escape from the battle!");
            }
            else
            {
                Console.WriteLine($"{monster.Name} fled from battle. You're safe... for now.");
            }
        }

        private void DisplayCombatOptions()
        {
            Console.WriteLine("What will you do?");
            Console.WriteLine("1. Attack");
            Console.WriteLine("2. Use Potion");
            Console.WriteLine("3. Run Away");
            Console.WriteLine("4. Equip Strongest Weapon");
            Console.Write("Choice: ");
        }

        private void HandlePotionUsage(Player player)
        {
            List<Potions> potions = player.GetAllHealingItems();

            if (potions.Count == 0)
            {
                Console.WriteLine("You don't have any potions!");
                return;
            }

            Console.WriteLine("Available Potions:");
            for (int i = 0; i < potions.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {potions[i].Name} (Heals {potions[i].HealAmount} HP)");
            }

            Console.WriteLine("Select a potion to use (0 to cancel):");
            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                if (choice == 0)
                {
                    Console.WriteLine("You decide not to use a potion.");
                    return;
                }

                if (choice >= 1 && choice <= potions.Count)
                {
                    Potions selectedPotion = potions[choice - 1];
                    int index = player.Inventory.IndexOf(selectedPotion);

                    if (index >= 0)
                    {
                        player.UsePotion(index);
                    }
                }
                else
                {
                    Console.WriteLine("Invalid selection.");
                }
            }
            else
            {
                Console.WriteLine("Invalid input.");
            }
        }

        private bool TryToEscape()
        {
            Console.WriteLine("You attempt to escape...");

            Random random = new Random();
            int escapeChance = random.Next(100);

            if (escapeChance < 60)
            {
                Console.WriteLine("You successfully escaped!");
                return true;
            }
            else
            {
                Console.WriteLine("You failed to escape!");
                return false;
            }
        }

        // Updated action prompt that includes the option to engage monsters in special rooms.
        private void PromptForAction(Player player)
        {
            Room currentRoom = GetCurrentRoom();
            bool hasPuzzle = currentRoom is SpecialRoom;
            bool hasMonster = currentRoom.Monster != null && currentRoom.Monster.IsAlive;

            Console.WriteLine("\nWhat would you like to do?");
            Console.WriteLine("1. Look around");
            Console.WriteLine("2. Check inventory");
            Console.WriteLine("3. Collect an item");
            Console.WriteLine("4. Go to another room");
            Console.WriteLine("5. View stats");
            Console.WriteLine("6. Exit game");

            // Only show puzzle option if room has unsolved puzzle
            if (hasPuzzle && !((SpecialRoom)currentRoom).PuzzleSolved)
            {
                Console.WriteLine("7. Attempt puzzle");
            }

            // Option to engage monster
            if (hasMonster)
            {
                Console.WriteLine("8. Engage the monster");
            }

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": // Look around
                    GetCurrentRoom().DisplayRoom();
                    PromptForAction(player);
                    break;

                case "2": // Check inventory
                    Console.Clear();
                    Console.WriteLine("===== INVENTORY =====");
                    player.ShowInventory();

                    Console.WriteLine("\nInventory Options:");
                    Console.WriteLine("1. Use an item");
                    Console.WriteLine("2. Equip strongest weapon");
                    Console.WriteLine("3. Sort by name");
                    Console.WriteLine("4. Filter weapons only");
                    Console.WriteLine("5. Filter potions only");
                    Console.WriteLine("6. Back to room");

                    string inventoryChoice = Console.ReadLine();

                    switch (inventoryChoice)
                    {
                        case "1": // Use an item
                            Console.WriteLine("Enter the number of the item to use (0 to cancel):");
                            if (int.TryParse(Console.ReadLine(), out int itemIndex) && itemIndex > 0 && itemIndex <= player.Inventory.Count)
                            {
                                player.UseItem(itemIndex - 1);
                            }
                            else if (itemIndex != 0)
                            {
                                Console.WriteLine("Invalid item number.");
                            }
                            break;

                        case "2": // Equip strongest weapon
                            player.EquipStrongestWeapon();
                            break;

                        case "3": // Sort by name
                            player.ShowInventory(null, (a, b) => string.Compare(a.Name, b.Name));
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();
                            break;

                        case "4": // Filter weapons only
                            player.ShowInventory(item => item is Weapons);
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();
                            break;

                        case "5": // Filter potions only
                            player.ShowInventory(item => item is Potions);
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();
                            break;
                    }

                    PromptForAction(player);
                    break;

                case "3": // Collect an item
                    Room currentRoomForItems = GetCurrentRoom();
                    currentRoomForItems.DisplayItems();

                    if (currentRoomForItems.Items.Count > 0)
                    {
                        Console.WriteLine("Enter the number of the item to collect (0 to cancel):");
                        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= currentRoomForItems.Items.Count)
                        {
                            currentRoomForItems.CollectItem(index - 1, player);
                            if (_stats != null) _stats.ItemCollected();
                        }
                        else if (index != 0)
                        {
                            Console.WriteLine("Invalid item number.");
                        }
                    }

                    PromptForAction(player);
                    break;

                case "4": // Go to another room
                    List<int> availableRooms = GetAvailableRooms();

                    Console.WriteLine("Available rooms:");
                    foreach (int roomId in availableRooms)
                    {
                        Room room = GetRoom(roomId);
                        Console.Write($"{roomId}. {room.Name}");

                        if (GetCurrentRoom().IsConnectionLocked(roomId))
                        {
                            Console.Write(" (Locked)");
                        }

                        Console.WriteLine();
                    }

                    Console.WriteLine("Enter the number of the room to enter (0 to stay here):");
                    if (int.TryParse(Console.ReadLine(), out int roomChoice) && availableRooms.Contains(roomChoice))
                    {
                        MoveToAnotherRoom(roomChoice, player);
                    }
                    else if (roomChoice != 0)
                    {
                        Console.WriteLine("Invalid room selection.");
                        PromptForAction(player);
                    }
                    else
                    {
                        PromptForAction(player);
                    }
                    break;

                case "5": // View stats
                    Console.Clear();
                    if (_stats != null) _stats.DisplayStatistics();
                    player.DisplayExperience();
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    PromptForAction(player);
                    break;

                case "6": // Exit game
                    Console.WriteLine("Are you sure you want to exit? (y/n)");
                    if (Console.ReadLine().ToLower().StartsWith("y"))
                    {
                        Console.WriteLine("Thanks for playing! Goodbye!");
                        Environment.Exit(0);
                    }
                    else
                    {
                        PromptForAction(player);
                    }
                    break;

                case "7": // Attempt puzzle
                    if (hasPuzzle && !((SpecialRoom)currentRoom).PuzzleSolved)
                    {
                        SpecialRoom specialRoom = (SpecialRoom)currentRoom;
                        Console.WriteLine("Enter your puzzle attempt:");
                        string puzzleAttempt = Console.ReadLine();
                        bool puzzleSolved = specialRoom.AttemptPuzzle(player, puzzleAttempt);

                        if (puzzleSolved)
                        {
                            Console.WriteLine("You successfully completed the puzzle!");

                            // If monster present and puzzle solved, start combat
                            if (hasMonster)
                            {
                                Console.WriteLine($"The {currentRoom.Monster.Name} takes notice of you now that the puzzle is solved!");
                                Console.WriteLine("Prepare for combat!");
                                InitiateCombat(player, currentRoom.Monster);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("There is no unsolved puzzle in this room.");
                    }
                    PromptForAction(player);
                    break;

                case "8": // Engage monster.
                    if (hasMonster)
                    {
                        Console.WriteLine($"You approach the {currentRoom.Monster.Name} ready for battle!");
                        InitiateCombat(player, currentRoom.Monster);
                        PromptForAction(player);
                    }
                    else
                    {
                        Console.WriteLine("There is no monster to engage in this room.");
                        PromptForAction(player);
                    }
                    break;

                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    PromptForAction(player);
                    break;
            }
        }

        public void LoadGameMap()
        {
            // Create regular rooms
            Room entrance = new Room(1, "Entrance Hall", "A dimly lit hall with stone walls and a high ceiling. Torches flicker on the walls.");
            Room caveRoom = new Room(2, "Cave Chamber", "A natural cave with stalactites hanging from the ceiling. Water drips somewhere in the darkness.");

            // Special rooms with puzzles (replacing 3 of the existing rooms)
            RiddleRoom libraryRoom = new RiddleRoom(4, "Ancient Library",
                "Rows of dusty bookshelves filled with ancient tomes. A strange inscription is etched into the central lectern.");

            MemoryPuzzleRoom treasureRoom = new MemoryPuzzleRoom(3, "Memory Chamber",
                "A circular room with colored symbols glowing on the walls. In the center stands a pedestal with matching colored crystals.");

            ChessPuzzleRoom throneRoom = new ChessPuzzleRoom(7, "Throne Room",
                "An impressive chamber with a massive stone throne. A chess-like board is inlaid in the floor with strange pieces positioned on it.");

            // Set rewards for solving puzzles
            libraryRoom.SetReward(new Weapons("Tome of Power", "An ancient book radiating magical energy", 18));
            treasureRoom.SetReward(new Key("Gold Key", "A key made of pure gold", KeyType.Gold));
            throneRoom.SetReward(new Weapons("Royal Scepter", "A powerful royal scepter", 25));

            // Keep the remaining regular rooms
            Room dungeonRoom = new Room(5, "Dungeon Cell", "A grim prison cell with rusted bars and chains hanging from the walls.");
            Room gardenRoom = new Room(6, "Overgrown Garden", "What was once a beautiful garden is now overgrown with strange luminescent plants.");

            // Add rooms to map
            AddRoom(entrance);
            AddRoom(caveRoom);
            AddRoom(treasureRoom); // Puzzle room
            AddRoom(libraryRoom);  // Puzzle room
            AddRoom(dungeonRoom);
            AddRoom(gardenRoom);
            AddRoom(throneRoom);   // Puzzle room

            // Room connections
            entrance.ConnectRoom(2);
            entrance.ConnectRoom(4);

            caveRoom.ConnectRoom(1);
            caveRoom.ConnectRoom(3);
            caveRoom.ConnectLockedRoom(5, KeyType.Bronze);

            treasureRoom.ConnectRoom(2);

            libraryRoom.ConnectRoom(1);
            libraryRoom.ConnectLockedRoom(6, KeyType.Silver);

            dungeonRoom.ConnectRoom(2);

            gardenRoom.ConnectRoom(4);
            gardenRoom.ConnectLockedRoom(7, KeyType.Gold);

            throneRoom.ConnectRoom(6);

            // Add monsters to rooms
            caveRoom.SetMonster(new FrogMonster());
            dungeonRoom.SetMonster(new GnomeMonster());
            gardenRoom.SetMonster(new SeagullMonster());
            throneRoom.SetMonster(new BossMonster());

            // Add items to regular rooms
            entrance.AddItem(new Weapons("Rusty Dagger", "An old but still sharp dagger", 7));
            entrance.AddItem(new Potions("Small Health Potion", "A minor healing elixir", 15));

            caveRoom.AddItem(new Key("Bronze Key", "A key made of bronze, could open a simple lock", KeyType.Bronze));

            dungeonRoom.AddItem(new Weapons("Prisoner's Shiv", "A makeshift but deadly weapon", 9));

            gardenRoom.AddItem(new Potions("Nature's Essence", "A potion made from magical plants", 25));

            libraryRoom.AddItem(new Key("Silver Key", "A shimmering silver key", KeyType.Silver));
        }
    }
}
