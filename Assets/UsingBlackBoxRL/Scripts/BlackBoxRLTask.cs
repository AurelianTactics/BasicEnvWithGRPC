using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace AurelianTactics.BlackBoxRL
{
	/// <summary>
	/// Sends and receives information to and from AgentSession
	/// </summary>
	
	//To do:
    //code here for starting a new episode
	//i think design docs say each agentsession has its own task
	//design doc helps grab available avatar or create a new avatar for the agent. for now just having both in AgentSession
	//tasks define and return the reward. i'm not sure how the reward manipulation logic should be done. In the task or in the scene?
	//define done
	//start a new episode
	//LATER control time: many examples of this like speeding up/slowing down perception; starting new episode by letting x number of seconds go etc
	//maybe add a reward clip
	//add functions for getting the reward
		//probably abstract this and then inherit and set up reward functions
	//add function for telling done from the state



	public class BlackBoxRLTask : MonoBehaviour
    {
        public BasicController basicController;

        private float reward;
		private bool done;
		public int pos = 10; //example of env config option being passed from GRPC. I need to flesh this out a bit

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
			Debug.Log("TEST: in bbrltask, starting episode start");
			/*How to start a new episode.
            A Task implements a StartEpisode function to begin a new episode.At the start of an episode,
            the simulation is also typically reset to a state drawn from a stationary distribution*/
			var resetMessage = await basicController.ResetAgent(this.pos);
			Debug.Log("TEST: in bbrltask, starting episode end");
			return resetMessage;
		}

	}

	

	
	
}
