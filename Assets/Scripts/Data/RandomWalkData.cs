using UnityEngine;


[CreateAssetMenu(fileName= "RandomWalkParameters_" ,menuName="PCG/RandomWalkData")]
public class RandomWalkData : ScriptableObject 
{
    public int iterations = 10;
    public int walkLength = 10;
    public bool startRandomly = true; // each iteration will start at a random point
}
