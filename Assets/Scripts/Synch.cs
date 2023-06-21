using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CLASSE PER LA GESTIONE DELLO SPAWNER
public class Synch : MonoBehaviour
{
    private static Synch _spawn; // Campo statico per il riferimento a Synch (Utilizzato in "Hit")
    
    public GameObject[] spheres;
    public List<GameObject> spawnedSpheres = new List<GameObject>(); // Lista dei cubi istanziati
    public Transform[] points;
    public float moveSpeed; // Velocità di movimento dei cubi

    public Transform player;
    private float _playerPosZ; // Posizione sull'asse z del giocatore

    private AudioSource _musicSource; // Componente AudioSource per la musica
    private Coroutine _spawnCoroutine; // Riferimento alla coroutine di spawn delle sfere
    public float beat = (60f / 105f) * 2f;
    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Trova il giocatore e ottiene il suo componente Transform
        _playerPosZ = player.position.z; // Calcola la posizione del giocatore sull'asse Z
        _musicSource = GetComponent<AudioSource>(); // Ottiene il componente AudioSource
    }

    // Coroutine per l'istanziazione dei cubi
    private IEnumerator SpawnSphereCoroutine()
    {
        while (true)
        {
            SpawnSphere();
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

    // Update is called once per frame
    void Update()
    {
        MoveSpheres();
        
        // Controlla lo stato di riproduzione della musica e interrompe/riavvia la coroutine di spawn dei cubi
        if (_musicSource.isPlaying && _spawnCoroutine == null)
        {
            _spawnCoroutine = StartCoroutine(SpawnSphereCoroutine());
        }
        else if (!_musicSource.isPlaying && _spawnCoroutine != null)
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
            
            // Verifica se la sfera ha raggiunto il giocatore
            if (distance < 0.1f)
            {
                Destroy(sphere);
                spawnedSpheres.RemoveAt(i);
                i--; // Aggiorna l'indice dopo la rimozione
            }
        }
    }
}