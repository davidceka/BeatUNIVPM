using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.XR;
// CLASSE PER LA GESTIONE DEGLI INPUT E DELLA COLLISIONE
public class Hit : MonoBehaviour
{
    /// <summary>
    /// Variabili divise in due gruppi:
    /// Primo Gruppo => riferimenti alle altre classi
    /// Secondo gruppo => input
    /// </summary>


    private DebugPanel debugPanel;


    // Primo Gruppo
    public Synch spawn;
    public ScoreManager scoreManager;
    public PowerUp powerUp;

    private List<InputDevice> foundControllers;
    private bool isPrimaryButtonPressed = false;
    private bool isSecondaryButtonPressed = false;
    private InputDevice device;

    // Secondo Gruppo
    public KeyCode button = KeyCode.Space;
    public KeyCode buttonA = KeyCode.A;
    private bool _isButtonPressed = false; // Per capire se il tasto è premuto
    private bool _isButtonPressedA = false;
    
    // Terzo gruppo
    public GameObject particleObj;
    private ParticleSystem _particles;
    
    // Componente AudioSource per il suono di collisione
    public AudioSource soundSource;
    
    // Variabili per il riferimento ai gameobject dei cubi e delle spade
    private Renderer _swordRight;
    private Renderer _swordLeft;
    private Color _colorRight;
    private Color _colorLeft;


    string debug;

    // Start is called before the first frame update
    void Start()
    {
        debugPanel = FindObjectOfType<DebugPanel>();
        // Si dichiara dove trovare i riferimenti agli oggetti delle altre classi
        spawn = GameObject.FindGameObjectWithTag("Respawn").GetComponent<Synch>();
        scoreManager = FindObjectOfType<ScoreManager>();
        powerUp = FindObjectOfType<PowerUp>();
        
        soundSource = GetComponent<AudioSource>(); // Ottiene il componente AudioSource

        _swordRight = GameObject.FindGameObjectWithTag("SwordRight").GetComponent<Renderer>();
        _swordLeft = GameObject.FindGameObjectWithTag("SwordLeft").GetComponent<Renderer>();
        _colorRight = _swordRight.material.color;
        _colorLeft = _swordLeft.material.color;


        InputDeviceCharacteristics rightTrackedControllerFilter = InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Right, rightHandedControllers;

        foundControllers = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(rightTrackedControllerFilter, foundControllers);
        device = foundControllers[0];
        debugPanel.UpdateDebugText(device.characteristics.ToString());


        
        
    }

