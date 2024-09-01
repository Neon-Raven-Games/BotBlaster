using Gameplay.Enemies;
using TMPro;
using UnityEngine;

public enum StatusEffectiveness
{
    Strong,
    Weak,
    Normal
}
namespace UI.DamageNumbers
{
    public static class ElementTextTween
    {
        public static void TweenElementText(TextMeshPro text, ElementFlag element, StatusEffectiveness effectiveness)
        {
            switch (element)
            {
                case ElementFlag.Fire:
                    HandleFire(text, effectiveness);
                    break;
                case ElementFlag.Water:
                    HandleWater(text, effectiveness);
                    break;
                case ElementFlag.Rock:
                    HandleRock(text, effectiveness);
                    break;
                case ElementFlag.Wind:
                    HandleWind(text, effectiveness);
                    break;
                case ElementFlag.Electricity:
                    HandleElectricity(text, effectiveness);
                    break;
            }
        }
        
        private static void HandleFire(TextMeshPro text, StatusEffectiveness statusEffectiveness)
        {
            text.color = new UnityEngine.Color(1, 0.5f, 0);
        }
        
        private static void HandleWater(TextMeshPro text, StatusEffectiveness statusEffectiveness)
        {
            text.color = new UnityEngine.Color(0, 0.5f, 1);
        }
        
        private static void HandleRock(TextMeshPro text, StatusEffectiveness statusEffectiveness)
        {
            text.color = new UnityEngine.Color(0.5f, 0.5f, 0.5f);
        }
        
        private static void HandleWind(TextMeshPro text, StatusEffectiveness statusEffectiveness)
        {
            text.color = new UnityEngine.Color(0.5f, 1, 0.5f);
        }
        
        private static void HandleElectricity(TextMeshPro text, StatusEffectiveness statusEffectiveness)
        {
            text.color = new UnityEngine.Color(1, 1, 0);
        }
    }
}