using System;
using System.Collections;
using System.Collections.Generic;

namespace AurelianTactics.BlackBoxRL
{
    /// <summary>
    /// Not implemented
    /// Eventually dynamically handle action and obs specs based on the env
    /// at some point might build upon the code based here but idk
    /// </summary>

    /*
    public class ActionSpec
    {



        //dm_env bsuite catch 0 example
        //print(env.action_spec())
        //DiscreteArray(shape= (), dtype= int64, name= action, minimum= 0, maximum= 2, num_values= 3)
        //just doing a default one for now

        //string dtype; //int, float, double
        //bool isDiscrete; //continuous or discrete, not sure if needed
        List<int> actionShape; //list can be dynamic but arrays are set in C# as far as I know. can return an this as an array
        List<Tuple<int, int>> actionMinMax; //min and max for each part of the action shape
        public int numValues; // total number of actions to enter. basically a prod of actionShape

        //initial default
        //index 0:
        //continue action from last turn
        //wait
        //move
        //attack
        //primary
        //secondary

        //index 1: 
        //ability to select
        //if wait: 0 to 3 for N to W going clockwise

        //index 2:
        //x coordinate relative to unit
        //	ie can be negative

        //index 3:
        //y coordinate relative to unit

        //if invalid then wait or do negative reward

        public ActionSpec()
        {
            //this.dtype = "int";
            //this.isDiscrete = true;
            this.numValues = 4;
            this.actionShape = new List<int>(new int[this.numValues]);
            this.actionMinMax = new List<Tuple<int, int>>();
            this.actionMinMax.Add(new Tuple<int, int>(0, 5)); //type of action
            this.actionMinMax.Add(new Tuple<int, int>(0, 100)); //index of action
            this.actionMinMax.Add(new Tuple<int, int>(-100, 100)); //relative x coord
            this.actionMinMax.Add(new Tuple<int, int>(-100, 100)); //relative y coord


        }


    }



    /// <summary>
    /// Idea is to do something similar to gym/dm_env envs where you send back and observation broad details
    /// can then use these details to set up agent value function and what not
    /// however on many complicated envs I have seen the obs are large and jagged
    /// and part of the challenge is turning this huge amount of obs into something workable
    /// I'm leaning towards just sending back a dictionary, and the agent side can decide how to interpret
    /// maybe build in a default way as well but idk
    /// </summary>
    public class ObservationSpec
    {
        //dm_env busuite catch 0 example
        //print(env.observation_spec())
        //BoundedArray(shape= (10, 5), dtype= dtype('float32'), name= 'board', minimum= 0.0, maximum= 1.0)

        string dtype; //int, float, double
        List<int> obsShape; // depending on how I do this, I might need a list of lists with tons of varying size
        float min;
        float max;

        //not sure if i want static obs or some sort of dictionary return thing
        //kind of leaning towards a dictionary and then some code on the other side to treat it
        //idk maybe both?
        //double[,] ServicePoint = new double[10,9];//<-ok (2)
        public ObservationSpec()
        {

        }
    }
    */


    //enums to hold status of action 
    public enum NextActionState
    {
        Waiting, //awaiting next action input from Avatar
        Ready, //action has been inputted, env can now read it and use it
        InProgress, //env is inputing the action and updating the env 
        Finished //action has been finished, can clear the action and set to waiting
    }
    
}
