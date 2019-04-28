using System.Collections;
using UnityEngine;

public class TrainSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] MoveModeObjects;
    [SerializeField] GameObject Coin;
    [SerializeField] GameObject RailLight;
    [SerializeField] int coinProb = 5;
    [SerializeField] int minTrainCount = 12;
    [SerializeField] int maxTrainCount = 25;
    private Player player;
    private int posZ;
    int[] rotations = { 0, 90, 180, 270 };
    int direction;

    // Start is called before the first frame update
    void Start()
    {
        //Updates the matrix
        posZ = Mathf.RoundToInt(transform.position.z) + 4;
        player = GameObject.Find("Player").GetComponent<Player>();
        player.matrix[posZ, 10] = 1;
        player.matrix[posZ, 0] = 1;

        //Spawns rail light
        GameObject SpawnedRailLight = Instantiate(RailLight, new Vector3(Random.Range(-2, 3) + 0.5f, -0.465f, transform.position.z + 0.499f), Quaternion.identity);
        SpawnedRailLight.transform.parent = gameObject.transform;

        //Spawn coin
        Vector3 position = new Vector3(4f, 0.35f, transform.position.z); //Set spawns position (x is non-static)
        while (position.x <= 4 && position.x >= -4)   //Spawns while in inner range
        {
            //spawn probability
            if (Random.Range(0, 100) < coinProb)
            {
                Instantiate(Coin, position, Quaternion.Euler(0, rotations[Random.Range(0, 4)], 0));
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

    IEnumerator PreTrain()
    {
        GameObject light0 = transform.Find("RailRoad1(Clone)/Light0").gameObject;
        GameObject light1 = transform.Find("RailRoad1(Clone)/Light1").gameObject;
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(6, 10)); //Wait between trains
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
        localObject = Instantiate(MoveModeObjects[0], new Vector3(14 * direction, 0, transform.position.z), Quaternion.Euler(0, Rotation, 0)) as GameObject;
        localObject.GetComponent<Mover>().speed = 50 * direction;  
        //spawns First Train
        localObject = Instantiate(MoveModeObjects[1], localObject.transform.position + new Vector3(3.8f * direction, 0, 0), Quaternion.Euler(0, Rotation , 0)) as GameObject;
        localObject.GetComponent<Mover>().speed = 50 * direction;      
        //spawns Trains
        int trainCount = Random.Range(minTrainCount, maxTrainCount);
        while(trainCount != 0)
        {
            localObject = Instantiate(MoveModeObjects[1], localObject.transform.position + new Vector3(4 * direction, 0, 0), Quaternion.Euler(0, Rotation, 0)) as GameObject;
            localObject.GetComponent<Mover>().speed = 50 * direction;
            trainCount--;
        }
        //spawns Back Train
        localObject = Instantiate(MoveModeObjects[2], localObject.transform.position + new Vector3(4.2f * direction, 0, 0), Quaternion.Euler(0, Rotation, 0)) as GameObject;
        localObject.GetComponent<Mover>().speed = 50 * direction;
    }
}
