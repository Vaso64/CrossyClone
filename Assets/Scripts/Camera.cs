using UnityEngine;

public class Camera : MonoBehaviour
{
    public float speed = 0;
    public float pusher;
    private float sidePusher;
    private GameObject player;


    private void Start()
    {
        player = GameObject.Find("Player");
    }
    // Update is called once per frame
    void Update()
    {
        if(speed != 0)
        {
            pusher = player.transform.position.z - (transform.position.z + 8);
            if (pusher < 0) { pusher = 0; }
            speed = 0.6f + pusher;
        }
        sidePusher = (Mathf.Clamp(player.transform.position.x, -4f, 4f) - (transform.position.x - 2.8f)) * 2;
        transform.position += (Vector3.forward * speed + Vector3.right * sidePusher) * Time.deltaTime; //Move camera
    }
}
