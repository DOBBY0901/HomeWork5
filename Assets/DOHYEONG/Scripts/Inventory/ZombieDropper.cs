using UnityEngine;

public class ZombieDropper : MonoBehaviour
{
    [Header("Drop Item")]
    [SerializeField] private GameObject dropItemPrefab;

    [Header("Drop Chance")]
    [Range(0f, 1f)]
    [SerializeField] private float dropChance = 1f;

    [Header("Drop Position")]
    [SerializeField] private Vector3 dropOffset = Vector3.zero;

    public void Drop()
    {
        if (dropItemPrefab == null)
        {
            Debug.LogWarning($"{gameObject.name}에 Drop Item Prefab이 연결되지 않았습니다.");
            return;
        }

        float randomValue = Random.value;

        if (randomValue > dropChance)
            return;

        Vector3 spawnPosition = transform.position + dropOffset;

        Instantiate(dropItemPrefab, spawnPosition, Quaternion.identity);
    }
}