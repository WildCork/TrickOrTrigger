using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Weapon;
using static DontDestroyData;
using static Loading;
using static InputController;
using Photon.Pun;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region Variables


    public enum AnimState
    {
        Jump_airborne, Jump_land, Jump_slash, Walk_shoot, Jump_shoot, Run_shoot,
        Idle, Walk, Run, Jump, Shoot, Die, Hurt, Stab, Slash
    };
    public static GameManager gameManager = null;
    public string _playerNickname = "";

    public Map _map = null;
    public CharacterBase[] _characterTypes;
    public CharacterBase _character = null;
    public PlayerBar _playerBar = null;
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

    public HashSet<CharacterBase> _characterSet = new();
    public HashSet<PlayerBar> _playerBarSet = new();
    public HashSet<Storage> _storageSet = new();

    public Dictionary<WeaponType, List<Weapon>> _weaponStorage = new();

    [HideInInspector] public LayerMask _inLayer = -1;    //In
    [HideInInspector] public LayerMask _outLayer = -1;   //Out
    [HideInInspector] public LayerMask _doorLayer = -1;  //Door
    [HideInInspector] public LayerMask _wallLayer = -1;  //Wall
    [HideInInspector] public LayerMask _playerLayer = -1;  //Player
    [HideInInspector] public LayerMask _bulletLayer = -1;  //Bullet
    [HideInInspector] public LayerMask _itemLayer = -1;  //Bullet

    [Header("Tag")]
    public string _playerTag = "Player";
    public string _bottomTag = "Bottom";
    public string _groundTag = "Ground";

    [Header("ObjectName")]
    public string _storageName = "Storage";
    public string _pistolName = "Pistol";
    public string _machineGunName = "MachineGun";
    public string _shotGunName = "ShotGun";
    public string _knifeName = "Knife";
    public string _bombName = "Bomb";
    public string _playBarName = "PlayerBar"; 
    public string _mapName = "MapCollider";

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
    //����, ������ ����
    //������ ��� ����Ʈ ��, ��ũ �� ���� UI ����
    //�������� ��� �� ���� UI ����
    //����: �ٿ�Ƽ, ����
    //������: ������ġ

    //��� ���� ����

    //�κ� ����Ʈ���� ����, �ε��� ������ ���� ���

    private void Quit()
    {
        loading.ShowLoading(true, NetworkState.StopGame);
        PhotonNetwork.LoadLevel(_lobbyName);
    }

    private void Awake()
    {
        if (gameManager)
        {
            Destroy(this);
            return;
        }
        gameManager = this;
        loading.GetComponent<Canvas>().worldCamera = Camera.main;
        _character = _characterTypes[(int)_dontDestroyData._characterKind];
        loading.RefreshDirectly("Init", 0.1f);
        InitLayerValue();
        StartCoroutine(InitGame());
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

    public void MatchPlayerBar()
    {
        foreach (var character in _characterSet)
        {
            foreach (var playerBar in _playerBarSet)
            {
                if (character.photonView.Owner == playerBar.photonView.Owner)
                {
                    playerBar.Init(character);
                    break;
                }
            }
        }
    }

    public void MatchStorage()
    {
        foreach (var character in _characterSet)
        {
            foreach (var storage in _storageSet)
            {
                if (character.photonView.Owner == storage.photonView.Owner)
                {
                    storage._ownerStorage = character._bulletStorage;
                    break;
                }
            }
        }
    }


    #endregion

    #region Spawn
    private void SpawnPlayerBar()
    {
        _playerBar = PhotonNetwork.Instantiate(_playerBar.gameObject.name, Vector3.zero, Quaternion.identity).GetComponent<PlayerBar>();
    }
    private void SpawnPlayer(int actNum)
    {
        _character = PhotonNetwork.Instantiate(_character.gameObject.name,
            _spawnPoints[_respawnSeeds[_seed1][(_seed2 + actNum)% _respawnSeeds.First().Count]].position, 
            Quaternion.identity).GetComponent<CharacterBase>();
        _playerNickname = PhotonNetwork.LocalPlayer.NickName;
    }
    private void SpawnStorage()
    {
        _storage = PhotonNetwork.Instantiate(_storage.name, Vector3.down * 100, Quaternion.identity);
    }
    #endregion

    #region Init
    IEnumerator InitGame()
    {
        //Seed
        loading.RefreshDirectly("Locate", 0.3f);
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

        //SpawnPlayerBar
        loading.RefreshDirectly("Spawn Player", 0.5f);
        int actNum = ActNumber();
        SpawnPlayerBar();
        do
        {
            yield return _waitForSecond;
        } while (_playerBarSet.Count < PhotonNetwork.CurrentRoom.PlayerCount);
        yield return _waitForSecond;

        //SpawnPlayer
        SpawnPlayer(actNum);
        do
        {
            yield return _waitForSecond;
        } while (_characterSet.Count < PhotonNetwork.CurrentRoom.PlayerCount);
        MatchPlayerBar();
        yield return _waitForSecond;

        //SpawnStorage
        loading.RefreshDirectly("Spawn Storage", 0.7f);
        SpawnStorage();
        do
        {
            yield return _waitForSecond;
        } while (_storageSet.Count < PhotonNetwork.CurrentRoom.PlayerCount);
        MatchStorage();

        //Start
        loading.RefreshDirectly("Start!!", 1f);
        InitGameSetting();
        StartGame_RPC(RpcTarget.AllBufferedViaServer);
        do
        {
            yield return _waitForSecond;
        } while (!_isGame);
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

    private void InitGameSetting()
    {
        InitBulletStorage();
        InitMap();
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
        LoadBullet(_storage.transform.Find(_pistolName + _storageName), WeaponType.Pistol);
        LoadBullet(_storage.transform.Find(_machineGunName + _storageName), WeaponType.Machinegun);
        LoadBullet(_storage.transform.Find(_shotGunName + _storageName), WeaponType.Shotgun);
        string debug = "";
        foreach (var item in _weaponStorage)
        {
            debug += item.Key.ToString() + ": " + item.Value.Count.ToString() + ", "; 
        }
        //Debug.Log(debug);
    }

    private Weapon[] weapons;
    private void LoadBullet(Transform storage, WeaponType _weaponType)
    {
        weapons = storage.GetComponentsInChildren<Weapon>();
        _weaponStorage.Add(_weaponType, new());
        foreach (Weapon weapon in weapons)
        {
            weapon._weaponType = _weaponType;
            _weaponStorage[_weaponType].Add(weapon);
        }
    }

    private void InitMap()
    {
        if (!_map) _map = GameObject.Find(_mapName).GetComponent<Map>();
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
        Debug.LogError("Act number�� ã�� ���߽��ϴ�");
        return -1;
    }
}
