using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3FantasyHud
    {
        private Stage3DefeatPresentation defeatPresentation;
        public void BindDefeatPresentation(Stage3DefeatPresentation presenter) => defeatPresentation = presenter;
        public Rect RetryButtonRect => new Rect((Screen.width - 480f * UiScale) * .5f,
            Screen.height * .53f, 480f * UiScale, 82f * UiScale);

        private void DrawDefeatRetry()
        {
            var opacity = defeatPresentation.RetryOpacity;
            if (opacity <= 0) return;
            var saved = GUI.color;
            GUI.color = new Color(1, 1, 1, opacity);
            // Existing lunar materials and native ornaments; no new illustrated asset.
            Fill(new Rect(0, 0, Screen.width, Screen.height), new Color(.985f, .974f, .959f));
            var s = UiScale;
            var center = Screen.width * .5f;
            var titleY = Screen.height * .36f;
            Fill(new Rect(center - 190f * s, titleY - 36f * s, 380f * s, s), Silver);
            DrawDisc(new Rect(center - 24f * s, titleY - 61f * s, 48f * s, 48f * s), new Color(.985f,.974f,.959f));
            DrawMedallion(new Rect(center - 28f * s, titleY - 65f * s, 56f * s, 56f * s));
            FitText(new Rect(center - 490f * s, titleY, 980f * s, 64f * s), "Ещё одна попытка", 40, false);
            Label(new Rect(center - 490f * s, titleY + 70f * s, 980f * s, 40f * s),
                "Вернитесь к подготовке и измените исход", 22, TextAnchor.MiddleCenter, true);
            var button = RetryButtonRect;
            if (Button(button, "", false, defeatPresentation.CanRetry)) defeatPresentation.Retry();
            PaleLabel(ButtonContentRect(button), "Повторить волну", 26, TextAnchor.MiddleCenter);
            Label(new Rect(center - 480f * s, button.yMax + 32f * s, 960f * s, 34f * s),
                game.RunDriver.Run.PreserveProgressOnRetry ? "Прогресс сохранён · Новый день для подготовки" :
                "Состояние до волны · Полное время подготовки", 17, TextAnchor.MiddleCenter, true);
            GUI.color = saved;
        }
    }
}
