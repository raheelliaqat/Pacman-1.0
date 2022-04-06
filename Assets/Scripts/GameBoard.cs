using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameBoard : MonoBehaviour
{
    public int pacManLives = 3;
    private static int boardWidth = 28;
    private static int boardHeight = 36;
    private bool didStartDeath = false;
    private bool didStartConsumed = false;

    private static int playerLevel = 1;


    public int playerPelletsConsumed = 0;


    public int totalPellets = 0;
    public static int player1Score = 0;

    public bool shouldBlink = false;

    public float blinkIntervalTime = 0.1f;
    private float blinkIntervalTimer = 0;

    public AudioClip ConsumedGhostAudioClip;
    public AudioClip backgroundAudioNormal;
    public AudioClip backgroundAudioFrightened;
    public AudioClip backgroundAudioPacMantDeath;

    public Sprite mazeBlue;
    public Sprite mazeWhite;

    public static int ghostConsumedRunningScore;

    public Text readyText;
    public Text highScoreText;
    public Text playerOneUp;
    public Text playerOneScoreText;
    public Image playerLives2;
    public Image playerLives3;
    public Text consumedGhostScoreText;
    public GameObject[,] board = new GameObject[boardWidth, boardHeight];
    // Start is called before the first frame update
    void Start()
    {
        Object[] objects = GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (GameObject o in objects)
        {
            Vector2 pos = o.transform.position;

            if (o.name != "PacMan" && o.name != "Nodes" && o.name != "NonNodes" && o.name != "Maze" && o.name != "Pellets" && o.tag != "Ghost" && o.tag != "ghostHome" && o.tag!="UIElements")
            {

                if (o.GetComponent<Tile>() != null)
                {
                    if (o.GetComponent<Tile>().isPellet || o.GetComponent<Tile>().isSupperPellet)
                    {
                        totalPellets++;
                    }
                }
                board[(int)pos.x, (int)pos.y] = o;
            }
            else
            {
                //Debug.Log("Found Pacman at:" + pos);
            }
        }
        StartGame();
    }

     void Update()
    {
        UpdateUI();

        CheckPelletsConsumed();

        CheckShouldBlink();
    }

    void UpdateUI()
    {
        playerOneScoreText.text = player1Score.ToString();
        if(pacManLives == 3)
        {
            playerLives3.enabled = true;
            playerLives2.enabled = true;
        }
        else
        if(pacManLives == 2)
        {
            playerLives3.enabled = false;
            playerLives2.enabled = true;
        }
        else
        if(pacManLives == 1)
        {
            playerLives3.enabled = false;
            playerLives2.enabled = false;
        }
    }

    void CheckPelletsConsumed()
    {
        if(totalPellets == playerPelletsConsumed)
        {
            PlayerWin();
        }

    }

    void PlayerWin()
    {
        //Debug.Log("Player can now move to the next level");
        playerLevel++;
        StartCoroutine(ProcessWin(2));
    }

    IEnumerator ProcessWin(float delay)
    {
        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<PacMan>().canMove = false;
        pacMan.transform.GetComponent<Animator>().enabled = false;
        transform.GetComponent<AudioSource>().Stop();
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach(GameObject ghosts in o)
        {
            ghosts.transform.GetComponent<ghost>().canMove = false;
            ghosts.transform.GetComponent<Animator>().enabled = false;
        }   

        yield return new WaitForSeconds(delay);
        StartCoroutine(BlinkBoard(2));
    }

    IEnumerator BlinkBoard(float delay)
    {
        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach(GameObject ghosts in o)
        {
            ghosts.transform.GetComponent<SpriteRenderer>().enabled = false;
        }
        //- Blink Board
        shouldBlink = true;
        yield return new WaitForSeconds(delay);
        // Restart the Game at the next level
        shouldBlink = false;
        StartNextLevel();
    }

    private void StartNextLevel()
    {
        SceneManager.LoadScene("GameMenu");
    }

    private void CheckShouldBlink()
    {
        if(shouldBlink)
        {
            if(blinkIntervalTimer < blinkIntervalTime)
            {
                blinkIntervalTimer += Time.deltaTime; 
            }
            else
            {
                blinkIntervalTimer = 0;
                if(GameObject.Find("Maze").transform.GetComponent<SpriteRenderer>().sprite == mazeBlue)
                {
                    GameObject.Find("Maze").transform.GetComponent<SpriteRenderer>().sprite = mazeWhite;
                }
                else
                {
                    GameObject.Find("Maze").transform.GetComponent<SpriteRenderer>().sprite = mazeBlue;
                } 
            }
        }
    }

    public void StartGame()
    {

        StartCoroutine(StartBlinking(playerOneUp));
        // Hide all ghosts
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach(GameObject ghost in o)
        {
            ghost.transform.GetComponent<SpriteRenderer>().enabled = false;
            ghost.transform.GetComponent<ghost>().canMove = false;
        }

        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;
        pacMan.transform.GetComponent<PacMan>().canMove = false;
        StartCoroutine(ShowObjectsAfter(2.2f));
    }
    public void StartConsumed(ghost consumedGhost)
    {
        if(!didStartConsumed)
        {
            didStartConsumed = true;
            // - pause all the ghosts
            GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
            foreach(GameObject ghosts in o)
            {
                ghosts.transform.GetComponent<ghost>().canMove = false;
            }

            // - pause PacMan
            GameObject pacMan = GameObject.Find("PacMan");
            pacMan.transform.GetComponent<PacMan>().canMove = false;

            // - Hide PacMan
            pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;

            // - Hide Consumed Ghost
            consumedGhost.transform.GetComponent<SpriteRenderer>().enabled = false;

            // - Stop BackGround Music
            transform.GetComponent<AudioSource>().Stop();

            Vector2 pos = consumedGhost.transform.position;
            Vector2 viewPortPoint = Camera.main.WorldToViewportPoint(pos);

            consumedGhostScoreText.GetComponent<RectTransform>().anchorMin = viewPortPoint;
            consumedGhostScoreText.GetComponent<RectTransform>().anchorMax = viewPortPoint;

            consumedGhostScoreText.text = ghostConsumedRunningScore.ToString();
            
            consumedGhostScoreText.GetComponent<Text>().enabled = true;

            //-Play the Consumed Sound
            transform.GetComponent<AudioSource>().PlayOneShot(ConsumedGhostAudioClip);

            //- wait for Audio clip to finish
            StartCoroutine(ProcessConsumedAfter(0.75f, consumedGhost));
        }
    }
    IEnumerator StartBlinking(Text blinkText)
    {
        yield return new WaitForSeconds(0.25f);
        blinkText.GetComponent<Text>().enabled = !blinkText.GetComponent<Text>().enabled;
        StartCoroutine(StartBlinking(blinkText));
    }

    IEnumerator ProcessConsumedAfter(float delay, ghost consumedGhost)
    {
        yield return new WaitForSeconds(delay);
        // - Hide the Score
        consumedGhostScoreText.GetComponent<Text>().enabled = false;

        // - Show PacMan
        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = true;

        // - Show Consumed Ghost
        consumedGhost.transform.GetComponent<SpriteRenderer>().enabled = true;

        // - Resume all ghosts
        GameObject [] o = GameObject.FindGameObjectsWithTag("Ghost");

        foreach(GameObject ghosts in o)
        {
            ghosts.transform.GetComponent<ghost>().canMove = true;
        }

        // - Resume Pacman
        pacMan.transform.GetComponent<PacMan>().canMove = true;

        // - Start Background Music
        transform.GetComponent<AudioSource>().Play();

        didStartConsumed = false;
    }

    IEnumerator ShowObjectsAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<SpriteRenderer>().enabled = true;
        }

        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = true;

        StartCoroutine(StartGameAfter(2));
    }

    IEnumerator StartGameAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<ghost>().canMove = true;
        }

        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<PacMan>().canMove = true;


        readyText.transform.GetComponent<Text>().enabled = false;

        transform.GetComponent<AudioSource>().clip = backgroundAudioNormal;
        transform.GetComponent<AudioSource>().Play();
    }

    public void StartDeath()
    {
        if (!didStartDeath)
        {
            StopAllCoroutines();
            playerOneUp.GetComponent<Text>().enabled = true;
            didStartDeath = true;

            GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

            foreach (GameObject ghosts in o)
            {
                ghosts.transform.GetComponent<ghost>().canMove = false;
            }

            GameObject pacMan = GameObject.Find("PacMan");
            pacMan.transform.GetComponent<PacMan>().canMove = false;
            pacMan.transform.GetComponent<Animator>().enabled = false;
            transform.GetComponent<AudioSource>().Stop();
            StartCoroutine(ProcessDeathAfter(2));
        }
    }

    IEnumerator ProcessDeathAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

        foreach (GameObject ghosts in o)
        {
            ghosts.transform.GetComponent<SpriteRenderer>().enabled = false;
        }

        StartCoroutine(ProcessDeathAnimation (1.9f));
    }

    IEnumerator ProcessDeathAnimation(float delay)
    {
        GameObject pacMan = GameObject.Find("PacMan");

        pacMan.transform.localScale = new Vector3(1, 1, 1);
        pacMan.transform.localRotation = Quaternion.Euler(0, 0, 0);

        pacMan.transform.GetComponent<Animator>().runtimeAnimatorController = pacMan.transform.GetComponent<PacMan>().deathAnimation;
        pacMan.transform.GetComponent<Animator>().enabled = true;

        transform.GetComponent<AudioSource>().clip = backgroundAudioPacMantDeath;
        transform.GetComponent<AudioSource>().Play();

        yield return new WaitForSeconds(delay);
        StartCoroutine(ProcessRestart(2));
    }

    IEnumerator ProcessRestart(float delay)
    {
        pacManLives -= 1;
        if (pacManLives == 0)
        {
            readyText.transform.GetComponent<Text>().text = "GAME OVER";
            readyText.transform.GetComponent<Text>().color = Color.red;
            readyText.transform.GetComponent<Text>().enabled = true;
            GameObject pacMan = GameObject.Find("PacMan");
            pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;

            transform.GetComponent<AudioSource>().Stop();
            StartCoroutine(ProcessGameOver(2));
        }
        else
        {
            
            readyText.transform.GetComponent<Text>().enabled = true;

            GameObject pacMan = GameObject.Find("PacMan");
            pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;

            transform.GetComponent<AudioSource>().Stop();

            yield return new WaitForSeconds(delay);
            GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
            foreach (GameObject ghost in o)
            {
                ghost.transform.GetComponent<SpriteRenderer>().enabled = true;
                ghost.transform.GetComponent<ghost>().MoveToStartingPosition();
                ghost.transform.GetComponent<ghost>().ChangeMode(global::ghost.Mode.scatter);//Calling Scattered Mode again when Ghosts consume you during frightened Mode
            }


            pacMan.transform.GetComponent<SpriteRenderer>().enabled = true;
            pacMan.transform.GetComponent<PacMan>().MoveToStartingPosition();
            Restart();
        }
    }


    IEnumerator ProcessGameOver(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("GameMenu");
    }
    public void Restart()
    {
        readyText.transform.GetComponent<Text>().enabled = false;
        didStartDeath = false;
        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<PacMan>().Restart();
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach(GameObject ghosts in o)
        {
            ghosts.transform.GetComponent<ghost>().Restart();
        }
        transform.GetComponent<AudioSource>().clip = backgroundAudioNormal;
        transform.GetComponent<AudioSource>().Play();
        didStartDeath = false;
    }
    
}
