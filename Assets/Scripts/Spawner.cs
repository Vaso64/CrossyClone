using System.Collections;
using UnityEngine;

public class Spawner: MonoBehaviour
{
    [SerializeField] int identity;
    [SerializeField] GameObject[] MoveModeObjects;
    [SerializeField] GameObject[] StaticModeObject;
    [SerializeField] GameObject Coin;
    [SerializeField] int staticModeProbability = 25;
    [SerializeField] float minSpeed = 3;
    [SerializeField] float maxSpeed = 6;
    [SerializeField] int minOffset = 3;
    [SerializeField] int maxOffset = 6;
    [SerializeField] int staticInProb = 15;
    [SerializeField] int staticOutProb = 90;
    [SerializeField] int coinProb = 5;
    [SerializeField] private GameObject[] spawnedObjects = new GameObject[10];
    private bool staticMode;
    private int spawnObjectIndex = -1;
    private Player player;
    private int posZ;
    int[] rotations = { 0, 90, 180, 270 };
    private float speed;
    private int offset;
    private float spawnPos;
    private float spawnRot;
    private int rotator;

    // Start is called before the first frame update
    void Start()
    {
        //Updates the matrix
        posZ = Mathf.RoundToInt(transform.position.z) + 4;
        player = GameObject.Find("Player").GetComponent<Player>();
        player.obstacleMatrix[posZ, 10] = 1;
        player.obstacleMatrix[posZ, 0] = 1;

        if (Random.Range(0, 100f) < staticModeProbability) //decide spawn mode
        {
            //spawns static obstacles (trees, rocks, waterlilys)
            staticMode = true;
            StaticMode();
        }
        else
        {
            //spawns moving obstacles (cars, logs, trains)
            staticMode = false;
            // Randomly pick direction (50/50)
            if (Random.Range(0, 2) == 0) //Set right 
            {
                spawnPos = -8.5f;  
                spawnRot = 180;
                rotator = -1;
            }
            else  //Set left
            {
                spawnPos = 11.5f;
                spawnRot = 0;
                rotator = 1;
            }

            offset = Random.Range(minOffset, maxOffset + 1); //Space between spawns
            speed = Random.Range(minSpeed, maxSpeed); //Speed
            InvokeRepeating("MoveMode", 0f, offset / speed); //Spawn moving object each seconds
        }
    }

    private void StaticMode()
    {
        bool noWaterlily = true;
        GameObject localObject;
        Vector3 position = new Vector3(8f, 0.45f, transform.position.z); //Set spawns position (x is non-static)
        while(position.x > -8) //Spawns while in WORLD range
        {
            while (position.x >= 5 || position.x <= -5 && position.x >= -8) //Spawns while in outer range
            {
                //Spawns static objects with outer probability
                if (Random.Range(0, 100) < staticOutProb)
                {
                    Instantiate(StaticModeObject[Random.Range(0, StaticModeObject.Length)], position, Quaternion.Euler(0, rotations[Random.Range(0,4)], 0));
                }
                position.x -= 1; //Move position
            }
            while (position.x <= 4 && position.x >= -4)   //Spawns while in inner range
            {
                //Spawns static object with inner probability
                if (Random.Range(0, 100) < staticInProb)
                {
                    localObject = Instantiate(StaticModeObject[Random.Range(0, StaticModeObject.Length)], position, Quaternion.Euler(0, rotations[Random.Range(0, 4)], 0)) as GameObject;
                    //Exclude waterlily from obstacle matrix
                    if(localObject.name != "Waterlily(Clone)")
                    {
                        player.obstacleMatrix[posZ, Mathf.RoundToInt(position.x) + 5] = 1;
                    }

                    //Keep at least one waterlily on line
                    else if(localObject.name == "Waterlily(Clone)")
                    {
                        spawnedObjects[Mathf.RoundToInt(position.x) + 4] = localObject;
                        noWaterlily = false;
                    }
                }
                //Spawns coins with coin probability
                else if (Random.Range(0, 100) < coinProb)
                {
                    Instantiate(Coin, position, Quaternion.Euler(0, rotations[Random.Range(0, 4)], 0));
                }
                position.x -= 1; //Move position
            }
        }
        //Spawn waterlily if none found
        if (noWaterlily && gameObject.name == "WaterLine(Clone)")
        {
            Instantiate(StaticModeObject[0], new Vector3(Random.Range(-4, 4), 0.45f, transform.position.z), Quaternion.Euler(0, rotations[Random.Range(0, 4)], 0));
        }
    }

