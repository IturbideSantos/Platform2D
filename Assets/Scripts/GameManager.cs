using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public Image[] HealthBar;

    public GameObject panelMenu;
    public GameObject panelPlay;
    public GameObject panelLevelCompleted;
    public GameObject panelGameOver;

    public Image[] LevelDozen;
    public Image[] LevelUnity;

    public GameObject[] levels;

    //public GameObject CharLifeIcon;

    public static GameManager Instance { get; private set; }

    public enum State { MENU, INIT, PLAY, LEVELCOMPLETED, LOADLEVEL, GAMEOVER }
    State _state;
    GameObject _currentPlayer;
    GameObject _currentLevel;
    bool _isSwitchingState;
    public Image[] CharLives;

    private int _lives;
    public int Lives
    {
        get { return _lives; }
        set
        {
            _lives = value;
            for (var i = 0; i < _lives; i++)
            {
                CharLives[i].enabled = true;
            }
            for (var i = _lives; i < CharLives.Length; i++)
            {
                CharLives[i].enabled = false;
            }
        }
    }

    private int _health;
    public int Health
    {
        get { return _health; }
        set
        {
            _health = value;
            for (var i = 0; i < _health; i++)
            {
                HealthBar[i].enabled = true;
            }
            for (var i = _health; i < HealthBar.Length; i++)
            {
                HealthBar[i].enabled = false;
            }
        }
    }

    private int _level;

    public GameObject GetPlayer()
    {
        return _currentPlayer;
    }
    public int Level
    {
        get { return _level; }
        set
        {
            _level = value;
            int levelCorrected = _level + 1;
            for (var i = 0; i < LevelDozen.Length; i++)
            {
                int unity = levelCorrected % 10; // Improve to avoid compilation errors
                int dozen = levelCorrected / 10; // Improve to avoid compilation 

                if (i == dozen) LevelDozen[i].enabled = true;
                else LevelDozen[i].enabled = false;

                if (i == unity) LevelUnity[i].enabled = true;
                else LevelUnity[i].enabled = false;

            }
        }
    }


    public void PlayClicked()
    {
        SwitchState(State.INIT);
    }

    void Start()
    {
        Instance = this;
        SwitchState(State.MENU);
    }


    public void SwitchState(State newState, float delay = 0)
    {
        StartCoroutine(SwitchDelay(newState, delay));
    }

    IEnumerator SwitchDelay(State newState, float delay)
    {
        _isSwitchingState = true;
        yield return new WaitForSeconds(delay);
        EndState();
        _state = newState;
        BeginState(newState);
        _isSwitchingState = false;
    }


    void BeginState(State newState)
    {
        switch (newState)
        {
            case State.MENU:
                Cursor.visible = true;
                panelMenu.SetActive(true);
                break;
            case State.INIT:
                Cursor.visible = false;
                panelPlay.SetActive(true);
                Level = 0;
                Lives = 3;
                if (_currentLevel != null)
                {
                    Destroy(_currentLevel);
                }
                SwitchState(State.LOADLEVEL);
                break;
            case State.PLAY:
                break;
            case State.LEVELCOMPLETED:
                Destroy(_currentPlayer);
                Destroy(_currentLevel);
                Level++;
                panelLevelCompleted.SetActive(true);
                SwitchState(State.LOADLEVEL, 2f);
                break;
            case State.LOADLEVEL:
                if (Level >= levels.Length)
                {
                    SwitchState(State.GAMEOVER);
                }
                else
                {
                    _currentLevel = Instantiate(levels[Level]);
                    SwitchState(State.PLAY);
                }
                break;
            case State.GAMEOVER:
                Destroy(_currentLevel);
                panelGameOver.SetActive(true);
                break;
        }
    }

    void Update()
    {
        switch (_state)
        {
            case State.MENU:
                break;
            case State.INIT:
                break;
            case State.PLAY:
                if (_currentPlayer == null)
                {
                    if (Lives > 0)
                    {
                        _currentPlayer = Instantiate(playerPrefab);
                        UnityEngine.Camera.main.GetComponent<CameraScript>().player = _currentPlayer;
                        
                    }
                    else
                    {
                        SwitchState(State.GAMEOVER);
                    }
                }
                if (_currentLevel != null && _currentLevel.transform.childCount == 0 && !_isSwitchingState)
                {
                    SwitchState(State.LEVELCOMPLETED);
                }
                break;
            case State.LEVELCOMPLETED:
                break;
            case State.LOADLEVEL:
                break;
            case State.GAMEOVER:
                if (Input.anyKeyDown)
                {
                    SwitchState(State.MENU);
                }
                break;
        }
    }

    void EndState()
    {
        switch (_state)
        {
            case State.MENU:
                panelMenu.SetActive(false);
                break;
            case State.INIT:
                break;
            case State.PLAY:
                break;
            case State.LEVELCOMPLETED:
                panelPlay.SetActive(false);
                panelLevelCompleted.SetActive(false);
                break;
            case State.LOADLEVEL:
                break;
            case State.GAMEOVER:
                panelPlay.SetActive(false);
                panelGameOver.SetActive(false);
                break;
        }
    }



}

