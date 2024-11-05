using UnityEngine;
using UnityEngine.Events;
using VRT.Orchestrator.Wrapping;
using VRT.Fishnet;
#if VRT_WITH_STATS
using Statistics = Cwipc.Statistics;
#endif

namespace VRT.Pilots.Common
{


	/// <summary>
	/// Component that sends triggers (think: button presses) to other instances of the VR2Gather experience.
	/// NOTE: This class is a backward-compatibility feature for older VR2Gather experiences.
	/// Please use VRTFishnetTrigger for new development.
	/// </summary>
	public class NetworkTrigger : VRTFishnetTrigger
	{
		

		[Tooltip("If true only the master participant can make this trigger happen")]
		public bool MasterOnlyTrigger = false;

		[Tooltip("Event called when either a local or remote trigger happens.")]
		public UnityEvent OnTrigger;


		protected override void Awake() {
			base.Awake();
			if (Events.Count != 0) {
				Debug.LogError($"{Name()}: Events is initialized. Don't do this, use the OnTrigger field, or better: port your code to use VRTFishnetTrigger.");
			}
			Events.Add(OnTrigger);
		}

		/// <summary>
		/// Call this method locally when the user interaction has happened. It will transmit the event to
		/// other participants, and all participants (including the local one) will call the OnTrigger callback.
		/// </summary>
		public void Trigger()
		{
			if (MasterOnlyTrigger && !OrchestratorController.Instance.UserIsMaster)
			{
				return;
			}
			LocalEventTrigger(0);
#if obsolete
			Debug.Log($"NetworkTrigger({name}): Trigger id = {NetworkId}");
			var triggerData = new NetworkTriggerData()
			{
				NetworkBehaviourId = NetworkId,
			};

			if (!OrchestratorController.Instance.UserIsMaster)
			{
				OrchestratorController.Instance.SendTypeEventToMaster(triggerData);
			}
			else
			{
				OnTrigger.Invoke();
#if VRT_WITH_STATS
                Statistics.Output("NetworkTrigger", $"name={name}, sessionId={OrchestratorController.Instance.CurrentSession.sessionId}");
#endif
				OrchestratorController.Instance.SendTypeEventToAll(triggerData);
			}
#endif
		}


#if UNITY_EDITOR
		[ContextMenu("Force Trigger (Editor-only hack)")]
		private void ForceTrigger()
		{
			OnTrigger.Invoke();
		}
#endif
	}
}