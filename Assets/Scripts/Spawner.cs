using UnityEngine;

public class Spawner: MonoBehaviour
{
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
    private Player player;
    private int posZ;
    int[] rotations = { 0, 90, 180, 270 };
    private float speed;
    private int offset;
    enum Direction {left, right };
    Direction direction;

    // Start is called before the first frame update
    void Start()
    {
        //Updates the matrix
        posZ = Mathf.RoundToInt(transform.position.z) + 4;
        player = GameObject.Find("Player").GetComponent<Player>();
        player.matrix[posZ, 10] = 1;
        player.matrix[posZ, 0] = 1;

        if (Random.Range(0, 100f) < staticModeProbability) //decide spawn mode
        {
            //spawns static obstacles (trees, rocks, waterlilys)
            StaticMode();
        }
        else
        {
            //spawns moving obstacles (cars, logs, trains)
            if (Random.Range(0, 2) == 0) // Randomly pick direction (50/50)
            {
                direction = Direction.right; //Set right 
            }
            else
            {
                direction = Direction.left;  //Set left
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
                        player.matrix[posZ, Mathf.RoundToInt(position.x) + 5] = 1;
                    }

                    //Prevent no waterlily on line
                    else if(gameObject.name == "WaterLine(Clone)")
                    {
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
        GameObject localObject;
        if (direction == Direction.left) //Left direction
        {
            localObject = Instantiate(MoveModeObjects[Random.Range(0, MoveModeObjects.Length)], new Vector3(11.5f, -0.35f, transform.position.z), Quaternion.identity) as GameObject;
            localObject.GetComponent<Mover>().speed = speed;      //pass the speed variable into object
        }
        else                            //Right direction
        {
            localObject = Instantiate(MoveModeObjects[Random.Range(0, MoveModeObjects.Length)], new Vector3(-8.5f, -0.35f, transform.position.z), Quaternion.Euler(new Vector3(0, 180, 0))) as GameObject;
            localObject.GetComponent<Mover>().speed = speed * -1; //pass the speed variable into object
        }
    }
}
