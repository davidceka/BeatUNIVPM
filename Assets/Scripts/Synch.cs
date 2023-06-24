using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;

// CLASSE PER LA GESTIONE DELLO SPAWNER
public class Synch : MonoBehaviour
{
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

    public Transform player;
    private float _playerPosZ; // Posizione sull'asse z del giocatore

    public AudioSource musicSource; // Componente AudioSource per la musica
    private Coroutine _spawnCoroutine; // Riferimento alla coroutine di spawn delle sfere
    public float beat = (60f / 105f) * 2f;
    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Trova il giocatore e ottiene il suo componente Transform
        _playerPosZ = player.position.z; // Calcola la posizione del giocatore sull'asse Z
        
        musicSource = GetComponent<AudioSource>(); // Ottiene il componente AudioSource
        
        filename = "trace_test.txt";
        _filepath = Path.Combine(Application.dataPath, "Scripts", filename); // Ottiene il percorso per leggere il file del pattern

        // Si dichiara dove trovare i riferimenti agli oggetti delle altre classi:
        powerUp = FindObjectOfType<PowerUp>();
        scoreManager = FindObjectOfType<ScoreManager>();
    }

    // Coroutine per la lettura del file txt e l'avvio del pattern descritto
    private IEnumerator SpawnSphereCoroutine()
    {
        while (true) // la courutine in questo modo continuerà a ripetere il pattern finchè non si ferma la musica
        {
            if (File.Exists(_filepath))
            {
                string[] lines = File.ReadAllLines(_filepath);

                foreach (string line in lines)
                {
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
                
                    // Espressione che viene utilizzata all'interno di un metodo coroutine
                    // in Unity per creare un ritardo di tempo specifico (in questo caso, il beat)
                    yield return new WaitForSeconds(beat);
                }
            }
            else
            {
                // Qualora il percorso del file indicato fosse errato
                Debug.Log("File not found: " + _filepath);
            }
        }
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
        if (musicSource.isPlaying && _spawnCoroutine == null)
        {
            _spawnCoroutine = StartCoroutine(SpawnSphereCoroutine());
        }
        else if (!musicSource.isPlaying && _spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
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