    // Update is called once per frame
    void Update()
    {
        //debugPanel.UpdateDebugText("isbuttonpressed:"+_isButtonPressed.ToString()+"\n isbuttonpressedA:"+_isButtonPressedA.ToString());
        // Rileva la pressione del tasto Spazio
        if (device.TryGetFeatureValue(CommonUsages.primaryButton, out isPrimaryButtonPressed) && isPrimaryButtonPressed &&!isSecondaryButtonPressed && powerUp.slider.value >= powerUp.slider.maxValue - 0.1f)
        {
            _isButtonPressed = true;
            //debugPanel.UpdateDebugText(_isButtonPressed.ToString()+"   primary button pressed");
            scoreManager.reward = 1000;
            powerUp.active = true;

        }

        // Rileva il rilascio del tasto Spazio
        if (device.TryGetFeatureValue(CommonUsages.primaryButton, out isPrimaryButtonPressed) && isPrimaryButtonPressed)
        {
            _isButtonPressed = false;
        }

        // Rileva la pressione del tasto A
        if (device.TryGetFeatureValue(CommonUsages.secondaryButton, out isSecondaryButtonPressed) && isSecondaryButtonPressed &&!isPrimaryButtonPressed && powerUp.slider.value >= powerUp.slider.maxValue - 0.1f)
        {
            
            _isButtonPressedA = true;
            //debugPanel.UpdateDebugText(_isButtonPressedA.ToString() + "   secondary button pressed");
            _swordLeft.material.color = Color.white;
            _swordRight.material.color = Color.white;
            powerUp.activeSecond = true;
        }

        // Rileva il rilascio del tasto A
        // Rileva la pressione del tasto A
        if (device.TryGetFeatureValue(CommonUsages.secondaryButton, out isSecondaryButtonPressed) && isSecondaryButtonPressed)
        {
            _isButtonPressedA = false;
        }

        // Rileva se le condizioni sono soddisfatte e attiva il power up
        if (_isButtonPressed && powerUp.slider.value >= powerUp.slider.maxValue - 0.1f)
        {
            debugPanel.UpdateDebugText("qui dentro ci entra");
            scoreManager.reward = 20;
            powerUp.active = true;
        }
        
        // Rileva se le condizioni sono soddisfatte e attiva il power up
        if (_isButtonPressedA && powerUp.slider.value >= powerUp.slider.maxValue - 0.1f)
        {
            debugPanel.UpdateDebugText("qui pure");
            _swordLeft.material.color = Color.white;
            _swordRight.material.color = Color.white;
            powerUp.activeSecond = true;
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
        
        // Gestisce il power up con l'attivazione di un timer
        if (powerUp.activeSecond)
        {
            powerUp.TimerPowerUp(); // Decrementa progressivamente la barra dei power up
            
            // Al termine del timer, le variabili vengono reimpostate (power up disattivato)
            if (powerUp.slider.value <= 0.3f)
            {
                powerUp.activeSecond = false;

                for (int i = 0; i < spawn.spawnedSpheres.Count; i++)
                {
                    GameObject cube = spawn.spawnedSpheres[i];

                    if (cube.CompareTag("CCube"))
                    {
                        cube.GetComponent<Renderer>().material.color = _colorRight;
                    }
                    else if (cube.CompareTag("PCube"))
                    {
                        cube.GetComponent<Renderer>().material.color = _colorLeft;
                    }
                    //cube.GetComponent<Renderer>().material.color = Color.white;
                }
                _swordLeft.material.color = _colorLeft;
                _swordRight.material.color = _colorRight;
            }
        }
    }
    
    // Metodo per la gestione della collisione tra le armi e gli spawn
    private void OnCollisionEnter(Collision collision)
    {
        // Controlla se la collisione è avvenuta con un cubo
        if (collision.gameObject.CompareTag("CCube") || collision.gameObject.CompareTag("PCube"))
        {
            soundSource.Play(); // Attiva l'effetto sonoro

            // Ottieni il cubo colpito, il suo colore e quello dell'arma che la colpisce
            GameObject sphere = collision.gameObject;
            Color sphereColor = sphere.GetComponent<Renderer>().material.color;
            Color swordColor = GetComponent<Renderer>().material.color;
            
            // inizializzo le particelle nella stessa posizione dei cubi
            GameObject particles = Instantiate(particleObj, sphere.transform.position, Quaternion.identity);
            ParticleSystem particleSys = particles.GetComponent<ParticleSystem>();
            particleSys.GetComponent<Renderer>().material.color = sphere.GetComponent<Renderer>().material.color;

            // Rimuovi il cubo dalla lista dei cubi generati
            if (spawn.spawnedSpheres.Contains(sphere))
            {
                spawn.spawnedSpheres.Remove(sphere);
            }

            // Confronta il colore dell'arma con quello del cubo ed effettua delle operazioni
            if (swordColor == sphereColor)
            {
                scoreManager.IncreaseScore(scoreManager.reward); // Incrementa il punteggio del giocatore
                powerUp.IncreaseHealth(scoreManager.reward); // Incrementa la vita del giocatore
                scoreManager.count += 1; // Incrementa il contatore combo di una unità
                
                // Incremento la barra dei power up, se non attivo un power up
                if (!powerUp.active && !powerUp.activeSecond)
                {
                    powerUp.IncreaseBar(scoreManager.reward);
                }
            }
            else
            {
                scoreManager.DecreaseScore(scoreManager.penalty); // Applica una penalità
                scoreManager.count = 0; // Fa il reset a zero del contatore combo
            }
            
            Destroy(sphere); // Distruzione del cubo
            
            particleSys.Play(); // Avvia le particelle
            Destroy(particles, 2.0f); // Distruzione delle particelle
        }
        else if (collision.gameObject.CompareTag("Bomba")) // Se la collisione è con un cubo bomba
        {
            GameObject sphere = collision.gameObject; // Cubo bomba
            
            // inizializzo le particelle nella stessa posizione dei cubi
            GameObject particles = Instantiate(particleObj, sphere.transform.position, Quaternion.identity);
            ParticleSystem particleSys = particles.GetComponent<ParticleSystem>();
            particleSys.GetComponent<Renderer>().material.color = sphere.GetComponent<Renderer>().material.color;
            
            // Rimuovi il cubo bomba dalla lista dei cubi generati
            if (spawn.spawnedSpheres.Contains(sphere))
            {
                spawn.spawnedSpheres.Remove(sphere);
            }
            
            scoreManager.DecreaseScore(scoreManager.reward); // Decrementa il punteggio di una quantità pari al reward
            powerUp.slider.value = 0.3f; // Decrementa instantaneamente la barra dei power up
            powerUp.DecreaseHealth(scoreManager.reward); // Decrementa la vita di una quantità pari al reward
            scoreManager.count = 0; // Fa il reset a zero del contatore combo
            
            if (powerUp.active)
            {
                powerUp.active = false; // Disattiva il power up
                scoreManager.reward = 10;
            }
            
            Destroy(sphere); // Distruzione del cubo
            
            particleSys.Play(); // Avvia le particelle
            Destroy(particles, 2.0f); // Distruzione delle particelle
        }
    }
}