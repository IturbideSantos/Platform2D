using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
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
    private bool _jump;
    private bool _crounch;
    private bool _grounded;
    private bool _shooting;
    private GameManager _gameManager;
    private Renderer _renderer;

    private enum Death { BYENEMY, BYFALLING }
    public enum State {START, IDLE, JUMP, FALLING, LANDING, RUNNING, CROUNCHING, SHOOTING, DYING}
    State _state;
    bool _isSwitchingState;
    bool moveActive;
    bool jumpActive;
    bool decreaseHealthActive;
    bool shootingActive;
    bool lastPlayingAnimation = false;

    IEnumerator SwitchDelay(State newState, float delay)
    {
        _isSwitchingState = true;
        yield return new WaitForSeconds(delay);
        EndState();
        _state = newState;
        BeginState(newState);
        _isSwitchingState = false;
    }

    public void SwitchState(State newState, float delay = 0)
    {
        StartCoroutine(SwitchDelay(newState, delay));
    }

    private void BeginState(State newState)
    {
        switch (newState)
        {
            case State.START:
                _gameManager.Health = 3;
                break;
            case State.IDLE:
                moveActive = true;
                jumpActive = true;
                decreaseHealthActive = true;
                shootingActive = true;
                break;
            case State.JUMP:
                moveActive = true;
                jumpActive = false;
                decreaseHealthActive = true;
                shootingActive = true;
                break;
            case State.FALLING:
                moveActive = true;
                jumpActive = false;
                decreaseHealthActive = true;
                shootingActive = true;
                break;
            case State.LANDING:
                moveActive = true;
                jumpActive = false;
                decreaseHealthActive = true;
                shootingActive = true;
                break;
            case State.RUNNING:
                moveActive = true;
                jumpActive = true;
                decreaseHealthActive = true;
                shootingActive = true;
                break;
            case State.CROUNCHING:
                moveActive = false;
                jumpActive = false;
                decreaseHealthActive = true;
                shootingActive = true;
                break;
            case State.SHOOTING:
                moveActive = true;
                jumpActive = true;
                decreaseHealthActive = true;
                shootingActive = true;
                break;
            case State.DYING:
                moveActive = false;
                jumpActive = false;
                decreaseHealthActive = false;
                shootingActive = false;

                Killed(Death.BYENEMY);
                break;

        }
    }

    private void EndState()
    {
        switch (_state)
        {
            case State.START:
                break;
            case State.IDLE:
                break;
            case State.JUMP:
                break;
            case State.FALLING:
                break;
            case State.LANDING:
                break;
            case State.RUNNING:
                break;
            case State.CROUNCHING:
                break;
            case State.SHOOTING:
                break;
            case State.DYING:
                break;
        }
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _renderer = GetComponent<Renderer>();
        _gameManager = GameManager.Instance;
        SwitchState(State.START);
    }

    private void Update()
    {
        _horizontal = Input.GetAxisRaw("Horizontal");
        _jump = (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow));
        _crounch = (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow));
        _shooting = Input.GetKeyDown(KeyCode.Space);
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && _grounded)
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }

        switch (_state)
        {
            case State.START:
                Debug.Log("START");
                SwitchState(State.IDLE);
                break;
            case State.IDLE:
                if (_horizontal != 0) SwitchState(State.RUNNING);
                if (_jump) SwitchState(State.JUMP);
                if (_crounch) SwitchState(State.CROUNCHING);
                if (_shooting) SwitchState(State.SHOOTING);
                break;
            case State.JUMP:
                if (!_grounded) SwitchState(State.FALLING);
                break;
            case State.FALLING:
                if (_grounded) SwitchState(State.LANDING);
                break;
            case State.LANDING:
                SwitchState(State.IDLE);
                break;
            case State.RUNNING:
                if (_horizontal == 0 && _grounded) SwitchState(State.IDLE);
                if (_jump) SwitchState(State.JUMP);
                if (_crounch) SwitchState(State.CROUNCHING);
                if (_shooting) SwitchState(State.SHOOTING);
                break;
            case State.CROUNCHING:
                if (_horizontal == 0 && !_crounch) SwitchState(State.IDLE);
                if (_shooting) SwitchState(State.SHOOTING);
                break;
            case State.SHOOTING:
                SwitchState(State.IDLE);
                break;
            case State.DYING:
                if (AnimatorIsPlaying("DeathByEnemy"))
                {
                    lastPlayingAnimation = true;
                }
                if (lastPlayingAnimation && !AnimatorIsPlaying("DeathByEnemy"))
                {
                    Debug.Log("Waiting animation end");
                    Destroy(this.gameObject);
                }
                break;
        }
        if (_gameManager.Health == 0 && _state != State.START && _state != State.DYING)
        {
            Debug.Log("DEAD");
            SwitchState(State.DYING);
        }
    }

    private bool AnimatorIsPlaying(string animationName)
    {
        return _animator.GetCurrentAnimatorStateInfo(0).length >
               _animator.GetCurrentAnimatorStateInfo(0).normalizedTime &&
               _animator.GetCurrentAnimatorStateInfo(0).IsName(animationName);
    }
    private void FixedUpdate()
    {
        Move();

        CheckRotation();

        CheckGrounded();
    }

    private void Move()
    {
        if (moveActive)
        {
            _rb.velocity = new Vector2(_horizontal * speed, _rb.velocity.y);

            if (_grounded) _animator.SetBool("running", _horizontal != 0);
        }
    }

    private void Killed(Death dead)
    {
        switch (dead)
        {
            case Death.BYENEMY:
                UnityEngine.Camera.main.GetComponent<AudioSource>().PlayOneShot(deathAudio);
                _animator.SetTrigger("killed");
                _gameManager.Lives--;
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
        if (moveActive)
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
        if (jumpActive)
        {
            _rb.AddForce(Vector2.up * jumpHeight);
            UnityEngine.Camera.main.GetComponent<AudioSource>().PlayOneShot(jumpAudio);
        }
    }

    private void Shoot()
    {
        if (shootingActive)
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
        _gameManager.Health--;
        Camera.main.GetComponent<AudioSource>().PlayOneShot(hurtAudio);
        _animator.SetTrigger("hit");
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "DeathZone")
        {
            Debug.Log("DeathZone");
            Killed(Death.BYFALLING);
        }
    }
}
