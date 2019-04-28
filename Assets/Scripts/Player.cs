using UnityEngine.SceneManagement;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int[,] matrix = new int[999, 11];
    private int PreJumpID = Animator.StringToHash("SpaceHold");
    private int JumpID = Animator.StringToHash("SpaceRelease");
    private Animator animator;
    enum State { idle, move, dead, floating};
    [SerializeField] State state = State.idle;
    private float startTime;
    private Vector3 nextPos;
    private Vector3 startPos;
    private Quaternion nextRot;
    private Quaternion startRot;
    private float t = 0;
    [SerializeField] float moveTime = 0.2f;
    private Vector3 mover;

    private void Start()
    {
        animator = transform.Find("Chicken").GetComponent<Animator>(); //get Animator
    }

    // Update is called once per frame
    private void Update()
    {
        transform.position += mover * Time.deltaTime; //Move object to direction
        if(state != State.dead && (transform.position.x < -4.5f || transform.position.x > 4.5f)) //dead on outside of world space
        {
            state = State.dead;
        }
        InputManager(); //Handles the input
        if (state == State.move || state == State.floating)
        {
            MoveUpdate();
        }
    }

    //Move the object acording to input
    private void MoveUpdate() //Update part of Move()
    {
        t += Time.deltaTime / moveTime; //time of movement
        transform.localPosition = Vector3.Lerp(startPos, nextPos, t); //move
        transform.localRotation = Quaternion.Lerp(startRot, nextRot, t*2);
        //Return to idle on end
        if (t >= 1)
        {
            state = State.idle;
            t = 0;
        }
    }
    private void InputManager() //Handles Input
    {
        if (Input.GetKey(KeyCode.R)) //RESTART
        {
            SceneManager.LoadScene(0);
        }
        if (state == State.idle || state == State.floating)
        {
            if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) && matrix[Mathf.RoundToInt(transform.position.z) + 5, Mathf.RoundToInt(transform.position.x) + 5] != 1) //Move forward
            {
                Move(0, Vector3.forward);
            }
            if (Input.GetKeyDown(KeyCode.D) && matrix[Mathf.RoundToInt(transform.position.z) + 4, Mathf.RoundToInt(transform.position.x) + 6] != 1) //Turn right
            {
                Move(90, Vector3.right);
            }
            if (Input.GetKeyDown(KeyCode.A) && matrix[Mathf.RoundToInt(transform.position.z) + 4, Mathf.RoundToInt(transform.position.x) + 4] != 1) //Turn left
            {
                Move(-90, Vector3.left);
            }
            if (Input.GetKeyDown(KeyCode.S) && matrix[Mathf.RoundToInt(transform.position.z) + 3, Mathf.RoundToInt(transform.position.x) + 5] != 1) //Turn back
            {
                Move(180, Vector3.back);
            }
        }
    }

    private void Move(float rot, Vector3 direction) //Move player in world
    {
        state = State.move;
        startPos = transform.position;
        nextPos = startPos + direction;
        if(transform.position.z == nextPos.z)
        {
            nextPos += mover * moveTime;
        }
        startRot = transform.rotation;
        nextRot = Quaternion.Euler(new Vector3(0, rot, 0));
        if(transform.position.z != nextPos.z)
        {
            nextPos = new Vector3(Mathf.Round(startPos.x), startPos.y, startPos.z) + direction;
            mover = Vector3.zero;
        }
        animator.SetTrigger("Jump");
    }

    private void OnTriggerEnter(Collider other) //Handles collisions
    {
        if(other.gameObject.tag == "Enemy") //on Death
        {
            if(state == State.move && (nextRot.y == 0 || nextRot.y == 180) && nextPos.z == other.gameObject.transform.position.z) //Play bump up "animation"
            {
                animator.SetTrigger("Bumpup");
                transform.rotation = nextRot * Quaternion.Euler(0,0,Random.Range(-30,30)); //Random rotation
                transform.position = startPos + new Vector3(0, 0, 0.1f);
                mover = Vector3.left * other.gameObject.GetComponent<Mover>().speed;
            }
            else //Play run over animation
            {
                animator.SetTrigger("Runover");
                transform.position = new Vector3(transform.position.x, transform.position.y, other.gameObject.transform.position.z);
            }
            state = State.dead;
        }

        if(other.gameObject.tag == "Coin") //Pickup coin
        {
            Destroy(other.gameObject);
        }
        if(other.gameObject.tag == "Floater") //Start floating on water
        {
            //other.gameObject.GetComponentInChildren<Animator>().SetTrigger("Float");
            state = State.floating;
            animator.SetTrigger("Float");
            mover = Vector3.left * other.gameObject.GetComponent<Mover>().speed;
        }
        if(other.gameObject.tag == "Water" && state != State.floating) //Fall to water
        {
            animator.SetTrigger("Water");
        }
    }
}
