using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class CameraController : MonoBehaviour
{
    private CharacterBase _character
    {
        get { return gameManager._character; }
    }

    //[SerializeField] private float _cameraSize;
    [SerializeField] private float _smoothTime;
    [SerializeField] private float _minSmoothTime;
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _maxDistanceBetween;
    private const float cameraPosValueZ = -30;
    private const float cameraMinValueY = 13;
    private Vector3 _targetPos;
    private Vector3 _velocity;



    void Update()
    {
        if (!gameManager._isGame)
        {
            return;
        }
        _targetPos = _character.transform.position;
        _targetPos.z = cameraPosValueZ;
        if (_targetPos.y < cameraMinValueY)
        {
            _targetPos.y = cameraMinValueY;
        }

        transform.position = _targetPos;
    }
}
