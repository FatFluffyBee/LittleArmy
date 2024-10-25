using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathBallDisplay : MonoBehaviour
{
    public GameObject ballPathPrefab;
    private List<BallPath> ballsInPath = new List<BallPath>();
    public float distBetweenBalls;
    private Color color;

    public void Initialize(Color unitColor){
        color = unitColor;
    }

     public void DisplayUnitPathWithBalls(List<Vector3> path){ //Add sphere to display the path of a unit
        List<Vector3> ballsPos = ConvertPathIntoSpherePos(path);

        for(int i = 0; i < ballsPos.Count; i++) { 
            if(i < ballsInPath.Count-1) { //if ball exist for path just move it and show it
                ballsInPath[i].SetVisibility(true);
                ballsInPath[i].transform.position = ballsPos[i];
            }
            else { //else create a new ball and initialise it
                BallPath ball = Instantiate(ballPathPrefab, ballsPos[i], Quaternion.identity).GetComponent<BallPath>();
                ball.transform.parent = transform;
                ball.Initialize(color);
                ballsInPath.Add(ball);
            }
        }

        if(ballsPos.Count < ballsInPath.Count) //hide all the remaining balls 
            for(int i = ballsPos.Count - 1; i < ballsInPath.Count; i++)
                ballsInPath[i].SetVisibility(false);
    }
    public List<Vector3> ConvertPathIntoSpherePos(List<Vector3> path){
        path.Reverse(); //we reverse so we can start at the end and add more easily final ball exactly at end path

        List<Vector3> ballsPos = new List<Vector3>() {path[0]}; //final ball
        Vector3 currentPos = path[0];
        int count = 0;
        float distanceTillNextBall = distBetweenBalls;

        for(int i = 0; i < path.Count-1; i++){
            Vector3 dir = (path[i+1] - path[i]).normalized;
            while(count < 100){
                if(Vector3.Distance(path[i+1], currentPos) > distanceTillNextBall) {
                    currentPos += dir * distanceTillNextBall;
                    ballsPos.Add(currentPos);
                    distanceTillNextBall = distBetweenBalls;
                }
                else {
                    distanceTillNextBall -= Vector3.Distance(path[i+1], currentPos);
                    currentPos += dir * Vector3.Distance(path[i+1], currentPos);
                    break;
                }
                count++;
            }
        }
        return ballsPos;
    }
    public void EraseAllPathBalls() {
        foreach(BallPath ball in ballsInPath)
            Destroy(ball.gameObject);
        ballsInPath.Clear();
    }
    
}
