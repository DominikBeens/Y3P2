﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour {

    public float upForce = 10;
    public float forwardForce = 2000;

    private bool launched;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Launch(Rigidbody player)
    {
        player.AddRelativeForce(Vector3.forward * forwardForce, ForceMode.Impulse);
        player.AddRelativeForce(Vector3.up * upForce, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.root.tag == "Player" && !launched)
        {
            Photon.Pun.PhotonView pv = other.transform.root.GetComponent<Photon.Pun.PhotonView>();
            if (pv)
            {
                if (pv.IsMine)
                {
                    Launch(other.transform.root.GetComponent<Rigidbody>());
                    StartCoroutine(Wait());
                }

                if (audioSource)
                {
                    audioSource.Play();
                }
            }
        }
    }

    private IEnumerator Wait()
    {
        launched = true;
        yield return new WaitForSeconds(.1F);
        launched = false;
    }
}
