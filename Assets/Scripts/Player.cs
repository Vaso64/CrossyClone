using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class Player : MonoBehaviour
{
    public GameObject[] world = new GameObject[500];
    [SerializeField] public GameObject[] lines;
    public int[,] obstacleMatrix = new int[999, 11];
    private Animator animator;
    public int score = 0;
    public int coins;
    public enum State { idle, move, dead, floating, restarting, readytostart, paused, unpausing};
    public State state = State.readytostart;
    private State previousState;
    public GameObject pauseOverlay;
    public Text pauseText;
    private int spawnPos = 0;
    private Animator canvas;
    private Touch touch;
    private Vector2 touchStart;
    private Vector2 touchDirection;
    private Text scoreText;
    public Text coinText;
    private float startTime;
    public Vector3 nextPos;
    private Vector3 startPos;
    public Quaternion nextRot;
    private Quaternion startRot;
    private Camera camera;
    private float t = 0;
    [SerializeField] public float moveTime = 0.2f;
    public Vector3 mover;
    
    private void Start()
    {
        //Store starting lines into world array
        for (; spawnPos != 7; spawnPos++)
        {   
            world[spawnPos] = GameObject.Find("GrassLine" + spawnPos);
        }
        //Generate more lines into world array
        for (; spawnPos != 30; spawnPos++)
        {
            world[spawnPos] = Instantiate(lines[UnityEngine.Random.Range(0, 5)], new Vector3(0, 0, spawnPos), Quaternion.identity) as GameObject;
        }

        //Get various components
        canvas = GameObject.Find("Canvas").GetComponent<Animator>(); //get Canvas Animator (UI Render)
        camera = GameObject.Find("Main Camera").GetComponent<Camera>(); //get Camera
        animator = transform.Find("Chicken").GetComponent<Animator>(); //get Animator
        scoreText = GameObject.Find("Canvas").transform.Find("Score").GetComponent<Text>();
        coinText = GameObject.Find("Canvas").transform.Find("CoinText").GetComponent<Text>();

        //Load Coin value
        coins = PlayerPrefs.GetInt("Money");
        coinText.text = coins.ToString();

        //Rotates device screen/resolution
        InvokeRepeating("ScreenRotator", 0, 0.5f);

        //Lock display refresh rate
        //Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    private void Update()
    {
        transform.position += mover * Time.deltaTime; //Move object to direction
        if(state != State.dead && (transform.position.x < -5f || transform.position.x > 5f)) //dead on outside of world space
        {
            state = State.dead;
        }
        InputManager(); //Handles the input
        if (state == State.move || state == State.floating)
        {
            MoveUpdate();
        }
    }

    //Move the object acording to input
    private void MoveUpdate() //Update part of Move()
    {
        t += Time.deltaTime / moveTime; //time of movement
        transform.localPosition = Vector3.Lerp(startPos, nextPos, t); //move
        transform.localRotation = Quaternion.Lerp(startRot, nextRot, t * 2);
        //Return to idle on end
        if (t >= 1)
        {
            state = State.idle;
            t = 0;
        }
    }
    private void InputManager() //Handles Input
    {

        //TOCUHSCREEN HANDLER
        if(Input.touchCount > 0 && state == State.idle || state == State.floating)
        {
            touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStart = touch.position;
                    break;
                case TouchPhase.Ended:
                    touchDirection = touchStart - touch.position;
                    if(Mathf.Abs(touchDirection.x) > Mathf.Abs(touchDirection.y) && Mathf.Abs(touchDirection.x) > 100)
                    {
                        if(touchDirection.x > 0) { Move(-90, Vector3.left); } //LEFT
                        else { Move(90, Vector3.right); } //RIGHT
                    }
                    else
                    {
                        if(touchDirection.y > 0 && Mathf.Abs(touchDirection.y) > 100) { Move(180, Vector3.back); } //DOWN
                        else { Move(0, Vector3.forward); } //UP
                    }
                    break;
            } 
        }
        if((Input.anyKeyDown || Input.touchCount == 1 ) && state == State.readytostart)
        {
            state = State.idle;
            canvas.SetTrigger("StartGame");
            camera.speed = 0.6f;
        }

        if (state != State.paused && Input.GetKeyDown(KeyCode.Escape)) //PAUSE
        {
            Pause();
        }
        else if (state == State.paused && (Input.anyKeyDown || Input.touchCount > 0)) //UNPAUSE
        {
            Pause();
        }

        
        //KEYBOARD HANDLER
        if ((Input.GetKeyDown(KeyCode.R) && state != State.restarting && state != State.readytostart) || state == State.dead && (Input.touchCount > 0 || Input.anyKeyDown)) //RESTART
        {
            StartCoroutine(Restart());
        }

        //Debug.Log(state);
        if (state == State.idle || state == State.floating)
        {
            
            if (Input.GetKeyDown(KeyCode.W)) //Move forward
            {
                Move(0, Vector3.forward);
            }
            if (Input.GetKeyDown(KeyCode.D)) //Turn right
            {
                Move(90, Vector3.right);
            }
            if (Input.GetKeyDown(KeyCode.A)) //Turn left
            {
                Move(-90, Vector3.left);
            }
            if (Input.GetKeyDown(KeyCode.S)) //Turn back
            {
                Move(180, Vector3.back);
            }
        }
        
    }

    IEnumerator Restart()
    {
        state = State.restarting;
        canvas.SetTrigger("Restart");
        yield return new WaitForSeconds(1.2f); //Wait for faded screen
        //Return player to start
        animator.Rebind();
        mover = Vector3.zero;
        transform.position = new Vector3(0, 1.2f, 4);
        transform.rotation = Quaternion.Euler(Vector3.zero);

        //Clear world
        for(int x = 7; x != world.Length; x++)
        {
            Destroy(world[x]);
        }
        Array.Clear(obstacleMatrix, 0, obstacleMatrix.Length);
        for (int x = 0; x != 7; x++)
        {
            foreach (Transform child in world[x].transform)
            {
                if(child.tag == "Obstacle" || child.tag == "Coin")
                Destroy(child.gameObject);
            }
            world[x].GetComponent<Grassline>().Start();
        }

        //Adjust other variables
        spawnPos = 0;
        camera.speed = 0;
        camera.transform.position = new Vector3(2.8f, 10, -3);
        state = State.readytostart;
        animator.SetTrigger("Restart");
        Start();
    }

    private void Move(float rot, Vector3 direction) //Move player in world
    {
        if (obstacleMatrix[Mathf.RoundToInt(transform.position.z + 4 + direction.z), Mathf.RoundToInt(transform.position.x + 5 + direction.x)] != 1) //Check for obstacle in world
        {
            //Starts moving
            state = State.move;

            //Sets from-to transform
            startPos = transform.position;
            nextPos = startPos + direction;
            startRot = transform.rotation;
            nextRot = Quaternion.Euler(new Vector3(0, rot, 0));
            int nextPosZint = Mathf.RoundToInt(nextPos.z);
            int startPosZint = Mathf.RoundToInt(startPos.z);

            //Spawn next line (RENDER)
            if (nextPos.z > spawnPos - 20)
            {
                WolrdSpawner();
            }

            //Update score
            if (nextPos.z - 4 > score)
            {
                score++;
                scoreText.text = score.ToString();
            }

            //Garbage Collector
            if(nextPosZint - 12 > 6 && world[nextPosZint - 12] != null)
            {
                Destroy(world[nextPosZint - 12]);
                world[nextPosZint - 12] = null;
            }

            //Stops old kill timer
            switch (world[Mathf.RoundToInt(startPos.z)].tag)
            {
                case "Road":
                    world[startPosZint].GetComponent<Roadline>().StopAllCoroutines();
                    break;
                case "Water":
                    world[startPosZint].GetComponent<Waterline>().StopAllCoroutines();
                    break;
                default:
                    break;
            }

            //Starts new kill timer
            switch (world[nextPosZint].tag)
            {
                case "Grass":
                    world[nextPosZint].GetComponent<Grassline>().WorldChecker(nextPos.x);
                    break;
                case "Road":
                    world[nextPosZint].GetComponent<Roadline>().WorldChecker(nextPos.x);
                    break;
                case "Water":
                    world[nextPosZint].GetComponent<Waterline>().WorldChecker(nextPos.x);
                    break;
                case "Rail":
                    world[nextPosZint].GetComponent<Railline>().WorldChecker(nextPos.x);
                    break;
                default:
                    break;
            }
        }
    }

    void WolrdSpawner() //TODO More complex spawning system
    {
        world[spawnPos] = Instantiate(lines[UnityEngine.Random.Range(0, 5)], new Vector3(0, 0, spawnPos), Quaternion.identity) as GameObject;
        spawnPos++;
    }

    public void Pause()
    {
        if(state != State.unpausing)
        {
            StartCoroutine(PauseTime());
        }
    }

    private IEnumerator PauseTime()
    {
        //Pause
        if (state != State.paused)
        {
            pauseOverlay.SetActive(true);
            Time.timeScale = 0;
            previousState = state;
            state = State.paused;
            yield break;
        }

        //Unpause
        if(state == State.paused)
        {
            state = State.unpausing;
            for(int x = 3; x > 0; x--)
            {
                pauseText.text = x.ToString();
                yield return new WaitForSecondsRealtime(1);
            }
            pauseOverlay.SetActive(false);
            pauseText.text = "PAUSED";
            state = previousState;
            Time.timeScale = 1;
        }
    }

    private void ScreenRotator()
    {
        switch (Input.deviceOrientation)
        {
            case DeviceOrientation.Portrait:
                Screen.SetResolution(1080, 1920, true);
                GameObject.Find("Main Camera").GetComponent<UnityEngine.Camera>().orthographicSize = 6f;
                break;
            case DeviceOrientation.PortraitUpsideDown:
                Screen.SetResolution(1080, 1920, true);
                GameObject.Find("Main Camera").GetComponent<UnityEngine.Camera>().orthographicSize = 6f;
                break;
            case DeviceOrientation.LandscapeLeft:
                Screen.SetResolution(1920, 1080, true);
                GameObject.Find("Main Camera").GetComponent<UnityEngine.Camera>().orthographicSize = 4f;
                break;
            case DeviceOrientation.LandscapeRight:
                Screen.SetResolution(1920, 1080, true);
                GameObject.Find("Main Camera").GetComponent<UnityEngine.Camera>().orthographicSize = 4f;
                break;
        }
    }
}
