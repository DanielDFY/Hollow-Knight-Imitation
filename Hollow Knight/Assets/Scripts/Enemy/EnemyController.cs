using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyController : MonoBehaviour
{
    public int health;
    public float detectDistance;
    public int damageToPlayer;

    public Vector2 hurtRecoil;
    public float hurtRecoilTime;
    public Vector2 deathForce;
    public float destroyDelay;

    protected State _currentState;
    protected float _playerEnemyDistance;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        string layerName = LayerMask.LayerToName(collision.collider.gameObject.layer);

        if (layerName == "Player")
        {
            PlayerController playerController = collision.collider.GetComponent<PlayerController>();
            playerController.hurt(damageToPlayer);
        }
    }


    public float playerEnemyDistance()
    {
        return _playerEnemyDistance;
    }

    public abstract float behaveInterval();

    public abstract void hurt(int damage);

    protected abstract void die();

    public abstract class State
    {
        public abstract bool checkValid(EnemyController enemyController);
        public abstract void Execute(EnemyController enemyController);
    }
}