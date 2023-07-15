using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

public class PauseGame : MonoBehaviour
{
    private bool isPaused = false;
    private DebugPanel debugPanel;
    private string debug;
    private bool isStartButtonPressed = false;
    private InputDevice device;
    private List<InputDevice> foundControllers;
    private int cont = 0;

    public Synch spawn;


    [SerializeField]
    private GameObject countdownPanel;
    [SerializeField]
    private GameObject pausePanel;

    [SerializeField]
    private XRInteractorLineVisual lineRendererLeft;

    [SerializeField]
    private XRInteractorLineVisual lineRendererRight;
    private void Start()
    {
        spawn = GameObject.FindGameObjectWithTag("Respawn").GetComponent<Synch>();
        debugPanel = FindObjectOfType<DebugPanel>();
        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(inputDevices);

        foreach (var device in inputDevices)
        {
            debug += device.name + "||||" + device.characteristics.ToString() + "\n";
        }

        InputDeviceCharacteristics leftTrackedControllerFilter = InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Left, leftHandedControllers;

        foundControllers = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(leftTrackedControllerFilter, foundControllers);
        device = foundControllers[0];

        // Trova i pulsanti "Resume" e "Quit Game" all'interno del pannello di pausa
        Button resumeButton = pausePanel.GetComponentInChildren<Button>();
        Button quitButton = pausePanel.GetComponentsInChildren<Button>()[1];

        // Assegna le funzioni di callback ai pulsanti
        resumeButton.onClick.AddListener(ResumeGame);
        quitButton.onClick.AddListener(QuitGame);

        // Nascondi il pannello all'inizio
        pausePanel.SetActive(false);
        countdownPanel.SetActive(false);

    }

    void Update()
    {
        if (device.TryGetFeatureValue(CommonUsages.menuButton, out isStartButtonPressed) && isStartButtonPressed)
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                spawn.musicSource.Pause();
                Time.timeScale = 0f;
                // Mostra il pannello di pausa
                pausePanel.SetActive(true);
                lineRendererRight.enabled = true;
                lineRendererLeft.enabled = true;
            }
        }
    }

    void ResumeGame()
    {
        isPaused = false;
        
        pausePanel.SetActive(false);
        spawn.musicSource.UnPause();
        // Mostra il pannello di countdown
        //countdownPanel.SetActive(true);
        lineRendererRight.enabled = false;
        lineRendererLeft.enabled = false;
        Time.timeScale = 1f;
        //CountdownCoroutine();
    }

    private IEnumerator CountdownCoroutine()
    {
        TMP_Text countdownText = countdownPanel.GetComponentInChildren<TMP_Text>();
        int countdownValue = 3;

        while (countdownValue > 0)
        {
            countdownText.text = countdownValue.ToString();
            yield return new WaitForSeconds(1f);
            countdownValue--;
        }

        // Riprendi la scena
        countdownPanel.SetActive(false);
        Time.timeScale = 1f;
    }


    void QuitGame()
    {
        // Esci dal gioco
        Application.Quit();
    }
}
