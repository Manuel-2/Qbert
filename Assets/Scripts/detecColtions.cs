using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class detecColtions : MonoBehaviour
{
    bool colided;

    private void Start()
    {
        colided = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (colided == false && collision.gameObject.tag == "Enemy" && this.gameObject.tag == "Player")
        {
            colided = true;
            GameManager.sharedInstance.PlayerDied();
        }
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    Debug.Log("tuger");
    //    if (colided == false && collision.gameObject.tag == "Enemy")
    //    {
    //        colided = true;
    //        GameManager.sharedInstance.PlayerDied();
    //    }
    //}
}