    //Spawns moving object continouesly
    private void MoveMode()
    {
        spawnObjectIndex++;
        if (spawnObjectIndex == 10) { spawnObjectIndex = 0; }
        spawnedObjects[spawnObjectIndex] = Instantiate(MoveModeObjects[Random.Range(0, MoveModeObjects.Length)], new Vector3(spawnPos, -0.35f, transform.position.z), Quaternion.Euler(new Vector3(0, spawnRot, 0))) as GameObject;
        spawnedObjects[spawnObjectIndex].GetComponent<Mover>().speed = speed * rotator;      //pass the speed variable into object
    }

    public float WorldChecker(float playerPos)
    {
        int playerPosInt = Mathf.RoundToInt(playerPos);
        float objPos;
        bool searching = true;
        Animator playerAnimator = player.transform.Find("Chicken").GetComponent<Animator>();
        switch (identity)
        {
            case 0: //Grass
                player.nextPos.y = 0.92f;
                player.nextPos.x = Mathf.RoundToInt(playerPos);
                playerAnimator.SetTrigger("Jump");
                return -1;
            case 1: //Water
                if (staticMode) //Waterlilies
                {
                    if(spawnedObjects[playerPosInt + 4] != null) //Check for waterlily in front of player
                    {
                        player.nextPos.y = 0.92f;
                        playerAnimator.SetTrigger("Float");
                        StartCoroutine(WorldCheckerTime(0, playerPosInt));
                        return -1;
                    }
                    else
                    {
                        playerAnimator.SetTrigger("Drown");
                        return 0.42f;
                    }
                }

                else if(!staticMode) //Logs
                {
                    float size;
                    player.nextPos.y = 1.03f;
                    float sizeAdjuster;
                    for(int x = spawnObjectIndex; searching == true; x--) //look for log in front of player
                    {
                        if (x == -1) { x = 9; } //go from 0 to 9
                        if (spawnedObjects[x] == null) //if no log found
                        {
                            searching = false;
                            playerAnimator.SetTrigger("Drown");
                            return 0.42f;
                        }
                        else
                        {
                            size = spawnedObjects[x].GetComponent<Mover>().size;
                            sizeAdjuster = (size / 2 % 1 * -1) - 0.5f; //2log = -0.5  3log = -1
                            objPos = spawnedObjects[x].transform.position.x;
                            if (playerPos >= objPos - (size / 2) && playerPos <= objPos + (size / 2)) //if Player position is in size of log
                            {
                                for (int pos = 0; pos < Mathf.RoundToInt(size); pos++)
                                {
                                    //Found exact nextPos point
                                    if (playerPos >= objPos + sizeAdjuster + pos - 0.5f && playerPos <= objPos + sizeAdjuster + pos + 0.5f)
                                    {
                                        player.nextPos.x = objPos + sizeAdjuster + pos + (speed * player.moveTime * rotator * -1);
                                        //Time.timeScale = 0;
                                        playerAnimator.SetTrigger("Float");
                                        StartCoroutine(WorldCheckerTime(1, x));
                                        return -1;
                                    }
                                }
                            }
                        }
                    }
                }
                return -1;
            case 2: //Road
                player.nextPos.y = 0.82f;
                player.nextPos.x = Mathf.RoundToInt(playerPos);
                float smallestDistance = 100;
                float objDist;
                for (int x = spawnObjectIndex; searching == true; x--)
                {
                    if (x == -1) { x = 9; } //go from 0 to 9
                    if (spawnedObjects[x] == null) { searching = false; }
                    else
                    {
                        objPos = spawnedObjects[x].transform.position.x;
                        objDist = Mathf.Abs(playerPos - objPos);
                        if ((rotator == 1 && objPos > playerPos || rotator == -1 && objPos < playerPos) && objDist < smallestDistance) //Find car nearest to player
                        {
                            smallestDistance = objDist;
                        }
                    }
                }
                playerAnimator.SetTrigger("Jump");
                return smallestDistance / speed;
            case 3: //Railroad
                return -1;
            default:
                return -1;
        }
    }

    IEnumerator WorldCheckerTime(int switcher, int playerPos) //timed part of WorldChecker()
    {
        switch (switcher)
        {
            case 0: //
                yield return new WaitForSeconds(player.moveTime);
                spawnedObjects[playerPos + 4].transform.Find("default").GetComponent<Animator>().SetTrigger("Float");
                break;
            case 1:
                yield return new WaitForSeconds(player.moveTime);
                spawnedObjects[playerPos].transform.Find("default").GetComponent<Animator>().SetTrigger("Float");
                player.mover = new Vector3(speed * rotator * -1, 0, 0);
                break;

        }
    }
}
