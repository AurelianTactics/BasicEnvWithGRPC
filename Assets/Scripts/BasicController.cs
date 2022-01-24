using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using AurelianTactics.BlackBoxRL;
using System.Threading.Tasks;
using System;
using System.Collections;

/// <summary>
/// Simple example from Unity ML Agents: https://github.com/Unity-Technologies/ml-agents
/// Modified slightly to work with BlackBoxRL implementation
/// Controls the env and agent position
/// </summary>

/*
Key differences between base implementation:
	addition of async tasks
	pushing any calls to renderer (ie GameObjects moving) to the main thread with UnityMainThreadDispatcher
*/

/*
to do:
error handling
sending back messages from async Tasks in a standardized way
integrate other standardizations from other parts (tasks, avatars, spects, etc) to this controller
*/

public class BasicController : MonoBehaviour
{
    public float timeBetweenDecisionsAtInference;
    float m_TimeSinceDecision;
    [FormerlySerializedAs("m_Position")]
    [HideInInspector]
    public int position;
    const int k_SmallGoalPosition = 7;
    const int k_LargeGoalPosition = 17;
    public GameObject largeGoal;
    public GameObject smallGoal;
    const int k_MinPosition = 0;
    const int k_MaxPosition = 20;
    public const int k_Extents = k_MaxPosition - k_MinPosition;

    public bool isDone; //adding this to work with BlackBoxRLTask architecture
    public float lastReward; //adding this to work with BlackBoxRLTask architecture

    public void OnEnable()
    {
        position = 10;
        transform.position = new Vector3(position - 10f, 0f, 0f);
        smallGoal.transform.position = new Vector3(k_SmallGoalPosition - 10f, 0f, 0f);
        largeGoal.transform.position = new Vector3(k_LargeGoalPosition - 10f, 0f, 0f);
        isDone = false;
        lastReward = 0.0f;
	}

	/// <summary>
	/// Controls the movement of the GameObject based on the actions received.
	/// </summary>
	/// <param name="direction"></param>
	async Task<string> MoveDirection(int direction)
    {
		//Debug.Log(" top of move Direction, direction and position are " + direction + " " + position);
        position += direction;
        if (position < k_MinPosition) { position = k_MinPosition; }
        if (position > k_MaxPosition) { position = k_MaxPosition; }

		//gameObject.transform.position = new Vector3(position - 10f, 0f, 0f);
		UnityMainThreadDispatcher.Instance().Enqueue(MoveAgent(position));

		lastReward = -0.01f;

        if (position == k_SmallGoalPosition)
        {
            isDone = true;
            lastReward += 0.1f;
        }

        if (position == k_LargeGoalPosition)
        {
            isDone = true;
            lastReward += 1f;
        }

		//Debug.Log("TEST bottom of move Direction, direction and position are " + direction + " " + position);
		//to do: try/catch block and step message
		return "stepMessage";
	}

    public async Task<string> ResetAgent(int pos=10)
    {
		try
		{
			position = pos;

			UnityMainThreadDispatcher.Instance().Enqueue(MoveAgent(position));
			isDone = false;
			lastReward = 0.0f;
			//Debug.Log("ResetAgent worked");
			return "";
		}
		catch(Exception e)
		{
			Debug.Log("ERROR in ResetAgent caught an exception " + e);
			return "error";
		}
    }

	IEnumerator MoveAgent(int pos)
	{
		//Debug.Log("TEST: top of MoveAgent " + position);
		gameObject.transform.position = new Vector3(position - 10f, 0f, 0f);
		//Debug.Log("TEST: bottom of MoveAgent " + position);
		yield break;
	}

    /// <summary>
    /// basicController assumes actions are -1, 0, 1 and moves object accordingly
    /// my actions are 0 (no action (ie 0), 1 (right ie 1), and 2 (left ie -1)
    /// </summary>
    /// <param name="action"></param>
    public async Task<string> ConvertActionToDirectionAndMove(int action)
    {
        if (action == 2)
            action = -1;
        var stepMessage = await MoveDirection(action);
		return stepMessage;
    }


}
