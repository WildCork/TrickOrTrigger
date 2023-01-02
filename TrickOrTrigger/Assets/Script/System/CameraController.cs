using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class CameraController : MonoBehaviour
{
    private CharacterBase _characterBase
    {
        get { return gameManager._characterBase; }
    }

    //[SerializeField] private float _cameraSize;
    [SerializeField] private float _minSmoothTime;
    [SerializeField] private float _maxSpeed;
    private const float cameraPosValueZ = -30;
    private const float cameraMinValueY = 13;
    private Vector3 _targetPos1 = Vector3.zero;
    public Vector3 _targetPos2 = Vector3.zero;
    private Vector3 _velocity;



    void Update()
    {
        if (gameManager._isGame)
        {
            if (_characterBase._isFaint)
            {
                _targetPos2.z = cameraPosValueZ;
                transform.position = Vector3.SmoothDamp(transform.position, _targetPos2, ref _velocity, _minSmoothTime);
            }
            else
            {
                _targetPos1 = _characterBase.transform.position;
                _targetPos1.z = cameraPosValueZ;
                if (_targetPos1.y < cameraMinValueY)
                {
                    _targetPos1.y = cameraMinValueY;
                }

                transform.position = _targetPos2 = _targetPos1;
            }
        }
    }
}
