using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour {

    public float upBounce = 300;

    private Rigidbody2D rb2D;

    private void Awake() {
        rb2D = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        Debug.Log("Bird Dead, Game Over!");

        Die();
    }

    public void SetFree() {
        rb2D.bodyType = RigidbodyType2D.Static;
    }

    public void SetControl() {
        rb2D.bodyType = RigidbodyType2D.Dynamic;
    }

    public void Fly() { 
        rb2D.velocity = Vector2.zero;

        Vector2 upForce = Vector2.up * upBounce;
        rb2D.AddForce(upForce);

        SoundManager.instance.PlayFly();
    }

    private void Die() {
        rb2D.velocity = Vector2.zero;

        GameController.instance.GameOver();
    }
}
