using System.Collections;
using UnityEngine;

public class ElevatorMovement : MonoBehaviour
{
    [Header("Points")]
    public Transform pointA;
    public Transform pointB;

    [Header("Mouvement")]
    public float speed = 2f;

    [Header("Décompte")]
    public float countdownTime = 3f;

    private bool isMoving = false;
    private bool countdownStarted = false;
    private bool hasDoneRoundTrip = false;

    private Transform targetPoint;

    private void Start()
    {
        transform.position = pointA.position;
        targetPoint = pointB;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !countdownStarted && !isMoving && !hasDoneRoundTrip)
        {
            StartCoroutine(CountdownAndMove());
        }
    }

    IEnumerator CountdownAndMove()
    {
        countdownStarted = true;

        float timer = countdownTime;
        while (timer > 0)
        {
            Debug.Log("Départ dans : " + timer);
            yield return new WaitForSeconds(1f);
            timer--;
        }

        isMoving = true;
    }

    private void Update()
    {
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.01f)
        {
            transform.position = targetPoint.position;
            isMoving = false;
            countdownStarted = false;

            if (targetPoint == pointB)
            {
                targetPoint = pointA;
                StartCoroutine(CountdownAndMove());
            }
            else
            {
                hasDoneRoundTrip = true;
            }
        }
    }
}
