using System.Collections;
using UnityEngine;

public class Waterline : MonoBehaviour
{
    [SerializeField] GameObject[] Logs;
    [SerializeField] GameObject Waterlily;
    [SerializeField] GameObject Coin;
    [SerializeField] float waterlilyModeProb = 25;
    [SerializeField] float waterlilyProb = 15;
    [SerializeField] int coinProb = 5;
    [SerializeField] float minSpeed = 2;
    [SerializeField] float maxSpeed = 4;
    [SerializeField] int minOffset = 5;
    [SerializeField] int maxOffset = 8;
    private GameObject[] spawnedObjects = new GameObject[11];
    private GameObject[] Coins = new GameObject[11];
    private Player player;
    private int[] rotations = { 0, 90, 180, 270 };
    private float speed;
    private int offset;
    private float spawnPos;
    private float spawnRot;
    private int rotator;
    private int spawnObjectIndex = -1;
    private bool waterlilyMode;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>(); //Get Player

        //Updates the matrix
        int posZ = Mathf.RoundToInt(transform.position.z) + 5;
        player.obstacleMatrix[posZ, 10] = 1;
        player.obstacleMatrix[posZ, 0] = 1;


        //Decide spawn mode

        //WATERLILY MODE
        if (Random.Range(0, 100f) < waterlilyModeProb)
        {
            waterlilyMode = true;
            bool noWaterlily = true;
            Vector3 position = new Vector3(4f, 0.45f, transform.position.z); //Set spawns position (x is non-static)
            while (position.x <= 4 && position.x >= -4)   //Spawns while in inner range
            {
                //Spawns static object with inner probability
                if (Random.Range(0, 100) < waterlilyProb)
                {
                    spawnedObjects[Mathf.RoundToInt(position.x) + 4] = Instantiate(Waterlily, position, Quaternion.Euler(0, rotations[Random.Range(0, 4)], 0)) as GameObject;
                    spawnedObjects[Mathf.RoundToInt(position.x) + 4].transform.SetParent(transform);
                    noWaterlily = false;
                    //Spawns coins with coin probability
                    if (Random.Range(0, 100) < coinProb)
                    {
                        Coins[Mathf.RoundToInt(position.x) + 4] = Instantiate(Coin, position, Quaternion.Euler(0, rotations[Random.Range(0, 4)], 0)) as GameObject;
                        Coins[Mathf.RoundToInt(position.x) + 4].transform.SetParent(transform);
                    }

                }
                position.x -= 1; //Move position
            }
            if (noWaterlily) //Spawns one waterlily if none spawneds
            {
                int randomX = Random.Range(-4, 5);
                spawnedObjects[Mathf.RoundToInt(randomX) + 4] = Instantiate(Waterlily, new Vector3(randomX, 0.45f, transform.position.z), Quaternion.Euler(0, rotations[Random.Range(0, 4)], 0));
                spawnedObjects[Mathf.RoundToInt(randomX) + 4].transform.SetParent(transform);
                if (Random.Range(0, 100) < coinProb)
                {
                    Coins[Mathf.RoundToInt(randomX) + 4] = Instantiate(Coin, new Vector3(randomX, 0.45f, transform.position.z), Quaternion.Euler(0, rotations[Random.Range(0, 4)], 0)) as GameObject;
                    Coins[Mathf.RoundToInt(randomX) + 4].transform.SetParent(transform);
                }
            }
        }

