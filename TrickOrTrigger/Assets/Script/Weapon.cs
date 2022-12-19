using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using static CharacterBase;
using static GameManager;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Weapon : ObjectBase , IPunObservable
{

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }

    public enum WeaponType { Pistol = 0, Machinegun, Shotgun, Knife}
    public WeaponType _weaponType = WeaponType.Pistol;
    [SerializeField] ParticleSystem _particleSystem;
    [Header("Time")]
    [SerializeField] private float _lifeTime = 0f;
    [SerializeField] private float _maxLifeTime = 2f;
    public const float c_lifeCycleTime = 0.1f;

    [Header("Stats")]
    public bool _isShoot = false;
    public bool _isHit = false;
    public float _upDownOffset;
    public int _damage;
    public float _shotSpeed;

    private WaitForSeconds _lifeCycleSeconds = new WaitForSeconds(c_lifeCycleTime);
    private float _inStatusValueZ
    {
        get { return Map._walls.transform.position.z; }
    }
    private float _outStatusValueZ
    {
        get { return Map._outMap.transform.position.z; }
    }

    protected override void Awake()
    {
        base.Awake();
        _particleSystem = GetComponent<ParticleSystem>();
        _particleSystem.Stop();
    }

    public void GoBackStorage_RPC()
    {
        //Debug.Log("GoBackStorage_RPC");
        photonView.RPC(nameof(GoBackStorage), RpcTarget.All);
    }

    [PunRPC]
    public void GoBackStorage()
    {
        _particleSystem.Stop();
        StartCoroutine(GoBackStorage_Routine());
    }

    IEnumerator GoBackStorage_Routine()
    {
        while (_particleSystem.isPlaying)
        {
            yield return c_lifeCycleTime;
        }
        _isShoot = false;
        _collider2D.enabled = false;
        _rigidbody2D.velocity = Vector3.zero;
        transform.position = gameManager._storage.transform.position;
        _triggerWallSet.Clear();
        gameManager._weaponStorage[_weaponType].Add(this);
    }

    private float ReturnTime(AnimState animState)
    {
        return _spineTimeDict[_weaponType][gameManager._animNameDict[animState]];
    }
    private float ReturnDelayTime(bool isOnGround, float horizontal, bool walk)
    {
        if (isOnGround)
        {
            if (horizontal != 0)
            {
                if (walk)
                {
                    return ReturnTime(AnimState.Walk_shoot);
                }
                else
                {
                    return ReturnTime(AnimState.Run_shoot);
                }
            }
            else
            {
                return ReturnTime(AnimState.Shoot);
            }
        }
        else
        {
            return ReturnTime(AnimState.Jump_shoot);
        }
    }

    public void Shoot(CharacterBase character, bool isOnGround, float horizontal, bool walk)
    {
        _isShoot = true;
        _isHit = false;
        _lifeTime = 0;
        _collider2D.enabled = true;
        gameManager._weaponStorage[_weaponType].RemoveAt(0);

        character.currentShootDelay = ReturnDelayTime(isOnGround, horizontal, walk);
        character.isShootUpDown *= -1;

        _locationStatus = character._locationStatus;
        Vector3 pos = character.ShootPos + character.isShootUpDown * Vector3.up * _upDownOffset;
        Vector3 localscale = gameObject.transform.localScale;
        switch (character.direction)
        {
            case Direction.Left:
                _rigidbody2D.velocity = Vector2.left * _shotSpeed;
                localscale.x = -Mathf.Abs(localscale.x);
                break;
            case Direction.Right:
                _rigidbody2D.velocity = Vector2.right * _shotSpeed;
                localscale.x = Mathf.Abs(localscale.x);
                break;
            default:
                break;
        }
        transform.position = pos;
        transform.localScale = localscale;
        photonView.RPC(nameof(Shoot_RPC), RpcTarget.All);
    }

    [PunRPC]
    public void Shoot_RPC()
    {
        _isShoot = true;
        _particleSystem.Play();
    }

    private void Update()
    {
        if (_isShoot)
        {
            _lifeTime += Time.deltaTime;
            if (_lifeTime > _maxLifeTime)
            {
                _isShoot = false;
                GoBackStorage_RPC();
            }
        }
    }

    protected override void RefreshLocationStatus(LocationStatus locationStatus)
    {
        base.RefreshLocationStatus(locationStatus);
        Vector3 pos = transform.position;
        switch (locationStatus)
        {
            case LocationStatus.Out:
                pos.z = _outStatusValueZ;
                break;
            case LocationStatus.In:
            case LocationStatus.Door:
                pos.z = _inStatusValueZ;
                break;
            default:
                break;
        }
        transform.position = pos;
    }

    protected override void Hit(Collider2D collision)
    {
        switch (_locationStatus)
        {
            case LocationStatus.In:
            case LocationStatus.Door:
                if (collision.gameObject.layer == gameManager._wallLayer)
                {
                    HitWall();
                }
                break;
            default:
                break;
        }
    }

    private void HitWall()
    {
        GoBackStorage_RPC();
    }
}
