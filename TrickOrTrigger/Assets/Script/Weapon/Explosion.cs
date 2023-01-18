using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class Explosion : MonoBehaviourPunCallbacks
{
    public int _splashDamage = -1;
    public float _radius = 20;


    public void Splash()
    {

        Collider2D[] _targetsInArea = Physics2D.OverlapCircleAll(transform.position,  _radius);
        foreach (var target in _targetsInArea)
        {
            if (target.gameObject.layer == gameManager._playerLayer)
            {
                CharacterBase characterBase = target.transform.GetComponent<CharacterBase>();
                if (characterBase)
                {
                    Debug.Log(target.transform.name + " " + Vector2.SqrMagnitude(target.transform.position - transform.position));
                    characterBase.Damage_Player(_splashDamage, photonView.Owner.ActorNumber);
                }
            }
        }
    }
}
