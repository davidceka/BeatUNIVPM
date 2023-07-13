using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PauseGame : MonoBehaviour
{
    private bool isPaused = false;
    private DebugPanel debugPanel;
    private string debug;
    private List<InputDevice> foundControllers;
    private bool isStartButtonPressed = false;
    private InputDevice device;
    private int cont = 0;


    [SerializeField]
    private GameObject countdownPanel;
    [SerializeField]
    private GameObject pausePanel;

    private void Start()
    {
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
                Time.timeScale = 0f;
                // Mostra il pannello di pausa
                pausePanel.SetActive(true);
            }
        }
    }

    void ResumeGame()
    {
        isPaused = false;
        
        pausePanel.SetActive(false);

        // Mostra il pannello di countdown
        //countdownPanel.SetActive(true);
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
