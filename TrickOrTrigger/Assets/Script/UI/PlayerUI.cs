using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static GameManager;
using static UnityEngine.Rendering.DebugUI;

public class PlayerUI : MonoBehaviourPunCallbacks, IPunObservable
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
            stream.SendNext(_canvas.sortingOrder);
            stream.SendNext(StarCnt);
        }
        else
        {
            _curPos = (Vector3)stream.ReceiveNext();
            bulletSliderColorIndex = (int)stream.ReceiveNext();
            _bulletCntSlider.value = (float)stream.ReceiveNext();
            _canvas.sortingOrder = (int)stream.ReceiveNext();
            StarCnt = (int)stream.ReceiveNext();
        }
    }
    #endregion

    #region Variables
    public Canvas _canvas = null;

    [Header("Star")]
    [SerializeField] private int _starCnt = 0;
    [SerializeField] private GameObject _starIcon;
    [SerializeField] private Image[] _starImages;
    private float _starDistance = 1.2f;

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
    public Text _invincibleText = null;
    public Text _invincibleTimeText = null;

    [Header("Color")]
    public Color[] _bulletSliderColor;
    public Color[] _nicknameColor;
    public int _bulletSliderColorIndex = 0;


    public int bulletSliderColorIndex
    {
        get { return _bulletSliderColorIndex; }
        set
        {
            _bulletSliderColorIndex = value;
            _bulletCntImage.color = _bulletSliderColor[value];
        }
    }

    public int StarCnt
    {
        get { return _starCnt; }
        set 
        {
            if (value <= _starImages.Length && _starCnt != value)
            {
                RefreshStarCnt(_starCnt = value);
            }
        }
    }

    public CharacterBase _characterBase = null;


    #endregion

    private void Awake()
    {
        gameManager._playerUIDic[photonView.OwnerActorNr] = (this);
        _nicknameText.text = photonView.Owner.NickName;
    }

    public void Init(CharacterBase characterBase = null)
    {
        if (characterBase)
        {
            _characterBase = characterBase;
        }
        if (photonView.IsMine)
        {
            _canvas.sortingOrder = 100;
        }
        else
        {
            _canvas.sortingOrder = 99;
        }
        RefreshHP();
        RefreshBulletCnt();
        StarCnt = 2;
        switch (_characterBase._side)
        {
            case CharacterBase.Side.Mine:
                _canvas.sortingOrder = 100;
                _hpFillImage.sprite = _mineImage;
                _hpFillImage.pixelsPerUnitMultiplier = 1f;
                _nicknameText.color = _nicknameColor[0];
                _bulletCntSlider.gameObject.SetActive(true);
                _bulletCntText.gameObject.SetActive(true);
                break;
            case CharacterBase.Side.Ally:
                _canvas.sortingOrder = 99;
                _hpFillImage.sprite = _allyImage;
                _hpFillImage.pixelsPerUnitMultiplier = 1f;
                _nicknameText.color = _nicknameColor[1];
                _bulletCntSlider.gameObject.SetActive(true);
                _bulletCntText.gameObject.SetActive(false);
                break;
            case CharacterBase.Side.Enemy:
                _canvas.sortingOrder = 99;
                _hpFillImage.sprite = _enemyImage;
                _hpFillImage.pixelsPerUnitMultiplier = 0.3f;
                _nicknameText.color = _nicknameColor[2];
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
            if (photonView.IsMine && _characterBase)
            {
                transform.position = _characterBase.transform.position + Vector3.up * _offsetY;
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
        _hpText.text = _characterBase.hp.ToString();
        _hpSlider.value = (float)_characterBase.hp / _characterBase._maxHp;
    }

    public void RefreshBulletCnt()
    {
        if (_characterBase.bulletCnt < 0)
        {
            bulletSliderColorIndex = 1;
            _bulletCntText.text = "";
            _bulletCntSlider.value = 1;
        }
        else
        {
            bulletSliderColorIndex = 0;
            if (_characterBase.bulletCnt < _bulletShowCnt)
            {
                _bulletCntText.text = _characterBase.bulletCnt.ToString();
            }

            _bulletCntSlider.value = (float)_characterBase.bulletCnt / _characterBase._maxBulletCnt;
        }
    }

    private void RefreshStarCnt(int cnt)
    {
        for (int i = 0; i < _starImages.Length; i++)
        {
            if (i < cnt)
            {
                _starImages[i].gameObject.SetActive(true);
            }
            else
            {
                _starImages[i].gameObject.SetActive(false);
            }
        }
        switch (cnt)
        {
            case 2:
                LocateStar(ref _starImages[0], -_starDistance / 2);
                LocateStar(ref _starImages[1], _starDistance / 2);
                break;
            case 3:
                LocateStar(ref _starImages[0], -_starDistance);
                LocateStar(ref _starImages[1], _starDistance);
                LocateStar(ref _starImages[2]);
                break;
            case 4:
                LocateStar(ref _starImages[0], -_starDistance / 2);
                LocateStar(ref _starImages[1], _starDistance / 2);
                LocateStar(ref _starImages[2], -_starDistance / 2, _starDistance);
                LocateStar(ref _starImages[3], _starDistance / 2, _starDistance);
                break;
            case 5:
                LocateStar(ref _starImages[0], -_starDistance);
                LocateStar(ref _starImages[1], _starDistance);
                LocateStar(ref _starImages[2]);
                LocateStar(ref _starImages[3], -_starDistance / 2, _starDistance);
                LocateStar(ref _starImages[4], _starDistance / 2, _starDistance);
                break;
            case 6:
                LocateStar(ref _starImages[0], -_starDistance);
                LocateStar(ref _starImages[1], _starDistance);
                LocateStar(ref _starImages[2]);
                LocateStar(ref _starImages[3], -_starDistance, _starDistance);
                LocateStar(ref _starImages[4], _starDistance, _starDistance);
                LocateStar(ref _starImages[5], 0, _starDistance);
                break;
            case 7:
                LocateStar(ref _starImages[0], -_starDistance, _starDistance / 2);
                LocateStar(ref _starImages[1], _starDistance, _starDistance / 2);
                LocateStar(ref _starImages[2]);
                LocateStar(ref _starImages[3], -_starDistance, _starDistance + _starDistance / 2);
                LocateStar(ref _starImages[4], _starDistance, _starDistance + _starDistance / 2);
                LocateStar(ref _starImages[5], 0, _starDistance);
                LocateStar(ref _starImages[6], 0, _starDistance * 2);
                break;
            default:
                Debug.LogWarning("Max star cnt is 7!!");
                break;
        }
    }

    private void LocateStar(ref Image image, float xOffset = 0, float yOffset = 0)
    {
        image.transform.position = _starIcon.transform.position +  new Vector3(xOffset, yOffset);
    }

    public void StartInvincibleRoutine()
    {
        StartCoroutine(InvincibleRoutine());
    }
    public bool _isRoutine = false;
    IEnumerator InvincibleRoutine()
    {
        _isRoutine = true;
        _starIcon.SetActive(false);
        _invincibleText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        for (int i = 3; i > 0; i--)
        {
            _invincibleTimeText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        _invincibleTimeText.text = "";
        _invincibleText.gameObject.SetActive(false);
        _starIcon.SetActive(true);
        _isRoutine= false;
        yield return null;
    }

    #endregion
}
