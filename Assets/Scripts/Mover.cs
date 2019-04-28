using UnityEngine;

public class Mover : MonoBehaviour
{
    public float speed;

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.left * Time.deltaTime * speed; //Move
        InvokeRepeating("Dispawner", 0f, 0.5f); //check for position each seconds
    }

    //dispawn object out of range
    void Dispawner()
    {
        if (transform.position.x < -12 && speed > 0) //On position (left)
        {
            Destroy(gameObject); //DISPAWN
        }
        if (transform.position.x > 12 && speed < 0) //On position (right)
        {
            Destroy(gameObject); //DISPAWN
        }
    }
}
