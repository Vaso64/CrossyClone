using System.Collections;
using UnityEngine;

public class Roadline : MonoBehaviour
{
    [SerializeField] GameObject[] Cars;
    [SerializeField] GameObject Coin;
    [SerializeField] float minSpeed = 3;
    [SerializeField] float maxSpeed = 6;
    [SerializeField] int minOffset = 3;
    [SerializeField] int maxOffset = 6;
    [SerializeField] int coinProb = 5;
    private GameObject[] spawnedObjects = new GameObject[10];
    private GameObject[] Coins = new GameObject[10];
    private int[] rotations = { 0, 90, 180, 270 };
    private int spawnObjectIndex = -1;
    private Player player;
    private Animator playerAnimator;
    private float speed;
    private int offset;
    private float spawnPos;
    private float spawnRot;
    private int rotator;

    // Start is called before the first frame update
    void Start()
    {
        //Updates the matrix
        int posZ = Mathf.RoundToInt(transform.position.z) + 4;
        player = GameObject.Find("Player").GetComponent<Player>();
        playerAnimator = player.transform.Find("Chicken").GetComponent<Animator>();
        player.obstacleMatrix[posZ, 10] = 1;
        player.obstacleMatrix[posZ, 0] = 1;

        //Spawn coins
        Vector3 position = new Vector3(4f, 0.3f, transform.position.z); //Set spawns position (x is non-static)
        while (position.x <= 4 && position.x >= -4)   //Spawns while in inner range
        {
            if (Random.Range(0, 100) < coinProb)
            {
                Coins[Mathf.RoundToInt(position.x) + 5] = Instantiate(Coin, position, Quaternion.Euler(0, rotations[Random.Range(0, 4)], 0)) as GameObject;
                Coins[Mathf.RoundToInt(position.x) + 5].transform.SetParent(transform);
            }
            position.x -= 1; //Move position
        }

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

    //Spawns moving object continouesly
    private void MoveMode()
    {
        spawnObjectIndex++;
        if (spawnObjectIndex == 10) { spawnObjectIndex = 0; }
        spawnedObjects[spawnObjectIndex] = Instantiate(Cars[Random.Range(0, Cars.Length)], new Vector3(spawnPos, -0.45f, transform.position.z), Quaternion.Euler(new Vector3(0, spawnRot, 0))) as GameObject;
        spawnedObjects[spawnObjectIndex].transform.SetParent(transform);
        spawnedObjects[spawnObjectIndex].GetComponent<Mover>().speed = speed * rotator;      //pass the speed variable into object
    }

    public void WorldChecker(float playerPos)
    {
        player.mover = Vector3.zero;
        player.nextPos.y = 0.72f;
        player.nextPos.x = Mathf.RoundToInt(playerPos);
        float smallestDistance = 100;
        float objPos;
        float objDist;
        float length;
        bool searching = true;
        int x;

        //Check for possible bump up
        for(x = spawnObjectIndex; searching == true; x--)
        {
            if (x == -1) { x = 9; } //go from 0 to 9
            if (spawnedObjects[x] == null) { searching = false; }
            else
            {
                objPos = spawnedObjects[x].transform.position.x + speed * rotator * -0.083f;
                length = spawnedObjects[x].GetComponent<Mover>().size;
                if (rotator == 1 && playerPos > objPos && playerPos < objPos + length || rotator == -1 && playerPos < objPos && playerPos > objPos - length)
                {
                    if (Mathf.RoundToInt(player.transform.position.x) == playerPos)
                    {
                        playerAnimator.SetTrigger("Bumpup");
                        StartCoroutine(WorldCheckerTime(0, playerPos, player.moveTime / 2));
                        return;
                    }
                }
            }
        }

        searching = true;
        //Get run over time
        for (x = spawnObjectIndex; searching == true; x--)
        {
            if (x == -1) { x = 9; } //go from 0 to 9
            if (spawnedObjects[x] == null) { searching = false; }
            else
            {
                objPos = spawnedObjects[x].transform.position.x;
                objDist = Mathf.Abs(player.transform.position.x - objPos);
                if ((rotator == 1 && objPos > player.transform.position.x || rotator == -1 && objPos < player.transform.position.x) && objDist < smallestDistance) //Find car nearest to player
                {
                    smallestDistance = Mathf.Abs(playerPos - objPos);
                }
            }
        }
        //Pickup coin
        if (Coins[Mathf.RoundToInt(playerPos) + 5] != null)
        {
            StartCoroutine(WorldCheckerTime(5, playerPos, 0.125f));
        }

        playerAnimator.SetTrigger("Jump");
        StartCoroutine(WorldCheckerTime(2, playerPos, smallestDistance / speed));
        return;
    }

    public IEnumerator WorldCheckerTime(int switcher, float playerPos, float waitTime) //timed part of WorldChecker()
    {
        yield return new WaitForSeconds(waitTime);
        switch (switcher)
        {
            case 0: //Bumpup
                player.transform.rotation = Quaternion.Euler(player.nextRot.eulerAngles.x, player.nextRot.eulerAngles.y, Random.Range(-30, 30));
                player.mover = new Vector3(speed * rotator * -1, 0, 0);
                player.state = Player.State.dead;
                GameObject.Find("Main Camera").GetComponent<Camera>().speed = 0;
                break;
            case 2: //Runover
                playerAnimator.SetTrigger("Runover");
                player.state = Player.State.dead;
                GameObject.Find("Main Camera").GetComponent<Camera>().speed = 0;
                break;
            case 5: //Coin
                player.coins++;
                player.coinText.text = player.coins.ToString();
                PlayerPrefs.SetInt("Money", player.coins);
                Destroy(Coins[Mathf.RoundToInt(playerPos) + 5]);
                break;
        }
    }
}
