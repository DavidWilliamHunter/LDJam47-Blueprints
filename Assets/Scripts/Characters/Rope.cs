using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Rope : MonoBehaviour
{
    public CharacterController2D[] players;
    protected Rigidbody2D[] playerRBs;
    public int ControllingPlayer = 0;

    public float RopeWidth = 0.2f;
    public float RopeMinForceLength = 1.0f;
    public float RopeForceMultiplier = 10.0f;

    public Transform RopeSegmentPrefab;

    List<RopeSegment> ropeSegments;

    public bool modificationBarrier = false;

    public float MaxRope = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
        playerRBs = new Rigidbody2D[players.Length];
        for(int i=0; i<players.Length; ++i)
        {
            playerRBs[i] = players[i].GetComponent<Rigidbody2D>();
        }

        SetupRope();

    }

    // Update is called once per frame
    void Update()
    {
        if (!modificationBarrier)
        {
            modificationBarrier = true;
            if (ropeSegments.Count == 1)
            {
                ropeSegments[0].SetRopeSegment(players[0].transform.position, players[1].transform.position);
            }
            else
            {
                ropeSegments[0].SetRopeSegment(players[0].transform.position, ropeSegments[0].endPosition);
                ropeSegments[ropeSegments.Count - 1].SetRopeSegment(ropeSegments[ropeSegments.Count - 1].startPosition, players[1].transform.position);
            }
            modificationBarrier = false;
        }
    }

    private void FixedUpdate()
    {
        if (!modificationBarrier)
        {
            modificationBarrier = true;
            Vector2 diff = (players[1].transform.position - players[0].transform.position);


            if (ropeSegments.Count == 1)
            {
                players[0].SetRopeForce(CalculateRopeForce(players[0].transform.position, players[1].transform.position));
                players[1].SetRopeForce(CalculateRopeForce(players[1].transform.position, players[0].transform.position));

                    players[0].MaxRope = CalculateRopeLength() > MaxRope;
                    players[1].MaxRope = CalculateRopeLength() > MaxRope;

            }
            else
            {
                players[0].SetRopeForce(CalculateRopeForce(players[0].transform.position, ropeSegments[0].endPosition));
                players[1].SetRopeForce(CalculateRopeForce(ropeSegments[ropeSegments.Count - 1].startPosition, players[1].transform.position));
            }

            // now check the end rope that the players are attached to, to make sure the rope should still be attached.

            // start with player 0

            
            if (ropeSegments.Count > 1) // only needed if we have more than one segment
            {
                if(!CheckForReleaseAngle(0, 1, true))
                    CheckForReleaseAngle(ropeSegments.Count-2, ropeSegments.Count - 1, false);
                //if(!CheckForReleaseAngle(true))
                //    CheckForReleaseAngle(false);        // we need to do check here because we can only release one segment at a time
                /*
                // we want to check if the player attached segment is either parallel with or beyond parallel with the next segment
                RopeSegment currentSegment = ropeSegments[0];
                RopeSegment nextSegment = ropeSegments[1];
                // calculate attachment normals;
                Vector2 segmentDirection = nextSegment.endPosition - nextSegment.startPosition;
                segmentDirection.Normalize();

                Vector2 orthoDirection = segmentDirection;
                float temp = orthoDirection.x;
                orthoDirection.x = -orthoDirection.y;
                orthoDirection.y = temp;
                Vector2 attachmentNormal = (Vector2.Dot(orthoDirection, nextSegment.attachmentNormalEnd) >= 0) ? orthoDirection : -orthoDirection;

                Vector2 thisSegmentDirection = currentSegment.endPosition - currentSegment.startPosition;
                if (Vector2.Dot(thisSegmentDirection, attachmentNormal) > 0)
                {
                    //Debug.Log(thisSegmentDirection);
                    //Debug.Log(attachmentNormal);
                    //Debug.Log(Vector2.Dot(thisSegmentDirection, attachmentNormal));
                    //Debug.DrawRay(nextSegment.startPosition, nextSegment.attachmentNormalStart, Color.green, 10000, false);
                    //Debug.DrawRay(nextSegment.startPosition, attachmentNormal, Color.red, 10000, false);
                    //Debug.DrawRay(nextSegment.startPosition, thisSegmentDirection, Color.black, 10000, false);
                    Vector2 SorthoDirection = segmentDirection;
                    temp = SorthoDirection.x;
                    SorthoDirection.x = -SorthoDirection.y;
                    SorthoDirection.y = temp;
                    //Debug.DrawRay(nextSegment.startPosition, SorthoDirection, Color.blue, 10000, false);
                    //Debug.Log("Can Remove Segment");
                    //RemoveSegment(1);
                }
                */
            } 
            modificationBarrier = false;
        }
    }

    float CalculateRopeLength()
    {
        float result = 0.0f;
        foreach(var segment in ropeSegments)
        {
            result += segment.Length;
        }
        return result;
    }

    Vector2 CalculateRopeForce(Vector2 startPos, Vector2 endPos)
    {
        Vector2 diff = (endPos - startPos);
        float m = CalculateRopeLength();
        if (m> RopeMinForceLength)
        {
            m -= RopeMinForceLength;
            diff.Normalize();
            diff *= m;
        } else
        {
            diff = Vector2.zero;
        }
        return diff;
    }


    bool CheckForReleaseAngle(int first, int second, bool direction)
    {
        if (ropeSegments.Count > 1) // only needed if we have more than one segment
        {
            // we want to check if the player attached segment is either parallel with or beyond parallel with the next segment
            RopeSegment currentSegment = ropeSegments[first];
            RopeSegment nextSegment = ropeSegments[second];
            // calculate attachment normals;
            Vector2 segmentDirection;
            if (direction)
            {
                segmentDirection = nextSegment.endPosition - nextSegment.startPosition;
            } else
            {
                segmentDirection = currentSegment.startPosition - currentSegment.endPosition;
            }
            
            segmentDirection.Normalize();

            Vector2 orthoDirection = segmentDirection;
            float temp = orthoDirection.x;
            orthoDirection.x = -orthoDirection.y;
            orthoDirection.y = temp;
            Vector2 attachmentNormal;
            if(direction)
                attachmentNormal = (Vector2.Dot(orthoDirection, nextSegment.attachmentNormalStart) >= 0) ? orthoDirection : -orthoDirection;
            else
                attachmentNormal = (Vector2.Dot(orthoDirection, currentSegment.attachmentNormalEnd) >= 0) ? orthoDirection : -orthoDirection;

            Vector2 thisSegmentDirection;
            if (direction)
                thisSegmentDirection = currentSegment.endPosition - currentSegment.startPosition;
            else 
                thisSegmentDirection = nextSegment.startPosition - nextSegment.endPosition;

            segmentDirection.Normalize();
            thisSegmentDirection.Normalize();

            //Debug.DrawRay(nextSegment.startPosition, nextSegment.attachmentNormalStart, Color.green, 10000, false);
            //Debug.DrawRay(nextSegment.startPosition, attachmentNormal, Color.red, 10000, false);
            //Debug.DrawRay(nextSegment.startPosition, thisSegmentDirection, Color.black, 10000, false);

            if(Vector2.Dot(segmentDirection, thisSegmentDirection) < -0.75f)  // if the segment are almost exact opposite directions.
            {
                Debug.Log("Segments are in opposite directions");
                RemoveSegment(direction ? second : first, direction);
                return true;
            }

            if (Vector2.Dot(thisSegmentDirection, attachmentNormal) < 0)
            {
                //Debug.Log(thisSegmentDirection);
                //Debug.Log(attachmentNormal);
                //Debug.Log(Vector2.Dot(thisSegmentDirection, attachmentNormal));
                if (direction)
                {
                    //Debug.DrawRay(nextSegment.startPosition, nextSegment.attachmentNormalStart, Color.green, 10000, false);
                    //Debug.DrawRay(nextSegment.startPosition, attachmentNormal, Color.red, 10000, false);
                    //Debug.DrawRay(nextSegment.startPosition, thisSegmentDirection, Color.black, 10000, false);
                    Vector2 SorthoDirection = segmentDirection;
                    temp = SorthoDirection.x;
                    SorthoDirection.x = -SorthoDirection.y;
                    SorthoDirection.y = temp;
                    ////Debug.DrawRay(nextSegment.startPosition, orthoDirection, Color.blue, 10000, false);
                }
                else
                {
                    //Debug.DrawRay(currentSegment.endPosition, currentSegment.attachmentNormalEnd, Color.green, 10000, false);
                    //Debug.DrawRay(currentSegment.endPosition, attachmentNormal, Color.red, 10000, false);
                    //Debug.DrawRay(currentSegment.endPosition, thisSegmentDirection, Color.black, 10000, false);
                    Vector2 SorthoDirection = segmentDirection;
                    temp = SorthoDirection.x;
                    SorthoDirection.x = -SorthoDirection.y;
                    SorthoDirection.y = temp;
                    //Debug.DrawRay(nextSegment.endPosition, orthoDirection, Color.blue, 10000, false);
                }
                Debug.Log("Can Remove Segment");
                RemoveSegment(direction? second : first, direction);
                return true;
            }
        }
        return false;

    }


    /*
     * Rope Segment Operations
     */

    protected void SetupRope()
    {
        DestroyRope(); // Get rid of the old rope

        ropeSegments = new List<RopeSegment>();

        RopeSegment segment = CreateRopeSegment();

        InsertSegmentAt(segment,0);    
    }

    protected void DestroyRope()
    {
        if (ropeSegments != null)
        {
            foreach (var segment in ropeSegments)
                Destroy(segment);   // Mark old segments for destruction.

            ropeSegments.Clear();
        }
    }

    protected RopeSegment CreateRopeSegment()
    {
        Transform go = Instantiate(RopeSegmentPrefab, transform);

        RopeSegment newSegment = go.GetComponent<RopeSegment>();

        Assert.IsNotNull(newSegment);

        newSegment.rope = this;

        return newSegment;
    }

    protected void InsertSegmentAt(RopeSegment segment, int location)
    {
        ropeSegments.Insert(location, segment);
        for(int i=0; i<ropeSegments.Count; ++i)
        {
            ropeSegments[i].segmentNumber = i; // renumber all the segments so that they match the list.
        }
    }

    protected void RemoveSegmentAt(int location)
    {
        RopeSegment ropeSegment = ropeSegments[location];
        ropeSegments.RemoveAt(location);
        for (int i = 0; i < ropeSegments.Count; ++i)
        {
            ropeSegments[i].segmentNumber = i; // renumber all the segments so that they match the list.
        }
        Destroy(ropeSegment.gameObject);

    }

    protected void RemoveSegment(RopeSegment segment)
    {
        Destroy(segment.gameObject);
        ropeSegments.Remove(segment);
        for (int i = 0; i < ropeSegments.Count; ++i)
        {
            ropeSegments[i].segmentNumber = i; // renumber all the segments so that they match the list.
        }
    }

    public void SplitSegment(RopeSegment toSplit, Vector2 pivotPoint, Vector2 pivotPointNormal)
    {
        // get distance from pivot point to start and end positions;
        float distToStart = (pivotPoint - toSplit.startPosition).magnitude;
        float distToEnd = (pivotPoint - toSplit.endPosition).magnitude;

        if (distToStart < 0.5f || distToEnd < 0.5f) // don't split if it will create tiny segments.
            return;

        if (ropeSegments.Count > 32)
            return;

        RopeSegment newSegment = CreateRopeSegment();

        newSegment.splitBarrier = true;
        toSplit.splitBarrier = true;

        Vector2 oldEndPosition = toSplit.endPosition;

        toSplit.SetRopeSegment(toSplit.startPosition, pivotPoint);
        toSplit.attachmentNormalEnd = pivotPointNormal;
        toSplit.isAttachmentPointEnd = true;
        newSegment.SetRopeSegment(pivotPoint, oldEndPosition);
        newSegment.attachmentNormalStart = pivotPointNormal;
        newSegment.isAttachmentPointStart = true;
        InsertSegmentAt(newSegment, toSplit.segmentNumber+1);

        newSegment.splitBarrier = false;
        toSplit.splitBarrier = false;
    }

    void RemoveSegment(int segmentNumber, bool direction)
    {
        if (segmentNumber >= 0 && segmentNumber < ropeSegments.Count)
        {
            RopeSegment thisSegment = ropeSegments[segmentNumber];
            thisSegment.splitBarrier = true;

            if (direction)
            {
                // start with prevSegment
                if (segmentNumber > 0)
                {
                    RopeSegment prevSegment = ropeSegments[segmentNumber - 1];

                    prevSegment.splitBarrier = true;

                    prevSegment.SetRopeSegment(prevSegment.startPosition, thisSegment.endPosition);
                    prevSegment.isAttachmentPointEnd = thisSegment.isAttachmentPointEnd;
                    prevSegment.attachmentNormalEnd = thisSegment.attachmentNormalEnd;

                    prevSegment.splitBarrier = false;
                }
            }
            else
            {
                // pair with next
                // start with prevSegment
                Assert.IsTrue(segmentNumber < ropeSegments.Count - 1);
                if (segmentNumber < ropeSegments.Count-1)
                {
                    RopeSegment nextSegment = ropeSegments[segmentNumber + 1];

                    nextSegment.splitBarrier = true;

                    nextSegment.SetRopeSegment(thisSegment.startPosition, nextSegment.endPosition);
                    nextSegment.isAttachmentPointStart = thisSegment.isAttachmentPointStart;
                    nextSegment.attachmentNormalStart = thisSegment.attachmentNormalStart;

                    nextSegment.splitBarrier = false;
                }
            }

            /*if (segmentNumber < ropeSegments.Count-1)
            {
                RopeSegment nextSegment = ropeSegments[segmentNumber + 1];

                nextSegment.splitBarrier = true;

                nextSegment.SetRopeSegment(thisSegment.startPosition, nextSegment.endPosition);
                nextSegment.isAttachmentPointStart = thisSegment.isAttachmentPointStart;
                nextSegment.attachmentNormalStart = thisSegment.attachmentNormalStart;

                nextSegment.splitBarrier = false;
            }*/
            RemoveSegmentAt(segmentNumber);
            
        }
    }

}
