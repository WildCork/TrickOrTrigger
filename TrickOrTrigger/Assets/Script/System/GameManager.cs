using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using static DontDestroyData;
using static Loading;
using static InputController;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region Variables

    public enum WeaponType { Pistol = 0, Machinegun, Shotgun, Knife = 7 }
    public enum AnimState
    {
        Jump_airborne, Jump_land, Jump_slash, Walk_shoot, Jump_shoot, Run_shoot,
        Idle, Walk, Run, Jump, Shoot, Die, Hurt, Stab, Slash
    };
    public static GameManager gameManager = null;
    public string _playerNickname = "";

    public Map _map = null;
    public CharacterBase[] _characterTypes;
    [HideInInspector] public CharacterBase _characterBase = null;
    public CameraController _cameraController = null;
    public PlayerUI _playerUI = null;
    public GameObject _storage = null;

    public List<Transform> _spawnPoints = null;
    private List<List<int>> _respawnSeeds = new List<List<int>>
    {
        new List<int>{ 3, 1, 5, 6, 7, 2, 4, 0 },
        new List<int>{ 5, 1, 2, 3, 4, 0, 7, 6 },
        new List<int>{ 7, 6, 5, 2, 0, 3, 4, 1 },
        new List<int>{ 1, 0, 5, 7, 3, 2, 6, 4 },
        new List<int>{ 4, 6, 2, 3, 7, 5, 0, 1 }
    };

    public bool _isGame = false;

    public int _seed1 = -1;
    public int _seed2 = -1;

    public Dictionary<int, CharacterBase> _characterDic = new();
    public Dictionary<int, PlayerUI> _playerUIDic = new();
    public Dictionary<int, Storage> _storageDic = new();

    public Dictionary<WeaponType, List<Bullet>> _weaponStorage = new();

    [HideInInspector] public LayerMask _inLayer = -1;    //In
    [HideInInspector] public LayerMask _outLayer = -1;   //Out
    [HideInInspector] public LayerMask _doorLayer = -1;  //Door
    [HideInInspector] public LayerMask _wallLayer = -1;  //Wall
    [HideInInspector] public LayerMask _playerLayer = -1;  //Player
    [HideInInspector] public LayerMask _bulletLayer = -1;  //Bullet
    [HideInInspector] public LayerMask _itemLayer = -1;  //Bullet

    [Header("Point")]
    public Vector3 _bulletStorage = Vector3.zero;

    [Header("Tag")]
    public string _playerTag = "Player";
    public string _bottomTag = "Bottom";
    public string _groundTag = "Ground";

    [Header("AnimationName")]
    public Dictionary<AnimState, string> _animNameDict = new(){
        {AnimState.Jump_airborne,"Jump_airborne"},
        //{AnimState.Jump_land,"Jump_land"},
        {AnimState.Walk_shoot,"Walk_shoot"},
        {AnimState.Jump_shoot,"Jump_shoot"},
        {AnimState.Jump_slash,"Jump_slash"},
        {AnimState.Run_shoot,"Run_shoot" },
        {AnimState.Idle,"Idle" },
        {AnimState.Walk, "Walk"},
        {AnimState.Run,"Run"},
        //{AnimState.Jump,"Jump"},
        {AnimState.Shoot,"Shoot"},
        {AnimState.Die,"Die"},
        {AnimState.Hurt,"Hurt"},
        {AnimState.Stab,"Stab"},
        {AnimState.Slash, "Slash"}
    };

    [Header("Scene")]
    public string _lobbyName = "Lobby";


    private WaitForSeconds _waitForSecond = new(0.1f);
    public float _frameCycle;

    #endregion

    //TODO: 
    //팀전, 개인전 선택
    //팀전의 경우 라이트 팀, 다크 팀 선택 UI 생성
    //개인전의 경우 팀 선택 UI 삭제
    //팀전: 바운티, 핫존
    //개인전: 데스매치

    //브금 사운드 구현

    //TODO:
    //와이파이 안될때 예외처리
    //샷건 데미지, 판정 콜라디어 두개 운영

    private void Awake()
    {
        if (gameManager)
        {
            Destroy(this);
            return;
        }
        gameManager = this;
        loading.GetComponent<Canvas>().worldCamera = Camera.main;
        _characterBase = _characterTypes[(int)_dontDestroyData._characterKind];
        loading.RefreshDirectly("Init", 0.1f);
        InitLayerValue();
        StartCoroutine(SetGame());
    }
    private void Update()
    {
        _frameCycle = Time.deltaTime * 1000f;
        if (inputController._quitDown)
        {
            Quit();
        }
    }


    [PunRPC]
    private void SendRespawnSeed(int seed1, int seed2)
    {
        _seed1 = seed1;
        _seed2 = seed2;
    }

    #region Matching

    public void MatchPlayerUI()
    {
        foreach (var character in _characterDic)
        {
            foreach (var playerUI in _playerUIDic)
            {
                if (character.Key == playerUI.Key)
                {
                    character.Value._playerUI = playerUI.Value;
                    playerUI.Value.Init(character.Value);
                    break;
                }
            }
        }
    }

    public void MatchStorage()
    {
        foreach (var character in _characterDic)
        {
            foreach (var storage in _storageDic)
            {
                if (character.Key == storage.Key)
                {
                    storage.Value._ownerStorage = character.Value._bulletStorage;
                    break;
                }
            }
        }
    }


    #endregion

    #region Spawn

    private void SpawnPlayerUI()
    {
        _playerUI = PhotonNetwork.Instantiate(_playerUI.gameObject.name, Vector3.zero, Quaternion.identity).GetComponent<PlayerUI>();
    }

    private void SpawnPlayer()
    {
        _characterBase = PhotonNetwork.Instantiate(_characterBase.gameObject.name, Vector3.zero, Quaternion.identity).GetComponent<CharacterBase>();
        _playerNickname = PhotonNetwork.LocalPlayer.NickName;
    }

    private void SpawnStorage()
    {
        _storage = PhotonNetwork.Instantiate(_storage.name, _bulletStorage, Quaternion.identity);
    }

    public void Respawn()
    {
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        _playerUI._canvas.sortingOrder = -1201;
        yield return new WaitForSeconds(3f);
        _seed2 = Random.Range(0, _respawnSeeds.First().Count);
        _characterBase.Init(_spawnPoints[_seed2].position);
        _cameraController._targetPos2 = _spawnPoints[_seed2].position;
    }


    #endregion

    #region Init

    IEnumerator SetGame()
    {
        //Seed
        loading.RefreshDirectly("Init Stage", 0.3f);
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            photonView.RPC(nameof(SendRespawnSeed), RpcTarget.All,
                Random.Range(0, _respawnSeeds.Count), Random.Range(0, _respawnSeeds.First().Count));
        }
        do
        {
            yield return _waitForSecond;
        } while (_seed1 < 0);
        yield return _waitForSecond;

        //Spawn
        loading.RefreshDirectly("Spawn", 0.5f);
        SpawnPlayerUI();
        SpawnPlayer();
        SpawnStorage();
        do
        {
            yield return _waitForSecond;
        } while (_playerUIDic.Count < PhotonNetwork.CurrentRoom.PlayerCount
        || _characterDic.Count < PhotonNetwork.CurrentRoom.PlayerCount
        || _storageDic.Count < PhotonNetwork.CurrentRoom.PlayerCount);

        //Match

        loading.RefreshDirectly("Match", 0.7f);
        MatchPlayerUI();
        MatchStorage();

        //Start
        loading.RefreshDirectly("Start!!", 1f);
        _characterBase.Init(_spawnPoints[_respawnSeeds[_seed1][(_seed2 + ActNumber()) % _respawnSeeds.First().Count]].position);
        InitBulletStorage();
        StartGame_RPC(RpcTarget.AllBufferedViaServer);
        do
        {
            yield return _waitForSecond;
        } while (!_isGame || (_playerUI._characterBase == null));
        loading.ShowLoading(false);
    }

    private void StartGame_RPC(RpcTarget rpcTarget)
    {
        photonView.RPC(nameof(StartGame), rpcTarget);
    }

    [PunRPC]
    private void StartGame()
    {
        _isGame = true;
    }


    private void InitLayerValue()
    {
        _inLayer = LayerMask.NameToLayer("In");
        _outLayer = LayerMask.NameToLayer("Out");
        _doorLayer = LayerMask.NameToLayer("Door");
        _wallLayer = LayerMask.NameToLayer("Wall");
        _playerLayer = LayerMask.NameToLayer("Player");
        _bulletLayer = LayerMask.NameToLayer("Bullet");
        _itemLayer = LayerMask.NameToLayer("Item");
    }

    private void InitBulletStorage()
    {
        LoadBullet(_storage.transform.GetChild(0), WeaponType.Pistol);
        LoadBullet(_storage.transform.GetChild(1), WeaponType.Machinegun);
        LoadBullet(_storage.transform.GetChild(2), WeaponType.Shotgun);
        string debug = "";
        foreach (var item in _weaponStorage)
        {
            debug += item.Key.ToString() + ": " + item.Value.Count.ToString() + ", "; 
        }
        //Debug.Log(debug);
    }

    private Bullet[] weapons;
    private void LoadBullet(Transform storage, WeaponType _weaponType)
    {
        weapons = storage.GetComponentsInChildren<Bullet>();
        _weaponStorage.Add(_weaponType, new());
        foreach (Bullet weapon in weapons)
        {
            weapon._weaponType = _weaponType;
            _weaponStorage[_weaponType].Add(weapon);
        }
    }


    #endregion


    public void RenewMap()
    {
        _map.RenewMap();
    }
    public int ActNumber()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                return i;
            }
        }
        Debug.LogError("Act number를 찾지 못했습니다");
        return -1;
    }

    private void Quit()
    {
        loading.ShowLoading(true, NetworkState.StopGame);
        PhotonNetwork.LoadLevel(_lobbyName);
    }

}
