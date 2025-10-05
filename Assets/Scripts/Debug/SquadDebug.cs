using UnityEngine;
using UnityEngine.AI;
using System.Collections;

// ONLY USED TO DEBUG AND TEST SQUAD RELATED STUFF!!!!!!!!!!!!!!!!!

[RequireComponent(typeof(Squad))]
public class SquadDebug : MonoBehaviour
{
    [Header("Test Settings")]
    public float initialMoveDelay = 3.0f;

    [Tooltip("The center of the area where the squad can move randomly.")]
    public Vector3 testAreaCenter = Vector3.zero;

    [Tooltip("The size (width, height, depth) of the random movement area.")]
    public Vector3 testAreaSize = new Vector3(50, 0, 50);

    private Squad squad;

    void Start()
    {
        squad = GetComponent<Squad>();
        StartCoroutine(MovementTestRoutine());
    }

    private IEnumerator MovementTestRoutine()
    {
        yield return new WaitForSeconds(initialMoveDelay);

        while (true)
        {
            MoveToRandomDestination();

            yield return new WaitForSeconds(Random.Range(5f, 10f));
        }
    }

    private void MoveToRandomDestination()
    {
        float randomX = Random.Range(-testAreaSize.x / 2, testAreaSize.x / 2);
        float randomZ = Random.Range(-testAreaSize.z / 2, testAreaSize.z / 2);
        Vector3 randomPoint = testAreaCenter + new Vector3(randomX, 0, randomZ);

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            Debug.Log($"Squad '{gameObject.name}' moving to new random position: {hit.position}");
            squad.MoveTo(hit.position);
        }
        else
        {
            Debug.LogWarning($"Could not find a valid NavMesh point near {randomPoint} for squad '{gameObject.name}'.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.3f); 
        Gizmos.DrawCube(testAreaCenter, testAreaSize);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(testAreaCenter, testAreaSize);
    }
}