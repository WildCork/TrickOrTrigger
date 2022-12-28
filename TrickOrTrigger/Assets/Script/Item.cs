using Photon.Pun.Demo.PunBasics;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Weapon;

public class Item : ObjectBase
{
    public enum ItemType { Bullet, Health, Grenade }
    public enum SizeType { Small, Large }

    public ItemType _itemType = ItemType.Bullet;
    public SizeType _sizeType = SizeType.Small;

    public WeaponType _bulletKind = WeaponType.Pistol;
    public bool _isHit = false;
    [SerializeField] private const float c_smallPenaltyRate = 0.8f;
    [SerializeField] private const float c_largePenaltyRate = 0.6f;

    private void Start()
    {
        _isHit = false;
    }

    public void Reload(CharacterBase character)
    {
        int bulletCnt = 0;
        switch (_sizeType)
        {
            case SizeType.Small:
                switch (_bulletKind)
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
                switch (_bulletKind)
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
        if (character.currentWeaponType == _bulletKind)
        {
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
            character.bulletCnt += bulletCnt;
        }
        else
        {
            character.currentWeaponType = _bulletKind;
            character.bulletCnt = bulletCnt;
        }
    }

    public void Heal(CharacterBase characterbase)
    {
        switch (_sizeType)
        {
            case SizeType.Small:
                characterbase.RefreshHP_Player(characterbase.hp + 20);
                break;
            case SizeType.Large:
                characterbase.RefreshHP_Player(characterbase.hp + 50);
                break;
            default:
                break;
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
