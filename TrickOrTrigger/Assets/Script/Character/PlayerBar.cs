using Photon.Pun;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static GameManager;
using static UnityEngine.Rendering.DebugUI;

public class PlayerBar : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Photon
    Vector3 _curPos;
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(bulletSliderColorIndex);
            stream.SendNext(_bulletCntSlider.value);
        }
        else
        {
            _curPos = (Vector3)stream.ReceiveNext();
            bulletSliderColorIndex = (int)stream.ReceiveNext();
            _bulletCntSlider.value = (float)stream.ReceiveNext();
        }
    }
    #endregion

    #region Variables
    public Canvas _canvas = null;

    [Header("Value")]
    //[SerializeField] private int _actNum = -1;
    [SerializeField] private float _offsetY = 6f;
    [SerializeField] private int _bulletShowCnt = 30;

    [Header("String")]
    public string _hpTextString = "HPText";
    public string _hpString = "HPBar";
    public string _bulletCntString = "BulletCntBar";
    public string _fillAreaString = "Fill Area";

    [Header("Text")]
    [SerializeField] private Text _hpText;
    [SerializeField] private Text _nicknameText;
    [SerializeField] private Text _bulletCntText;

    [Header("Slider")]
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private Slider _bulletCntSlider;

    [Header("Image")]
    [SerializeField] private Sprite _mineImage = null;
    [SerializeField] private Sprite _allyImage = null;
    [SerializeField] private Sprite _enemyImage = null;
    [SerializeField] private Image _hpFillImage = null;
    [SerializeField] private Image _bulletCntImage = null;

    [Header("Color")]
    public Color[] _bulletSliderColor;
    public int _bulletSliderColorIndex = 0;
    public int bulletSliderColorIndex
    {
        get { return _bulletSliderColorIndex; }
        set
        {
            _bulletSliderColorIndex = value;
            _bulletCntImage.color = _bulletSliderColor[_bulletSliderColorIndex];
        }
    }

    private CharacterBase _character = null;
    #endregion

    private void Awake()
    {
        gameManager._playerBarSet.Add(this);
    }

    public void Init(CharacterBase character)
    {
        character._playerBar = this;
        _character = character;
        _nicknameText.text = photonView.Owner.NickName;
        RefreshHP();
        RefreshBulletCnt();
        switch (character._side)
        {
            case CharacterBase.Side.Mine:
                _canvas.sortingOrder = 100;
                _hpFillImage.sprite = _mineImage;
                _hpFillImage.pixelsPerUnitMultiplier = 1f;
                _bulletCntSlider.gameObject.SetActive(true);
                _bulletCntText.gameObject.SetActive(true);
                break;
            case CharacterBase.Side.Ally:
                _canvas.sortingOrder = 99;
                _hpFillImage.sprite = _allyImage;
                _hpFillImage.pixelsPerUnitMultiplier = 1f;
                _bulletCntSlider.gameObject.SetActive(true);
                _bulletCntText.gameObject.SetActive(false);
                break;
            case CharacterBase.Side.Enemy:
                _canvas.sortingOrder = 99;
                _hpFillImage.sprite = _enemyImage;
                _hpFillImage.pixelsPerUnitMultiplier = 0.3f;
                _bulletCntSlider.gameObject.SetActive(false);
                _bulletCntText.gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }


    private void Update()
    {
        if (gameManager._isGame)
        {
            if (photonView.IsMine)
            {
                transform.position = _character.transform.position + Vector3.up * _offsetY;
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

    #region Refresh
    public void RefreshHP()
    {
        _hpText.text = _character.hp.ToString();
        _hpSlider.value = (float)_character.hp / _character._maxHp;
    }

    public void RefreshBulletCnt()
    {
        if (_character.bulletCnt < 0)
        {
            _bulletSliderColorIndex = 1;
            _bulletCntText.text = "";
            _bulletCntSlider.value = 1;
        }
        else
        {
            _bulletSliderColorIndex = 0;

            if (_character.bulletCnt < _bulletShowCnt)
            {
                _bulletCntText.text = _character.bulletCnt.ToString();
            }

            _bulletCntSlider.value = (float)_character.bulletCnt / _character._maxBulletCnt;
        }
    }
    #endregion
}
