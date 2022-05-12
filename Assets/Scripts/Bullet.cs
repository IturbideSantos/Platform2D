using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public AudioClip shootAudio;
    public float speed;

    private Rigidbody2D _rb;
    private Vector2 _direction;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        Camera.main.GetComponent<AudioSource>().PlayOneShot(shootAudio);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        _rb.velocity = _direction * speed;
    }

    public void SetDirection(Vector2 dir)
    {
        this._direction = dir;
    }

    public void DestroyBullet()
    {
        Destroy(gameObject);
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        Enemy enemy = other.GetComponent<Enemy>();

        if (player != null)
        {
            player.DecreaseHealth();
        }

        if (enemy != null)
        {
            enemy.DecreaseHealth();
            Debug.Log("enemy hit");
        }

        DestroyBullet();
    }

}
