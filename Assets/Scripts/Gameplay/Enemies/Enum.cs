using System;
using System.Collections.Generic;

namespace Gameplay.Enemies
{
    public enum CannonState
    {
        Idle,
        Sucking,
        Shooting,
    }

    [Flags]
    public enum ElementFlag
    {
        None = 0,
        Fire = 1 << 1,
        Water = 1 << 2,
        Rock = 1 << 3,
        Wind = 1 << 4,
        Electricity = 1 << 5,
    }

    [Flags]
    public enum Weakness
    {
        None = ElementFlag.None,
        Fire = ElementFlag.Water,
        Water = ElementFlag.Electricity,
        Rock = ElementFlag.Water,
        Wind = ElementFlag.Electricity,
        Electricity = ElementFlag.Rock,
    }
    
    [Flags]
    public enum Strength
    {
        None = 0,
        Fire = ElementFlag.Wind,
        Water = ElementFlag.Fire,
        Rock = ElementFlag.Wind,
        Wind = ElementFlag.Rock,
        Electricity = ElementFlag.Wind,
    }

    public enum EnemyType
    {
        Grunt,
        Swarm,
        GlassCannon,
        Tank,
        // Boss
    }

    public static class ElementRelations
    {
        public static readonly Dictionary<ElementFlag, ElementFlag> Strengths = new()
        {
            { ElementFlag.Wind, ElementFlag.Rock },          // Wind is strong against Rock
            { ElementFlag.Water, ElementFlag.Fire },         // Water is strong against Fire
            { ElementFlag.Fire, ElementFlag.Wind },          // Fire is strong against Wind
            { ElementFlag.Rock, ElementFlag.Wind },          // Rock is strong against Wind
            { ElementFlag.Electricity, ElementFlag.Wind }    // Electricity is strong against Wind
        };

        public static readonly Dictionary<ElementFlag, ElementFlag> Weaknesses = new()
        {
            { ElementFlag.Wind, ElementFlag.Electricity },   // Wind is weak against Electricity
            { ElementFlag.Water, ElementFlag.Electricity },  // Water is weak against Electricity
            { ElementFlag.Fire, ElementFlag.Water },         // Fire is weak against Water
            { ElementFlag.Rock, ElementFlag.Water },         // Rock is weak against Water
            { ElementFlag.Electricity, ElementFlag.Rock }    // Electricity is weak against Rock
        };
    }

}
