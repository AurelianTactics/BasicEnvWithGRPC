using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace AurelianTactics.BlackBoxRL
{
	/// <summary>
	/// Sends and receives information to and from AgentSession
	/// handles done, reward, start episode, and reset epiosode
	/// </summary>
	
	//To do:
	//rething the implementation of this: i think design docs say each agentsession has its own task, i'm not really doing it in that way
	//abstract this and make it mroe dynamic
	//design doc helps grab available avatar or create a new avatar for the agent. for now just having both in AgentSession
	//tasks define and return the reward. i'm not sure how the reward manipulation logic should be done. In the task or in the scene?
	//LATER control time: many examples of this like speeding up/slowing down perception; starting new episode by letting x number of seconds go etc
	//add function for telling done from the state
	//more reward functionality like: reward penalty for choosing non available action, reward clip etc
	//setting keys should be string for name and key should verify the type


	public class BlackBoxRLTask : MonoBehaviour
    {
        public BasicController basicController;
		
        private float reward;
		private bool done;
		public int pos = 10; //example of env config option being passed from GRPC. I need to flesh this out a bit

		Dictionary<string, string> settingKeys;
		
		public BlackBoxRLTask(string config)
		{
			this.reward = 0.0f;
			this.done = false;
			 
		}


		public float GetReward()
		{
            SetReward(basicController.lastReward);
			return this.reward;
		}

		public bool GetDone()
		{
            SetDone(basicController.isDone);
			return this.done;
		}

		public void SetDone(bool d)
		{
			this.done = d;
		}

		public void SetReward(float r)
		{
			this.reward = r;
		}

        public async Task<string> StartEpisode()
        {
			//Debug.Log("TEST: in bbrltask, starting episode start");
			/*How to start a new episode.
            A Task implements a StartEpisode function to begin a new episode.At the start of an episode,
            the simulation is also typically reset to a state drawn from a stationary distribution*/
			var resetMessage = await basicController.ResetAgent(this.pos);
			//Debug.Log("TEST: in bbrltask, starting episode end");
			return resetMessage;
		}

		/// <summary>
		/// Based on settings from a request (unpacked into RequestQueueSettings object),
		/// update an avatar which may change the action and obs specs
		/// </summary>
		/// <param name="rqs"></param>
		/// <param name="ava"></param>
		public void UpdateSettings(RequestQueueSettings rqs, Avatar ava)
		{
			// grab the settingsKeys from the envController

			// to do: make a verification dict of allowable setting keys
			
			// pass the settings to the  envController

			// update the specs in the avatar
			ava.UpdateSpecs();
		}

		public async Task<string> ResetWorld()
		{
			var resetMessage = await basicController.ResetAgent(this.pos);
			//Debug.Log("TEST: in bbrltask, starting episode end");
			return resetMessage;
		}

	}

	

	
	
}
