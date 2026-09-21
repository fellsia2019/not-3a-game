using UnityEngine;
using UnityEngine.UI;

namespace Not3A.Stage1
{
    public sealed class Stage1Hud : MonoBehaviour
    {
        [SerializeField] private Text resourcesText;
        [SerializeField] private Text phaseText;
        [SerializeField] private Text objectiveText;
        [SerializeField] private Text throneText;
        [SerializeField] private Text contextText;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private Text resultText;
        [SerializeField] private Button restartButton;

        public Button RestartButton => restartButton;

        public void Configure(
            Text resources,
            Text phase,
            Text objective,
            Text throne,
            Text context,
            GameObject results,
            Text result,
            Button restart)
        {
            resourcesText = resources;
            phaseText = phase;
            objectiveText = objective;
            throneText = throne;
            contextText = context;
            resultPanel = results;
            resultText = result;
            restartButton = restart;
        }

        public void SetResources(int wood, int stone)
        {
            resourcesText.text = $"Дерево: {wood}    Камень: {stone}";
        }

        public void SetPhase(string text) => phaseText.text = text;
        public void SetObjective(string text) => objectiveText.text = text;
        public void SetThrone(int current, int maximum) => throneText.text = $"Трон: {current} / {maximum}";

        public void SetContext(string text)
        {
            contextText.text = "WASD — движение   E — добыча   B — башня   ЛКМ — поставить   Esc — отмена\n" + text;
        }

        public void HideResult()
        {
            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }
        }

        public void ShowResult(string text)
        {
            resultText.text = text;
            resultPanel.SetActive(true);
        }
    }
}
