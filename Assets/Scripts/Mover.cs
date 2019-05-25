using UnityEngine;

public class Mover : MonoBehaviour
{
    public float speed;
    [SerializeField] public float size;
    private void Start()
    {
        InvokeRepeating("Dispawner", 0f, 1f); //check for position each seconds
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.left * Time.deltaTime * speed; //Move
    }

    //dispawn object out of range
    void Dispawner()
    {
        if (transform.position.x < -15 && speed > 0 || transform.position.x > 15 && speed < 0) //Check for position
        {
            Destroy(gameObject); //DISPAWN
        }
    }
}
