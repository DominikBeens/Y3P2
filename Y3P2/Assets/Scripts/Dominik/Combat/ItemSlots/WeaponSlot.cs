﻿using Photon.Pun;
using System;
using UnityEngine;

public class WeaponSlot : EquipmentSlot
{

    public static Weapon currentWeapon;
    public static bool canAttack = true;

    private float nextAttackTime;

    public static event Action OnFireWeapon = delegate { };
    public static event Action<Weapon> OnEquipWeapon = delegate { };

    [SerializeField] private Transform weaponSpawn;
    [SerializeField] private Weapon startingWeapon;

    public override void Initialise(bool local)
    {
        base.Initialise(local);

        if (local)
        {
            EquipWeapon(startingWeapon);
        }
    }

    private void Update()
    {
        if (currentWeapon != null && equipedItem != null)
        {
            HandleFiring();
        }
    }

    private void HandleFiring()
    {
        if (Input.GetMouseButton(0))
        {
            if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + currentWeapon.fireRate;
                OnFireWeapon();
            }
        }
    }

    public void EquipWeapon(Weapon weapon)
    {
        int[] ids = Equip(weapon, weaponSpawn);
        currentWeapon = currentEquipment as Weapon;
        if (currentWeapon != null)
        {
            ParentEquipment(ids[0], ids[1]);
        }

        OnEquipWeapon(weapon);
    }

    protected override void ParentEquipment(int equipmentID, int parentID)
    {
        photonView.RPC("ParentWeapon", RpcTarget.All, equipmentID, parentID);
    }

    [PunRPC]
    private void ParentWeapon(int equipmentID, int parentID)
    {
        PhotonView pv = PhotonNetwork.GetPhotonView(equipmentID);
        if (pv)
        {
            pv.transform.SetParent(PhotonNetwork.GetPhotonView(parentID).transform);
            pv.transform.localPosition = Vector3.zero;
            pv.transform.localRotation = Quaternion.identity;
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        int[] ids = GetEquipedItemIDs(weaponSpawn);
        ParentEquipment(ids[0], ids[1]);
    }
}