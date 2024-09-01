using TMPro;

namespace UI.DamageNumbers
{
    public static class ScoreTextTween
    {
        public static void TweenScoreText(TextMeshPro text, int score, int multiplier)
        {
            switch (multiplier)
            {
                case 1:
                    OneTimeMultiplier(text, score);
                    break;
                case 2:
                    TwoTimeMultiplier(text, score);
                    break;
                case 3:
                    ThreeTimeMultiplier(text, score);
                    break;
                case 4:
                    FourTimeMultiplier(text, score);
                    break;
                case 5:
                    FiveTimeMultiplier(text, score);
                    break;
            }
        }

        private static void OneTimeMultiplier(TextMeshPro text, int score)
        {
        }

        private static void TwoTimeMultiplier(TextMeshPro text, int score)
        {
        }

        private static void ThreeTimeMultiplier(TextMeshPro text, int score)
        {
        }

        private static void FourTimeMultiplier(TextMeshPro text, int score)
        {
        }

        private static void FiveTimeMultiplier(TextMeshPro text, int score)
        {
        }
    }
}