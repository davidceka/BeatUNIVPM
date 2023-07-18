using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Note = Melanchall.DryWetMidi.Interaction.Note;
using UnityEngine.Networking;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using TMPro;



// CLASSE PER LA GESTIONE DELLO SPAWNER
public class Synch : MonoBehaviour
{
    //debug panel
    private DebugPanel debugPanel;


    // Gruppo di variabili per riferimenti alle altre classi
    public PowerUp powerUp;
    public ScoreManager scoreManager;
    private static Synch _spawn; // Campo statico per il riferimento a Synch (Utilizzato in "Hit")
    
    public GameObject[] spheres;
    public GameObject sphere;
    public List<GameObject> spawnedSpheres = new List<GameObject>(); // Lista dei cubi istanziati
    public string filename; // Nome del file da cui leggere il pattern
    private string _filepath; // Percorso del file da cui leggere il pattern
    public float moveSpeed; // Velocità di movimento dei cubi

    string fileContent;

    public Transform player;
    private float _playerPosZ; // Posizione sull'asse z del giocatore

    public AudioSource musicSource; // Componente AudioSource per la musica
    private string _musicpath;
    private string _musicname;
    private Coroutine _spawnCoroutine; // Riferimento alla coroutine di spawn delle sfere

    // Gruppo di variabile utilizzate per la gestione del ritmo di spawn
    public MidiFile midiFile;
    public string midiname;
    private string _midiPath;
    List<Note> notes = new List<Note>();
    private Melanchall.DryWetMidi.Interaction.Note[] array;
    public List<double> timeStamps = new List<double>();
    public List<double> beats = new List<double>();

    private float _time;

    [SerializeField]
    private GameObject reviewPanel;

    [SerializeField]
    private XRInteractorLineVisual lineRendererLeft;

    [SerializeField]
    private XRInteractorLineVisual lineRendererRight;

    [SerializeField]
    private TMP_Text textNotesHit;

    [SerializeField]
    private TMP_Text textPercentage;

    [SerializeField]
    private TMP_Text textRank;

    private InputDevice leftDevice;
    private InputDevice rightDevice;
    private List<InputDevice> foundControllers;

    private string selectedSong;
    
    private int _count = 0;


    // Start is called before the first frame update
    IEnumerator Start()
    {
        Time.timeScale = 1f;
        lineRendererRight.enabled = false;
        lineRendererLeft.enabled = false;
        reviewPanel.SetActive(false);
        _time = Time.deltaTime;
        
        debugPanel = FindObjectOfType<DebugPanel>();

        player = GameObject.FindGameObjectWithTag("Player").transform; // Trova il giocatore e ottiene il suo componente Transform
        _playerPosZ = player.position.z; // Calcola la posizione del giocatore sull'asse Z
        
        musicSource = GetComponent<AudioSource>(); // Ottiene il componente AudioSource
        
        selectedSong = PlayerPrefs.GetString("SelectedSong");
        switch(selectedSong)
        {
            case "uprising":
                filename = "uprising.txt";
                midiname = "uprising.mid";
                _musicname = "uprising.mp3";
                break;
            case "heatwaves":
                filename = "heatwaves.txt";
                midiname = "heatwaves.mid";
                _musicname = "heatwaves.mp3";
                break;
            case "sunflower":
                filename = "sunflower.txt";
                midiname = "sunflower.mid";
                _musicname = "sunflower.mp3";
                break;

        }
        


        _filepath = Path.Combine(Application.streamingAssetsPath, filename);
        UnityWebRequest www = UnityWebRequest.Get(_filepath);


        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            fileContent = www.downloadHandler.text;
            //debugPanel.UpdateDebugText("Contenuto del file: " + fileContent);
        }
        else
        {
            debugPanel.UpdateDebugText("Errore nell'accesso al file: " + www.error);
        }


        _midiPath = Path.Combine(Application.streamingAssetsPath, midiname);
        www = UnityWebRequest.Get(_midiPath);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            byte[] fileContent = www.downloadHandler.data;
            
