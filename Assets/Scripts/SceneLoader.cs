using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static string selectedSong = "";
    [SerializeField] private GameObject buttonClicked;


    // Start is called before the first frame update
    public void LoadGameScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
      
    }
    public void QuitGame()
    {
        Application.Quit();
    }


    public void OnSongButtonClick()
    {
        // Ottieni il riferimento al pulsante che ha causato l'evento

        // Ottieni il componente Text associato al pulsante
        string selectedSong = buttonClicked.name;
        PlayerPrefs.SetString("SelectedSong", selectedSong);

        SceneManager.LoadScene("Game");

    }





}
