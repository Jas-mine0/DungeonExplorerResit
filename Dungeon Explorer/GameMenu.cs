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
    // Design decisions justification (GameMenu class):
    // This has been added so the player has a way of interacting with the game.
    // In addition to this, it keeps the code in one place, making it more organised.

    public class GameMenu
    {
        private Player _player;
        private GameMap _gameMap;
        private Statistics _stats;

        public GameMenu(Player player, GameMap gameMap, Statistics stats = null)
        {
            _player = player;
            _gameMap = gameMap;
            _stats = stats;

            if (_stats != null)
            {
                _gameMap.SetStatistics(_stats);
            }
        }

        public void ShowMenu()
        {
            bool exitGame = false;

            while (!exitGame)
            {
                Console.Clear();
                Console.WriteLine("==================");
                Console.WriteLine("    GAME MENU     ");
                Console.WriteLine("==================");
                Console.WriteLine("1. Start Adventure");
                Console.WriteLine("2. View Player Stats");
                Console.WriteLine("3. View Game Statistics");
                Console.WriteLine("4. Save Game");
                Console.WriteLine("5. Load Game");
                Console.WriteLine("6. Exit Game");
                Console.WriteLine("==================");
                Console.WriteLine("Choose an option:");

                string input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        Console.Clear();
                        Room currentRoom = _gameMap.GetCurrentRoom();
                        if (currentRoom != null)
                        {
                            currentRoom.DisplayRoom();

                            Console.WriteLine("\nAvailable Exits:");
                            List<int> availableRooms = _gameMap.GetAvailableRooms();
                            foreach (int roomId in availableRooms)
                            {
                                Room room = _gameMap.GetRoom(roomId);
                                if (room != null)
                                {
                                    Console.WriteLine($"{roomId}. {room.Name}" +
                                        (currentRoom.IsConnectionLocked(roomId) ? " (Locked)" : ""));
                                }
                            }

                            Console.WriteLine("\nWhich room would you like to enter? (Enter number)");
                            string roomChoice = Console.ReadLine();
                            if (int.TryParse(roomChoice, out int roomNumber) &&
                                availableRooms.Contains(roomNumber))
                            {
                                _gameMap.MoveToAnotherRoom(roomNumber, _player);
                            }
                            else
                            {
                                Console.WriteLine("Invalid room selection.");
                                WaitForKey();
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error: Could not find current room. Game map may not be initialized properly.");
                            WaitForKey();
                        }
                        break;

                    case "2":
                        Console.Clear();
                        ShowPlayerStatistics();
                        WaitForKey();
                        break;

                    case "3":
                        Console.Clear();
                        if (_stats != null)
                        {
                            _stats.DisplayStatistics();
                        }
                        else
                        {
                            Console.WriteLine("Game statistics are not available.");
                        }
                        WaitForKey();
                        break;

                    case "4":
                        SaveGame();
                        break;

                    case "5":
                        LoadGame();
                        break;

                    case "6":
                        Console.WriteLine("Are you sure you want to exit? (y/n)");
                        if (Console.ReadLine().ToLower().StartsWith("y"))
                        {
                            exitGame = true;
                            Console.WriteLine("Thanks for playing! Goodbye!");
                        }
                        break;

                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        WaitForKey();
                        break;
                }
            }
        }

        private void ShowPlayerStatistics()
        {
            Console.WriteLine("================================");
            Console.WriteLine("     PLAYER STATISTICS         ");
            Console.WriteLine("================================");
            Console.WriteLine($"Name: {_player.Name}");
            Console.WriteLine($"Health: {_player.CurrentHealth}/{_player.MaxHealth}");
            Console.WriteLine($"Attack Power: {_player.AttackPower}");
            Console.WriteLine($"Defense: {_player.Defense}");
            Console.WriteLine($"Gold: {_player.Gold}");
            _player.DisplayExperience();
            Console.WriteLine("================================");

            if (_player.EquippedWeapon != null)
            {
                Console.WriteLine($"Equipped Weapon: {_player.EquippedWeapon.Name} (+{_player.EquippedWeapon.Damage} damage)");
            }
            else
            {
                Console.WriteLine("No weapon equipped");
            }

            Console.WriteLine("\nWould you like to see your inventory? (y/n)");
            if (Console.ReadLine().ToLower().StartsWith("y"))
            {
                Console.Clear();
                _player.ShowInventory();
            }
        }

        private void SaveGame()
        {
            try
            {
                Console.WriteLine("Saving game...");

                var saveData = new Dictionary<string, string>
                {
                    {"PlayerName", _player.Name},
                    {"PlayerHealth", _player.CurrentHealth.ToString()},
                    {"PlayerMaxHealth", _player.MaxHealth.ToString()},
                    {"PlayerAttack", _player.AttackPower.ToString()},
                    {"PlayerDefense", _player.Defense.ToString()},
                    {"PlayerExp", _player.Experience.ToString()},
                    {"CurrentRoom", _gameMap.GetCurrentRoom().ID.ToString()}
                };

                using (StreamWriter writer = new StreamWriter("savegame.txt"))
                {
                    foreach (var item in saveData)
                    {
                        writer.WriteLine($"{item.Key}={item.Value}");
                    }
                }

                Console.WriteLine("Game saved successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save game: {ex.Message}");
            }

            WaitForKey();
        }

        private void LoadGame()
        {
            try
            {
                Console.WriteLine("Loading game...");

                if (!File.Exists("savegame.txt"))
                {
                    Console.WriteLine("No saved game found.");
                    WaitForKey();
                    return;
                }

                var saveData = new Dictionary<string, string>();

                using (StreamReader reader = new StreamReader("savegame.txt"))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            saveData[parts[0]] = parts[1];
                        }
                    }
                }

                if (saveData.ContainsKey("PlayerName"))
                {
                    Console.WriteLine($"Loaded save data for player: {saveData["PlayerName"]}");

                    if (saveData.ContainsKey("CurrentRoom"))
                    {
                        int roomId = int.Parse(saveData["CurrentRoom"]);
                        Console.WriteLine($"Restoring position to room {roomId}");
                    }
                }

                Console.WriteLine("Game loaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load game: {ex.Message}");
            }

            WaitForKey();
        }

        private void WaitForKey()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
