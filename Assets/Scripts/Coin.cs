using System.Collections;
using UnityEngine;

public class Coin : MonoBehaviour
{
    Animator anim;
    void Start()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(CoinAnim());
    }

    IEnumerator CoinAnim()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(4f, 10f));
            anim.SetInteger("Coin", Random.Range(1, 2));
            yield return new WaitForSeconds(0.1f);
            anim.SetInteger("Coin", 0);
        }
    }
}
