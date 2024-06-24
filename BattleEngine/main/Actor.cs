﻿using System.IO;
using System.Text.Json;
using System.Xml.Linq;
using static BattleEngine.common.Global;
using static BattleEngine.main.Move;

namespace BattleEngine.main
{
    public record Actor
    {
        public static int TotalActors { get; private set; }

        public int ID { get; private set; }
        public string InternalName { get; private set; }
        public string DisplayName { get; set; }

        private double _maxhp;
        public double MaxHealth
        {
            get { return _maxhp; }
            private set{
                if (value <= 1){
                    _maxhp = 1;
                }else if (value > (double)Values.HEALTHCAP){
                    _maxhp = (double)Values.HEALTHCAP;
                }else{
                    _maxhp = value;
                }
            }
        }

        private double _health;
        public double Health
        {
            get { return _health; }
            private set
            {
                if (value < 0)
                {
                    _health = 0;
                }
                else
                {
                    _health = value;
                }
            }
        }

        private double _mitigationvalue;
        public double MitigationValue
        {
            get { return _mitigationvalue; }
            private set
            {
                if (_mitigationvalue < 0)
                {
                    _mitigationvalue = 0;
                }
                else if (_mitigationvalue > MaxHealth)
                {
                    _mitigationvalue = MaxHealth;
                }
                else
                {
                    _mitigationvalue = value;
                }
            }
        }

        private int _level;
        public int Level {
            get { return _level; }
            private set
            {
                if (value <= 1)
                {
                    _level = 1;
                }
                else if (value > (int)Values.LEVELCAP)
                {
                    _level = (int)Values.LEVELCAP;
                }
                else
                {
                    _level = value;
                }
            }
        }

        public Dictionary<string, int> Attributes { get; private set; }
        public List<Move> MoveSet { get; set; }

        static Actor()
        {
            TotalActors = 0;
        }

        public Actor()
        {
            ID = TotalActors;
            TotalActors += 1;

            InternalName = $"Actor {ID}";
            DisplayName = "Placeholder";

            Level = 5;

            MaxHealth = 100 * Level;
            Health = MaxHealth;

            MitigationValue = 0;
            IsHurt += Mitigate;

            Attributes = new Dictionary<string, int>
            {
                ["ATK"] = Level,
                ["VIT"] = Level,
                ["INT"] = Level,
                ["DEF"] = Level,
                ["DGE"] = Level
            };

            MoveSet = new List<Move>() { new Move() };
        }
        public Actor(string internalname, string displayname, int lvl, Move[] moves)
        {
            ID = TotalActors;
            TotalActors += 1;

            InternalName = internalname;
            DisplayName = displayname;

            Level = lvl;

            MaxHealth = 100 * Level;
            Health = MaxHealth;

            MitigationValue = 0;
            IsHurt += Mitigate;

            Attributes = new Dictionary<string, int>
            {
                ["ATK"] = Level,
                ["VIT"] = Level,
                ["INT"] = Level,
                ["DEF"] = Level,
                ["DGE"] = Level
            };

            MoveSet = new List<Move>();
            MoveSet.AddRange(moves);
        }
        public Actor(string internalname, string displayname, int lvl, Dictionary<string, int> attributes, Move[] moves)
        {
            ID = TotalActors;
            TotalActors += 1;

            InternalName = internalname;
            DisplayName = displayname;

            Level = lvl;

            MaxHealth = 100 * Level;
            Health = MaxHealth;

            MitigationValue = 0;
            IsHurt += Mitigate;

            Attributes = new Dictionary<string, int>(attributes);

            MoveSet = new List<Move>();
            MoveSet.AddRange(moves);
        }
        public Actor(string internalfile)
        {
            string JsonString = File.ReadAllText($@"{User.ActorPath}\{internalfile}.json");
            ActorSchema origin = JsonSerializer.Deserialize<ActorSchema>(JsonString, JsonFormatter);

            ID = origin.ID;

            InternalName = origin.InternalName;
            DisplayName = origin.DisplayName;

            Level = origin.Level;

            MaxHealth = origin.MaxHealth;
            Health = MaxHealth;

            MitigationValue = 0;
            IsHurt += Mitigate;

            Attributes = new Dictionary<string, int>(origin.Attributes);

            MoveSet = new List<Move>(origin.MoveSet);
        }

        public static implicit operator Actor(ActorSchema schema)
        {
            Actor actor = new Actor();

            actor.ID = schema.ID;
            actor.InternalName = schema.InternalName;
            actor.DisplayName = schema.DisplayName;
            actor.Level = schema.Level;
            actor.MaxHealth = schema.MaxHealth;
            actor.Attributes = schema.Attributes;
            actor.MoveSet = schema.MoveSet;

            return actor;
        }

        public virtual void LevelUp()
        {
            Level += 1;
            MaxHealth = Level * (Attributes["VIT"] * 1.5);
            Health = MaxHealth;
        }

        public virtual void Mitigate(Move move, double result, Actor target)
        {
            if (target == this)
            {
                if (move.Component == Components.PHYSICAL)
                {
                    MitigationValue = result - (Attributes["DEF"] + Attributes["VIT"]);
                }
                else
                {
                    MitigationValue = result;
                }
                Health -= MitigationValue;
            }
        }

        public virtual void Attack(int move, Actor target)
        {
            MoveSet[move].Trigger(target, this);
        }
    }

    //Json Schema
    public struct ActorSchema
    {
        public string Version;

        public int ID;

        public string InternalName;
        public string DisplayName;

        public double MaxHealth;

        public int Level;

        public Dictionary<string, int> Attributes;
        public List<Move> MoveSet;

        public ActorSchema()
        {
            Version = "0.0.1";
            ID = -1;

            InternalName = $"Actor Schematic";
            DisplayName = "PlaceHolder";

            MaxHealth = -1;

            Level = -1;

            Attributes = new Dictionary<string, int>();
            MoveSet = new List<Move>();
        }

        public static explicit operator ActorSchema(Actor actor)
        {
            ActorSchema schema = new ActorSchema();

            schema.ID = actor.ID;
            schema.InternalName = actor.InternalName;
            schema.DisplayName = actor.DisplayName;
            schema.Level = actor.Level;
            schema.MaxHealth = actor.MaxHealth;
            schema.Attributes = actor.Attributes;
            schema.MoveSet = actor.MoveSet;

            return schema;
        }
    }
}
