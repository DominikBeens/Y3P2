﻿using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

public class GetPickUp : MonoBehaviourPunCallbacks
{

    [SerializeField]
    private float minCooldown;
    [SerializeField]
    private float maxCooldown;
    [SerializeField]
    private PickUp myPickup;
    [SerializeField]
    private GameObject pickUpObject;

    private bool cooldown;

    private void Start()
    {
        SpawnPickUp();
    }

    public IEnumerator Cooldown()
    {
        cooldown = true;
        yield return new WaitForSeconds(Random.Range(minCooldown, maxCooldown));
        cooldown = false;

        SpawnPickUp();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.tag == "Player")
        {
            if (!cooldown)
            {
                other.transform.root.GetComponent<PlayerPickUpManager>().CheckChildren();
                // other.transform.root.GetComponent<PickUpActivater>().ActivatePickUp(myPickup);
                 other.transform.root.GetComponent<PlayerPickUpManager>().SetPickUp(myPickup);
                UIManager.instance.SetPickUpImage(myPickup.PickUpSprite, false);

                StartCoroutine(Cooldown());
                DestroyObject();
            }
        }
    }

    private void DestroyObject()
    {
        if (PhotonNetwork.IsMasterClient && pickUpObject != null)
        {
            PhotonNetwork.Destroy(pickUpObject);
        }

        myPickup = null;
    }

    private void SpawnPickUp()
    {
        // Only the master client handles pickups and syncs it to other clients using OnPhotonSerializeView().
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        int pickupType = Random.Range(0, GameManager.instance.PickUps.Count);
        if (myPickup == null)
        {
            photonView.RPC("SpawnPickupRPC", RpcTarget.All, pickupType);
        }
    }

    [PunRPC]
    private void SpawnPickupRPC(int pickupType)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            pickUpObject = PhotonNetwork.InstantiateSceneObject(GameManager.instance.PickUps[pickupType].itemPrefab.name, transform.position + new Vector3(0, 1, 0), Quaternion.identity);
        }

        myPickup = GameManager.instance.PickUps[pickupType];
    }

    // Receive myPickup data.
    [PunRPC]
    private void SyncMyPickup(int pickupType)
    {
        myPickup = GameManager.instance.PickUps[pickupType];
    }

    // Send myPickup to everyone so that the new player receives the correct data.
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncMyPickup", RpcTarget.Others, (int)myPickup.Type);
        }
    }
}