        //LOG MODE
        else
        {
            waterlilyMode = false;
            // Randomly pick direction (50/50)
            if (Random.Range(0, 2) == 0) //Set right 
            {
                spawnPos = -15f;
                spawnRot = 180;
                rotator = -1;
            }
            else  //Set left
            {
                spawnPos = 15f;
                spawnRot = 0;
                rotator = 1;
            }

            offset = Random.Range(minOffset, maxOffset + 1); //Space between spawns
            speed = Random.Range(minSpeed, maxSpeed); //Speed
            InvokeRepeating("MoveMode", 0f, offset / speed); //Spawn moving object each seconds
        }
    }

    //Spawns moving object continouesly
    private void MoveMode()
    {
        spawnObjectIndex++;
        if (spawnObjectIndex == 10) { spawnObjectIndex = 0; }
        spawnedObjects[spawnObjectIndex] = Instantiate(Logs[Random.Range(0, Logs.Length)], new Vector3(spawnPos, -0.35f, transform.position.z), Quaternion.Euler(new Vector3(0, spawnRot, 0))) as GameObject;
        spawnedObjects[spawnObjectIndex].transform.SetParent(transform);
        spawnedObjects[spawnObjectIndex].GetComponent<Mover>().speed = speed * rotator;      //pass the speed variable into object
    }

    public void WorldChecker(float playerPos)
    {
        player.mover = Vector3.zero;
        int playerPosInt = Mathf.RoundToInt(playerPos);
        float objPos;
        bool searching = true;
        Animator playerAnimator = player.transform.Find("Chicken").GetComponent<Animator>();
        if (waterlilyMode) //Waterlilies
        {
            player.nextPos.y = 0.87f;
            player.nextPos.x = Mathf.RoundToInt(playerPos);
            if (spawnedObjects[playerPosInt + 4] != null) //Check for waterlily in front of player
            {
                //Pickup coin
                if (Coins[Mathf.RoundToInt(playerPos) + 4] != null)
                {
                    StartCoroutine(WorldCheckerTime(5, playerPos, 0.125f));
                }
                playerAnimator.SetTrigger("Float");
                StartCoroutine(WorldCheckerTime(0, playerPosInt, 0.166f));
                return;
            }
            else
            {
                playerAnimator.SetTrigger("Drown");
                StartCoroutine(WorldCheckerTime(2, playerPos, 0.42f));
                return;
            }
        }

        else //Logs
        {
            float size;
            player.nextPos.y = 1.03f;
            float sizeAdjuster;
            float estimater0;
            float estimater1;

            //Estimaite next player position based on log movement
            if (player.transform.position.z == transform.position.z) //Not estimating when already on log
            {
                estimater0 = 0;
                estimater1 = 0;
            }
            else
            {
                estimater0 = speed * rotator * -1 * player.moveTime * Mathf.Clamp(rotator, 0, 1);
                estimater1 = speed * rotator * -1 * player.moveTime * Mathf.Clamp(rotator * -1, 0, 1);
            }

            for (int x = spawnObjectIndex; searching == true; x--) //look for log in front of player
            {
                if (x == -1) { x = 9; } //go from 0 to 9
                if (spawnedObjects[x] == null) //if no log found
                {
                    searching = false;
                    if(player.transform.position.z == transform.position.z) { player.mover = new Vector3(speed * rotator * -1, 0, 0); }
                    playerAnimator.SetTrigger("Drown");
                    StartCoroutine(WorldCheckerTime(2, playerPos, 0.42f));
                    return;
                }
                else
                {
                    size = spawnedObjects[x].GetComponent<Mover>().size;
                    sizeAdjuster = (size / 2 % 1 * -1) - 0.5f; //2log = -0.5  3log = -1
                    objPos = spawnedObjects[x].transform.position.x;
                    if (playerPos >= objPos - (size / 2) + estimater0 && playerPos <= objPos + (size / 2) + estimater1) //if Player position is in size of log
                    {
                        for (int pos = 0; pos < Mathf.RoundToInt(size); pos++)
                        {
                            //Found exact nextPos point
                            if (playerPos >= objPos + sizeAdjuster + pos - 0.5f + estimater0 && playerPos <= objPos + sizeAdjuster + pos + 0.5f + estimater1)
                            {
                                player.nextPos.x = objPos + sizeAdjuster + pos + (speed * player.moveTime * rotator * -1);
                                playerAnimator.SetTrigger("Float");
                                StartCoroutine(WorldCheckerTime(1, x, 0.166f));
                                return;
                            }
                        }
                    }
                }
            }
        }
    }

    IEnumerator WorldCheckerTime(int switcher, float playerPos, float waitTime) //timed part of WorldChecker()
    {
        yield return new WaitForSeconds(waitTime);
        switch (switcher)
        {
            case 0: //Waterlilies
                spawnedObjects[Mathf.RoundToInt(playerPos) + 4].transform.Find("default").GetComponent<Animator>().SetTrigger("Float");
                break;
            case 1: //Logs
                spawnedObjects[Mathf.RoundToInt(playerPos)].transform.Find("default").GetComponent<Animator>().SetTrigger("Float");
                player.mover = new Vector3(speed * rotator * -1, 0, 0);
                break;
            case 2: //Drown
                player.state = Player.State.dead;
                GameObject.Find("Main Camera").GetComponent<Camera>().speed = 0;
                player.mover = Vector3.zero;
                break;
            case 5: //Coin
                player.coins++;
                player.coinText.text = player.coins.ToString();
                PlayerPrefs.SetInt("Money", player.coins);
                Destroy(Coins[Mathf.RoundToInt(playerPos) + 4]);
                break;
        }
    }
}
