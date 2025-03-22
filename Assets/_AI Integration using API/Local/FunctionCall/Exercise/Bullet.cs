using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;

    void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        
        Destroy(other.gameObject); // Destroy the object the bullet collided with
        Destroy(gameObject); // Destroy the bullet itself
    }
}