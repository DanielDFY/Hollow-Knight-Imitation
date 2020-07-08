using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingTrap : Trap
{
    public float destroyDelay;

    private bool _isTriggered;
    private Rigidbody2D _rigidbody;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _rigidbody = gameObject.GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 0;
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        _isTriggered = false;
    }

    private void Update()
    {
        if (_isTriggered)
        {
            _isTriggered = false;
            fall();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag != "Platform")
            return;

        gameObject.layer = LayerMask.NameToLayer("Decoration");

        StartCoroutine(fadeCoroutine());
    }

    public override void trigger()
    {
        _isTriggered = true;
    }

    public void fall()
    {
        _rigidbody.gravityScale = 1;
    }

    private IEnumerator fadeCoroutine()
    {
        while (destroyDelay > 0)
        {
            destroyDelay -= Time.deltaTime;

            if (_spriteRenderer.color.a > 0)
            {
                Color newColor = _spriteRenderer.color;
                newColor.a -= Time.deltaTime / destroyDelay;
                _spriteRenderer.color = newColor;
                yield return null;
            }
        }

        Destroy(gameObject);
    }
}
