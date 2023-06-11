using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// CLASSE PER LA GESTIONE DELLO SCORE
public class ScoreManager : MonoBehaviour
{
    public int score = 0; // Punteggio del giocatore
    public int reward = 10; // Premio che si ottine per aver colpito un cubo con l'arma giusta
    public int penalty = 5; // Penalità per aver colpito un cubo con l'arma sbagliata
    private TMP_Text _scoreTextMesh;
    // Start is called before the first frame update
    
    void Start()
    {
        // Trova il componente TextMeshPro nel figlio "ScoreText"
        _scoreTextMesh = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Metodo per la visualizzazione del punteggio
    public void ViewScore()
    {
        _scoreTextMesh.text = "Punteggio: " + score;
    }
    
    // Metodo per l'incremento del punteggio
    public void IncreaseScore(int value)
    {
        score += value;
        ViewScore(); // Visualizza lo score nella scena e sulla console
    }

    // Metodo per il decremento del punteggio
    public void DecreaseScore(int value)
    {
        if (score > 0)
        {
            score -= value;
        }

        if (score < 0)
        {
            score = 0;
        }
        ViewScore(); // Visualizza lo score nella scena e sulla console
    }
}