using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunnerController : EnemyController
{
    public float shootInterval;
    public GameObject projectilePrefab;

    private bool _isShooting;

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

        _currentState = new Idle();
    }

    // Update is called once per frame
    void Update()
    {
        // update distance between player and enemy
        _playerEnemyDistance = _playerTransform.position.x - _transform.position.x;

        // flip sprite
        int direction = _playerEnemyDistance > 0 ? 1 : _playerEnemyDistance < 0 ? -1 : 0;

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

        _currentState.Execute(this);
    }

    private void shootPlayer()
    {
        _animator.SetTrigger("attack");

        Vector2 direction = _playerTransform.position - _transform.position;
        StartCoroutine(shootPlayerCoroutine(direction));
    }

    private IEnumerator shootPlayerCoroutine(Vector2 direction)
    {
        yield return new WaitForSeconds(0.2f);

        Vector3 position = _transform.position;
        Quaternion rotation = _transform.rotation;
        GameObject projectileObj = Instantiate(projectilePrefab, position, rotation);
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        projectile.direction = direction;
        projectile.trigger();
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
    protected override void die()
    {
        _animator.SetTrigger("isDead");

        _rigidbody.bodyType = RigidbodyType2D.Dynamic;

        Vector2 newVelocity;
        newVelocity.x = 0;
        newVelocity.y = 0;
        _rigidbody.velocity = newVelocity;

        gameObject.layer = LayerMask.NameToLayer("Decoration");

        Vector2 newForce;
        newForce.x = _transform.localScale.x * deathForce.x;
        newForce.y = deathForce.y;
        _rigidbody.AddForce(newForce, ForceMode2D.Impulse);

        StartCoroutine(fadeCoroutine());
    }

    private IEnumerator hurtCoroutine()
    {
        yield return new WaitForSeconds(recoilTime);

        Vector2 newVelocity;
        newVelocity.x = 0;
        newVelocity.y = 0;
        _rigidbody.velocity = newVelocity;
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

        if (health > 0)
            Destroy(gameObject);
    }

    public class Idle : State
    {
        public override bool checkValid(EnemyController enemyController)
        {
            float playerEnemyDistanceAbs = Math.Abs(enemyController.playerEnemyDistance());
            return playerEnemyDistanceAbs > enemyController.detectDistance;
        }

        public override void Execute(EnemyController enemyController)
        {
            
        }
    }

    public class Shooting : State
    {
        private bool _isShootable = true;

        public override bool checkValid(EnemyController enemyController)
        {
            float playerEnemyDistanceAbs = Math.Abs(enemyController.playerEnemyDistance());
            return playerEnemyDistanceAbs <= enemyController.detectDistance;
        }

        public override void Execute(EnemyController enemyController)
        {
            GunnerController gunnerController = (GunnerController)enemyController;

            if (_isShootable)
            {
                gunnerController.shootPlayer();

                gunnerController.StartCoroutine(executeCoroutine(gunnerController.behaveInterval()));
            }
        }

        private IEnumerator executeCoroutine(float delay)
        {
            _isShootable = false;
            yield return new WaitForSeconds(delay);
            if (!_isShootable)
                _isShootable = true;
        }
    }
}
