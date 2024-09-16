using System.Collections;
using System.Collections.Generic;
using Gameplay.Enemies;
using UnityEngine;

public class ElementDecorator : MonoBehaviour
{
   public const float DEBUFF_TICK = 1f;
   public const int MAX_STACKS = 5;
   public const float BASE_MULTIPLIER = 0.1f;
   public const float DEBUFF_DURATION = 5f;
   public const float STRENGTH_MULTIPLIER = 0.2f;  // 20% boost for strengths per level
   public const float WEAKNESS_MULTIPLIER = 0.5f;  // 50% reduction for weaknesses per level
}