            using (var memoryStream = new MemoryStream(fileContent))
            {
                midiFile = MidiFile.Read(memoryStream);
                // Esegui l'elaborazione delle note o qualsiasi altra logica desiderata

            }
        }
        else
        {
            debugPanel.UpdateDebugText("Errore nell'accesso al file: " + www.error);
        }
        GetNotesFromMidiFile();
        GetBeats(array);

        
        _musicpath = Path.Combine(Application.streamingAssetsPath, _musicname);
        www = UnityWebRequestMultimedia.GetAudioClip(_musicpath, AudioType.MPEG);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
            musicSource.clip = audioClip;
            musicSource.Play();
        }
        else
        {
            debugPanel.UpdateDebugText("Errore nell'accesso al file: " + www.error);
        }
        
        _spawnCoroutine = StartCoroutine(SpawnSphereCoroutine());

        // Si dichiara dove trovare i riferimenti agli oggetti delle altre classi:
        powerUp = FindObjectOfType<PowerUp>();
        scoreManager = FindObjectOfType<ScoreManager>();


        debugPanel.UpdateDebugText("Left Button: Pause Menu" + "\n" + "A: Score PowerUp" + "\n" + "B: Color PowerUp");

    }
    
    // Metodo per prendere le note dal file midi (richiamato nello start)
    private void GetNotesFromMidiFile()
    {
        notes = new List<Note>();
        IEnumerable<Note> notesCollection = midiFile.GetNotes();

        foreach (Note note in notesCollection)
        {
            notes.Add(note);
        }
        
        Note[] notesArray = notes.ToArray();
        
        // Le note vengono copiate all'interno di un array, oggetto della libreria Melanchall.
        // Da questo array si può ricavare il TempoMap della canzone (nel metodo GetBeats()). 
        array = new Melanchall.DryWetMidi.Interaction.Note[notesArray.Length];

        for (int i = 0; i < notesArray.Length; i++)
        {
            array[i] = notesArray[i];
        }
    }
    
    private void GetBeats(Melanchall.DryWetMidi.Interaction.Note[] array)
    {
        foreach (var note in array)
        {
            //if (note.NoteName == noteRestriction) => per applicare la restrizione ad un sottoinsieme di note
            
            // Se non avessimo copiato le note nell'array, oggetto della libreria Melanchall,
            // Il metodo TimeConverter avrebbe generato un errore.
            var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, midiFile.GetTempoMap());
            // Aggiunge il timestamp di ogni nota alla lista timeStamps
            timeStamps.Add((double)metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds + (double)metricTimeSpan.Milliseconds / 1000f);
        }

        double previousTimestamp = timeStamps[0];

        for (int i = 1; i < timeStamps.Count; i++)
        {
            double currentTimestamp = timeStamps[i];
            double timeDifference = currentTimestamp - previousTimestamp;
            // Lista in cui vengono salvati i tempi di spawn
            beats.Add(timeDifference);

            // Esegui qui le azioni desiderate quando si raggiunge il timestamp corrente

            previousTimestamp = currentTimestamp;
        }
    }

    // Coroutine per la lettura del file txt e l'avvio del pattern descritto
    private IEnumerator SpawnSphereCoroutine()
    {
        //while (true) // la courutine in questo modo continuerà a ripetere il pattern finchè non si ferma la musica
        //string[] lines = File.ReadAllLines(_filepath);
        string[] lines = fileContent.Split('\n');

        foreach (var pair in beats.Zip(lines, (beat, line) => new { Beat = beat, Line = line }))
            {
                var beat = pair.Beat;
                var line = pair.Line;

                string[] data = line.Split(' '); // Dividi la riga per ottenere l'indice del cubo e le coordinate di spawn

                    if (data.Length == 2)
                    {
                        int sphereindex = int.Parse(data[0]);

                        if (sphereindex == 0 || sphereindex == 1)
                        {
                            _count += 1; //conteggio dei cubi che vengono istanziati (potrebbero essere anche più delle note)
                        }
                        
                        // Rimuovi parentesi quadre e separa le coordinate
                        string[] coordinates = data[1].Trim('[', ']').Split(',');

                        if (coordinates.Length == 3)
                        {
                            // Ottieni x,y,z dalla lista cordinates
                            float x = float.Parse(coordinates[0], CultureInfo.InvariantCulture); 
                            float y = float.Parse(coordinates[1], CultureInfo.InvariantCulture); 
                            float z = float.Parse(coordinates[2], CultureInfo.InvariantCulture); 
                            // Calcolo della posizione
                            Vector3 position = new Vector3(x, y, z);
                            // Richiama il metodo per istanziare il cubo
                            SpawnSphere(sphereindex, position);
                        }
                        else
                        {
                            // In caso di formattazione delle coordinate non corretta
                            Debug.Log("Cordinates Error");
                        }
                    }
                    else if (data.Length == 4)
                    {
                        int sphereindex1 = int.Parse(data[0]);
                        int sphereindex2 = int.Parse(data[2]);
                        
                        if (sphereindex1 == 0 || sphereindex1 == 1)
                        {
                            _count += 1; //conteggio dei cubi che vengono istanziati (potrebbero essere anche più delle note)
                        }
                        if (sphereindex2 == 0 || sphereindex2 == 1)
                        {
                            _count += 1; //conteggio dei cubi che vengono istanziati (potrebbero essere anche più delle note)
                        }
                        
                        // Rimuovi parentesi quadre e separa le coordinate
                        string[] coordinates1 = data[1].Trim('[', ']').Split(',');
                        string[] coordinates2 = data[3].Trim('[', ']').Split(',');

                        if (coordinates1.Length == 3 && coordinates2.Length == 3)
                        {
                            // Ottieni x,y,z dalla lista cordinates
                            float x1 = float.Parse(coordinates1[0], CultureInfo.InvariantCulture);
                            float y1 = float.Parse(coordinates1[1], CultureInfo.InvariantCulture);
                            float z1 = float.Parse(coordinates1[2], CultureInfo.InvariantCulture);
                            
                            float x2 = float.Parse(coordinates2[0], CultureInfo.InvariantCulture);
                            float y2 = float.Parse(coordinates2[1], CultureInfo.InvariantCulture);
                            float z2 = float.Parse(coordinates2[2], CultureInfo.InvariantCulture);
                            
                            // Calcolo della posizione
                            Vector3 position1 = new Vector3(x1, y1, z1);
                            Vector3 position2 = new Vector3(x2, y2, z2);
                            
                            // Richiama il metodo per istanziare il cubo
                            SpawnSphere(sphereindex1, position1);
                            SpawnSphere(sphereindex2, position2);
                        }
                        
                        else
                        {
                            // In caso di formattazione delle coordinate non corretta
                            Debug.Log("Cordinates Error");
                        }
                    }
                    else
                    {
                        // Qualora il percorso del file indicato fosse errato
                        Debug.Log("File not found: " + _filepath);
                    }
                        
                    // Espressione che viene utilizzata all'interno di un metodo coroutine
                    // in Unity per creare un ritardo di tempo specifico (in questo caso, il beat)
                    //yield return new WaitForSeconds(_beat);
                        
                    yield return new WaitForSeconds((float)beat);
            }
            
            StopCoroutine(_spawnCoroutine);
    }

    // Metodo per l'instanziazione di un cubo
    private void SpawnSphere(int index, Vector3 point)
    {
        sphere = Instantiate(spheres[index], point, Quaternion.identity);
        spawnedSpheres.Add(sphere); // Il cubo creato viene aggiunto alla lista di elementi
    }

    // Update is called once per frame
    void Update()
    {
        MoveSpheres();
        _time += 1;
        if (!musicSource.isPlaying && _time > 5f && Time.timeScale!=0f)
        {
            Time.timeScale = 0f;
            reviewPanel.SetActive(true);
            lineRendererLeft.enabled = true;
            lineRendererRight.enabled = true;
            textNotesHit.text = scoreManager.countNotesHit.ToString();
            float percentage = Mathf.Round((scoreManager.countNotesHit * 100f) / _count);
            textPercentage.text = percentage.ToString()+"%";
            if (percentage >= 90)
            {
                textRank.text = "Rank S!!!!!";
            }
            else if(percentage>=70 && percentage < 90)
            {
                textRank.text = "Rank A!!!!!";
            }
            else if (percentage >= 40 && percentage<70)
            {
                textRank.text = "Rank B!!!!!";
            }
            else if (percentage < 40)
            {
                textRank.text = "Rank C!!!!!";
            }
        }
        
    }

    // Metodo per far muovere i cubi verso il giocatore
    private void MoveSpheres()
    {
        float distanceToMove = moveSpeed * Time.deltaTime;
        
        // Per ciclare su ogni cubo presente nella scena
        for (int i = 0; i < spawnedSpheres.Count; i++)
        {
            // Prendo il cubo i e lo sposto verso il giocatore
            // ( NOTA: Il giocatore si trova dietro rispetto allo spawner )
            GameObject sphere = spawnedSpheres[i];
            
            if (powerUp.activeSecond)
            {
                if (sphere.GetComponent<Renderer>().material.color != Color.red)
                {
                    sphere.GetComponent<Renderer>().material.color = Color.white;
                    sphere.GetComponent<Light>().color = Color.white;
                }
                
            }
            
            sphere.transform.Translate(Vector3.back * distanceToMove);

            // Calcolo ad ogni frame la posizione dei cubi lungo l'asse Z
            // E la distanza rispetto alla posizione lungo l'asse Z del giocatore
            float spherePosZ = sphere.transform.position.z;
            float distance = Mathf.Abs(spherePosZ - _playerPosZ);

            // Verifica se il cubo ha raggiunto il giocatore
            if (distance < 0.1f)
            {
                if (sphere.CompareTag("Bomba"))
                {
                    Destroy(sphere); // Distrugge il cubo
                    spawnedSpheres.RemoveAt(i); // Rimuove il cubo dalla lista dei cubi
                    i--; // Aggiorna l'indice dopo la rimozione
                }
                else
                {
                    Destroy(sphere); // Distrugge il cubo
                    spawnedSpheres.RemoveAt(i); // Rimuove il cubo dalla lista dei cubi
                    i--; // Aggiorna l'indice dopo la rimozione
                    powerUp.DecreaseHealth(5f); // Decrementa la vita del giocatore
                    scoreManager.count = 0; // Fa il reset a zero del contatore combo
                }
            }
        }
    }
}