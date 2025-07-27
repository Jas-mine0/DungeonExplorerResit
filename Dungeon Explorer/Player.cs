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
    // Design decisions justification (Player Class):
    // This class deals with inventory management, allowing the player to add, use, or discard items
    // in their inventory.
    // The code also makes use of LINQ so the player can query their inventory items.

    public class Player : Creature
    {
        public string playerName;
        private List<Items> _inventory;
        private Weapons _equippedWeapon;
        private int _gold;
        private int _experience;
        private int _inventoryCapacity;

        public List<Items> Inventory => _inventory;
        public Weapons EquippedWeapon => _equippedWeapon;
        public int Gold { get => _gold; set => _gold = value; }
        public int Experience { get => _experience; set => _experience = value; }
        public bool SkipNextTurn { get; set; } = false;
        public int InventoryCapacity { get => _inventoryCapacity; set => _inventoryCapacity = Math.Max(1, value); }

        public Player(string name, int maxHealth, int attackPower, int defense)
                : base(name, maxHealth, attackPower, defense)
        {
            _inventory = new List<Items>();
            _gold = 0;
            _experience = 0;
            _inventoryCapacity = 10;
            GetPlayerName();
        }

        public Player(string name, int maxHealth, int attackPower, int defense, int inventoryCapacity)
                : this(name, maxHealth, attackPower, defense)
        {
            _inventoryCapacity = inventoryCapacity;
        }

        private void GetPlayerName()
        {
            while (true)
            {
                Console.WriteLine("Please enter your name: ");
                playerName = Console.ReadLine();
                if (string.IsNullOrEmpty(playerName))
                {
                    Console.WriteLine("You can't have an empty name. Please enter a name");
                }
                else
                {
                    Console.WriteLine($"Hello, {playerName}");
                    _name = playerName;
                    Console.WriteLine($"Your health is: {CurrentHealth}/{MaxHealth}");
                    break;
                }
            }
        }

        public override void Attack(IDamageable target)
        {
            if (!IsValidTarget(target)) return;

            int damageAmount = _attackPower;
            if (_equippedWeapon != null)
            {
                damageAmount += _equippedWeapon.Damage;
            }
            Console.WriteLine($"{_name} attacks with {(_equippedWeapon != null ? _equippedWeapon.Name : "bare hands")} for {damageAmount} damage");
            target.DamageTaken(damageAmount);
        }

        public void RoomSelection(GameMap gameMap)
        {
            List<int> availableRooms = gameMap.GetAvailableRooms();

            Console.WriteLine("Available rooms to explore:");
            foreach (int roomId in availableRooms)
            {
                Room room = gameMap.GetRoom(roomId);
                if (room != null)
                {
                    Console.WriteLine($"{roomId}. {room.Name}" + (gameMap.GetCurrentRoom().IsConnectionLocked(roomId) ? " (Locked)" : ""));
                }
            }

            Console.WriteLine("Pick a room to go into: (enter room number)");
            string input = Console.ReadLine();
            if (int.TryParse(input, out int roomNumber))
            {
                if (availableRooms.Contains(roomNumber))
                {
                    gameMap.MoveToAnotherRoom(roomNumber, this);
                }
                else
                {
                    Console.WriteLine("That room is not accessible from here.");
                }
            }
            else
            {
                Console.WriteLine("Please enter a valid room number.");
            }
        }

        public void EquipWeapon(Weapons weapon)
        {
            if (weapon == null)
            {
                Console.WriteLine("No weapon to equip.");
                return;
            }

            _equippedWeapon = weapon;
            Console.WriteLine($"{_name} equipped {weapon.Name}");
        }

        public void EquipStrongestWeapon()
        {
            var strongestWeapon = _inventory
                .Where(item => item is Weapons)
                .Cast<Weapons>()
                .OrderByDescending(w => w.Damage)
                .FirstOrDefault();

            if (strongestWeapon != null)
            {
                EquipWeapon(strongestWeapon);
            }
            else
            {
                Console.WriteLine("You don't have any weapons to equip.");
            }
        }

        public bool AddToInventory(Items item)
        {
            if (_inventory.Count >= _inventoryCapacity)
            {
                Console.WriteLine($"Your inventory is full. (Limit: {_inventoryCapacity} items)");
                Console.WriteLine("Would you like to discard an item to make space? (y/n)");
                string response = Console.ReadLine().ToLower();

                if (response == "y" || response == "yes")
                {
                    if (DiscardItem())
                    {
                        _inventory.Add(item);
                        Console.WriteLine($"{item.Name} added to inventory.");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("No item discarded. Cannot add new item.");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("Item not added to inventory.");
                    return false;
                }
            }

            _inventory.Add(item);
            Console.WriteLine($"{item.Name} added to inventory.");
            return true;
        }

        private bool DiscardItem()
        {
            Console.WriteLine("Select an item to discard:");
            ShowInventory();

            Console.WriteLine("Enter the number of the item to discard (0 to cancel):");
            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                if (choice == 0)
                {
                    return false;
                }

                if (choice > 0 && choice <= _inventory.Count)
                {
                    Items itemToRemove = _inventory[choice - 1];
                    _inventory.RemoveAt(choice - 1);
                    Console.WriteLine($"Discarded {itemToRemove.Name}.");
                    return true;
                }
                else
                {
                    Console.WriteLine("Invalid item number.");
                    return false;
                }
            }
            else
            {
                Console.WriteLine("Invalid input. No item discarded.");
                return false;
            }
        }

        public void UsePotion(int inventoryIndex)
        {
            if (inventoryIndex < 0 || inventoryIndex >= _inventory.Count)
            {
                Console.WriteLine("Invalid inventory index.");
                return;
            }

            Items item = _inventory[inventoryIndex];
            if (item is Potions potion)
            {
                potion.Use(this);
                _inventory.RemoveAt(inventoryIndex);
            }
            else
            {
                Console.WriteLine("That item is not a potion!");
            }
        }

        public bool UseItem(int inventoryIndex)
        {
            if (inventoryIndex < 0 || inventoryIndex >= _inventory.Count)
            {
                Console.WriteLine("Invalid inventory index.");
                return false;
            }

            Items item = _inventory[inventoryIndex];
            if (item is IUsable usableItem)
            {
                bool result = usableItem.Use(this);
                if (result && usableItem.IsConsumed)
                {
                    _inventory.RemoveAt(inventoryIndex);
                }
                return result;
            }
            else if (item is Weapons weapon)
            {
                EquipWeapon(weapon);
                return true;
            }
            else
            {
                Console.WriteLine($"You can't use {item.Name} in this way.");
                return false;
            }
        }

        public void CollectItem(ICollectable item)
        {
            if (item == null)
            {
                Console.WriteLine("There is nothing to collect.");
                return;
            }

            if (item.CanBeCollected)
            {
                item.OnCollection(this);
            }
            else
            {
                Console.WriteLine($"You cannot collect {item.Name} at this time.");
            }
        }

        public void ShowInventory(Func<Items, bool> filter = null, Comparison<Items> sortComparison = null)
        {
            if (_inventory.Count == 0)
            {
                Console.WriteLine("Your inventory is empty");
                return;
            }

            List<Items> itemsToShow = new List<Items>(_inventory);

            if (filter != null)
            {
                itemsToShow = itemsToShow.Where(filter).ToList();
                if (itemsToShow.Count == 0)
                {
                    Console.WriteLine("No items match the filter criteria.");
                    return;
                }
            }

            if (sortComparison != null)
            {
                itemsToShow.Sort(sortComparison);
            }

            Console.WriteLine($"Inventory ({itemsToShow.Count}/{_inventoryCapacity} slots used):");
            for (int i = 0; i < itemsToShow.Count; i++)
            {
                DisplayItemDetails(i + 1, itemsToShow[i]);
            }
        }

        private void DisplayItemDetails(int index, Items item)
        {
            Console.WriteLine($"{index}. {item.Name} - {item.Description}");

            if (item is Potions potion)
            {
                Console.WriteLine($"   Healing: +{potion.HealAmount} HP");
            }
            else if (item is Weapons weapon)
            {
                Console.WriteLine($"   Damage: +{weapon.Damage}" + (weapon == _equippedWeapon ? " (Equipped)" : ""));
            }
            else if (item is Key key)
            {
                Console.WriteLine($"   Type: {key.KeyType} Key");
            }
        }

        public List<Potions> GetAllHealingItems()
        {
            return _inventory
                .Where(item => item is Potions)
                .Cast<Potions>()
                .OrderByDescending(p => p.HealAmount)
                .ToList();
        }

        public bool HasKeyOfType(KeyType keyType)
        {
            return _inventory.Any(item => item is Key key && key.KeyType == keyType);
        }

        public void UseKey(KeyType keyType)
        {
            for (int i = 0; i < _inventory.Count; i++)
            {
                if (_inventory[i] is Key key && key.KeyType == keyType)
                {
                    Console.WriteLine($"Used {key.Name} to unlock the door.");
                    _inventory.RemoveAt(i);
                    return;
                }
            }
        }

        public void DisplayExperience()
        {
            int level = Experience / 50 + 1;
            int expToNextLevel = (level * 50) - Experience;

            Console.WriteLine($"Experience: {Experience} (Level {level})");
            Console.WriteLine($"Experience needed for next level: {expToNextLevel}");
        }
    }

    public interface IUsable
    {
        bool Use(Creature target);
        bool IsConsumed { get; }
    }
}
