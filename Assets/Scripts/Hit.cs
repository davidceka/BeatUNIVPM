using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CLASSE PER LA GESTIONE DEGLI INPUT E DELLA COLLISIONE
public class Hit : MonoBehaviour
{
    /// <summary>
    /// Variabili divise in due gruppi:
    /// Primo Gruppo => riferimenti alle altre classi
    /// Secondo gruppo => input
    /// </summary>
    
    // Primo Gruppo
    public Synch spawn;
    public ScoreManager scoreManager;
    public PowerUp powerUp;

    // Secondo Gruppo
    public KeyCode button = KeyCode.Space;
    private bool _isButtonPressed = false; // Per capire se il tasto è premuto

    // Start is called before the first frame update
    void Start()
    {
        // Si dichiara dove trovare i riferimenti agli oggetti delle altre classi
        spawn = GameObject.FindGameObjectWithTag("Respawn").GetComponent<Synch>();
        scoreManager = FindObjectOfType<ScoreManager>();
        powerUp = FindObjectOfType<PowerUp>();
    }

    // Update is called once per frame
    void Update()
    {
        // Rileva la pressione del tasto
        if (Input.GetKeyDown(button) && !_isButtonPressed)
        {
            _isButtonPressed = true;
        }

        // Rileva il rilascio del tasto
        if (Input.GetKeyUp(button) && _isButtonPressed)
        {
            _isButtonPressed = false;
        }
        
        // Rileva se le condizioni sono soddisfatte e attiva il power up
        if (_isButtonPressed && powerUp.slider.value >= powerUp.slider.maxValue)
        {
            scoreManager.reward = 20;
            powerUp.active = true;
        }
        
        // Gestisce il power up con l'attivazione di un timer
        if (powerUp.active)
        {
            powerUp.TimerPowerUp(); // Decrementa progressivamente la barra dei power up
            
            // Al termine del timer, le variabili vengono reimpostate (power up disattivato)
            if (powerUp.slider.value <= 0.3f)
            {
                powerUp.active = false;
                scoreManager.reward = 10;
            }
        }
    }
    
    // Metodo per la gestione della collisione tra le armi e gli spawn
    private void OnCollisionEnter(Collision collision)
    {
        
        // Controlla se la collisione è avvenuta con un cubo
        if (collision.gameObject.CompareTag("Respawn"))
        {
            // Ottieni il cubo colpito, il suo colore e quello dell'arma che la colpisce
            GameObject sphere = collision.gameObject;
            Color sphereColor = sphere.GetComponent<Renderer>().material.color;
            Color swordColor = GetComponent<Renderer>().material.color;

            // Rimuovi il cubo dalla lista dei cubi generati
            if (spawn.spawnedSpheres.Contains(sphere))
            {
                spawn.spawnedSpheres.Remove(sphere);
            }

            // Confronta il colore dell'arma con quello del cubo ed effettua delle operazioni
            if (swordColor == sphereColor)
            {
                scoreManager.IncreaseScore(scoreManager.reward); // Incrementa il punteggio del giocatore
                
                // Incremento la barra dei power up, se attivo un power up
                if (!powerUp.active)
                {
                    powerUp.IncreaseBar(scoreManager.reward);
                }
            }
            else
            {
                scoreManager.DecreaseScore(scoreManager.penalty); // Applica una penalità
            }
            Destroy(sphere); // Distruzione del cubo
        }
        else if (collision.gameObject.CompareTag("Bomba")) // Se la collisione è con un cubo bomba
        {
            GameObject sphere = collision.gameObject; // Cubo bomba
            
            // Rimuovi il cubo bomba dalla lista dei cubi generati
            if (spawn.spawnedSpheres.Contains(sphere))
            {
                spawn.spawnedSpheres.Remove(sphere);
            }
            scoreManager.DecreaseScore(scoreManager.reward); // Decrementa il punteggio di una quantità pari al reward
            powerUp.slider.value = 0.3f; // Decrementa instantaneamente la barra dei power up
            if (powerUp.active)
            {
                powerUp.active = false; // Disattiva il power up
                scoreManager.reward = 10;
            }
            
            Destroy(sphere); // Distruzione del cubo
        }
    }
}