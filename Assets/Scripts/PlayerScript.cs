using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    private Rigidbody2D rd2d;

    public float speed;
    public Text countText;
    public Text winText;
    public Text livesText;
    public AudioClip musicClipOne;
    private bool isOnGround;
    public Transform groundcheck;
    public float checkRadius;
    public LayerMask allGround;

    public AudioClip musicClipTwo;

    public AudioSource musicSource;

    private int count;
    private int lives;
    Animator anim; 
    private bool facingRight = true;

    private int level = 1;


    // Start is called before the first frame update
    void Start()
    {
        rd2d = GetComponent<Rigidbody2D>();
        winText.text = "";
        count = 0;
        lives = 3;
        SetLivesText();
        SetCountText();
        anim = GetComponent<Animator>();
        musicSource.Play();
        musicSource.clip = musicClipOne;
        musicSource.loop = true;
        isOnGround = Physics2D.OverlapCircle(groundcheck.position, checkRadius, allGround);


    }


    // Update is called once per frame
    void FixedUpdate()
    {
        float hozMovement = Input.GetAxis("Horizontal");
        float vertMovement = Input.GetAxis("Vertical");
        rd2d.AddForce(new Vector2(hozMovement * speed, vertMovement * speed));
        if (facingRight == false && hozMovement > 0)
        {
            Flip();
        }
        else if (facingRight == true && hozMovement < 0)
        {
            Flip();
        }


        if (Input.GetKeyDown(KeyCode.D))
        {
            anim.SetInteger("State", 1);
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            anim.SetInteger("State", 0);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            anim.SetInteger("State", 1);
        }

        if (Input.GetKeyUp(KeyCode.A))
        {
            anim.SetInteger("State", 0);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Coin"))
        {
            other.gameObject.SetActive(false);
            count = count + 1;
            SetCountText();
        }

        else if (other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.SetActive(false);
            lives = lives - 1;

            SetLivesText();

        }
    }

    void SetLivesText()
    {
        livesText.text = "Lives: " + lives.ToString();
        if (lives == 0)
        {
            Destroy(gameObject);
            winText.text = "You lose! Game created by Kyle Remy!";
        }
    }



    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.tag == "Ground")
        {
            anim.SetBool("Jump", false);

            if (Input.GetKey(KeyCode.W))
            {
                rd2d.AddForce(new Vector2(0, 3), ForceMode2D.Impulse);
                anim.SetBool("Jump", true);

            }

        }
        
        
    }

    void SetCountText()
    {
        countText.text = "Count: " + count.ToString();
        if (count == 4 && level == 1)
        {
            level = 2;
            transform.position = new Vector2(100.0f, 100.0f);
            lives = 3;
            SetLivesText();

        }

        if (count == 8)
        {
            winText.text = "You Win! Game created by Kyle Remy!";
            musicSource.loop = false;
            musicSource.clip = musicClipTwo;
            musicSource.Play();

        }

    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector2 Scaler = transform.localScale;
        Scaler.x = Scaler.x * -1;
        transform.localScale = Scaler;
    }

}