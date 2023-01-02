using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using static CharacterBase;
using static GameManager;

public class Bullet : ObjectBase , IPunObservable
{
    #region Photon
    private Vector3 _curPos = Vector3.zero;
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.localScale);
            stream.SendNext(ParticleIndex);
        }
        else
        {
            _curPos = (Vector3)stream.ReceiveNext();
            transform.localScale = (Vector3)stream.ReceiveNext();
            ParticleIndex = (int)stream.ReceiveNext();
        }
    }

    #endregion

    #region Variables
    public WeaponType _weaponType = WeaponType.Pistol;
    [Header("Time")]
    [SerializeField] private float _lifeTime = 0f;
    [SerializeField] private float _maxLifeTime = 2f;
    public const float c_lifeCycleTime = 0.1f;

    [Header("Stats")]
    public bool _isShoot = false;
    public bool _isHit = false;
    public float _upDownOffset = 0;
    public int _damage = 0;
    public float _shotSpeed = 0;

    [Header("Splash")]
    public bool _isSplash = false;
    public Explosion _explosion = null;

    [Header("Particle")]
    [SerializeField] private int _particleIndex = -1; //0: shoot 1: hitPlayer 2: hitWall 
    [SerializeField] private ParticleSystem[] _particles; //0: shoot 1: hitPlayer 2: hitWall 

    //Sound
    //0: hitPlayer
    //1: hitWall
    //2: 
    //3: 
    #endregion

    #region Property
    private int ParticleIndex
    {
        get
        {
            return _particleIndex;
        }
        set
        {
            if(_particleIndex == value)
                return;
            if (_particleIndex >= 0)
            {
                _particles[_particleIndex].Stop();
                if (_particleIndex == 0)
                {
                    _particles[_particleIndex].gameObject.SetActive(false);
                }
            }
            _particleIndex = value;
            if (value >= 0)
            {
                _particles[_particleIndex].Play();
            }
        }
    }

    private WaitForSeconds _lifeCycleSeconds = new WaitForSeconds(c_lifeCycleTime);
    private float _inStatusValueZ
    {
        get { return Map._walls.transform.position.z; }
    }
    private float _outStatusValueZ
    {
        get { return Map._outMap.transform.position.z; }
    }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        transform.position = _curPos = transform.parent.position;
        ParticleIndex = -1;
    }
    private void Update()
    {
        if (photonView.IsMine)
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
        else
        {
            if ((transform.position - _curPos).sqrMagnitude >= 100)
            {
                transform.position = _curPos;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, _curPos, Time.deltaTime * 10);
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

    #region Shoot
    public void Shoot(CharacterBase character, bool isOnGround, float horizontal, bool walk)
    {
        _isShoot = true;
        _isHit = false;
        _lifeTime = 0;
        _collider2D.enabled = true;
        gameManager._weaponStorage[_weaponType].RemoveAt(0);

        character.currentAttackDelay = ReturnDelayTime(isOnGround, horizontal, walk);
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
        _isShoot = true;
        ParticleIndex = 0;
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
    private float ReturnTime(AnimState animState)
    {
        return _spineTimeDict[_weaponType][animState];
    }

    public void GoBackStorage_RPC()
    {
        //Debug.Log("GoBackStorage_RPC");
        photonView.RPC(nameof(GoBackStorage), RpcTarget.All);
    }

    [PunRPC]
    public void GoBackStorage()
    {
        _isShoot = false;
        _collider2D.enabled = false;

        ParticleIndex = -1;
        _rigidbody2D.velocity = Vector3.zero;
        transform.position = gameManager._bulletStorage;
        _triggerWallSet.Clear();
        gameManager._weaponStorage[_weaponType].Add(this);
        for (int i = 0; i < _particles.Length; i++)
        {
            _particles[i].gameObject.SetActive(true);
        }
    }
    #endregion

    #region Hit
    protected override void Hit(Collider2D collision)
    {
        if (_isShoot)
        {
            CharacterBase characterBase = collision.GetComponent<CharacterBase>();
            if (characterBase && characterBase.photonView.Owner!= photonView.Owner 
                && characterBase._locationStatus == _locationStatus)
            {
                if (_isSplash && _explosion)
                {
                    Splash();
                }
                else
                {
                    HitPlayer(ref characterBase);
                }
            }
            else
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
        }
    }

    private void HitPlayer(ref CharacterBase characterBase)
    {
        characterBase.Damage_Player(_damage);
        PlaySound_RPC(0);
        ParticleIndex = 1;
        _rigidbody2D.velocity= Vector2.zero;
        Invoke(nameof(GoBackStorage_RPC), _particles[ParticleIndex].main.startLifetimeMultiplier);
    }

    private void HitWall()
    {
        ParticleIndex = 2;
        PlaySound_RPC(1);
        _rigidbody2D.velocity = Vector2.zero;
        Invoke(nameof(GoBackStorage_RPC), _particles[ParticleIndex].main.startLifetimeMultiplier);
    }

    private void Splash()
    {
        RaycastHit2D[] _targetsInArea = Physics2D.CircleCastAll(transform.position, 
            _explosion._splashLength, Vector2.zero, 0f, gameManager._playerLayer);
        foreach (var target in _targetsInArea)
        {
            target.transform.GetComponent<CharacterBase>().Damage_Player(_explosion._splashDamage);
        }
        PlaySound_RPC(0);
        ParticleIndex = 1;
        _rigidbody2D.velocity = Vector2.zero;
        Invoke(nameof(GoBackStorage_RPC), _particles[ParticleIndex].main.startLifetimeMultiplier);
    }
    #endregion
}
