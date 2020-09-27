﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Oculus.Platform;
using Oculus.Platform.Models;

[RequireComponent(typeof(Button))]
public class LevelSelectButton : MonoBehaviour
{
	const float OwnershipCheckInterval = 5;

	[SerializeField] LoadLevelButton targetButton;
	[SerializeField] string level;
	[SerializeField] string productId;
	[SerializeField] Sprite noAccessSprite;
	[SerializeField] Material skyboxMat;
	[SerializeField] VideoClip meditationClip;

	Image image;
	Button button;
	Sprite normalSpite;
	bool canAccess = false;
	SessionSettingsUI session;

	void Awake()
	{

		if (targetButton == null)
			targetButton = FindObjectOfType<LoadLevelButton>();

		session = FindObjectOfType<SessionSettingsUI>();
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
		if (productId == "" || LoginManager.currentUser.IsPremiumUser)
			ProductOwned();
		else
			StartCoroutine(WaitCheckOwnership());
#endif
	}


	IEnumerator WaitCheckOwnership()
	{
		while(enabled)
		{
			IAP.GetViewerPurchases().OnComplete(OnFetchedPurchases);
			yield return new WaitForSeconds(OwnershipCheckInterval);
		}
	}


	void OnFetchedPurchases(Message<PurchaseList> msg)
	{
		if (msg.IsError)
			Debug.LogError("Failed to fetch dlc purchase: " + msg.GetError().Message);
		else
		{
			foreach (var purchase in msg.GetPurchaseList())
			{
				if(purchase.Sku == productId)
				{
					ProductOwned();
					break;
				}
			}
		}
	}


	void ProductOwned()
	{
		button.interactable = true;
		image.sprite = normalSpite;
		canAccess = true;
		StopAllCoroutines();
	}


	void ClickedButton()
	{
		if (targetButton != null && session != null)
		{
			if (canAccess)
			{
				BuyDLC_UI.targetDlcSKU = "";

				targetButton.SetTargetLevel(level);
				session.Open(skyboxMat, meditationClip, true);
			}
			else
			{
				BuyDLC_UI.targetDlcSKU = productId;
				targetButton.SetTargetLevel(null);
				session.Open(skyboxMat, meditationClip, false);
			}
		}
	}

    void Reset()
	{
		level = name;
	}
}
