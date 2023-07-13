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
    public Transform[] points;
    public string filename; // Nome del file da cui leggere il pattern
    private string _filepath; // Percorso del file da cui leggere il pattern
    public float moveSpeed; // Velocità di movimento dei cubi

    string fileContent;

    public Transform player;
    private float _playerPosZ; // Posizione sull'asse z del giocatore

    public AudioSource musicSource; // Componente AudioSource per la musica
    private string _musicpath;
    private string _musicname;
    public bool musicPlaying = false;
    private Coroutine _spawnCoroutine; // Riferimento alla coroutine di spawn delle sfere
    public bool startCoroutine = false;
    //private float _beat = (60f / 105f) * 2f;
    
    // Gruppo di variabile utilizzate per la gestione del ritmo di spawn
    public MidiFile midiFile;
    public string midiname;
    private string _midiPath;
    public Melanchall.DryWetMidi.MusicTheory.NoteName noteRestriction;
    List<Note> notes = new List<Note>();
    private Melanchall.DryWetMidi.Interaction.Note[] array;
    public List<double> timeStamps = new List<double>();
    public List<double> beats = new List<double>();

    // Start is called before the first frame update
    IEnumerator Start()
    {
        debugPanel = FindObjectOfType<DebugPanel>();

        player = GameObject.FindGameObjectWithTag("Player").transform; // Trova il giocatore e ottiene il suo componente Transform
        _playerPosZ = player.position.z; // Calcola la posizione del giocatore sull'asse Z
        
        musicSource = GetComponent<AudioSource>(); // Ottiene il componente AudioSource
        
        filename = "trace_test.txt";

        _filepath = Path.Combine(Application.streamingAssetsPath, "trace_test.txt");
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


        _midiPath = Path.Combine(Application.streamingAssetsPath, "butter.mid");
        www = UnityWebRequest.Get(_midiPath);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            byte[] fileContent = www.downloadHandler.data;
            //debugPanel.UpdateDebugText("Contenuto del file: " + fileContent);
            using (var memoryStream = new MemoryStream(fileContent))
            {
                midiFile = MidiFile.Read(memoryStream);
                // Esegui l'elaborazione delle note o qualsiasi altra logica desiderata
                //debugPanel.UpdateDebugText(midiFile.GetType().ToString());

            }
        }
        else
        {
            debugPanel.UpdateDebugText("Errore nell'accesso al file: " + www.error);
        }
        GetNotesFromMidiFile();
        GetBeats(array);

        _musicname = "butter.mp3";
        _musicpath = Path.Combine(Application.streamingAssetsPath, _musicname);
        www = UnityWebRequestMultimedia.GetAudioClip(_musicpath, AudioType.MPEG);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
            musicSource.clip = audioClip;
            musicSource.Play();
            //debugPanel.UpdateDebugText("file musicale: " + _musicname);
        }
        else
        {
            debugPanel.UpdateDebugText("Errore nell'accesso al file: " + www.error);
        }
        
        _spawnCoroutine = StartCoroutine(SpawnSphereCoroutine());

        // Si dichiara dove trovare i riferimenti agli oggetti delle altre classi:
        powerUp = FindObjectOfType<PowerUp>();
        scoreManager = FindObjectOfType<ScoreManager>();
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
        beats.Add(previousTimestamp);
        
        for (int i = 1; i < timeStamps.Count; i++)
        {
            double currentTimestamp = timeStamps[i];
            double timeDifference = currentTimestamp - previousTimestamp;
            // Lista in cui vengono salvati i tempi di spawn
            beats.Add(timeDifference);

            // Esegui qui le azioni desiderate quando si raggiunge il timestamp corrente

            previousTimestamp = currentTimestamp;
        }
        Debug.Log(beats.Count); // Da togliere a fine progetto
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
    
/*
    private IEnumerator SpawnSphereCoroutine()
    {
        while (true)
        {
            SpawnSphereTest();
            // Espressione che viene utilizzata all'interno di un metodo coroutine
            // in Unity per creare un ritardo di tempo specifico (in questo caso, il beat)
            yield return new WaitForSeconds(beat);
        }
    }

    // Istanza una nuovo cubo
    private void SpawnSphere()
    {
        // Il cubo si genera randomicamente da uno dei punti di spawn
        GameObject sphere;
        int randomNumber = UnityEngine.Random.Range(0, 101);
        if (randomNumber >= 0 && randomNumber <= 10)
            sphere = Instantiate(spheres[2], points[Random.Range(0, points.Length)]);
        else if (randomNumber >=11 && randomNumber <= 55)
            sphere = Instantiate(spheres[1], points[Random.Range(0, points.Length)]);
        else
            sphere = Instantiate(spheres[0], points[Random.Range(0, points.Length)]);

        sphere.transform.localPosition = Vector3.zero;
        // Lo spawn può avvenire con un angolazione diversa
        sphere.transform.Rotate(Vector3.forward, 90f * Random.Range(0, 4));

        spawnedSpheres.Add(sphere); // Il cubo creato viene aggiunto alla lista di elementi
    }
*/

    // Update is called once per frame
    void Update()
    {
        MoveSpheres();

        // Controlla lo stato di riproduzione della musica e interrompe/riavvia la coroutine di spawn dei cubi
        /*
        if (!startCoroutine && _spawnCoroutine == null)
        {
            _spawnCoroutine = StartCoroutine(SpawnSphereCoroutine());
            startCoroutine = true;
        }
        */
        /*
        else if (!musicSource.isPlaying && _spawnCoroutine != null)
        {
            Debug.Log("stop");
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }
        */
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
                sphere.GetComponent<Renderer>().material.color = Color.white;
                sphere.GetComponent<Light>().color = Color.white;
            }
            
            sphere.transform.Translate(Vector3.back * distanceToMove);

            // Calcolo ad ogni frame la posizione dei cubi lungo l'asse Z
            // E la distanza rispetto alla posizione lungo l'asse Z del giocatore
            float spherePosZ = sphere.transform.position.z;
            float distance = Mathf.Abs(spherePosZ - _playerPosZ);
            
            if (!musicPlaying && spherePosZ <= 1.95f)
            {
                musicSource.Play();
                musicPlaying = true;
            }
            
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