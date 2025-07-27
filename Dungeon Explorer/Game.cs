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

    // Design decisions justification (Game class):
    // This is the main entry point for the game, it initalises the game and provides the player with
    // a welcome message.
    // In addition to the above it also deals with showing the player the game menu.
    // I added the game menu so the player has a welcome message to the game.

    public class Game
    {
        public static void Main(string[] args)
        {
            GameInitializer initializer = new GameInitializer();
            Player player = initializer.CreatePlayer();
            GameMap gameMap = initializer.CreateGameMap();
            Statistics stats = new Statistics();

            stats.TrackPlayer(player);

            Console.WriteLine("=================================");
            Console.WriteLine("       DUNGEON EXPLORER    ");
            Console.WriteLine("=================================");
            Console.WriteLine("Press any key to start your adventure.");
            Console.ReadKey();

            GameMenu gameMenu = new GameMenu(player, gameMap, stats);
            gameMenu.ShowMenu();
        }
    }

    // Design decisions justification:
    // This class encapsulates the logic for the game components.
    // I have decided to give the player some starting items in the game
    // so they can start getting used to playing the game.
    // The code has been put in this class so the class above does not get too cluttered.

    public class GameInitializer
    {
        public Player CreatePlayer()
        {
            Player player = new Player("Default", 100, 15, 8);

            player.AddToInventory(new Weapons("Rusty Sword", "An old but reliable blade", 8));
            player.AddToInventory(new Potions("Health Potion", "Restores 25 health points", 25));
            player.AddToInventory(new Key("Bronze Key", "Opens a common lock", KeyType.Bronze));

            return player;
        }

        public GameMap CreateGameMap()
        {
            GameMap gameMap = new GameMap();
            gameMap.LoadGameMap();
            return gameMap;
        }
    }
}
