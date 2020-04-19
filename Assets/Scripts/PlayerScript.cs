using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerScript : MonoBehaviour
{
    public CharacterController2D controller;
    public Animator animator;

    


    
    


    public float runSpeed = 40f;
   
    float horizontalMove = 0f;
    bool jump = false;
    bool crouch = false;
    bool hurt = false;

    



    // Update is called once per frame
    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));


        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
            animator.SetBool("IsJumping", true);            
        }

        if (Input.GetButtonDown("Crouch"))
        {
            crouch = true;
        }
        else if (Input.GetButtonUp("Crouch"))
        {
            crouch = false;
        }

       

    }

    public void OnLanding()
    {
        animator.SetBool("IsJumping", false);
    }

    public void OnCrouching(bool isCrouching)
    {
        animator.SetBool("IsCrouching", isCrouching);
    }




    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("PowerUp"))
        {
            other.gameObject.SetActive(false);
            runSpeed = 100f;
            FindObjectOfType<AudioManager>().Play("PowerUp");
            GetComponent<SpriteRenderer>().color = Color.blue;
            StartCoroutine(ResetPower());

        }
        
    }





    void FixedUpdate()
    {
        //Move our character
        controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump);
        jump = false;

        
    }

    private IEnumerator ResetPower()
    {
        yield return new WaitForSeconds(10);
        runSpeed = 40f;
        GetComponent<SpriteRenderer>().color = Color.white;
    }


}