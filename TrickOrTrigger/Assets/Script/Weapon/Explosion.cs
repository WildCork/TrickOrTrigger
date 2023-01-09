using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class Explosion : MonoBehaviourPunCallbacks
{
    public int _splashDamage = -1;

    HashSet<CharacterBase> _targetsInArea = new();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == gameManager._playerLayer)
        {
            CharacterBase characterBase = collision.gameObject.GetComponent<CharacterBase>();
            _targetsInArea.Add(characterBase);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == gameManager._playerLayer)
        {
            CharacterBase characterBase = collision.gameObject.GetComponent<CharacterBase>();
            _targetsInArea.Remove(characterBase);
        }
    }

    public void Splash()
    {
        foreach (var target in _targetsInArea)
        {
            target.Damage_Player(_splashDamage, photonView.Owner.ActorNumber);
        }
    }
}
