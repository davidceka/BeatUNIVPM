using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using Slider = UnityEngine.UI.Slider;
using UnityEngine.SceneManagement;
using Image = UnityEngine.UI.Image;

// CLASSE PER LA GESTIONE DEI POWER UP
public class PowerUp : MonoBehaviour
{
    public Synch spawn; // Riferimento alla classe Synch
    
    public Slider slider; // Slider per scorrimento della barra
    public Slider health; // Slider per vita
    
    public bool active = false; // Variabile booleana per verificare se il power up è attivo
    public bool activeSecond = false;
    public bool isGameOver = false; // Variabile booleana per identificare il Game Over
    public float time = 0f; // Variabile per il conteggio del tempo

    private Image _fill;

    // Start is called before the first frame update
    void Start()
    {
        // Si dichiara dove trovare i riferimenti agli oggetti della classe Synch
        spawn = GameObject.FindGameObjectWithTag("Respawn").GetComponent<Synch>();
        
        // setta i valori di min e max degli slider
        slider.minValue = 0.3f;
        slider.maxValue = 100f;

        health.minValue = 0f;
        health.maxValue = 50f;
        
        SetBar(0.3f, 50f);

        // Settaggio per i riferimenti al colore della barra PowerUp
        _fill = GameObject.FindWithTag("FillPU").GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        // Conta il tempo ad ogni frame
        time = Time.deltaTime;
        
        // Controlla se la vita è a zero e, in caso, dichiare Game Over
        if (health.value <= health.minValue)
        {
            GameOver();
        }
        
        CheckColor();
    }

    // Metodo per il setting dello slider ad un valore specifico
    public void SetBar(float value, float valueHealth)
    {
        slider.value = value;
        health.value = valueHealth;
    }

    // Metodo per l'incremento dello slider
    public void IncreaseBar(float value)
    {
        // Incrementa la barra dei Power-Up
        slider.value += value;
        if (slider.value >= slider.maxValue)
        {
            slider.value = slider.maxValue;
        }
    }

    // Metodo per decrementare lo slider
    public void DecreaseHealth(float value)
    {
        // Decrementa la barra della vita
        health.value -= value;
        if (health.value <= health.minValue)
        {
            health.value = health.minValue;
        }
    }
    
    // Metodo per incrementare lo slider
    public void IncreaseHealth(float value)
    {
        // Incrementa la barra della vita
        health.value += value;
        if (health.value >= health.maxValue)
        {
            health.value = health.maxValue;
        }
    }

    // Metodo che serve per impostare un timer sul power up
    public void TimerPowerUp()
    {
        // Decrementa progressivamente la barra dei power up
        slider.value = Mathf.Lerp(slider.value, 0f, time * 0.1f);
    }

    // Metodo che implementa il Game Over
    public void GameOver()
    {
        Time.timeScale = 1f; // Blocca il gioco
        spawn.musicSource.Stop(); // Blocca la musica
        isGameOver = true;
        SceneManager.LoadScene("LoseScreen");
    }

    // Metodo che controlla lo stato della barra PowerUp e ne modifica il colore di conseguenza
    public void CheckColor()
    {
        if (slider.value >= slider.minValue && slider.value <= 30.3f)
        {
            _fill.color = Color.red;
        }
        else if (slider.value > 30.3f && slider.value <= slider.maxValue-1)
        {
            _fill.color = Color.yellow;
        }
        else
        {
            _fill.color = Color.green;
        }
    }

    /// <summary>
    /// Questo è un metodo in più che non so se potrebbe servire
    /// (forse per riavviare il gioco dopo il game over o dopo
    /// aver implementato un sistema di pausa)
    /// </summary>
    public void Restart()
    {
        Time.timeScale = 1f;
        isGameOver = false;
    }
}