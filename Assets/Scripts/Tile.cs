using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [System.Serializable]
    public class State {
        public Color fillColor;
        public Color outlineColor;
    }
    public State StateTile { get; private set;}
    public char Letter { get; private set; }
    private TextMeshProUGUI text;
    private Image fill;
    private Outline outline;
    private void Awake() {
        text = GetComponentInChildren<TextMeshProUGUI>();
        fill = GetComponent<Image>();
        outline = GetComponent<Outline>();
    }
    public void SetLetter(char letter) {
        Letter = letter;
        text.text = letter.ToString();
    }
    public void SetState(State state) {
        StateTile = state;
        fill.color = state.fillColor;
        outline.effectColor = state.outlineColor;
    }
}
