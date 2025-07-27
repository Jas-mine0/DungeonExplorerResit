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
    public class Statistics
    {
        private Player _player;
        private int _monstersDefeated;
        private int _roomsVisited;
        private int _itemsCollected;
        private DateTime _startTime;
        private int _score;

        public Statistics()
        {
            _monstersDefeated = 0;
            _roomsVisited = 0;
            _itemsCollected = 0;
            _startTime = DateTime.Now;
            _score = 0;
        }

        public void TrackPlayer(Player player) => _player = player;

        public void MonsterDefeated(int experienceValue)
        {
            _monstersDefeated++;
            UpdateScore(experienceValue * 10);
        }

        public void RoomVisited()
        {
            _roomsVisited++;
            UpdateScore(5);
        }

        public void ItemCollected()
        {
            _itemsCollected++;
            UpdateScore(2);
        }

        private void UpdateScore(int points) => _score += points;

        public int GetLevel() => (_player?.Experience ?? 0) / 50 + 1;

        public void DisplayStatistics()
        {
            TimeSpan playTime = DateTime.Now - _startTime;

            Console.WriteLine("==================================");
            Console.WriteLine("         GAME STATISTICS          ");
            Console.WriteLine("==================================");
            Console.WriteLine($"Player Level: {GetLevel()}");
            Console.WriteLine($"Monsters Defeated: {_monstersDefeated}");
            Console.WriteLine($"Rooms Visited: {_roomsVisited}");
            Console.WriteLine($"Items Collected: {_itemsCollected}");
            Console.WriteLine($"Play Time: {playTime.Hours:D2}:{playTime.Minutes:D2}:{playTime.Seconds:D2}");
            Console.WriteLine($"Score: {_score}");
            Console.WriteLine("==================================");
        }
    }
}
