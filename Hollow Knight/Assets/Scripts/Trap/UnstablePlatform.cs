using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnstablePlatform : Trap
{
    public float triggerDelay;
    public float selfDestroyDelay;

    private Animator _animator;

    private void Start()
    {
        _animator = gameObject.GetComponent<Animator>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject != GlobalController.Instance.player)
            return;

        trigger();
    }

    public override void trigger()
    {

        StartCoroutine(selfDestroyCoroutine());
    }

    private IEnumerator selfDestroyCoroutine()
    {
        yield return new WaitForSeconds(triggerDelay);
        _animator.SetTrigger("trigger");
        gameObject.layer = LayerMask.NameToLayer("Decoration");
        yield return new WaitForSeconds(selfDestroyDelay);
        Destroy(gameObject);
    }
}
