using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField] private float m_JumpForce = 400f;                          // Amount of force added when the player jumps.
    [Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;          // Amount of maxSpeed applied to crouching movement. 1 = 100%
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;  // How much to smooth out the movement
    [SerializeField] private bool m_AirControl = false;                         // Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
    [SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
    [SerializeField] private Transform m_CeilingCheck;                          // A position marking where to check for ceilings
    [SerializeField] private Collider2D m_CrouchDisableCollider;                // A collider that will be disabled when crouching

    [SerializeField] private Animator animator;

    public Text scoreText;
    public Text winText;
    public Text livesText;
    public float timeLeft = 3.0f;
    public Text startText; // used for showing countdown from 3, 2, 1 





    private int scoreValue = 0;
    private int livesValue = 3;
    private int level = 1;
    bool isHurting;



    const float k_GroundedRadius = 0.5f; // Radius of the overlap circle to determine if grounded
    private bool m_Grounded;            // Whether or not the player is grounded.
    const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true;  // For determining which way the player is currently facing.
    private Vector3 m_Velocity = Vector3.zero;

    [Header("Events")]
    [Space]

    public UnityEvent OnLandEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public BoolEvent OnCrouchEvent;
    private bool m_wasCrouching = false;



    void Start()
    {
        
        winText.text = "";
        
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        SetLivesText();
        SetScoreText();
        
    }

    private void Update()
    {
        timeLeft -= Time.deltaTime;
        startText.text = (timeLeft).ToString("0");
        if (timeLeft < 0)
        {
            Destroy(gameObject);
            winText.text = "You lose! Game created by Kyle Remy!";
            FindObjectOfType<AudioManager>().Play("GameOverSound");
            FindObjectOfType<AudioManager>().Stop("Mushroom Theme");

        }
    }

    void SetScoreText()
    {
        scoreText.text = "Score: " + scoreValue.ToString();
        if (scoreValue == 4 && level == 1)
        {
            level = 2;
            transform.position = new Vector2(98.34f, 101.6f);
            livesValue = 3;
            SetLivesText();


        }
        if (scoreValue == 8)
        {
            winText.text = "You Win! Game created by Kyle Remy!";
            FindObjectOfType<AudioManager>().Stop("Mushroom Theme");
            FindObjectOfType<AudioManager>().Play("VictoryTheme");
            timeLeft = 1000.0f;

        }
    }

    void SetLivesText()
    {
        livesText.text = "Lives: " + livesValue.ToString();
        if (livesValue == 0)
        {
            Destroy(gameObject);
            winText.text = "You lose! Game created by Kyle Remy!";
            FindObjectOfType<AudioManager>().Play("GameOverSound");
            FindObjectOfType<AudioManager>().Stop("Mushroom Theme");


        }
    }


    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();
    }

    private void FixedUpdate()
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;
                if (!wasGrounded && m_Rigidbody2D.velocity.y < 0)
                    OnLandEvent.Invoke();
                if (!isHurting)
                    m_Rigidbody2D.velocity = new Vector2(m_MovementSmoothing, m_Rigidbody2D.velocity.y);
            }
        }
    }


    public void Move(float move, bool crouch, bool jump)
    {
        // If crouching, check to see if the character can stand up
        if (!crouch)
        {
            // If the character has a ceiling preventing them from standing up, keep them crouching
            if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
            {
                crouch = true;
            }
        }

        //only control the player if grounded or airControl is turned on
        if (m_Grounded || m_AirControl)
        {

            // If crouching
            if (crouch)
            {
                if (!m_wasCrouching)
                {
                    m_wasCrouching = true;
                    OnCrouchEvent.Invoke(true);
                }

                // Reduce the speed by the crouchSpeed multiplier
                move *= m_CrouchSpeed;

                // Disable one of the colliders when crouching
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = false;
            }
            else
            {
                // Enable the collider when not crouching
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = true;

                if (m_wasCrouching)
                {
                    m_wasCrouching = false;
                    OnCrouchEvent.Invoke(false);
                }
            }

            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
            // And then smoothing it out and applying it to the character
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

            // If the input is moving the player right and the player is facing left...
            if (move > 0 && !m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0 && m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }
        }
        // If the player should jump...
        if (m_Grounded && jump)
        {
            // Add a vertical force to the player.
            m_Grounded = false;
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Coin"))
        {
            other.gameObject.SetActive(false);
            scoreValue = scoreValue + 1;
            SetScoreText();
            FindObjectOfType<AudioManager>().Play("CoinSound");

            

        }
        else if (other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.SetActive(false);
            livesValue = livesValue - 1;
            SetLivesText();
            StartCoroutine("Hurt");



        }
        if (other.gameObject.CompareTag("PowerUp2"))
        {
            other.gameObject.SetActive(false);
            timeLeft = 100.0f;
            FindObjectOfType<AudioManager>().Play("PowerUp");


        }




    }

    IEnumerator Hurt()
    {
        isHurting = true;
        m_Rigidbody2D.velocity = Vector2.zero;
        animator.SetBool("IsHurting", true);
        FindObjectOfType<AudioManager>().Play("DamageSound");


        if (m_FacingRight)
            m_Rigidbody2D.AddForce(new Vector2(-200f, 200f));
        else
            m_Rigidbody2D.AddForce(new Vector2(200f, 200f));

        yield return new WaitForSeconds(0.2f);

        isHurting = false;
        animator.SetBool("IsHurting", false);

    }

   




    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}



