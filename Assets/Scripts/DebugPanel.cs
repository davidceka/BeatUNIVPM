using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text debugText;

    private void Start()
    {
        // Ottieni il riferimento al componente di testo
        debugText = GetComponentInChildren<TMP_Text>();
    }

    public void UpdateDebugText(string newText)
    {
        // Aggiorna il testo di debug nel pannello
        debugText.text = newText;
    }
}
