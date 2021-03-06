﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaylistUI : MonoBehaviour
{
	[SerializeField] GameObject playlistEntryPrefab;
	[SerializeField] Transform playlistContainer;
	[SerializeField] Button playButton;
	[SerializeField] Color newEntryColor = Color.gray;

	private List<MeditationQueue.Session> playlist = new List<MeditationQueue.Session>();
	private List<PlaylistUIEntry> uiList = new List<PlaylistUIEntry>();
	private VerticalLayoutGroup layout;

	private void Start()
	{
		layout = GetComponentInChildren<VerticalLayoutGroup>();

		playButton.interactable = false;
		playButton.onClick.AddListener(Play);
	}

	public void SetNewEntryColor(Color color)
	{
		newEntryColor = color;
	}

	public void BeginDrag()
	{
		layout.enabled = false;
	}

	public void EndDrag()
	{
		layout.enabled = true;
	}

	public void SetOrder(int targetIndex, PlaylistUIEntry entry)
	{
		int index = uiList.IndexOf(entry);
		if (index > -1)
		{
			var uiTemp = uiList[index];
			uiList.RemoveAt(index);
			uiList.Insert(targetIndex, uiTemp);

			var pTemp = playlist[index];
			playlist.RemoveAt(index);
			playlist.Insert(targetIndex, pTemp);

			entry.transform.SetSiblingIndex(targetIndex);

			// Update UI
			int min = Mathf.Min(index, targetIndex);
			for (int i = min; i < playlist.Count; i++)
			{
				uiList[i].UpdateSession(playlist[i], i);
			}
		}
	}

	public int GetEntryDesiredIndex(PlaylistUIEntry entry)
	{
		float pos = entry.transform.localPosition.y;
		int index = 0;
		for (int i = 0; i < uiList.Count; i++)
		{
			if(uiList[i] != entry)
			{
				if (uiList[i].transform.localPosition.y > pos)
					index++;
			}
		}
		return index;
	}

	public void SetMusic(bool isOn, PlaylistUIEntry entry)
	{
		int index = uiList.IndexOf(entry);
		if(index > -1)
		{
			var session = playlist[index];
			session.playMusic = isOn;
			playlist[index] = session;

			uiList[index].UpdateSession(session, index);
		}
	}

	public void SetGuidance(bool isOn, PlaylistUIEntry entry)
	{
		int index = uiList.IndexOf(entry);
		if (index > -1)
		{
			var session = playlist[index];
			session.playGuidance = isOn;
			playlist[index] = session;

			uiList[index].UpdateSession(session, index);
		}
	}

	public void SetDuration(int durIndex, PlaylistUIEntry entry)
	{
		int index = uiList.IndexOf(entry);
		if (index > -1)
		{
			var session = playlist[index];
			session.durationIndex = durIndex;
			playlist[index] = session;

			uiList[index].UpdateSession(session, index);
		}
	}

	private void Play()
	{
		if(playlist.Count > 0)
		{
			MeditationQueue.SetQueue(playlist);
			LevelLoader.LoadLevel(playlist[0].level.name);
		}
	}

	public void AddEntry(LevelInfo level)
	{
		var session = new MeditationQueue.Session(level);
		playlist.Add(session);

		GameObject go = Instantiate(playlistEntryPrefab);
		go.transform.SetParent(playlistContainer);
		go.transform.localPosition = Vector3.zero;
		PlaylistUIEntry entry = go.GetComponent<PlaylistUIEntry>();
		entry.playlist = this;
		entry.SetColor(newEntryColor);
		entry.UpdateSession(session, playlist.Count - 1);
		uiList.Add(entry);

		playButton.interactable = true;
	}

	public void RemoveEntry(PlaylistUIEntry entry)
	{
		int index = uiList.IndexOf(entry);
		if (index > -1)
			RemoveEntry(index);
	}

	public void RemoveEntry(int index)
	{
		playlist.RemoveAt(index);

		Destroy(uiList[index].gameObject);
		uiList.RemoveAt(index);

		if (playlist.Count == 0)
			playButton.interactable = false;
	}
}
