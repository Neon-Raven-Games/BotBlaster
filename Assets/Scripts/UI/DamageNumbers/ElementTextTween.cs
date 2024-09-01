using System.Collections.Generic;
using DG.Tweening;
using Gameplay.Enemies;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum StatusEffectiveness
{
    Strong,
    Weak,
    Normal
}

public class DamageMaterialProperties
{
    public Color fromFaceColor { get; set; }
    public Color toFaceColor { get; set; }
    public Color glowColor { get; set; }
    public float glowOffset { get; set; }
    public float glowInner { get; set; }
    public float glowOuter { get; set; }
    public float glowPower { get; set; }
}

namespace UI.DamageNumbers
{
    public static class ElementTextTween
    {
        private static readonly int _SFaceColor = Shader.PropertyToID("_FaceColor");
        private static readonly int _SGlowColor = Shader.PropertyToID("_GlowColor");
        private static readonly int _SGlowPower = Shader.PropertyToID("_GlowPower");
        private static readonly int _SGlowOffset = Shader.PropertyToID("_GlowOffset");
        private static readonly int _SGlowInner = Shader.PropertyToID("_GlowInner");
        private static readonly int _SGlowOuter = Shader.PropertyToID("_GlowOuter");

        private static readonly Dictionary<ElementFlag, DamageMaterialProperties> _SElementMaterialProperties = new()
        {
            {
                ElementFlag.Fire, new DamageMaterialProperties
                {
                    fromFaceColor = new Color(1f, 0.6f, .5f, 1f),
                    toFaceColor = new Color(1, 0, 0, 1),
                    glowColor = new Color(1, 0.133618f, 0, 0.5019608f),
                    glowOffset = 0.6f,
                    glowInner = 0.66f,
                    glowOuter = 0.263f,
                    glowPower = 0.588f
                }
            },
            {
                ElementFlag.Water, new DamageMaterialProperties
                {
                    fromFaceColor = new Color(0, 0.7f, 1, 1f),
                    toFaceColor = new Color(0, 0.4f, 1, 1),
                    glowColor = new Color(0, 0.4f, 1, 0.5019608f),
                    glowOffset = 0.6f,
                    glowInner = 0.66f,
                    glowOuter = 0.263f,
                    glowPower = 0.588f
                }
            },
            {
                ElementFlag.Rock, new DamageMaterialProperties
                {
                    fromFaceColor = new Color(0.4f, 0.1f, 0, 1f),
                    toFaceColor = new Color(0.3207547f, 0.08784632f, 0, 1),
                    glowColor = new Color(0.3773585f, 0.05031447f, 0, 0.5019608f),
                    glowOffset = 0.6f,
                    glowInner = 0.66f,
                    glowOuter = 0.263f,
                    glowPower = 0.588f
                }
            },
            {
                ElementFlag.Wind, new DamageMaterialProperties
                {
                    fromFaceColor = new Color(1f, 1f, 1, 1f),
                    toFaceColor = new Color(0.35f, 0.94f, 1, 1),
                    glowColor = new Color(.36f, 0.96f, .9f, 0.5019608f),
                    glowOffset = 0.6f,
                    glowInner = 0.66f,
                    glowOuter = 0.263f,
                    glowPower = 0.588f
                }
            },
            {
                ElementFlag.Electricity, new DamageMaterialProperties
                {
                    fromFaceColor = new Color(1f, 1f, .38f, 1f),
                    toFaceColor = new Color(1, 1, 0, 1),
                    glowColor = new Color(1, 1, 0, 0.5019608f),
                    glowOffset = 0.6f,
                    glowInner = 0.66f,
                    glowOuter = 0.263f,
                    glowPower = 0.588f
                }
            }
        };

        private static readonly Dictionary<StatusEffectiveness, Color> _SFaceColors = new()
        {
            {StatusEffectiveness.Weak, new Color(0.5f, 0.5f, 0.5f, 1)},
            {StatusEffectiveness.Normal, new Color(1, 1, 1, 1)}
        };

        private static void SetFaceColor(Material mat, StatusEffectiveness status) =>
            mat.SetColor(_SFaceColor, _SFaceColors[status]);

        public static void TweenElementText(TextMeshPro text, ElementFlag element, StatusEffectiveness effectiveness,
            int number)
        {
            text.gameObject.SetActive(true);
            var rend = text.GetComponent<MeshRenderer>();
            text.text = number.ToString();
            SetGlowProperties(element, rend, effectiveness);
            text.DOFade(0, 2f)
                .SetEase(Ease.OutCubic)
                .From(1f);
        }

        private static void SetGlowProperties(ElementFlag element, MeshRenderer rend,
            StatusEffectiveness statusEffectiveness)
        {
            float glowMultiplier, pulsateDuration, scaleMultiplier;
            switch (statusEffectiveness)
            {
                case StatusEffectiveness.Strong:
                    glowMultiplier = 1f;
                    pulsateDuration = 0.5f; // Fast pulsation
                    scaleMultiplier = 1.3f; // Slightly larger scale
                    break;
                case StatusEffectiveness.Weak:
                    glowMultiplier = 0.25f;
                    pulsateDuration = 1.5f; // Slow pulsation
                    scaleMultiplier = 1f; // No scaling
                    break;
                case StatusEffectiveness.Normal:
                default:
                    glowMultiplier = 0.5f;
                    pulsateDuration = 1f; // Moderate pulsation
                    scaleMultiplier = 1.1f; // Slightly larger scale, but less than Strong
                    break;
            }

            var materialProperties = _SElementMaterialProperties[element];
            if (statusEffectiveness == StatusEffectiveness.Strong)
            {
                rend.material.DOColor(materialProperties.toFaceColor, _SFaceColor, pulsateDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .From(materialProperties.fromFaceColor);
            }
            else
            {
                SetFaceColor(rend.material, statusEffectiveness);
            }
            rend.material.SetColor(_SGlowColor, materialProperties.glowColor);
            SetGlowProperty(glowMultiplier, rend, pulsateDuration, _SGlowPower, materialProperties.glowPower);
            SetGlowProperty(glowMultiplier, rend, pulsateDuration, _SGlowOffset, materialProperties.glowOffset);
            SetGlowProperty(glowMultiplier, rend, pulsateDuration, _SGlowInner, materialProperties.glowInner);
            SetGlowProperty(glowMultiplier, rend, pulsateDuration, _SGlowOuter, materialProperties.glowOuter);

            rend.transform.DOScale(scaleMultiplier, pulsateDuration).SetEase(Ease.OutElastic).From(1f);
            rend.transform.DOMoveY(rend.transform.position.y + .4f, 2f).SetEase(Ease.OutCubic)
                .OnComplete(() => rend.gameObject.SetActive(false));

        }

        private static void SetGlowProperty(float glowMultiplier, Renderer rend, float pulsateDuration, int key,
            float value)
        {
            var initialValue = value * glowMultiplier;
            var targetValue = value * (glowMultiplier * 1.5f);
            rend.material.DOFloat(targetValue, key, pulsateDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .From(initialValue);
        }
    }
}