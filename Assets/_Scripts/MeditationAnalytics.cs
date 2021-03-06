﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public static class MeditationAnalytics
{
	[RuntimeInitializeOnLoadMethod]
	static void Init()
	{
		Application.quitting += OnQuit;
	}


	static void OnQuit()
	{
		currentSession.totalPlayTime = Time.unscaledTime;
	}

	static PlaySessionData currentSession;
	static List<MeditationSessionData> meditationSessions = new List<MeditationSessionData>();

	public static MeditationSessionData CurrentMeditationSession => meditationSessions.Count > 0 ? meditationSessions[meditationSessions.Count - 1] : null;

	public static void AddMeditationSession(MeditationSessionData data)
	{
		if (data != null)
		{
			meditationSessions.Add(data);
		}
	}

	public static void SendCurrentSession()
	{
		if(CurrentMeditationSession != null)
		{
			AnalyticsEvent.Custom("Session", CurrentMeditationSession.ToDictionary());
		}
	}

    public struct PlaySessionData
	{
		public float totalPlayTime;
	}

	public class MeditationSessionData
	{
		public MeditationSessionDataPoint<string> level;
		public MeditationSessionDataPoint<float> selectedDuration;
		public MeditationSessionDataPoint<float> playTime;
		public MeditationSessionDataPoint<bool> quitEarly;
		public MeditationSessionDataPoint<bool> initialMusicEnabled;
		public MeditationSessionDataPoint<bool> initialGuidanceEnabled;
		public MeditationSessionDataPoint<string> hmdId;

		public Dictionary<string, object> ToDictionary()
		{
			return new Dictionary<string, object>
			{
				[nameof(level)] = level.data,
				[nameof(selectedDuration)] = selectedDuration.data,
				[nameof(playTime)] = playTime.data,
				[nameof(quitEarly)] = quitEarly.data,
				[nameof(initialGuidanceEnabled)] = initialGuidanceEnabled.data,
				[nameof(initialMusicEnabled)] = initialMusicEnabled.data,
				[nameof(hmdId)] = hmdId.data,
			};
		}
	}


	public struct MeditationSessionDataPoint<T>
	{
		public T data;
		public float playtime;
		public System.DateTime timeStamp;
		public string tag;

		public MeditationSessionDataPoint(T data, string tag = "")
		{
			this.data = data;
			this.tag = tag;

			this.timeStamp = System.DateTime.UtcNow;
			this.playtime = Time.unscaledTime;
		}	

		public static implicit operator MeditationSessionDataPoint<T>(T data)
		{
			return new MeditationSessionDataPoint<T>(data);
		}
	}
}

