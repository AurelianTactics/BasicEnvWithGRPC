using System;
using UnityEngine;

namespace AurelianTactics.BlackBoxRL
{
	/// <summary>
	/// Manages the RequestQueue with process requests
	/// At some point should handle the global time for the env between all connected agents
	/// </summary>

/*
	to do 
	implement the time parts of WTM
	I'm not cleanly implementing WTM, SessionManager, RequestQueue, and SessionFactory like the paper outlines
	make it global like the paper
	make it manage time like the paper
	synchronicity
	I'm unclear on how I want to handle requests ie do I want to spin this and check next requests or have some sort of push from process requests?
*/
	public class WorldTimeManager
	{
		public RequestQueue requestQueue;

		public WorldTimeManager()
		{
			this.requestQueue = new RequestQueue();
		}

		public RequestQueueObject ProcessRequests()
		{
			if( this.requestQueue.IsRequestInQueue())
			{
				var rqo = this.requestQueue.GetNextRequest();
				return rqo;
			}

			return null;
		}
	}
}
