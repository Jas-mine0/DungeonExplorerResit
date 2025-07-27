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
    // Design decisions justification (Monster Class):
    // The code makes use of inheritance by using the base class "Creature".
    // This means that the "Monster" class inherits the same fields and methods that are in the
    // "Creature" base class.
    // I have used polymorphism within the class so each monster that inherits from the base class
    // "Monster" can do different things.
    // Each monster inherits from the base class "Monster" so that 
    // The "DecideBehaviour" allows the monsters to change their tactics.

    public abstract class Monster : Creature
    {
        protected string _description;
        protected List<Items> _loot;
        protected int _experienceValue;
        protected MonsterBehaviour _behaviour;

        public string Description => _description;
        public int ExperienceValue { get => _experienceValue; set => _experienceValue = value; }
        public MonsterBehaviour Behaviour => _behaviour;

        protected Monster(string name, string description, int maxHealth, int attackPower, int defense, int experienceValue)
            : base(name, maxHealth, attackPower, defense)
        {
            _description = description;
            _loot = new List<Items>();
            _experienceValue = experienceValue;
            _behaviour = MonsterBehaviour.Aggressive;
        }

        public override void Attack(IDamageable target)
        {
            if (!IsValidTarget(target)) return;

            Console.WriteLine($"{_name} attacks {target.Name} for {_attackPower} damage");
            target.DamageTaken(_attackPower);
        }

        public void AddLoot(Items item)
        {
            _loot.Add(item);
        }

        public List<Items> DropLoot()
        {
            if (_isAlive)
            {
                Console.WriteLine($"{_name} is still alive and won't drop loot.");
                return new List<Items>();
            }

            Console.WriteLine($"{_name} drops loot.");
            List<Items> droppedItems = new List<Items>(_loot);
            _loot.Clear();
            return droppedItems;
        }

        public virtual MonsterBehaviour DecideBehaviour()
        {
            double healthPercentage = (double)CurrentHealth / MaxHealth;

            if (healthPercentage < 0.2)
            {
                return MonsterBehaviour.Fleeing;
            }
            else if (healthPercentage < 0.5)
            {
                return MonsterBehaviour.Defensive;
            }
            else
            {
                return MonsterBehaviour.Aggressive;
            }
        }
    }

    public enum MonsterBehaviour
    {
        Aggressive,
        Defensive,
        Fleeing
    }

    public class FrogMonster : Monster
    {
        public FrogMonster()
            : base("Frog monster", "You have encountered an oversized frog.", 20, 5, 2, 10)
        {
            AddDefaultLoot();
        }

        private void AddDefaultLoot()
        {
            AddLoot(new Potions("Frog Potion", "A strange potion made from frog essence", 15));
        }

        public override void Attack(IDamageable target)
        {
            if (!IsValidTarget(target)) return;

            Console.WriteLine($"{_name} leaps at {target.Name} and attacks with its tongue for {_attackPower} damage");
            target.DamageTaken(_attackPower);
        }
    }

    public class GnomeMonster : Monster
    {
        public GnomeMonster()
            : base("Gnome", "You have encountered a mischievous gnome.", 25, 6, 4, 12)
        {
            AddDefaultLoot();
        }

        private void AddDefaultLoot()
        {
            AddLoot(new Weapons("Gardening Shovel", "A tiny but effective tool", 8));
        }

        public override void Attack(IDamageable target)
        {
            if (!IsValidTarget(target)) return;

            Console.WriteLine($"{_name} drops a plant pot on {target.Name}, dealing {_attackPower} damage");
            target.DamageTaken(_attackPower);
        }

        public override MonsterBehaviour DecideBehaviour()
        {
            double healthPercentage = (double)CurrentHealth / MaxHealth;

            if (healthPercentage < 0.4)
            {
                return MonsterBehaviour.Fleeing;
            }
            else
            {
                return MonsterBehaviour.Aggressive;
            }
        }
    }

    public class SeagullMonster : Monster
    {
        public SeagullMonster()
            : base("Seagull", "You have encountered an angry giant seagull.", 30, 7, 2, 13)
        {
            AddDefaultLoot();
        }

        private void AddDefaultLoot()
        {
            AddLoot(new Potions("Feather Essence", "A potion made from magical feathers", 18));
            AddLoot(new Key("Bronze Key", "A key the seagull had been carrying", KeyType.Bronze));
        }

        public override void Attack(IDamageable target)
        {
            if (!IsValidTarget(target)) return;

            Console.WriteLine($"{_name} pecks at {target.Name} aggressively, dealing {_attackPower} damage");
            target.DamageTaken(_attackPower);
        }
    }

    public class BossMonster : Monster
    {
        private bool _isActive;
        private int _specialAttackCooldown = 0;

        public bool IsActive { get => _isActive; set => _isActive = value; }

        public BossMonster()
            : base("Ancient Guardian", "A massive stone guardian awakens before you.", 75, 15, 10, 50)
        {
            _isActive = false;
            AddDefaultLoot();
        }

        private void AddDefaultLoot()
        {
            AddLoot(new Weapons("Guardian's Hammer", "An ancient weapon of immense power", 20));
            AddLoot(new Potions("Elixir of Life", "A legendary healing potion", 50));
            AddLoot(new Key("Crystal Key", "The key to the castle's treasure room", KeyType.Crystal));
        }

        public override void Attack(IDamageable target)
        {
            if (!_isActive || !IsValidTarget(target)) return;

            if (_specialAttackCooldown <= 0)
            {
                Console.WriteLine($"{_name} charges up and unleashes a devastating slam attack on {target.Name}, dealing {_attackPower * 2} damage.");
                target.DamageTaken(_attackPower * 2);
                _specialAttackCooldown = 3;
            }
            else
            {
                Console.WriteLine($"{_name} swings its massive fist at {target.Name}, dealing {_attackPower} damage");
                target.DamageTaken(_attackPower);
                _specialAttackCooldown--;
            }
        }

        public void Activate()
        {
            if (_isActive) return;

            _isActive = true;
            Console.WriteLine("The ancient guardian's eyes glow with ethereal light as it rises to defend its domain.");
        }

        public override MonsterBehaviour DecideBehaviour()
        {
            double healthPercentage = (double)CurrentHealth / MaxHealth;

            if (healthPercentage < 0.3)
            {
                Console.WriteLine($"{_name} enters a berserker rage as its health dwindles.");
            }

            return MonsterBehaviour.Aggressive;
        }
    }
}
