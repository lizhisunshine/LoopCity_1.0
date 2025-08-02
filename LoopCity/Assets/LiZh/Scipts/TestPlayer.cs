using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayer : MonoBehaviour
{
    Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
            this.transform.Translate(new Vector2(0, 1) * 10 * Time.deltaTime);
        if (Input.GetKey(KeyCode.A))
            this.transform.Translate(new Vector2(-1, 0) * 10 * Time.deltaTime);
        if (Input.GetKey(KeyCode.S))
            this.transform.Translate(new Vector2(0, -1) * 10 * Time.deltaTime);
        if (Input.GetKey(KeyCode.D))
            this.transform.Translate(new Vector2(1, 0) * 10 * Time.deltaTime);
        //if (Input.GetKey(KeyCode.W))
        //    rb.velocity =Vector2.up*10;
        // if (Input.GetKey(KeyCode.A))
        //    rb.velocity = Vector2.left * 10;
        // if (Input.GetKey(KeyCode.S))
        //    rb.velocity = Vector2.down * 10;
        // if (Input.GetKey(KeyCode.D))
        //    rb.velocity = Vector2.right * 10;
        // if(!Input.GetKey(KeyCode.W)&& !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.S) )
        //    rb.velocity = Vector2.zero;
        print("113");
    }
}
