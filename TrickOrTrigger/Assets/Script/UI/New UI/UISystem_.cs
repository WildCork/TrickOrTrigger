using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static FirebaseAuthManager;
using static Loading_;

public class UISystem_ : MonoBehaviourPunCallbacks
{
    public ClientState _clientState;
    public enum TeamMode { Team, Solo}
    public enum GameMode { Bounty}
    public enum CharacterType { Pumpkin, Santa }
    public enum MapType {Castle, City };

    [Header("Login")]
    public Canvas _loginCanvas= null;
    public TMP_InputField _login_id;
    public TMP_InputField _login_password;
    public Toggle _rememberMeToggle = null;

    [Header("SignUp")]
    public Canvas _signupCanvas= null;
    public TMP_InputField _signup_email;
    public TMP_InputField _signup_id;
    public TMP_InputField _signup_password;

    [Header("Lobby")]
    public Canvas _lobbyCanvas = null;

    [Header("Character")]
    public Canvas _characterDetail = null;
    public Canvas _characterSelect = null;

    [Header("Chat")]
    public Canvas _chatCanvas = null;
    public TMP_InputField _chatInput = null;
    public GameObject _myChat = null;
    public GameObject _theOtherChat_Blue = null;
    public GameObject _theOtherChat_Pink = null;
    public GameObject _joinChat = null;

    [Header("Option")]
    public Canvas _optionCanvas = null;
    public Canvas _languageCanvas = null;

    [Header("Event")]
    public Canvas _wrongEventCanvas = null;
    public Canvas _errorNetworkCanvas = null;
    //네트워크 끊김 화면



    //입장 화면
    //로그인
    //회원가입
    //로비
    //캐릭터 선택
    //캐릭터 세부 내용
    //게임 모드 선택
    //게임 모드 세부 내용
    //옵션
    //채팅



    private void Awake()
    {
        Screen.SetResolution(1920, 1080, false);
        firebaseManager.LoginState += OnChangedState;
        Init();
        firebaseManager.Init();
    }

    public void Init()
    {
        _loginCanvas.gameObject.SetActive(false);
        _signupCanvas.gameObject.SetActive(false);

        _rememberMeToggle.isOn = false;
        _login_id.text = "";
        _login_password.text = "";
        _signup_email.text = "";
        _signup_id.text = "";
        _signup_password.text = "";
    }

    void Update()
    {
        _clientState = PhotonNetwork.NetworkClientState;
        switch (_clientState)
        {
            case ClientState.PeerCreated:
                break;
            case ClientState.Authenticating:
                break;
            case ClientState.Authenticated:
                break;
            case ClientState.JoiningLobby:
                break;
            case ClientState.JoinedLobby:
                break;
            case ClientState.DisconnectingFromMasterServer:
                break;
            case ClientState.ConnectingToGameServer:
                break;
            case ClientState.ConnectedToGameServer:
                break;
            case ClientState.Joining:
                break;
            case ClientState.Joined:
                break;
            case ClientState.Leaving:
                break;
            case ClientState.DisconnectingFromGameServer:
                break;
            case ClientState.ConnectingToMasterServer:
                break;
            case ClientState.Disconnecting:
                break;
            case ClientState.Disconnected:
                break;
            case ClientState.ConnectedToMasterServer:
                break;
            case ClientState.ConnectingToNameServer:
                break;
            case ClientState.ConnectedToNameServer:
                break;
            case ClientState.DisconnectingFromNameServer:
                break;
            case ClientState.ConnectWithFallbackProtocol:
                break;
            default:
                break;
        }
    }


    #region LogIn

    private void OnChangedState(bool sign)
    {
        string _output = "";
        _output = sign ? "로그인: " : "로그아웃: ";
        _output += firebaseManager.UserID;
        Debug.Log(_output);
    }

    public void GoLogin()
    {
        Debug.Log("GoLogin");
        _signupCanvas.gameObject.SetActive(false);
        _loginCanvas.gameObject.SetActive(true);
        if(!_rememberMeToggle.isOn)
        {
            _login_id.text = "";
            _login_password.text = "";
        }
    }

    public void LogIn()
    {
        firebaseManager.LogIn(_login_id.text, _login_password.text);
    }

    public void CancelLogin()
    {
        _loginCanvas.gameObject.SetActive(false);
    }

    public void GoSignUp()
    {
        _signupCanvas.gameObject.SetActive(true);
        _loginCanvas.gameObject.SetActive(false);
    }

    public void SignUp()
    {
        firebaseManager.SignUp(_signup_email.text, _signup_id.text, _signup_password.text);
    }

    public void CancelSignUp()
    {
        _signupCanvas.gameObject.SetActive(false);
    }

    public void LogOut()
    {
        firebaseManager.LogOut();
    }
    #endregion

    #region Server


    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        loading.ShowLoading(false);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        loading.ShowLoading(false);
    }
    #endregion

    #region Room

    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();

    public void Disconnect() => PhotonNetwork.Disconnect();

    public override void OnJoinedRoom()
    {

    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        //Debug.Log("OnCreateRoomFailed");
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        //Debug.Log("OnJoinRandomFailed");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //Debug.Log("OnPlayerEnteredRoom");
        Chat_RPC(RpcTarget.AllBufferedViaServer, "<color=yellow> Player " + newPlayer.NickName + " enters this room.</color> (OnPlayerEnteredRoom)");
    }


    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //Debug.Log("OnPlayerLeftRoom");
        Chat_RPC(RpcTarget.AllBufferedViaServer, "<color=yellow> Player " + otherPlayer.NickName + " exits this room.</color> (OnPlayerLeftRoom)");
    }

    #endregion


    #region Chat
    public void Send()
    {
        if (_chatInput.text != "")
        {
            Chat_RPC(RpcTarget.AllBufferedViaServer, PhotonNetwork.NickName + " : " + _chatInput.text);
            _chatInput.text = "";
            _chatInput.ActivateInputField();
        }
    }
    public void RenewChat()
    {
        //_chatInput.text = "";
        //_chatInput.ActivateInputField();
        //for (int i = 0; i < _chatCells.Length; i++)
        //{
        //    _chatCells[i].text = "";
        //}
    }

    public void Chat_RPC(RpcTarget rpcTarget, string msg)
    {
        photonView.RPC(nameof(Chat), rpcTarget, msg);
    }

    [PunRPC] // _RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    public void Chat(string msg)
    {
        //for (int i = 0; i < _chatCells.Length; i++)
        //{
        //    if (i < _chatCells.Length - 1)
        //    {
        //        _chatCells[i].text = _chatCells[i + 1].text;
        //    }
        //    else
        //    {
        //        _chatCells[i].text = msg;
        //    }
        //}
    }
    #endregion

    public void ChangeScene_RPC(RpcTarget rpcTarget, string sceneName)
    {
        photonView.RPC(nameof(ChangeScene), rpcTarget, sceneName);
    }

    [PunRPC]
    public void ChangeScene(string sceneName)
    {
        loading.ShowLoading(true, NetworkState.PlayGame);
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(sceneName);
    }
}
