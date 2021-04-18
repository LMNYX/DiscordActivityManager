using Discord;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace act8
{
	#region Discord.Activity Class Extension
	public static class ActivityExtension
	{
		public static Activity AddState(this Activity activity, string value)
		{
			activity.State = value;
			return activity;
		}

		public static Activity AddDetails(this Activity activity, string value)
		{
			activity.Details = value;
			return activity;
		}

		public static Activity AddTimestamps(this Activity activity, ActivityTimestamps value)
		{
			activity.Timestamps = value;
			return activity;
		}

		public static Activity AddAssets(this Activity activity, ActivityAssets value)
		{
			activity.Assets = value;
			return activity;
		}
	}
	#endregion

	#region Discord.ActivityTimestamps Class Extension
	public static class ActivityTimestampsExtension
	{
		public static ActivityTimestamps AddStart(this ActivityTimestamps timestamps, long start)
		{
			timestamps.Start = start;
			return timestamps;
		}

		public static ActivityTimestamps AddEnd(this ActivityTimestamps timestamps, long end)
		{
			timestamps.End = end;
			return timestamps;
		}
	}
	#endregion

	#region Discord.ActivityAssets Class Extension
	public static class ActivityAssetsExtension
	{
		public static ActivityAssets AddLargeImage(this ActivityAssets assets, string largeImage)
		{
			assets.LargeImage = largeImage;
			return assets;
		}

		public static ActivityAssets AddLargeText(this ActivityAssets assets, string largeText)
		{
			assets.LargeText = largeText;
			return assets;
		}

		public static ActivityAssets AddSmallImage(this ActivityAssets assets, string smallImage)
		{
			assets.SmallImage = smallImage;
			return assets;
		}

		public static ActivityAssets AddSmallText(this ActivityAssets assets, string smallText)
		{
			assets.SmallText = smallText;
			return assets;
		}
	}
	#endregion

	#region Discord Activity Manager
	public class DiscordActivityManager : MonoBehaviour
	{
		#region Scene References

		/// <summary>
		/// Activities that are being set when appropriate Scene is loaded, if SceneActivity is enabled.
		/// </summary>
		public static readonly Dictionary<string, Activity> SceneActivityReferences = new Dictionary<string, Activity>()
		{
			// { "SceneName", new Activity().AddState("Activity State") }
		};

		#endregion

		/// <summary>
		/// DiscordActivityManager singleton.
		/// </summary>
		public static DiscordActivityManager singleton;

		/// <summary>
		/// Auto change Discord Activity based on loaded Scene (DiscordActivityManager.SceneActivityReferences).
		/// </summary>
		public static bool SceneActivity = true;

		private static readonly long CLIENT_ID = 0;

		private Discord.Discord DiscordInstance;
		private ActivityManager ActivityManager;
		private Activity CurrentActivity;

		private void Awake()
		{
			if (singleton)
			{
				Debug.LogWarning("DiscordActivityManager already exist on scene, deleting.");
				Destroy(gameObject);
			}
			else
			{
				singleton = this;
			}

			SetupDiscord();

			SceneManager.sceneLoaded += SceneLoadedCallback;
		}

		/// <summary>
		/// Try to setup Discord.
		/// </summary>
		public void SetupDiscord()
		{
			try
			{
				DiscordInstance = new Discord.Discord(CLIENT_ID, (ulong)CreateFlags.NoRequireDiscord);
				ActivityManager = DiscordInstance.GetActivityManager();
			}
			catch (Exception exception)
			{
				Debug.LogWarning("Failed to setup Discord: " + exception.Message);
			}
		}

		/// <summary>
		/// Get current Discord Activity.
		/// </summary>
		/// <returns>Current Activity</returns>
		public Activity GetDiscordActivity() => CurrentActivity;

		/// <summary>
		/// Set current Discord Activity.
		/// </summary>
		/// <param name="activity">Activity to set</param>
		public void SetDiscordActivity(Activity activity)
		{
			if (DiscordInstance == null) return;

			ActivityManager.UpdateActivity(activity, (result) =>
			{
				if (result == Result.Ok) CurrentActivity = activity;
				else Debug.LogWarning("Discord was unable to set activity: " + result.ToString());
			}
			);
		}

		private void Update() => RunCallbacks();
		private void OnDestroy() => Shutdown();

		private void SceneLoadedCallback(Scene scene, LoadSceneMode mode)
		{
			if (!SceneActivity) return;

			if (SceneActivityReferences.ContainsKey(scene.name))
				SetDiscordActivity(SceneActivityReferences[scene.name]);
			else
				SetDiscordActivity(new Activity().AddState("Idle"));
		}

		private void RunCallbacks()
		{
			if (DiscordInstance == null) return;
			DiscordInstance.RunCallbacks();
		}

		private void Shutdown()
		{
			if (DiscordInstance == null) return;
			ActivityManager.ClearActivity((result) => { });
			DiscordInstance.Dispose();
		}
	}
	#endregion
}