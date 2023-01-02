using Photon.Pun;
using UnityEngine;
using static GameManager;

public class Item : ObjectBase
{
    public enum ItemType { Bullet, Health, Grenade }
    public enum SizeType { Small, Large }

    public ItemType _itemType = ItemType.Bullet;
    public SizeType _sizeType = SizeType.Small;

    public WeaponType _weaponType = WeaponType.Pistol;
    public bool _isHit = false;
    [SerializeField] private const float c_smallPenaltyRate = 0.8f;
    [SerializeField] private const float c_largePenaltyRate = 0.6f;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        _isHit = false;
    }

    public int ReloadAmount(CharacterBase characterBase)
    {
        _isHit = true;
        int bulletCnt = 0;
        switch (_sizeType)
        {
            case SizeType.Small:
                switch (_weaponType)
                {
                    case WeaponType.Machinegun:
                        bulletCnt = 100;
                        break;
                    case WeaponType.Shotgun:
                        bulletCnt = 20;
                        break;
                    default:
                        bulletCnt = 0;
                        break;
                }
                break;
            case SizeType.Large:
                switch (_weaponType)
                {
                    case WeaponType.Machinegun:
                        bulletCnt = 200;
                        break;
                    case WeaponType.Shotgun:
                        bulletCnt = 50;
                        break;
                    default:
                        bulletCnt = 0;
                        break;
                }
                bulletCnt = (int)(bulletCnt * c_largePenaltyRate);
                break;
            default:
                break;
        }
        switch (_sizeType)
        {
            case SizeType.Small:
                bulletCnt = (int)(bulletCnt * c_smallPenaltyRate);
                break;
            case SizeType.Large:
                bulletCnt = (int)(bulletCnt * c_largePenaltyRate);
                break;
            default:
                break;
        }
        if (characterBase.currentWeaponType != _weaponType)
        {
            return characterBase.bulletCnt + bulletCnt;
        }
        else
        {
            return bulletCnt;
        }
    }

    public int HealAmount(CharacterBase characterBase)
    {
        _isHit = true;
        switch (_sizeType)
        {
            case SizeType.Small:
                return characterBase.hp + 20;
            case SizeType.Large:
                return characterBase.hp + 50;
            default:
                return 0;
        }
    }


    public void GoBackStorage_RPC()
    {
        //Debug.Log("GoBackStorage_RPC");
        photonView.RPC(nameof(GoBackStorage), RpcTarget.AllBufferedViaServer);
    }

    [PunRPC]
    public void GoBackStorage()
    {
        StopAllCoroutines();
    }
}
