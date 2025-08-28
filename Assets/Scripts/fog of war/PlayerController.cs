using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.WSA;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;
    [SerializeField] float speed;
    // Start is called before the first frame update

    public NPC chicken = new Chicken(5, 4);

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //MouseManipulator

        //Cursor.LockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.MovePosition(rb.position + (new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * speed));
    }
}
