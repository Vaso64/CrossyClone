using System.Collections;
using UnityEngine;

public class Grassline : MonoBehaviour
{
    [SerializeField] GameObject[] Obstacles;
    [SerializeField] GameObject Coin;
    [SerializeField] int staticInProb = 15;
    [SerializeField] int staticOutProb = 90;
    [SerializeField] int coinProb = 5;
    private GameObject[] Coins = new GameObject[10];
    private Player player;
    private int[] rotations = { 0, 90, 180, 270 };
    private GameObject localObject;
    // Start is called before the first frame update
    public void Start()
    {
        //Updates the matrix
        int posZ = Mathf.RoundToInt(transform.position.z) + 4;
        player = GameObject.Find("Player").GetComponent<Player>();
        player.obstacleMatrix[posZ, 10] = 1;
        player.obstacleMatrix[posZ, 0] = 1;

        //SPAWNS
        Vector3 position = new Vector3(12f, 0.15f, transform.position.z); //Set spawns position (x is non-static)
        while (position.x > -12) //Spawns while in WORLD range
        {
            while (position.x >= 5 || position.x <= -5 && position.x >= -12) //Spawns while in outer range
            {
                //Spawns static objects with outer probability
                if (Random.Range(0, 100) < staticOutProb)
                {
                    localObject = Instantiate(Obstacles[Random.Range(0, Obstacles.Length)], position, Quaternion.Euler(0, rotations[Random.Range(0, 4)], 0))as GameObject;
                    localObject.transform.SetParent(transform);
                }
                position.x -= 1; //Move position
            }
            while (position.x <= 4 && position.x >= -4)   //Spawns while in inner range
            {
                //Spawns static object with inner probability
                if (Random.Range(0, 100) < staticInProb)
                {
                    localObject = Instantiate(Obstacles[Random.Range(0, Obstacles.Length)], position, Quaternion.Euler(0, rotations[Random.Range(0, 4)], 0)) as GameObject;
                    localObject.transform.SetParent(transform);
                    player.obstacleMatrix[posZ, Mathf.RoundToInt(position.x) + 5] = 1;
                }
                //Spawns coins with coin probability
                else if (Random.Range(0, 100) < coinProb)
                {
                    Coins[Mathf.RoundToInt(position.x) + 5] = Instantiate(Coin, position + new Vector3(0, 0.25f, 0), Quaternion.Euler(0, rotations[Random.Range(0, 4)], 0)) as GameObject;
                    Coins[Mathf.RoundToInt(position.x) + 5].transform.SetParent(transform);
                }
                position.x -= 1; //Move position
            }
        }
    }

    public void WorldChecker(float playerPos)
    {
        player.mover = Vector3.zero;
        Animator playerAnimator = player.transform.Find("Chicken").GetComponent<Animator>();
        player.nextPos.y = 0.83f;
        player.nextPos.x = Mathf.RoundToInt(playerPos);
        //Pickup coin
        if (Coins[Mathf.RoundToInt(playerPos) + 5] != null)
        {
            StartCoroutine(WorldCheckerTime(5, playerPos));
        }
        playerAnimator.SetTrigger("Jump");
    }

    IEnumerator WorldCheckerTime(int switcher, float playerPos) //timed part of WorldChecker()
    {
        switch (switcher)
        {
            case 5: //Coin
                yield return new WaitForSeconds(0.125f);
                player.coins++;
                player.coinText.text = player.coins.ToString();
                PlayerPrefs.SetInt("Money", player.coins);
                Destroy(Coins[Mathf.RoundToInt(playerPos) + 5]);
                break;
        }
    }
}