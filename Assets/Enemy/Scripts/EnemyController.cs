using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public float walkSpeed = 5f;

    [SerializeField] private LayerMask _wallLayer;
    [SerializeField] private float _health = 50f;
    [SerializeField] private Transform _enemyFront;
    [SerializeField] private float detectDistance = 5f;
    [SerializeField] private float maxPatrolDistance = 5f;
    [SerializeField] private float _shootDistance = 2f;
    [SerializeField] private float _reloadTime = 2f;

    private Animator _animator;
    private SpriteRenderer _sprite;
    private Rigidbody2D _rb;

    private bool _isPatrolling = true;
    private bool _canShoot = true;

    private float _leftLimit;
    private float _rightLimit;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        float startX = transform.position.x;
        _leftLimit = startX - maxPatrolDistance;
        _rightLimit = startX + maxPatrolDistance;
        FaceRight();
    }

    void FixedUpdate()
    {
        if (_isPatrolling)
        {
            Patrol();
            CheckFrontWall();
            CheckPatrolLimits();
            DetectPlayer();
        }
        else
        {
            _rb.linearVelocity = Vector2.zero;
            if (_canShoot)
            {
                Shoot();
            }
        }
    }

    void Patrol()
    {
        float velocityX = walkSpeed * Mathf.Sign(transform.localScale.x);
        _rb.linearVelocity = new Vector2(velocityX, _rb.linearVelocityY);
        _animator.SetBool("isWalking", true);
        _animator.SetBool("isShooting", false);
    }

    void CheckFrontWall()
    {
        Vector2 origin = _enemyFront.position;
        Vector2 direction = Vector2.right * Mathf.Sign(transform.localScale.x);
        if (Physics2D.Raycast(origin, direction, 0.25f, _wallLayer))
        {
            FlipDirection();
        }
    }

    void CheckPatrolLimits()
    {
        if (transform.position.x <= _leftLimit)
        {
            FaceRight();
        }
        else if (transform.position.x >= _rightLimit)
        {
            FaceLeft();
        }
    }

    void DetectPlayer()
    {
        Vector2 origin = _enemyFront.position;
        Vector2 direction = Vector2.right * Mathf.Sign(transform.localScale.x);
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, detectDistance);
        if (hit && hit.collider.CompareTag("Player"))
        {
            _isPatrolling = false;
            _animator.SetBool("isWalking", false);
        }
    }

    void Shoot()
    {
        _canShoot = false;
        _animator.SetBool("isShooting", true);
        Vector2 origin = transform.position;
        Vector2 direction = Vector2.right * Mathf.Sign(transform.localScale.x);
        StartCoroutine(ShootBullet(origin + direction * 0.2f, direction));
        StartCoroutine(Reload());
    }

    IEnumerator ShootBullet(Vector2 origin, Vector2 direction)
    {
        yield return new WaitForSeconds(0.4f);

        float bulletSpeed = 20f;
        float travelTime = _shootDistance / bulletSpeed;

        LineRenderer line = GetComponentInChildren<LineRenderer>();
        if (line)
        {
            line.SetPosition(0, origin);
            line.SetPosition(1, origin);

            float elapsed = 0f;
            while (elapsed < travelTime)
            {
                elapsed += Time.deltaTime;
                Vector2 pos = Vector2.Lerp(origin, origin + direction * _shootDistance, elapsed / travelTime);
                line.SetPosition(1, pos);
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(travelTime);
        }

        RaycastHit2D hit = Physics2D.Raycast(origin + direction, direction, _shootDistance);
        if (hit && hit.collider.CompareTag("Player"))
        {
            hit.collider.GetComponent<PlayerController>().GetDamage(20);
        }

        AudioSource audio = GetComponentInChildren<AudioSource>();
        if (audio && !audio.isPlaying) audio.Play();

        if (line)
        {
            line.SetPosition(0, origin);
            line.SetPosition(1, origin);
        }
    }

    IEnumerator Reload()
    {
        yield return new WaitForSeconds(_reloadTime);
        _canShoot = true;
        _animator.SetBool("isShooting", false);
    }

    public void GetDamage(float damage)
    {
        _health -= damage;
        _rb.linearVelocity = Vector2.zero;
        _rb.AddForce(new Vector2(-Mathf.Sign(transform.localScale.x) * 2f, 1f), ForceMode2D.Impulse);
        StartCoroutine(HitFlash());
        if (_health <= 0)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator HitFlash()
    {
        _sprite.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        _sprite.color = Color.white;
    }

    void FlipDirection()
    {
        if (Mathf.Sign(transform.localScale.x) > 0)
            FaceLeft();
        else
            FaceRight();
    }

    void FaceLeft()
    {
        transform.localScale = new Vector3(-1, 1, 1);
    }

    void FaceRight()
    {
        transform.localScale = new Vector3(1, 1, 1);
    }

    void OnShootEnd()
    {
        
    }
}
