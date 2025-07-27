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
    // Design decisions justification (Creature):
    // The "Creature" class is abstract as it will allow the class to be used for inheritance.
    // This is because the "abstract" modifier indicates that it is supposed to be a base class
    // for other classes.
    // The class also implements "IDamageable" which deals with consistent damage handling.
    // The class makes use of the keyword "Virtual" so the method can be overidden.
    // In addition to this, the class also makes use of the keyword "protected" to ensure that pieces of
    // code are only accessed by code in the current class (or a class derived from the current one).

    public abstract class Creature : IDamageable
    {
        protected string _name;
        protected int _health;
        protected int _maxHealth;
        protected int _attackPower;
        protected int _defense;
        protected bool _isAlive;

        public string Name { get => _name; protected set => _name = value; }
        public int MaxHealth { get => _maxHealth; protected set => _maxHealth = value; }
        public int CurrentHealth
        {
            get => _health;
            set
            {
                _health = Math.Max(0, Math.Min(value, _maxHealth));
                if (_health <= 0)
                    _isAlive = false;
            }
        }
        public int AttackPower { get => _attackPower; set => _attackPower = value; }
        public int Defense { get => _defense; protected set => _defense = value; }
        public bool IsAlive { get => _isAlive; protected set => _isAlive = value; }

        protected Creature(string name, int maxHealth, int attackPower, int defense)
        {
            _name = name;
            _maxHealth = maxHealth;
            _health = maxHealth;
            _attackPower = attackPower;
            _defense = defense;
            _isAlive = true;
        }

        public virtual void DamageTaken(int amount)
        {
            int actualDamageTaken = Math.Max(1, amount - _defense);
            CurrentHealth -= actualDamageTaken;
            Console.WriteLine($"{_name} has taken {actualDamageTaken} damage. Health: {_health}/{_maxHealth}");
        }

        public abstract void Attack(IDamageable target);

        public virtual void Heal(int amount)
        {
            if (!_isAlive)
            {
                Console.WriteLine($"{_name} is defeated and cannot be healed.");
                return;
            }

            int previousHealth = _health;
            CurrentHealth += amount;
            Console.WriteLine($"{_name} heals for {_health - previousHealth} points. Health: {_health}/{MaxHealth}");
        }

        protected bool IsValidTarget(IDamageable target)
        {
            if (target == null)
            {
                Console.WriteLine("There is no target to attack!");
                return false;
            }

            if (!target.IsAlive)
            {
                Console.WriteLine("The target is already defeated.");
                return false;
            }

            if (!_isAlive)
            {
                Console.WriteLine("You cannot attack while defeated.");
                return false;
            }

            return true;
        }

        public override string ToString() => $"{_name} - Health: {_health}/{_maxHealth}, Attack: {_attackPower}, Defense: {_defense}";
    }
}
