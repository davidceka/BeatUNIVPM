using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using Slider = UnityEngine.UI.Slider;

// CLASSE PER LA GESTIONE DEI POWER UP
public class PowerUp : MonoBehaviour
{
    /// <summary>
    /// Variabili che potrebbero servire per gestire
    /// la percentuali di completamento della barra
    /// dei power up
    /// ( NOTA: non è stato implementato ancora nulla con la percentuale )
    /// </summary>
    public float currentValue = 0f; // Valore attuale della barra
    public float maxValue = 20f; // Valore massimo della barra
    
    public Slider slider; // Slider per scorrimento della barra
    public bool active = false; // Variabile booleana per verificare se il power up è attivo
    public float time = 0f; // Variabile per il conteggio del tempo
    
    // Start is called before the first frame update
    void Start()
    {
        // setta i valori di min e max dello slider ed il valore iniziale a 0.3
        slider.minValue = 0.3f;
        slider.maxValue = 30f;
        SetBar(0.3f);
    }

    // Update is called once per frame
    void Update()
    {
        // Conta il tempo ad ogni frame
        time = Time.deltaTime;
    }

    // Metodo per il setting dello slider ad un valore specifico
    public void SetBar(float value)
    {
        slider.value = value;
    }

    // Metodo per l'incremento dello slider
    public void IncreaseBar(float value)
    {
        // Incrementa la barra dei Power-Up
        slider.value += value;
        if (slider.value >= slider.maxValue)
        {
            SetBar(slider.maxValue);
        }
    }

    // Metodo per decrementare lo slider (non ancora utilizzato)
    public void DecreaseBar(float value)
    {
        // Decrementa la barra dei Power-Up
        slider.value -= value;
        if (slider.value <= slider.minValue)
        {
            SetBar(slider.minValue);
        }
    }

    // Metodo che serve per impostare un timer sul power up
    public void TimerPowerUp()
    {
        // Decrementa progressivamente la barra dei power up
        slider.value = Mathf.Lerp(slider.value, 0f, time * 0.1f);
    }
}