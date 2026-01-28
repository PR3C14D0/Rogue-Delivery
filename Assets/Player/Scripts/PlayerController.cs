using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 5.0f;

    public InputAction move;
    public InputAction shoot;

    private Rigidbody2D _rb;
    
    [SerializeField] private Animator _animator;
    public RectTransform canvasRect;
    
    [SerializeField] private Transform _visualTransform;

    private Vector2 _moveDirection;
    private bool _isWalking = false;
    private bool _isFlipped = false;

    [SerializeField] private Transform _playerFeet;
    [SerializeField] private float _health = 100.0f;
    public Scrollbar healthScrollbar;

    [Header("Shoot")]
    [SerializeField] private Transform _shootEnd;

    [SerializeField] private GameObject _aimObject;
    [SerializeField] private float _reloadTime = 2f;
    [SerializeField] private GameObject _shootButton;
    private bool _canShoot = true;

    private bool _shooting = false;

    private Gamepad _gamepad;

    void Awake()
    {
        InputActionAsset inputActions = GameManager.Instance.playerInputActions;
        move = inputActions.FindActionMap("Player").FindAction("Move");
        shoot = inputActions.FindActionMap("Player").FindAction("Shoot");   

        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        shoot.Enable();
        move.Enable();

        _gamepad = Gamepad.current;
    }

    void Start()
    {
        
    }

    void Update()
    {
       if(_gamepad != null && GetComponent<PlayerInput>().currentControlScheme == "Gamepad")
        {
            Vector2 dpadValue = _gamepad.dpad.ReadValue();
            _moveDirection = dpadValue;
        }
        else
        {
            _moveDirection = move.ReadValue<Vector2>();
        }

        if (_moveDirection.magnitude > 0.1)
        {
            _isWalking = true;

            if(_moveDirection.x > 0 && _isFlipped)
            {
                Flip();
            } else if(_moveDirection.x < 0 &&  !_isFlipped)
            {
                Flip();
            }

        } else
        {
            _isWalking = false;
        }

        _animator.SetBool("isWalking", _isWalking);
        
        if(shoot.ReadValue<float>() > 0 && _canShoot)
        {
            _shooting = true;
        }

        healthScrollbar.size = _health / 100;
    }

    void FixedUpdate()
    {
        _rb.linearVelocity = new Vector2(_moveDirection.x * walkSpeed, _rb.linearVelocityY);

        if(_shooting && _canShoot)
        {
            _canShoot = false;
            OnShoot();
        }
    }

    void Flip()
    {
        AimController aim = _aimObject.GetComponent<AimController>();
        aim.Flip();

        _isFlipped = !_isFlipped;

        Vector3 scale = _visualTransform.localScale;
        scale.x *= -1;
        _visualTransform.localScale = scale;
    }

    public void GetDamage(float damage)
    {
        _health -= Mathf.Min(_health, damage);
    }

    void OnShoot()
    {
        _shooting = false;
        _animator.SetBool("isShooting", true);

        Vector2 origin = transform.position;
        Vector2 direction = (_shootEnd.transform.position - transform.position).normalized;

        float offset = 1f;
        Vector2 startPoint = origin + (direction * offset); 

        StartCoroutine(ShootBullet(startPoint, direction));
        StartCoroutine(Reload());
    }

    IEnumerator ShootBullet(Vector2 origin, Vector2 direction)
    {
        float bulletSpeed = 20f;
        float distance = Vector2.Distance(origin, _shootEnd.transform.position) + _aimObject.GetComponent<AimController>().surfaceOffset;
        float travelTime = distance / bulletSpeed;

        LineRenderer line = GetComponentInChildren<LineRenderer>();
        if(line != null)
        {
            line.SetPosition(0, origin);
            line.SetPosition(1, origin);

            float elapsed = 0f;
            while(elapsed < travelTime)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / travelTime;

                Vector2 currentPos = Vector2.Lerp(origin, origin + direction * distance, progress);
                line.SetPosition(1, currentPos);
                yield return null;
            }
        } else
        {
            yield return new WaitForSeconds(travelTime);
        }

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance);
        if (hit && hit.collider.CompareTag("Enemy"))
        {
            GameObject enemy = hit.collider.gameObject;
            enemy.GetComponent<EnemyController>().GetDamage(20);
        }

        AudioSource audioSource = GetComponentInChildren<AudioSource>();
        if(audioSource != null && !audioSource.isPlaying) audioSource.Play();

        line.SetPosition(0, origin);
        line.SetPosition(1, origin);
    }

    IEnumerator Reload()
    {
        float elapsed = 0f;
        Image shootImage = _shootButton.GetComponent<Image>();
        Button shootButton = _shootButton.GetComponent<Button>();
        shootButton.interactable = false;
        shootImage.fillAmount = 0f;
        
        while(elapsed < _reloadTime)
        {
            elapsed += Time.deltaTime;
            shootImage.fillAmount = elapsed / _reloadTime;
            yield return null;
        }

        shootButton.interactable = true;

        shootImage.fillAmount = 1f;
        _canShoot = true;
    }

    public void OnShootEnd()
    {
        _animator.SetBool("isShooting", false);
    }
}