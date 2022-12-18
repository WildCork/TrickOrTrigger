using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class Storage : MonoBehaviourPun
{
    private void Awake()
    {
        gameManager._storageViewIdSet.Add(gameObject.GetPhotonView().ViewID);
    }
}
