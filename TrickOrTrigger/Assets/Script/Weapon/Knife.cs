using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using static GameManager;
using static CharacterBase;

public class Knife : MonoBehaviourPunCallbacks
{
    public const WeaponType _weaponType = WeaponType.Knife;
    public CharacterBase _owner = null;
    public HashSet<CharacterBase> _targetsInArea = new();
    [SerializeField] private int _damage = 30;


    private CharacterBase _characterBase = null;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_owner.photonView.IsMine)
        {
            return;
        }
        if (_characterBase = collision.GetComponent<CharacterBase>())
        {
            if (_characterBase._side == Side.Enemy)
            {
                _targetsInArea.Add(_characterBase);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!_owner.photonView.IsMine)
        {
            return;
        }
        if (_characterBase = collision.GetComponent<CharacterBase>())
        {
            if (_targetsInArea.Contains(_characterBase))
            {
                _targetsInArea.Remove(_characterBase);
            }
        }
    }

    public void Stab(bool isOnGround)
    {
        _owner.currentWeaponType = WeaponType.Knife;
        _owner.currentAttackDelay = ReturnDelayTime(isOnGround);
        foreach (var target in _targetsInArea)
        {
            target.Damage_Player(_damage, photonView.Owner.ActorNumber);
        }
    }

    private float ReturnDelayTime(bool isOnGround)
    {
        if (isOnGround)
        {
            return ReturnTime(AnimState.Stab);
        }
        else
        {
            return ReturnTime(AnimState.Jump_slash);
        }
    }
    private float ReturnTime(AnimState animState)
    {
        return _spineTimeDict[_weaponType][animState];
    }
}
