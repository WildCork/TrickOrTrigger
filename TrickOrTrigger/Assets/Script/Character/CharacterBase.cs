using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Spine;
using Spine.Unity;
using static Weapon;
using static Item;
using static GameManager;
using static InputController;
using System;

public class CharacterBase : ObjectBase, IPunObservable
{
    Vector3 _curPos = Vector3.zero;
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.localScale);
            stream.SendNext(_currentAnimName);
            stream.SendNext(hp);
            stream.SendNext(audioIndex);
        }
        else
        {
            _curPos = (Vector3)stream.ReceiveNext();
            transform.localScale = (Vector3)stream.ReceiveNext();
            _skeletonAnimation.AnimationName = (string)stream.ReceiveNext();
            hp = (int)stream.ReceiveNext();
            audioIndex = (int)stream.ReceiveNext();
        }
    }

    #region Variables
    public enum Direction { Left, Right }
    [SerializeField] private DetectGround m_detectGround;


    public PlayerBar _playerBar = null;

    public int _actNum = -1;
    [Header("Character Stats")]
    public int _maxHp = 200;
    public int _maxBulletCnt = 300;
    [SerializeField] private int _hp = 200;
    [SerializeField] private float _maxDropVelocity = 60;

    [Header("Weapon Stats")]
    [SerializeField] private int _bulletCnt = -1;
    [SerializeField] private WeaponType _weaponType = WeaponType.Pistol;

    [Header("Character Ability")]
    [Range(0, 20)]
    [SerializeField] private float _walkSpeed = 12;
    [Range(0, 30)]
    [SerializeField] private float _runSpeed = 20;
    [Range(0, 50)]
    [SerializeField] private float _jumpPower = 36;
    [Range(0, 2)]
    [SerializeField] private float _canShortJumpTime = 0.25f;
    [Range(0, 50)]
    [SerializeField] private float _forceToBlockJump = 25;

    [Header("Character Condition")]
    [SerializeField] private bool _isJump = false;
    [SerializeField] private bool _isAttack = false;
    [SerializeField] private bool _isStopJump = false;
    [SerializeField] private bool _isOnGround = false;
    [SerializeField] private float _onJumpTime = 0f;
    [SerializeField] private float _currentShootDelay = 0f;
    [SerializeField] private float _cancelShootDelay = 0.3f;

    [Header("Animation")]
    [SerializeField] private string _currentAnimName = "";
    [SerializeField] private SkeletonAnimation _skeletonAnimation;

    public Transform _bulletStorage = null;
    public ParticleSystem[] _shootEffectParticles;
    private ExposedList<Spine.Animation> _animationsList;
    public static Dictionary<WeaponType, Dictionary<string, string>> _spineNameDict = new();
    public static Dictionary<WeaponType, Dictionary<string, float>> _spineTimeDict = new();

    [Header("Sound")]
    public AudioListener _audioListener = null;
    public int _audioIndex = -1; // ground Sound (항시 사운드(쉬기, 달리기))
    public AudioSource _audioSource2 = null; // center Sound (이벤트성 사운드(총))

    //Sound
    //0: pistol shot
    //1: machine gun shot
    //2: shotgun shot
    //3: jump
    //4: landing
    //5: run
    //6: damaged
    //7: knife

    #endregion

    //TODO: 점프,착지,달리기,걷기,피격,idle 사운드 구현
    //

    #region Property

    [SerializeField]
    private Dictionary<WeaponType, Vector2> shootXOffset = new()
    {
        {WeaponType.Pistol,     4.5f * Vector2.right },
        {WeaponType.Machinegun, 5f * Vector2.right},
        {WeaponType.Shotgun,    10f * Vector2.right}
    };
    private Dictionary<WeaponType, Vector2> shootYOffset = new()
    {
        {WeaponType.Pistol,     3.5f * Vector2.up },
        {WeaponType.Machinegun, 3f * Vector2.up },
        {WeaponType.Shotgun,    3f * Vector2.up }
    };
    public Vector3 ShootPos
    {
        get
        {
            Vector2 pos = transform.position;
            switch (direction)
            {
                case Direction.Left:
                    return pos - shootXOffset[currentWeaponType] + shootYOffset[currentWeaponType];
                case Direction.Right:
                    return pos + shootXOffset[currentWeaponType] + shootYOffset[currentWeaponType];
                default:
                    return pos;
            }
        }
    }
    public Vector3 throwPos
    {
        get
        {
            switch (direction)
            {
                case Direction.Left:
                    return transform.position + Vector3.left + Vector3.up;
                case Direction.Right:
                    return transform.position + Vector3.right + Vector3.up;
                default:
                    return transform.position;
            }
        }
    }

    public WeaponType currentWeaponType
    {
        get { return _weaponType; }
        set
        {
            if (_weaponType != value)
            {
                _weaponType = value;
            }
        }
    }

    public int isShootUpDown
    {
        get { return _isShoorUpDown; }
        set
        {
            _isShoorUpDown = (value > 0 ? 1 : -1);
        }
    }

    public float currentAttackDelay
    {
        get { return _currentShootDelay; }
        set
        {
            if (value > 0)
            {
                _currentShootDelay = value;
            }
            else
            {
                _currentShootDelay = 0;
            }
        }
    }

    public int hp 
    {
        get { return _hp; }
        set
        {
            if (value > _hp)
            {
                if (value > _maxHp)
                {
                    _hp = _maxHp;
                }
                else
                {
                    _hp = value;
                }
                Recover();
            }
            else if (value < _hp)
            {
                if (value <= 0)
                {
                    _hp = 0;
                }
                else
                {
                    _hp = value;
                }
                Damaged();
            }
            if (_playerBar)
            {
                _playerBar.RefreshHP();
            }
        }
    }
    public int bulletCnt
    {
        get { return _bulletCnt; }
        set
        {
            if (value > _maxBulletCnt)
            {
                value = _maxBulletCnt;
            }
            else if (value <= 0)
            {
                value = -1;
            }
            _bulletCnt = value;
            _playerBar.RefreshBulletCnt();
        }
    }

    public Direction direction
    {
        get { return transform.localScale.x < 0 ? Direction.Right : Direction.Left; }
    }

    public int audioIndex
    {
        get { return _audioIndex; }
        set
        {
            if (value < 0)
            {
                _audioSource.clip = null;
            }
            else if (_audioSource.clip != _audioClips[value])
            {
                _audioSource.clip = _audioClips[value];
                _audioSource.Play();
            }
        }
    }

    #endregion


    protected override void Awake()
    {
        base.Awake();
        gameManager._characterViewIDSet.Add(gameObject.GetPhotonView().ViewID);
        gameManager._characterIDToCharacterBase[gameObject.GetPhotonView().ViewID] = this;
        if (photonView.IsMine)
        {
            _collider2D.isTrigger = false;
            _audioListener.enabled= true;
            _hp = _maxHp;
            _bulletCnt = -1;
            MatchAnimation();
        }
        else
        {
            _rigidbody2D.gravityScale = 0f;
            _audioListener.enabled= false;
            _collider2D.isTrigger = true;
        }
    }


    private void FixedUpdate()
    {
        if (!gameManager._isGame || !photonView.IsMine)
        {
            return;
        }
        Move(ref inputController._horizontal, ref inputController._walk);
    }

    private void Update()
    {
        if (gameManager._isGame)
        {
            if (photonView.IsMine)
            {
                if (_isOnGround)
                {
                    Turn(ref inputController._horizontal);
                    if (inputController._descend)
                    {
                        Descend();
                    }
                }
                TryAttack();
                TryJump();
                PlayAnim();
            }
            else if ((transform.position - _curPos).sqrMagnitude >= 100)
            {
                transform.position = _curPos;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, _curPos, Time.deltaTime * 10);
            }
        }
    }


    #region Spine Animation
    private void MatchAnimation()
    {
        _animationsList = _skeletonAnimation.SkeletonDataAsset.GetAnimationStateData().SkeletonData.Animations;
        foreach (Spine.Animation anim in _animationsList)
        {
            switch (anim.Name[0])
            {
                case '2':
                    MatchAnimToWeapon(WeaponType.Pistol, anim);
                    break;
                case '3':
                    MatchAnimToWeapon(WeaponType.Shotgun, anim);
                    break;
                case '4':
                    MatchAnimToWeapon(WeaponType.Machinegun, anim);
                    break;
                case '5':
                    MatchAnimToWeapon(WeaponType.Knife, anim);
                    break;
                default:
                    break;
            }
        }
    }

    private void MatchAnimToWeapon(WeaponType weaponType, Spine.Animation animation)
    {
        if (!_spineNameDict.ContainsKey(weaponType))
        {
            _spineNameDict.Add(weaponType, new());
        }
        if (!_spineTimeDict.ContainsKey(weaponType))
        {
            _spineTimeDict.Add(weaponType, new());
        }
        foreach (string animName in gameManager._animNameDict.Values)
        {
            if (!_spineNameDict[weaponType].ContainsKey(animName))
            {
                if (animation.Name.Contains(animName))
                {
                    _spineNameDict[weaponType].Add(animName, animation.Name);
                    if (weaponType == WeaponType.Machinegun)
                        _spineTimeDict[weaponType].Add(animName, 0.1f);
                    else if (weaponType == WeaponType.Shotgun && animName == gameManager._animNameDict[AnimState.Jump_shoot])
                        _spineTimeDict[weaponType].Add(animName, animation.Duration / 2);
                    else
                        _spineTimeDict[weaponType].Add(animName, animation.Duration);
                    return;
                }
            }
        }
    }

    private string ReturnAnimName(AnimState animState)
    {
        return _spineNameDict[currentWeaponType][gameManager._animNameDict[animState]];
    }

    private void PlayAnim()
    {
        if (_isOnGround)
        {
            if (inputController._horizontal != 0)
            {
                if (inputController._walk)
                {
                    if (_isAttack)
                        _currentAnimName = ReturnAnimName(AnimState.Walk_shoot);
                    else
                        _currentAnimName = ReturnAnimName(AnimState.Walk);
                }
                else
                {
                    if (_isAttack)
                        _currentAnimName = ReturnAnimName(AnimState.Run_shoot);
                    else
                        _currentAnimName = ReturnAnimName(AnimState.Run);
                }
            }
            else
            {
                if (_isAttack)
                    _currentAnimName = ReturnAnimName(AnimState.Shoot);
                else
                    _currentAnimName = ReturnAnimName(AnimState.Idle);
            }
        }
        else
        {
            if (_isAttack)
                _currentAnimName = ReturnAnimName(AnimState.Jump_shoot);
            else
                _currentAnimName = ReturnAnimName(AnimState.Jump_airborne);
        }
        _skeletonAnimation.AnimationName = _currentAnimName;
    }
    #endregion

    #region Move Part


    private void Move(ref float horizontalInput, ref bool walkInput)
    {
        if (!_isOnGround)
        {
            AirBorne(ref horizontalInput);
        }
        else
        {
            if (horizontalInput != 0f)
            {
                if (walkInput)
                    Walk(ref horizontalInput);
                else
                    Run(ref horizontalInput);
            }
            else
            {
                Idle();
            }
        }
        LimitDropVelocity();
    }

    private void Idle()
    {
        audioIndex = -1;//TODO: 사운드 추가
    }

    private void Walk(ref float horizontalInput)
    {
        transform.Translate(Vector2.right * horizontalInput * _walkSpeed * Time.deltaTime);
        audioIndex = -1;//TODO: 사운드 추가
    }

    private void Run(ref float horizontalInput)
    {
        transform.Translate(Vector2.right * horizontalInput * _runSpeed * Time.deltaTime);
        audioIndex = 5;
        //TODO: 오디오 소스 두개로 운영 로직 구현 -> 바디 중심, 발        
    }

    private void AirBorne(ref float horizontalInput)
    {
        transform.Translate(Vector2.right * horizontalInput * _runSpeed * Time.deltaTime);
        audioIndex = -1;//TODO: 사운드 추가
    }


    private void LimitDropVelocity()
    {
        if (_rigidbody2D.velocity.y < -_maxDropVelocity)
        {
            if (_rigidbody2D.gravityScale != 0f)
                _rigidbody2D.gravityScale = 0f;
        }
        else
        {
            if (_rigidbody2D.gravityScale == 0f)
                _rigidbody2D.gravityScale = 1f;
        }
    }

    private void Turn(ref float horizontalInput)
    {
        Vector2 preLocalScale = transform.localScale;
        if (horizontalInput != 0)
        {
            if ((horizontalInput > 0) == (preLocalScale.x > 0))
            {
                preLocalScale.x *= -1;
            }
        }
        transform.localScale = preLocalScale;
    }
    private void Descend()
    {
        m_detectGround.Descend();
    }

    #endregion

    #region Jump Part

    private void TryJump()
    {
        if (_isOnGround && !_isJump)
        {
            if (inputController._jumpDown)
            {
                Jump();
            }
        }
        else
        {
            if (_isJump && !_isStopJump)
            {
                _onJumpTime += Time.deltaTime;
                if (_onJumpTime < _canShortJumpTime && inputController._jumpUp)
                {
                    LimitJump();
                }
            }
        }
    }
    private void Jump()
    {
        _isOnGround = false;
        _isJump = true;
        _isStopJump = false;
        _rigidbody2D.AddForce(Vector2.up * _jumpPower, ForceMode2D.Impulse);
        PlaySound2_RPC(3);
    }

    private void LimitJump()
    {
        _isStopJump = true;
        _rigidbody2D.AddForce(Vector2.down * _forceToBlockJump, ForceMode2D.Impulse);
    }

    #endregion

    #region Attack Part

    private void TryAttack()
    {
        if (inputController._attackDown)
        {
            _isAttack = true;
        }
        else if (inputController._attackUp)
        {
            _isAttack = false;
            if(currentAttackDelay > _cancelShootDelay)
                currentAttackDelay = _cancelShootDelay;
        }

        if (_isAttack)
        {
            if (gameManager._weaponStorage[currentWeaponType].Count == 0)
            {
                Debug.LogError("There is no bullets!!");
                return;
            }
            Shoot();
        }
    }

    private int _isShoorUpDown = 1;
    private void Shoot()
    {
        currentAttackDelay -= Time.deltaTime;
        if (currentAttackDelay > 0)
        {
            return;
        }
        bulletCnt--;
        PlaySound2_RPC((int)currentWeaponType);
        photonView.RPC(nameof(ShootEffect), RpcTarget.All, (int)currentWeaponType);
        gameManager._weaponStorage[currentWeaponType][0].Shoot(this, _isOnGround, inputController._horizontal, inputController._walk);
        if (bulletCnt < 0)
        {
            ReturnToPistol();
        }
    }

    [PunRPC]
    public void ShootEffect(int weaponType)
    {
        _shootEffectParticles[weaponType].Play();
    }

    private void ReturnToPistol()
    {
        if (currentWeaponType != WeaponType.Pistol)
        {
            currentWeaponType = WeaponType.Pistol;
        }
    }

    #endregion

    #region Special Event

    public void RefreshHP_Player(int hp)
    {
        photonView.RPC(nameof(RefreshHP), photonView.Owner, hp);
    }

    [PunRPC]
    public void RefreshHP(int hp)
    {
        this.hp = hp;
    }


    private void Die()
    {

    }

    private void Recover()
    {

    }
    private void Damaged()
    {
        if(hp > 0)
        {

        }
        else
        {

        }
        PlaySound(6);
    }

    protected override void Hit(Collider2D collision)
    {
        if (!photonView.IsMine)
        {
            return;
        }
        base.Hit(collision);
        LayerMask layer = collision.gameObject.layer;
        if (layer == gameManager._itemLayer)
        {
            Item _item = collision.gameObject.GetComponent<Item>();
            if(_item._isHit)
            {
                return;
            }
            _item._isHit = true;
            switch (_item._itemType)
            {
                case ItemType.Bullet:
                    _item.Reload(this);
                    break;
                case ItemType.Health:
                    _item.Heal(this);
                    break;
                default:
                    break;
            }
        }
    }



    #endregion

    #region Sound

    protected void PlaySound2_RPC(int index) // Center 소리
    {
        photonView.RPC(nameof(PlaySound2), RpcTarget.All, index);
    }

    [PunRPC]
    protected void PlaySound2(int index)
    {
        if (index < 0)
        {
            _audioSource2.clip = null;
        }
        else
        {
            _audioSource2.clip = _audioClips[index];
        }
        _audioSource2.Play();
    }

    #endregion

    #region Refresh Map

    protected override void RefreshLocationStatus(LocationStatus locationStatus)
    {
        base.RefreshLocationStatus(locationStatus);
        gameManager.RenewMap();
    }
    public void RefreshOnGround(bool value)
    {
        if (_isOnGround != value)
        {
            _isOnGround = value;
            if (_isOnGround)
            {
                inputController._jumpUp = false;
                _isJump = false;
                _isStopJump = false;
                _onJumpTime = 0f;
                PlaySound2_RPC(4);
            }
        }
    }
    #endregion

}
