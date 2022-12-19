using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class Storage : MonoBehaviourPun
{
    public Transform _ownerStorage;
    private void Awake()
    {
        gameManager._storageViewIdSet.Add(gameObject.GetPhotonView().ViewID);
        gameManager._storageIDToStorage[gameObject.GetPhotonView().ViewID] = this;
    }

    private void Update()
    {
        if (_ownerStorage)
        {
            transform.position = _ownerStorage.position;
        }
    }
}
