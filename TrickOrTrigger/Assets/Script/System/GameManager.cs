using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Weapon;
using static Lobby;
using static Loading;
using static InputController;
using Photon.Pun;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks
{
    public enum AnimState
    {
        Jump_airborne, Jump_land, Walk_shoot, Jump_shoot, Run_shoot,
        Idle, Walk, Run, Jump, Shoot, Die, Hurt
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

    public HashSet<int> _characterViewIDSet = new();
    public HashSet<int> _playerBarViewIdSet = new();
    public HashSet<int> _storageViewIdSet = new();

    public Dictionary<WeaponType, List<Weapon>> _weaponStorage = new();

    public Dictionary<int, int> _characterIDToPlayerBarID = new();
    public Dictionary<int, int> _characterIDToStorageID = new();
    public Dictionary<int, string> _characterIDToNickname = new();

    public Dictionary<int, CharacterBase> _characterIDToCharacterBase = new();
    public Dictionary<int, PlayerBar> _playerBarIDToPlayerBar = new();
    public Dictionary<int, Storage> _storageIDToStorage = new();

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
        {AnimState.Run_shoot,"Run_shoot" },
        {AnimState.Idle,"Idle" },
        {AnimState.Walk, "Walk"},
        {AnimState.Run,"Run"},
        //{AnimState.Jump,"Jump"},
        {AnimState.Shoot,"Shoot"},
        {AnimState.Die,"Die"},
        {AnimState.Hurt,"Hurt"}
    };

    [Header("Scene")]
    public string _lobbyName = "Lobby";


    private float _frameCycle;
    private void Update()
    {
        _frameCycle = Time.deltaTime * 1000f;
        if(inputController._quitDown)
        {
            Quit();
        }
    }

    private void Quit()
    {
        loading.ShowLoading(true, NetworkState.StopGame);
        PhotonNetwork.LoadLevel(_lobbyName);
        _lobby.gameObject.SetActive(true);
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
        _character = _characterTypes[(int)_lobby._characterKind];
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            photonView.RPC(nameof(SendRespawnSeed), RpcTarget.AllBufferedViaServer, 
                Random.Range(0, _respawnSeeds.Count), Random.Range(0, _respawnSeeds.First().Count));
        }
        InitLayerValue();
        loading.RefreshDirectly("Set Stage",0.1f);
        StartCoroutine(InitGame());
    }

    [PunRPC]
    private void SendRespawnSeed(int seed1, int seed2)
    {
        _seed1 = seed1;
        _seed2 = seed2;
    }

    private WaitForSeconds waitForSeed = new(0.2f);
    IEnumerator InitGame()
    {
        //Seed
        do
        {
            yield return waitForSeed;
        } while (_seed1 < 0);
        yield return waitForSeed;

        //SpawnPlayerBar
        loading.RefreshDirectly("Locate", 0.3f);
        int actNum = ActNumber();
        SpawnPlayerBar();
        do
        {
            yield return waitForSeed;
        } while (_playerBarViewIdSet.Count < PhotonNetwork.CurrentRoom.PlayerCount);
        yield return waitForSeed;

        //SpawnPlayer
        loading.RefreshDirectly("Spawn", 0.5f);
        SpawnPlayer(actNum);
        do
        {
            yield return waitForSeed;
        } while (_characterViewIDSet.Count < PhotonNetwork.CurrentRoom.PlayerCount
            || _characterIDToNickname.Count < PhotonNetwork.CurrentRoom.PlayerCount
            || _characterIDToPlayerBarID.Count < PhotonNetwork.CurrentRoom.PlayerCount);
        MatchPlayerBar_RPC();
        yield return waitForSeed;

        //SpawnStorage
        loading.RefreshDirectly("Init", 0.7f);
        SpawnStorage();
        do
        {
            yield return waitForSeed;
        } while (_storageViewIdSet.Count < PhotonNetwork.CurrentRoom.PlayerCount);
        MatchStorage_RPC();

        //Start
        loading.RefreshDirectly("Start!!", 1f);
        yield return waitForSeed;
        InitGameSetting();
        _isGame = true;
        loading.ShowLoading(false);
    }

    public void MatchPlayerBar_RPC()
    {
        photonView.RPC(nameof(MatchPlayerBar), RpcTarget.AllBufferedViaServer);
    }
    public void MatchStorage_RPC()
    {
        photonView.RPC(nameof(MatchStorage), RpcTarget.AllBufferedViaServer);
    }

    [PunRPC]
    public void MatchPlayerBar()
    {
        foreach (int id in _characterViewIDSet)
        {
            CharacterBase character = _characterIDToCharacterBase[id];
            int playerBarId = _characterIDToPlayerBarID[id];
            string nickname = _characterIDToNickname[id];
            character._playerBar = _playerBarIDToPlayerBar[playerBarId];
            _playerBarIDToPlayerBar[playerBarId].RefreshDefault(character, nickname);
        }
    }

    [PunRPC]
    public void MatchStorage()
    {
        foreach (int id in _characterViewIDSet)
        {
            CharacterBase character = _characterIDToCharacterBase[id];
            int storageID = _characterIDToStorageID[id];
            _storageIDToStorage[storageID]._ownerStorage = character._bulletStorage;
        }
    }

    [PunRPC]
    private void SendPlayerBarInfo(int characterID, int playerBarID, string nickname)
    {
        _characterIDToPlayerBarID[characterID] = playerBarID;
        _characterIDToNickname[characterID] = nickname;
    }

    [PunRPC]
    private void SendStorageInfo(int characterID, int storageID)
    {
        _characterIDToStorageID[characterID] = storageID;
    }

    private void SpawnPlayer(int actNum)
    {
        _character = PhotonNetwork.Instantiate(_character.gameObject.name,
            _spawnPoints[_respawnSeeds[_seed1][(_seed2 + actNum)% _respawnSeeds.First().Count]].position, 
            Quaternion.identity).GetComponent<CharacterBase>();
        photonView.RPC(nameof(SendPlayerBarInfo), RpcTarget.AllBufferedViaServer,
            _character.gameObject.GetPhotonView().ViewID, _playerBar.gameObject.GetPhotonView().ViewID, PhotonNetwork.LocalPlayer.NickName);
        _playerNickname = PhotonNetwork.LocalPlayer.NickName;
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
    private void SpawnPlayerBar()
    {
        _playerBar = PhotonNetwork.Instantiate(_playerBar.gameObject.name, Vector3.zero, Quaternion.identity).GetComponent<PlayerBar>();
    }
    private void SpawnStorage()
    {
        _storage = PhotonNetwork.Instantiate(_storage.name, Vector3.down * 100, Quaternion.identity);
        photonView.RPC(nameof(SendStorageInfo), RpcTarget.All,
            _character.gameObject.GetPhotonView().ViewID, _storage.gameObject.GetPhotonView().ViewID);
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
        Debug.Log(debug);
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
    public void RenewMap()
    {
        _map.RenewMap();
    }
}
