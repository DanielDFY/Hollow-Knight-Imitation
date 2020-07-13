using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolController : EnemyController
{
    public float walkSpeed;
    public float edgeSafeDistance;
    public float behaveIntervalLeast;
    public float behaveIntervalMost;

    private int _reachEdge;
    private bool _isChasing;
    private bool _isMovable;

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

        _currentState = new Patrol();

        _isChasing = false;
        _isMovable = true;
    }

    // Update is called once per frame
    void Update()
    {
        // update distance between player and enemy
        _playerEnemyDistance = _playerTransform.position.x - _transform.position.x;

        // update edge detection
        Vector2 detectOffset;
        detectOffset.x = edgeSafeDistance * _transform.localScale.x;
        detectOffset.y = 0;
        _reachEdge = checkGrounded(detectOffset) ? 0 : (_transform.localScale.x > 0 ? 1 : -1);

        // update state
        if (!_currentState.checkValid(this))
        {
            if (_isChasing)
            {
                _currentState = new Patrol();
            }
            else
            {
                _currentState = new Chase();
            }

            _isChasing = !_isChasing;
        }

        if (_isMovable)
            _currentState.Execute(this);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        string layerName = LayerMask.LayerToName(collision.collider.gameObject.layer);

        if (layerName == "Player")
        {
            PlayerController playerController = collision.collider.GetComponent<PlayerController>();
            playerController.hurt(1);
        }
    }

    public override float behaveInterval()
    {
        return UnityEngine.Random.Range(behaveIntervalLeast, behaveIntervalMost);
    }

    public int reachEdge()
    {
        return _reachEdge;
    }

    public override void hurt(int damage)
    {
        health = Math.Max(health - damage, 0);

        _isMovable = false;

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
        _isMovable = true;
    }

    private bool checkGrounded(Vector2 offset)
    {
        Vector2 origin = _transform.position;
        origin += offset;

        float radius = 0.3f;

        // detect downwards
        Vector2 direction;
        direction.x = 0;
        direction.y = -1;

        float distance = 1.1f;
        LayerMask layerMask = LayerMask.GetMask("Platform");

        RaycastHit2D hitRec = Physics2D.CircleCast(origin, radius, direction, distance, layerMask);
        return hitRec.collider != null;
    }

    public void walk(float move)
    {
        int direction = move > 0 ? 1 : move < 0 ? -1 : 0;

        float newWalkSpeed = (direction == _reachEdge) ? 0 : direction * walkSpeed;

        // flip sprite
        if (direction != 0 && health > 0)
        {
            Vector3 newScale = _transform.localScale;
            newScale.x = direction;
            _transform.localScale = newScale;
        }

        // set velocity
        Vector2 newVelocity = _rigidbody.velocity;
        newVelocity.x = newWalkSpeed;
        _rigidbody.velocity = newVelocity;

        // animation
        _animator.SetFloat("Speed", Math.Abs(newWalkSpeed));
    }

    protected override void die()
    {
        _animator.SetTrigger("isDead");

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

    /* ######################################################### */

    public abstract class PatrolState
    {
        public abstract bool checkValid(PatrolController enemyController);
        public abstract void Execute(PatrolController enemyController);
    }

    public class Patrol : State
    {
        private PatrolState _currentState;
        private int _currentStateCase = 0;
        private bool _isFinished;   // ready for next state

        public Patrol()
        {
            _currentState = new Idle();
            _isFinished = true;
        }

        public override bool checkValid(EnemyController enemyController)
        {
            float playerEnemyDistanceAbs = Math.Abs(enemyController.playerEnemyDistance());
            return playerEnemyDistanceAbs > enemyController.detectDistance;
        }

        public override void Execute(EnemyController enemyController)
        {
            PatrolController patrolController = (PatrolController)enemyController;
            if (!_currentState.checkValid(patrolController) || _isFinished)
            {
                // randomly change current state
                int randomStateCase;
                do
                {
                    randomStateCase = UnityEngine.Random.Range(0, 3);
                } while (randomStateCase == _currentStateCase);

                _currentStateCase = randomStateCase;
                switch (_currentStateCase)
                {
                    case 0:
                        _currentState = new Idle();
                        break;
                    case 1:
                        _currentState = new WalkingLeft();
                        break;
                    case 2:
                        _currentState = new WalkingRight();
                        break;
                }

                patrolController.StartCoroutine(executeCoroutine(patrolController.behaveInterval()));
            }

            _currentState.Execute(patrolController);
        }

        private IEnumerator executeCoroutine(float delay)
        {
            _isFinished = false;
            yield return new WaitForSeconds(delay);
            if (!_isFinished)
                _isFinished = true;
        }
    }

    public class Chase : State
    {
        public override bool checkValid(EnemyController enemyController)
        {
            float playerEnemyDistanceAbs = Math.Abs(enemyController.playerEnemyDistance());
            return playerEnemyDistanceAbs <= enemyController.detectDistance;
        }

        public override void Execute(EnemyController enemyController)
        {
            PatrolController patrolController = (PatrolController)enemyController;
            float dist = patrolController.playerEnemyDistance();
            patrolController.walk(Math.Abs(dist) < 0.1f ? 0 : dist);
        }
    }

    public class Idle : PatrolState
    {
        public override bool checkValid(PatrolController patrolController)
        {
            return patrolController.reachEdge() == 0;
        }

        public override void Execute(PatrolController patrolController)
        {
            patrolController.walk(0);
        }
    }
    public class WalkingLeft : PatrolState
    {
        public override bool checkValid(PatrolController patrolController)
        {
            return patrolController.reachEdge() != -1;
        }

        public override void Execute(PatrolController patrolController)
        {
            patrolController.walk(-1);
        }
    }

    public class WalkingRight : PatrolState
    {
        public override bool checkValid(PatrolController patrolController)
        {
            return patrolController.reachEdge() != 1;
        }

        public override void Execute(PatrolController patrolController)
        {
            patrolController.walk(1);
        }
    }
}