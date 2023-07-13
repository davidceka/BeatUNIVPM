using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player; // Riferimento al transform del giocatore (XR Origin)
    public float heightOffset = 0.5f; // Offset dell'altezza dell'anello rispetto al giocatore

    void Update()
    {
        if (player != null)
        {
            // Aggiorna la posizione e la rotazione dell'oggetto vuoto in base al giocatore
            transform.position = player.position + Vector3.up * heightOffset;

            // Calcola la rotazione in base alla direzione del movimento della testa
            Vector3 direction = player.rotation * Vector3.forward;
            direction.y = 0f;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = targetRotation;
            }
        }
    }
}
