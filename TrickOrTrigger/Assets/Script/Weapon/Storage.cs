using Photon.Pun;
using UnityEngine;
using static GameManager;

public class Storage : MonoBehaviourPun
{
    public Transform _ownerStorage;
    private void Awake()
    {
        gameManager._storageDic[photonView.OwnerActorNr] =(this);
    }
}
