using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public AudioClip hurtAudio;
    public GameObject bulletPrefab;
    public int health;
    public float shootingInterval;
    public float distanceToShoot;

    private float _lastShoot;
    private GameObject _player;
    private GameManager _gameManager;
    void Start()
    {
        _gameManager = GameManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        _player = _gameManager.GetPlayer();

        if ( _player == null || _gameManager.Health <= 0) return;

        CheckRotation();

        if (DistanceToPlayer() < distanceToShoot && (Time.time-_lastShoot) > shootingInterval)
        {
            Shoot();
            _lastShoot = Time.time;
        }
    }

    private void CheckRotation()
    {
        Vector3 direction = _player.transform.position - transform.position;
        if (direction.x >= 0.0f) transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        else transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
    }

    private float  DistanceToPlayer()
    {
        return Mathf.Abs(_player.transform.position.x - transform.position.x);
    }

    private void Shoot()
    {
        Vector3 direction;
        if (transform.localScale.x == 1) direction = Vector2.right;
        else direction = Vector2.left;

        GameObject bullet = Instantiate(bulletPrefab, transform.position + direction * 0.1f, Quaternion.identity);
        bullet.GetComponent<Bullet>().SetDirection(direction);
    }

    public void DecreaseHealth()
    {
        health = health - 1;
        if (health == 0) Destroy(gameObject);
        Camera.main.GetComponent<AudioSource>().PlayOneShot(hurtAudio);
    }
}
