using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// CLASSE PER LA GESTIONE DELLO SCORE
public class ScoreManager : MonoBehaviour
{
    public int score = 0; // Punteggio del giocatore
    public int count = 0; // Contatore per il sistema di combo
    public int reward = 10; // Premio che si ottine per aver colpito un cubo con l'arma giusta
    public int penalty = 5; // Penalità per aver colpito un cubo con l'arma sbagliata
    
    // Variabili utili all'aggiornamento dell'interfaccia di gioco
    private TMP_Text _scoreTextMesh;
    private TMP_Text _comboTextMesh;

    // Start is called before the first frame update
    
    void Start()
    {
        // Trova il componente TextMeshPro nel figlio "ScoreText" o nel figlio "Combo"
        _scoreTextMesh = GetComponent<TextMeshProUGUI>();
        _comboTextMesh = GameObject.FindGameObjectWithTag("Combo").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        _comboTextMesh.text = "X " + count;
    }

    // Metodo per la visualizzazione del punteggio ( aggiorna il punteggio con il valore della combo )
    public void ViewScore()
    {
        if (count != 0)
        {
            score += count;  
        }
        _scoreTextMesh.text = "Score: " + score;
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