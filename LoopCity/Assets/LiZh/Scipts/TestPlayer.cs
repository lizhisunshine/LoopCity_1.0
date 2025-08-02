using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
         this.transform.Translate(new Vector2(0,1)*10*Time.deltaTime);
        if (Input.GetKey(KeyCode.A))
            this.transform.Translate(new Vector2(-1, 0) * 10 * Time.deltaTime);
        if (Input.GetKey(KeyCode.S))
            this.transform.Translate(new Vector2(0, -1) * 10 * Time.deltaTime);
        if (Input.GetKey(KeyCode.D))
            this.transform.Translate(new Vector2(1, 0) * 10 * Time.deltaTime);
        print("113");
    }
}
