﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Oculus.Platform;
using Oculus.Platform.Models;
#if STEAM_STORE
using Steamworks;
#endif

[RequireComponent(typeof(Button))]
public class LevelSelectButton : MonoBehaviour
{
	const float OwnershipCheckInterval = 5;

	[SerializeField] LoadLevelButton targetButton;
	[SerializeField] LevelInfo level;
	[SerializeField] DLCInfo targetDLC;
	[SerializeField] Sprite noAccessSprite;
	[SerializeField] Material skyboxMat;
	[SerializeField] VideoClip meditationClip;
	[SerializeField] PlaylistUI playList;
	[SerializeField] SessionSettingsUI session;

	Image image;
	Button button;
	Sprite normalSpite;
	bool canAccess = false;

	void Awake()
	{
		button = GetComponent<Button>();

		image = GetComponent<Image>();
		normalSpite = image.sprite;
		if(noAccessSprite != null)
			image.sprite = noAccessSprite;

		button.onClick.AddListener(ClickedButton);
	}


	void OnEnable()
	{
#if FLOWVR_UNLOCK_ALL_DLC
		ProductOwned();

#else
		//Not dlc, unlock right away
		if (targetDLC == null || targetDLC.alwaysUnlocked || LoginManager.currentUser.IsPremiumUser)
			ProductOwned();
		else
			StartCoroutine(WaitCheckOwnership());
#endif
	}


	IEnumerator WaitCheckOwnership()
	{
		while(enabled)
		{
#if OCULUS_STORE
			IAP.GetViewerPurchases().OnComplete(OnFetchedPurchases);
#elif STEAM_STORE
			OnFetchedDLC(SteamApps.DlcInformation());
#endif
			yield return new WaitForSeconds(OwnershipCheckInterval);
		}
	}

#if OCULUS_STORE
	void OnFetchedPurchases(Message<PurchaseList> msg)
	{
		if (msg.IsError)
			Debug.LogError("Failed to fetch dlc purchase: " + msg.GetError().Message);
		else
		{
			foreach (var purchase in msg.GetPurchaseList())
			{
				if(purchase.Sku == targetDLC.sku)
				{
					ProductOwned();
					break;
				}
			}
		}
	}
#endif

#if STEAM_STORE

	void OnFetchedDLC(IEnumerable<Steamworks.Data.DlcInformation> dlcs)
	{
		foreach (var dlc in dlcs)
		{
			if(dlc.AppId == targetDLC.steamAppid)
			{
				if (SteamApps.IsDlcInstalled(dlc.AppId))
					ProductOwned();
				break;
			}
		}
	}

#endif

	void ProductOwned()
	{
		button.interactable = true;
		image.sprite = normalSpite;
		canAccess = true;
		StopAllCoroutines();
	}


	void ClickedButton()
	{
		if (canAccess)
		{
			BuyDLC_UI.targetDlcSKU = "";

			if(targetButton != null)
				targetButton.SetTargetLevel(level);
			if (session != null)
				session.Open(skyboxMat, meditationClip, true);
			else if (playList != null)
				playList.AddEntry(level);
		}
		else
		{
			BuyDLC_UI.targetDlcSKU = targetDLC.sku;
			if (targetButton != null)
				targetButton.SetTargetLevel(null);
			if (session != null)
				session.Open(skyboxMat, meditationClip, false);
			if (playList != null) // Click meditation in playlist panel
				LevelLoader.LoadLevel("BuyDLC");
		}
	}

    void Reset()
	{
		level = LevelInfo.Get(name);
	}
}
