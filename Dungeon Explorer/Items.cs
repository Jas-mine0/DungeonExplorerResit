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
    // Design decisions justification (Items class):
    // This class uses the "ICollectable" interface.
    // The use of an interface instead of inheritance is because it is an "is a" relationship.
    // In addition to this, using an interface instead of inheritance allows me to use more than one
    // interface if I need to whereas if I used inheritance then I would only be able to inherit from
    // one class.
    // "ICollectable" allows the player to collect items consistently.

    public abstract class Items : ICollectable
    {
        protected string _name;
        protected string _description;
        protected bool _canBeCollected = true;

        public string Name { get => _name; protected set => _name = value; }
        public string Description { get => _description; protected set => _description = value; }
        public bool CanBeCollected { get => _canBeCollected; protected set => _canBeCollected = value; }

        protected Items(string name, string description)
        {
            _name = name;
            _description = description;
        }

        public abstract void OnCollection(Player collector);

        public override string ToString() => $"{_name}: {_description}";
    }

    public class Weapons : Items
    {
        private int _damage;
        public int Damage => _damage;

        public Weapons(string name, string description, int damage) : base(name, description)
        {
            _damage = damage;
        }

        public override void OnCollection(Player collector)
        {
            Console.WriteLine($"{collector.Name} collected {_name}");
            collector.AddToInventory(this);
        }
    }

    public class Potions : Items, IUsable
    {
        private int _healAmount;
        public int HealAmount => _healAmount;
        public bool IsConsumed => true;

        public Potions(string name, string description, int healAmount) : base(name, description)
        {
            _healAmount = healAmount;
        }

        public override void OnCollection(Player collector)
        {
            Console.WriteLine($"{collector.Name} collected {_name}");
            collector.AddToInventory(this);
        }

        public bool Use(Creature target)
        {
            Console.WriteLine($"Using {_name} on {target.Name}");
            target.Heal(_healAmount);
            _canBeCollected = false;
            return true;
        }
    }

    public enum KeyType
    {
        Bronze,
        Silver,
        Gold,
        Crystal
    }

    public class Key : Items, IUsable
    {
        private KeyType _keyType;
        public KeyType KeyType => _keyType;
        public bool IsConsumed => true;

        public Key(string name, string description, KeyType keyType) : base(name, description)
        {
            _keyType = keyType;
        }

        public override void OnCollection(Player collector)
        {
            Console.WriteLine($"{collector.Name} collected {_name}");
            collector.AddToInventory(this);
        }

        public bool Use(Creature target)
        {
            if (target is Player player)
            {
                Console.WriteLine($"{player.Name} tries to use {_name} but there's no lock here.");
                return false;
            }
            return false;
        }
    }

    public interface IDamageable
    {
        void DamageTaken(int amount);
        bool IsAlive { get; }
        int CurrentHealth { get; }
        string Name { get; }
    }

    public interface ICollectable
    {
        string Name { get; }
        string Description { get; }
        bool CanBeCollected { get; }
        void OnCollection(Player collector);
    }
}
