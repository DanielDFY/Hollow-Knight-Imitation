using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunnerController : EnemyController
{
    public float shootInterval;
    public GameObject projectilePrefab;

    private bool _isShooting;
    private bool _isShootable;

    private Transform _playerTransform;
    private Transform _transform;
    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        _playerTransform = GlobalController.Instance.player.GetComponent<Transform>();
        _transform = gameObject.GetComponent<Transform>();
        _rigidbody = gameObject.GetComponent<Rigidbody2D>();
        _animator = gameObject.GetComponent<Animator>();
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        _isShootable = true;
        _isShooting = false;

        // default to idle mode
        _currentState = new Idle();
    }

    // Update is called once per frame
    void Update()
    {
        // update distance between player and enemy
        _playerEnemyDistance = _playerTransform.position.x - _transform.position.x;

        // flip sprite
        int direction = _playerEnemyDistance > 0 ? 1 : _playerEnemyDistance < 0 ? -1 : 0;

        // flip sprite
        if (direction != 0 && health > 0)
        {
            Vector3 newScale = _transform.localScale;
            newScale.x = direction;
            _transform.localScale = newScale;
        }

        // update state
        if (!_currentState.checkValid(this))
        {
            if (_isShooting)
            {
                _currentState = new Shooting();
            }
            else
            {
                _currentState = new Idle();
            }

            _isShooting = !_isShooting;
        }

        if (health > 0)
            _currentState.Execute(this);
    }

    public override float behaveInterval()
    {
        return shootInterval;
    }

    public override void hurt(int damage)
    {
        health = Math.Max(health - damage, 0);

        if (health == 0)
        {
            die();
            return;
        }

        Vector2 newVelocity = hurtRecoil;
        newVelocity.x *= _transform.localScale.x;

        _rigidbody.velocity = newVelocity;

        StartCoroutine(hurtCoroutine());
    }

    private IEnumerator hurtCoroutine()
    {
        yield return new WaitForSeconds(hurtRecoilTime);

        Vector2 newVelocity;
        newVelocity.x = 0;
        newVelocity.y = 0;
        _rigidbody.velocity = newVelocity;
    }

    protected override void die()
    {
        _animator.SetTrigger("isDead");

        _rigidbody.bodyType = RigidbodyType2D.Dynamic;

        // stop movement
        Vector2 newVelocity;
        newVelocity.x = 0;
        newVelocity.y = 0;
        _rigidbody.velocity = newVelocity;

        // change layer to prevent collision
        gameObject.layer = LayerMask.NameToLayer("Decoration");

        // death recoil
        Vector2 newForce;
        newForce.x = _transform.localScale.x * deathForce.x;
        newForce.y = deathForce.y;
        _rigidbody.AddForce(newForce, ForceMode2D.Impulse);

        StartCoroutine(fadeCoroutine());
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

    private void shootPlayer()
    {
        if (_isShootable)
        {
            _animator.SetTrigger("attack");

            _isShootable = false;

            Vector2 direction = _playerTransform.position - _transform.position;
            StartCoroutine(shootPlayerCoroutine(direction, shootInterval));
        }
    }

    private IEnumerator shootPlayerCoroutine(Vector2 direction, float shootInterval)
    {
        yield return new WaitForSeconds(0.2f);

        // set diretion of shooting
        Vector3 position = _transform.position;
        Quaternion rotation = _transform.rotation;
        GameObject projectileObj = Instantiate(projectilePrefab, position, rotation);
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        projectile.direction = direction;
        // shoot the player with projectile
        projectile.trigger();

        yield return new WaitForSeconds(shootInterval);
        if (!_isShootable)
            _isShootable = true;
    }

    /* ######################################################### */

    public class Idle : State
    {
        public override bool checkValid(EnemyController enemyController)
        {
            float playerEnemyDistanceAbs = Math.Abs(enemyController.playerEnemyDistance());
            return playerEnemyDistanceAbs > enemyController.detectDistance;
        }

        public override void Execute(EnemyController enemyController)
        {
            // don't do anything while being idle
        }
    }

    public class Shooting : State
    {

        public override bool checkValid(EnemyController enemyController)
        {
            float playerEnemyDistanceAbs = Math.Abs(enemyController.playerEnemyDistance());
            return playerEnemyDistanceAbs <= enemyController.detectDistance;
        }

        public override void Execute(EnemyController enemyController)
        {
            GunnerController gunnerController = (GunnerController)enemyController;

            gunnerController.shootPlayer();
        }
    }
}
