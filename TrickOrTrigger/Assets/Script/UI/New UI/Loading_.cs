using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static DontDestroyData;

public class Loading_ : MonoBehaviour
{
    public static Loading_ loading = null;

    public enum NetworkState
    {
        None, EnterServer = 6, EnterRoom = 3, LeaveRoom = 4,
        PlayGame, StopGame = 2
    }

    public NetworkState _networkState;
    public string _networkString;

    [Space(10)]
    public Slider _loadingSlider = null;
    public TMP_Text _loadingText = null;

    private Canvas _canvas =null;
    public int _changeCnt = 0;

    [Space(10)]
    public GameObject[] _loadingCharacters;
    public int _characterIndex = 0;

    //private string[] _tipComents = {
    //    "무기 아이템을 먹으면 자동으로 무기가 교체됩니다!\n하지만 무제한은 아니므로 아껴쓰세요!!",
    //    "적의 위치가 보인다는 것은 적 또한 나의 위치를 안다는 것!\n신중히 움직이세요!!",
    //    "나의 목숨은 하나뿐! 그건 상대도 마찬가지!!"
    //    };

    private void Awake()
    {
        if (loading)
        {
            loading._canvas.worldCamera = Camera.main;
            loading.gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }
        else
        {
            loading = this;
            _loadingSlider.value = 0;
            _canvas = GetComponent<Canvas>();
            gameObject.SetActive(false);
        }
        DontDestroyOnLoad(gameObject);
    }
    
    public void RenewValue(string statusText)
    {
        _changeCnt++;
        switch (_networkState)
        {
            case NetworkState.EnterServer:
            case NetworkState.EnterRoom:
            case NetworkState.LeaveRoom:
                _loadingSlider.value = (_changeCnt / (float)_networkState);
                _loadingText.text = statusText;
                break;
            case NetworkState.PlayGame:
            case NetworkState.StopGame:
                //GameManager가 관리
                break;
            default:
                break;
        }
    }

    public void ShowLoading(bool condition, NetworkState networkState = NetworkState.None)
    {
        _loadingSlider.value = 0;
        _loadingText.text = "0%";
        _changeCnt = 0;
        _characterIndex = (int)_dontDestroyData._characterKind;

        for (int i = 0; i < _loadingCharacters.Length; i++)
        {
            if (i == _characterIndex)
            {
                _loadingCharacters[i].gameObject.SetActive(true);
            }
            else
            {
                _loadingCharacters[i].gameObject.SetActive(false);
            }
        }
        if (condition)
        {
            _networkState = networkState;
            _loadingText.text = _networkState.ToString();
            switch (_networkState)
            {
                case NetworkState.LeaveRoom:
                case NetworkState.StopGame:
                    //_tipText.text = "";
                    break;
                case NetworkState.PlayGame:
                case NetworkState.EnterRoom:
                case NetworkState.EnterServer:
                    //_tipText.text = _tipComents[Random.Range(0, _tipComents.Length)];
                    break;
                default:
                    break;
            }
        }

        gameObject.SetActive(condition);
    }

    public void RefreshDirectly(string text, float value)
    {
        _loadingSlider.value = value;
        _loadingText.text = text;
    }
}
