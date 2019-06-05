using System.Collections;
using UnityEngine;

public class Railline : MonoBehaviour
{
    [SerializeField] GameObject[] Trains;
    [SerializeField] GameObject Coin;
    [SerializeField] GameObject RailLight;
    [SerializeField] int coinProb = 5;
    [SerializeField] int minTrainCount = 12;
    [SerializeField] int maxTrainCount = 25;
    private GameObject[] Coins = new GameObject[10];
    private Player player;
    private Animator playerAnimator;
    private GameObject[] trainEnds = new GameObject[2];
    private Coroutine coroutine;
    private int posZ;
    private float waitTime;
    private float waitStartTime;
    private int[] rotations = { 0, 90, 180, 270 };
    private int direction;

    // Start is called before the first frame update
    void Start()
    {
        //Updates the matrix
        posZ = Mathf.RoundToInt(transform.position.z) + 4;
        player = GameObject.Find("Player").GetComponent<Player>();
        playerAnimator = player.transform.Find("Chicken").GetComponent<Animator>();
        player.obstacleMatrix[posZ, 10] = 1;
        player.obstacleMatrix[posZ, 0] = 1;

        //Spawns rail light
        GameObject SpawnedRailLight = Instantiate(RailLight, new Vector3(Random.Range(-2, 3) + 0.5f, -0.55f, transform.position.z + 0.499f), Quaternion.identity);
        SpawnedRailLight.transform.parent = gameObject.transform;

        //Spawn coin
        Vector3 position = new Vector3(4f, 0.3f, transform.position.z); //Set spawns position (x is non-static)
        while (position.x <= 4 && position.x >= -4)   //Spawns while in inner range
        {
            //spawn probability
            if (Random.Range(0, 100) < coinProb)
            {
                Coins[Mathf.RoundToInt(position.x) + 5] = Instantiate(Coin, position, Quaternion.Euler(0, rotations[Random.Range(0, 4)], 0)) as GameObject;
                Coins[Mathf.RoundToInt(position.x) + 5].transform.SetParent(transform);

            }
            position.x -= 1; //Move position
        }

        // Randomly pick direction (50/50)
        if (Random.Range(0, 2) == 0)
        {
            direction = -1; //Set right 
        }
        else
        {
            direction = 1;  //Set left
        }

        // Start pre-Main Coroutine
        StartCoroutine("PreTrain");
    }

    private IEnumerator PreTrain()
    {
        GameObject light0 = transform.Find("RailRoad1(Clone)/Light0").gameObject;
        GameObject light1 = transform.Find("RailRoad1(Clone)/Light1").gameObject;
        while (true)
        {
            waitTime = Random.Range(6f, 10f);
            waitStartTime = Time.time;
            yield return new WaitForSeconds(waitTime); //Wait between trains
            //Starts light warning
            for (int n = 3; n != 0; n--)
            {
                light0.SetActive(true);
                light1.SetActive(false);
                yield return new WaitForSeconds(0.3f);
                light0.SetActive(false);
                light1.SetActive(true);
                yield return new WaitForSeconds(0.3f);
            }
            Train(); //Spawns Train
            yield return new WaitForSeconds(0.5f);
            //Turn off lights
            light0.SetActive(false);
            light1.SetActive(false);
        }
    }

    //Spawns moving object continouesly
    private void Train()
    {
        GameObject localObject; //Empty object for passing variables
        //set spawning rotation according to direcion
        int Rotation = 0;
        if (direction == -1)
        {
            Rotation = 180;
        }

        //spawns Front Train
        trainEnds[0] = Instantiate(Trains[0], new Vector3(14 * direction, 0, transform.position.z), Quaternion.Euler(0, Rotation, 0)) as GameObject;
        trainEnds[0].transform.SetParent(transform);
        trainEnds[0].GetComponent<Mover>().speed = 50 * direction;
        //spawns First Train
        localObject = Instantiate(Trains[1], trainEnds[0].transform.position + new Vector3(3.8f * direction, 0, 0), Quaternion.Euler(0, Rotation, 0)) as GameObject;
        localObject.transform.SetParent(transform);
        localObject.GetComponent<Mover>().speed = 50 * direction;
        //spawns Trains
        int trainCount = Random.Range(minTrainCount, maxTrainCount);
        while (trainCount != 0)
        {
            localObject = Instantiate(Trains[1], localObject.transform.position + new Vector3(4 * direction, 0, 0), Quaternion.Euler(0, Rotation, 0)) as GameObject;
            localObject.transform.SetParent(transform);
            localObject.GetComponent<Mover>().speed = 50 * direction;
            trainCount--;
        }
        //spawns Back Train
        trainEnds[1] = Instantiate(Trains[2], localObject.transform.position + new Vector3(4.2f * direction, 0, 0), Quaternion.Euler(0, Rotation, 0)) as GameObject;
        trainEnds[1].transform.SetParent(transform);
        trainEnds[1].GetComponent<Mover>().speed = 50 * direction;
    }

    public void WorldChecker(float playerPos)
    {
        player.mover = Vector3.zero;
        player.nextPos.y = 0.72f;
        player.nextPos.x = Mathf.RoundToInt(playerPos);

        //Calculate train position
        float firstTrain;
        float lastTrain;
        if (trainEnds[0] == null) { firstTrain = 15 * direction * -1; }
        else { firstTrain = trainEnds[0].transform.position.x + 50 * direction * (player.moveTime / -2); }

        //Check for bumpup
        if (trainEnds [1] != null) //If train exists
        {
            lastTrain = trainEnds[1].transform.position.x + 50 * direction * (player.moveTime / -2);
            if (direction == 1 && playerPos > firstTrain && playerPos < lastTrain || direction == -1 && playerPos < firstTrain && playerPos > lastTrain)
            {
                playerAnimator.SetTrigger("Jump");
                StartCoroutine(WorldCheckerTime(0, playerPos, player.moveTime / 2));
                return;
            }
        }

        //Pickup coin
        if (Coins[Mathf.RoundToInt(playerPos) + 5] != null)
        {
            StartCoroutine(WorldCheckerTime(5, playerPos, 0.125f));
        }

        //Runover
        playerAnimator.SetTrigger("Jump");
        if (trainEnds[0] != null && (direction == 1 && playerPos < firstTrain || direction == -1 && playerPos > firstTrain))
        {
            coroutine = StartCoroutine(WorldCheckerTime(2, playerPos, Mathf.Abs(trainEnds[0].transform.position.x - playerPos) / 50));
        }
        else
        {
            coroutine = StartCoroutine(WorldCheckerTime(2, playerPos, waitTime - (Time.time - waitStartTime) + 1.84f + Mathf.Abs(playerPos - 14 * direction) / 50));
        }
        return;
    }

    IEnumerator WorldCheckerTime(int switcher, float playerPos, float waitTime) //timed part of WorldChecker()
    {
        yield return new WaitForSeconds(waitTime);
        switch (switcher)
        {
            case 0: //Bumpup
                player.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(-30, 30)));
                player.mover = new Vector3(50 * direction * -1, 0, 0);
                playerAnimator.SetTrigger("Bumpup");
                player.state = Player.State.dead;
                GameObject.Find("Main Camera").GetComponent<Camera>().speed = 0;
                break;
            case 2: //Runover
                if (player.transform.position.z > transform.position.z - 0.999f && player.transform.position.z < transform.position.z + 0.999f)
                {
                    player.mover = new Vector3(50 * -direction, 0, 0);
                    playerAnimator.SetTrigger("Runover");
                    player.state = Player.State.dead;
                    GameObject.Find("Main Camera").GetComponent<Camera>().speed = 0;
                }
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