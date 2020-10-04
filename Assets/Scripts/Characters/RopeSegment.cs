using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class RopeSegment : MonoBehaviour
{
    public float Length;
    public float RopeWidth = 1.0f;
    public Vector2 startPosition;
    public Vector2 endPosition;
    public Vector2 MidPoint;
    public Vector2 DiffDirection;
    public float rotZ;
    LayerMask groundLayerMask = 1 << 8; // ~(1 << 2 | 1 << 8);

    public Rope rope;
    public int segmentNumber;
    public Vector2 attachmentNormalStart = Vector2.zero;
    public Vector2 attachmentNormalEnd = Vector2.zero;
    public bool isAttachmentPointStart = false;
    public bool isAttachmentPointEnd = false;

    private SpriteRenderer sprite;
    private BoxCollider2D collider;

    public bool splitBarrier = false;
    public Ray2D ?lastUnBlockedRayForward = null;
    public Ray2D? lastUnBlockedRayBackward = null;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        collider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        if (!rope.modificationBarrier)
        {
            sprite.transform.position = MidPoint;
            //sprite.transform.localScale = new Vector3(Length, RopeWidth, 1.0f);
            sprite.transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotZ);
            sprite.size = new Vector2(Length, RopeWidth);
            collider.size = new Vector2(Length, RopeWidth);
        }
    }

    private void FixedUpdate()
    {
        CheckForSplit();
    }

    void CheckForSplit()
    {
        if (!splitBarrier && !rope.modificationBarrier)
        {
            if (!(isAttachmentPointStart && isAttachmentPointEnd))
            {
                Vector2 direction = DiffDirection;
                direction.Normalize();

                Ray2D ray = new Ray2D(startPosition, direction);


                RaycastHit2D raycastHit = Physics2D.Raycast(startPosition, direction, Length, groundLayerMask);

                //Debug.DrawRay(startPosition, direction, raycastHit ? Color.white : Color.black, 10000, false);

                if (raycastHit && lastUnBlockedRayForward != null)
                {
                    Ray2D newDirection = SearchForHit(lastUnBlockedRayForward.Value, new Ray2D(startPosition, direction));

                    raycastHit = Physics2D.Raycast(newDirection.origin, newDirection.direction, Length, groundLayerMask);

                    //Debug.DrawRay(startPosition, direction, raycastHit ? Color.blue : Color.yellow, 10000, false);

                    if (raycastHit)
                    {
                        rope.SplitSegment(this, raycastHit.point, raycastHit.normal);

                        //Debug.DrawRay(raycastHit.point, raycastHit.normal, Color.green, 10000, false);
                        lastUnBlockedRayForward = null;

                        return;
                    }
                    else
                        lastUnBlockedRayForward = ray;


                }
                else
                {
                    lastUnBlockedRayForward = ray;
                }

                // Now go the other way...
                direction = -DiffDirection;
                direction.Normalize();

                ray = new Ray2D(endPosition, direction);

                raycastHit = Physics2D.Raycast(endPosition, direction, Length, groundLayerMask);

                //Debug.DrawRay(endPosition, direction, raycastHit ? Color.white : Color.black, 10000, false);

                if (raycastHit && lastUnBlockedRayBackward != null)
                {
                    Ray2D newDirection = SearchForHit(lastUnBlockedRayBackward.Value, new Ray2D(endPosition, direction));

                    raycastHit = Physics2D.Raycast(newDirection.origin, newDirection.direction, Length, groundLayerMask);

                    //Debug.DrawRay(startPosition, direction, raycastHit ? Color.blue : Color.yellow, 10000, false);

                    if (raycastHit)
                    {
                        rope.SplitSegment(this, raycastHit.point, raycastHit.normal);

                        //Debug.DrawRay(raycastHit.point, raycastHit.normal, Color.green, 10000, false);
                        lastUnBlockedRayBackward = null;
                    }
                    else
                        lastUnBlockedRayBackward = ray;


                }
                else
                {
                    lastUnBlockedRayBackward = ray;
                }
            }
        }
    }

    public void SetRopeSegment(Vector2 startPos, Vector2 endPos)
    {
        startPosition = startPos;
        endPosition = endPos;
        DiffDirection = (endPos - startPos);
        MidPoint = startPos + (DiffDirection / 2.0f);
        Length = DiffDirection.magnitude;
        rotZ = Mathf.Atan2(DiffDirection.y, DiffDirection.x) * Mathf.Rad2Deg;
    }

    public Ray2D SearchForHit(Ray2D startRay, Ray2D endRay)
    {
        return SearchForHit(startRay, endRay, 0);
    }

    // Perform a sweep of ray casts to search for the first hit (assuming that an object is moving through space)
    // Assumes that startRay is not a hit and that endRay is. Also assumes a single locally convex hull.
    public Ray2D SearchForHit(Ray2D startRay, Ray2D endRay, int depth)
    {
        if (depth > 16)
            return endRay;

        Vector2 midPoint = Vector2.Lerp(startRay.origin, endRay.origin, 0.5f);
        Vector2 midAngle = Vector2.Lerp(startRay.direction, endRay.direction, 0.5f);

        //Debug.DrawRay(startRay.origin, startRay.direction, Color.gray, 10000, false);
        //Debug.DrawRay(endRay.origin, endRay.direction, Color.yellow, 10000, false);

        //Debug.DrawRay(midPoint, midAngle, Color.white, 10000, false);

        RaycastHit2D raycastHit = Physics2D.Raycast(midPoint, midAngle, Length, groundLayerMask);

        if(raycastHit)
        {
            return SearchForHit(startRay, new Ray2D(midPoint, midAngle), depth + 1);
        } else
        {
            return SearchForHit(new Ray2D(midPoint, midAngle), endRay, depth + 1);
        }


        
    }
}
