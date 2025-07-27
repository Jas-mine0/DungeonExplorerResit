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
    // Design decisions justification (Room Base Class):
    // This class keeps track of the ID, descriptions, items, monsters, connected rooms, and the required
    // keys.
    // In addition to keeping track of whether the room has been visted before and the name of the room.
    // Furthermore, it tells the player if a room is locked and if they can pick up an item or not.
    // The class also makes use of encapsulation, virtual methods, and public/private access modifiers.
    // The use of virtual methods allowing it to be overriden if it needs to be overriden.

    public class Room
    {
        private int _id;
        private string _description;
        private List<Items> _items;
        private Monster _monster;
        private Dictionary<int, bool> _connectedRooms;
        private Dictionary<int, KeyType> _requiredKeys;
        private bool _hasBeenVisited;
        private string _name;

        public int ID => _id;
        public string Description => _description;
        public List<Items> Items => _items;
        public Monster Monster => _monster;
        public bool HasBeenVisited { get => _hasBeenVisited; set => _hasBeenVisited = value; }
        public string Name => _name;

        public Room(int id, string name, string description)
        {
            _id = id;
            _name = name;
            _description = description;
            _items = new List<Items>();
            _monster = null;
            _connectedRooms = new Dictionary<int, bool>();
            _requiredKeys = new Dictionary<int, KeyType>();
            _hasBeenVisited = false;
        }

        public void SetMonster(Monster monster)
        {
            _monster = monster;
        }

        public void AddItem(Items item)
        {
            _items.Add(item);
        }

        public void ConnectRoom(int roomId)
        {
            _connectedRooms[roomId] = false;
        }

        public void ConnectLockedRoom(int roomId, KeyType keyType)
        {
            _connectedRooms[roomId] = true;
            _requiredKeys[roomId] = keyType;
        }

        public List<int> GetConnectedRooms()
        {
            return _connectedRooms.Keys.ToList();
        }

        public bool IsConnectionLocked(int roomId)
        {
            return _connectedRooms.ContainsKey(roomId) && _connectedRooms[roomId];
        }

        public KeyType GetRequiredKeyType(int roomId)
        {
            return _requiredKeys.ContainsKey(roomId) ? _requiredKeys[roomId] : KeyType.Bronze;
        }

        public virtual void DisplayRoom()
        {
            Console.WriteLine($"==== {_name} ====");
            Console.WriteLine(_description);

            if (_monster != null && _monster.IsAlive)
            {
                Console.WriteLine($"\nMonster: {_monster.Name} - {_monster.Description}");
            }

            DisplayItems();

            Console.WriteLine("\nExits:");
            foreach (var roomId in _connectedRooms.Keys)
            {
                Console.Write($"Room {roomId}" + (_connectedRooms[roomId] ? " (Locked)" : ""));
                Console.WriteLine();
            }
        }

        public void DisplayItems()
        {
            if (_items.Count > 0)
            {
                Console.WriteLine("\nItems in this room:");
                for (int i = 0; i < _items.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {_items[i].Name} - {_items[i].Description}");
                }
            }
            else
            {
                Console.WriteLine("\nThere are no items in this room.");
            }
        }

        public void CollectItem(int index, Player player)
        {
            if (_items.Count == 0)
            {
                Console.WriteLine("There are no items to collect in this room.");
                return;
            }

            if (index < 0 || index >= _items.Count)
            {
                Console.WriteLine("Invalid item selection.");
                return;
            }

            Items item = _items[index];
            if (player.AddToInventory(item))
            {
                _items.RemoveAt(index);
            }
        }

        public void GenerateRandomEncounter()
        {
            if (_monster != null && _monster.IsAlive) return;

            Random random = new Random();
            int encounterChance = random.Next(100);

            if (encounterChance < 30)
            {
                int monsterType = random.Next(5);

                switch (monsterType)
                {
                    case 0:
                        _monster = new FrogMonster();
                        break;
                    case 1:
                        _monster = new GnomeMonster();
                        break;
                    case 2:
                        _monster = new SeagullMonster();
                        break;
                    default:
                        _monster = new FrogMonster();
                        break;
                }

                Console.WriteLine($"As you explore the room, a {_monster.Name} appears.");
            }
        }
    }

    public abstract class SpecialRoom : Room
    {
        protected bool _puzzleSolved;
        protected Items _rewardItem;

        public bool PuzzleSolved => _puzzleSolved;

        public SpecialRoom(int id, string name, string description)
            : base(id, name, description)
        {
            _puzzleSolved = false;
        }

        public void SetReward(Items item)
        {
            _rewardItem = item;
        }

        protected void OnPuzzleSolved(Player player)
        {
            if (_puzzleSolved) return;

            _puzzleSolved = true;
            Console.WriteLine("Congratulations! You solved the puzzle.");

            if (_rewardItem != null)
            {
                Console.WriteLine($"You found a {_rewardItem.Name}!");
                player.AddToInventory(_rewardItem);
            }
        }

        public abstract bool AttemptPuzzle(Player player, string attempt);

        public override void DisplayRoom()
        {
            base.DisplayRoom();

            if (_puzzleSolved)
            {
                Console.WriteLine("\nPuzzle Status: SOLVED");
            }
            else
            {
                Console.WriteLine("\nPuzzle Status: UNSOLVED");
                DisplayPuzzle();
            }
        }

        protected abstract void DisplayPuzzle();
    }

    public class RiddleRoom : SpecialRoom
    {
        private Dictionary<string, string> _riddles;
        private string _currentRiddle;
        private string _currentAnswer;
        private int _attempts;

        public RiddleRoom(int id, string name, string description)
            : base(id, name, description)
        {
            _attempts = 0;
            InitializeRiddles();
            SelectRandomRiddle();
        }

        private void InitializeRiddles()
        {
            _riddles = new Dictionary<string, string>
            {
                { "I speak without a mouth and hear without ears. I have no body, but I come alive with wind. What am I?", "echo" },
                { "The more you take, the more you leave behind. What am I?", "footsteps" },
                { "What has keys but no locks, space but no room, and you can enter but not go in?", "keyboard" },
                { "What has a head, a tail, is brown, and has no legs?", "penny" },
                { "What goes up but never comes down?", "age" }
            };
        }

        private void SelectRandomRiddle()
        {
            List<string> riddleList = new List<string>(_riddles.Keys);
            Random random = new Random();
            int index = random.Next(riddleList.Count);
            _currentRiddle = riddleList[index];
            _currentAnswer = _riddles[_currentRiddle];
        }

        protected override void DisplayPuzzle()
        {
            Console.WriteLine("\n=== THE RIDDLE CHALLENGE ===");
            Console.WriteLine("Solve this riddle to progress:");
            Console.WriteLine($"Riddle: {_currentRiddle}");
            Console.WriteLine($"Attempts made: {_attempts}/3");
            Console.WriteLine("\nTo attempt an answer, type the answer");
        }

        public override bool AttemptPuzzle(Player player, string attempt)
        {
            if (_puzzleSolved)
            {
                Console.WriteLine("This puzzle has already been solved.");
                return true;
            }

            attempt = attempt.ToLower().Trim();
            _attempts++;

            if (attempt == _currentAnswer)
            {
                OnPuzzleSolved(player);
                return true;
            }
            else
            {
                Console.WriteLine("That's not the correct answer.");

                if (_attempts >= 3)
                {
                    Console.WriteLine("You've used all your attempts. Here's a hint:");
                    Console.WriteLine($"The first letter is '{_currentAnswer[0]}' and it has {_currentAnswer.Length} letters.");
                }
                else
                {
                    Console.WriteLine($"You have {3 - _attempts} attempts remaining.");
                }

                return false;
            }
        }
    }

    public class MemoryPuzzleRoom : SpecialRoom
    {
        private List<string> _sequence;
        private int _currentLevel;
        private readonly int _maxLevel = 3;

        public MemoryPuzzleRoom(int id, string name, string description)
            : base(id, name, description)
        {
            _sequence = new List<string>();
            _currentLevel = 1;
            GenerateSequence();
        }

        private void GenerateSequence()
        {
            _sequence.Clear();
            Random random = new Random();
            string[] colors = { "Red", "Blue", "Green", "Yellow" };

            for (int i = 0; i < 2 + _currentLevel; i++)
            {
                _sequence.Add(colors[random.Next(colors.Length)]);
            }
        }

        protected override void DisplayPuzzle()
        {
            Console.WriteLine("\n=== THE MEMORY CHALLENGE ===");
            Console.WriteLine($"Level {_currentLevel} of {_maxLevel}");
            Console.WriteLine("Memorize the sequence of colors that will be shown.");
            Console.WriteLine("Type 'start' to begin the sequence display, then repeat the sequence when prompted.");
            Console.WriteLine("To answer, type the colors separated by spaces (e.g., 'Red Blue Green')");
        }

        public void DisplaySequence()
        {
            Console.WriteLine("\nWatch carefully...");
            Thread.Sleep(1000);

            foreach (string color in _sequence)
            {
                Console.Clear();
                Console.WriteLine($"=== {color.ToUpper()} ===");
                Thread.Sleep(1000);
            }

            Console.Clear();
            Console.WriteLine("Now repeat the sequence:");
        }

        public override bool AttemptPuzzle(Player player, string attempt)
        {
            if (_puzzleSolved)
            {
                Console.WriteLine("This puzzle has already been solved.");
                return true;
            }

            if (attempt.ToLower() == "start")
            {
                DisplaySequence();
                return false;
            }

            string[] inputColors = attempt.Split(' ');

            if (inputColors.Length != _sequence.Count)
            {
                Console.WriteLine("The number of colors you entered doesn't match the sequence length.");
                return false;
            }

            bool correct = true;
            for (int i = 0; i < _sequence.Count; i++)
            {
                if (inputColors[i].ToLower() != _sequence[i].ToLower())
                {
                    correct = false;
                    break;
                }
            }

            if (correct)
            {
                Console.WriteLine("That's correct!");

                if (_currentLevel >= _maxLevel)
                {
                    OnPuzzleSolved(player);
                    return true;
                }
                else
                {
                    _currentLevel++;
                    Console.WriteLine($"Moving to level {_currentLevel}...");
                    GenerateSequence();
                    return false;
                }
            }
            else
            {
                Console.WriteLine("That's not correct. The sequence was:");
                Console.WriteLine(string.Join(" ", _sequence));
                Console.WriteLine("Let's try again with a new sequence.");
                GenerateSequence();
                return false;
            }
        }
    }

    public class ChessPuzzleRoom : SpecialRoom
    {
        private char[,] _board;
        private int _movesRemaining;
        private List<string> _validMoves;

        public ChessPuzzleRoom(int id, string name, string description)
            : base(id, name, description)
        {
            InitializeBoard();
            _movesRemaining = 3;
            _validMoves = GetValidMoves();
        }

        private void InitializeBoard()
        {
            _board = new char[5, 5]
            {
                { ' ', ' ', ' ', ' ', ' ' },
                { ' ', ' ', 'P', ' ', ' ' }, // P = player pawn
                { ' ', 'R', ' ', 'B', ' ' }, // R = rook, B = bishop
                { ' ', ' ', 'K', ' ', ' ' }, // K = king
                { ' ', ' ', 'X', ' ', ' ' }  // X = target position
            };
        }

        private List<string> GetValidMoves()
        {
            return new List<string>
            {
                "Kc2", // Move king to c2
                "Pb3", // Move pawn to b3
                "Rd1"  // Move rook to d1
            };
        }

        protected override void DisplayPuzzle()
        {
            Console.WriteLine("\n=== THE CHESS PUZZLE ===");
            Console.WriteLine("Move the pieces to checkmate the opponent's king in exactly 3 moves.");
            Console.WriteLine("The board is represented as follows:");
            Console.WriteLine("P = Pawn, R = Rook, B = Bishop, K = King, X = Target");
            Console.WriteLine("Use algebraic notation to move pieces: [Piece][destination]");
            Console.WriteLine("Example: 'Kc2' to move King to position c2");
            Console.WriteLine($"Moves remaining: {_movesRemaining}");

            Console.WriteLine("\n  a b c d e");
            Console.WriteLine("  ─────────");
            for (int i = 0; i < 5; i++)
            {
                Console.Write($"{5 - i}│");
                for (int j = 0; j < 5; j++)
                {
                    Console.Write(_board[i, j] + " ");
                }
                Console.WriteLine();
            }
        }

        public override bool AttemptPuzzle(Player player, string attempt)
        {
            if (_puzzleSolved)
            {
                Console.WriteLine("This puzzle has already been solved.");
                return true;
            }

            attempt = attempt.Trim();

            if (_validMoves.Contains(attempt))
            {
                char piece = attempt[0];
                string destination = attempt.Substring(1);

                Console.WriteLine($"You moved {GetPieceName(piece)} to {destination}.");
                _validMoves.Remove(attempt);
                _movesRemaining--;

                if (_movesRemaining == 0 || _validMoves.Count == 0)
                {
                    OnPuzzleSolved(player);
                    return true;
                }
                else
                {
                    Console.WriteLine($"That move is correct. {_movesRemaining} moves remaining.");
                    return false;
                }
            }
            else
            {
                Console.WriteLine("That's not a valid move for this puzzle.");

                if (_movesRemaining == 1)
                {
                    Console.WriteLine("Hint: The final move involves the Rook.");
                }

                return false;
            }
        }

        private string GetPieceName(char pieceSymbol)
        {
            switch (pieceSymbol)
            {
                case 'P': return "Pawn";
                case 'R': return "Rook";
                case 'B': return "Bishop";
                case 'K': return "King";
                default: return "Unknown Piece";
            }
        }
    }
}
