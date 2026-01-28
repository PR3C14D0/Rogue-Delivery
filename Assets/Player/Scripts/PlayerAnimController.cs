using UnityEngine;

public class PlayerAnimController : MonoBehaviour
{
    private PlayerController _playerController;
    
    void Awake()
    {
        _playerController = GetComponentInParent<PlayerController>();
    }
    
    public void OnShootEnd()
    {
        if(_playerController != null)
        {
            _playerController.OnShootEnd();
        }
    }
}