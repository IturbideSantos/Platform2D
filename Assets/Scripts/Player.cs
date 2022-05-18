using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public AudioClip jumpAudio;
    public AudioClip landingAudio;
    public AudioClip deathAudio;
    public AudioClip hurtAudio;
    public GameObject bulletPrefab;
    public float jumpHeight;
    public float speed;

    private Rigidbody2D _rb;
    private Animator _animator;
    private float _horizontal;
    private bool _grounded;
    private GameManager _gameManager;
    private Renderer _renderer;

    bool _moveActive;
    bool _jumpActive;
    bool _decreaseHealthActive;
    bool _shootingActive;

    private enum Death { BYENEMY, BYFALLING }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _renderer = GetComponent<Renderer>();
        _gameManager = GameManager.Instance;
        _gameManager.Health = 3;

        _moveActive = true;
        _jumpActive = true;
        _decreaseHealthActive = true;
        _shootingActive = true;
    }

    void Update()
    {
        _horizontal = Input.GetAxisRaw("Horizontal");

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && _grounded)
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }

    private void FixedUpdate()
    {

        Move();

        CheckRotation();

        CheckGrounded();
    }

    private void Move()
    {
        if (_moveActive)
        {
            _rb.velocity = new Vector2(_horizontal * speed, _rb.velocity.y);
            if (_grounded) _animator.SetBool("running", _horizontal != 0);
        }
    }

    private void Killed(Death dead)
    {
        _moveActive = false;
        _jumpActive = false;
        _decreaseHealthActive = false;
        _shootingActive = false;

        switch (dead)
        {
            case Death.BYENEMY:
                UnityEngine.Camera.main.GetComponent<AudioSource>().PlayOneShot(deathAudio);
                _animator.SetTrigger("killed");
                _gameManager.Lives--;
                Destroy(this.gameObject);
                break;

            case Death.BYFALLING:
                UnityEngine.Camera.main.GetComponent<AudioSource>().PlayOneShot(deathAudio);
                _gameManager.Lives--;
                Destroy(this.gameObject);
                break;
        }
    }

    private void CheckRotation()
    {
        if (_moveActive)
        {
            if (_horizontal < 0.0f)
            {
                transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
            }
            else if (_horizontal > 0.0f)
            {
                transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }
        }
    }

    private void CheckGrounded()
    {
        if (Physics2D.Raycast(transform.position, Vector3.down, 0.1f))
        {
            if (_grounded == false)
            {
                Land();
            }
            _grounded = true;
        }
        else
        {
            _grounded = false;
        }
    }

    private void Land()
    {
        Camera.main.GetComponent<AudioSource>().PlayOneShot(landingAudio);
        _animator.SetTrigger("landing");
    }
    private void Jump()
    {
        if (_jumpActive)
        {
            _rb.AddForce(Vector2.up * jumpHeight);
            UnityEngine.Camera.main.GetComponent<AudioSource>().PlayOneShot(jumpAudio);
        }
    }

    private void Shoot()
    {
        if (_shootingActive)
        {
            Vector3 direction;
            if (transform.localScale.x == 1) direction = Vector2.right;
            else direction = Vector2.left;

            GameObject bullet = Instantiate(bulletPrefab, transform.position + direction * 0.1f, Quaternion.identity);
            bullet.GetComponent<Bullet>().SetDirection(direction);
            _animator.SetTrigger("shooting");
        }
    }

    public void DecreaseHealth()
    {
        if (_decreaseHealthActive)
        {
            _gameManager.Health--;
            Camera.main.GetComponent<AudioSource>().PlayOneShot(hurtAudio);
            if (_gameManager.Health == 0)
            {
                Killed(Death.BYENEMY);
            }

            _animator.SetTrigger("hit");
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "DeathZone")
        {
            Debug.Log("DeathZone");
            
            Killed(Death.BYFALLING);
        }
        else if (collision.tag == "Bullet")
        {
            Debug.Log("Hit");
            DecreaseHealth();
        }
        else if (collision.tag == "Checkpoint")
        {
            _gameManager.PlayerLastLocation = collision.transform.position;
            collision.GetComponent<SpriteRenderer>().color = Color.yellow;
        }
    }
}
