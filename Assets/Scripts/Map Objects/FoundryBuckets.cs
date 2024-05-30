using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundryBuckets : MonoBehaviour {
    
    [SerializeField] private GameObject bucketPrefab;

    [SerializeField] private List<GameObject> leftTrussCores, centerTrussCores, rightTrussCores;

    private System.Random random = new System.Random(12345); // seed 12345

    private const float speed = 3f;

    private IEnumerator BucketSpawner(List<GameObject> cores, int index) {
        while(true) {
            if (GameStatsSyncer.instance == null) { yield return null; continue; }
            if ((int)GameStatsSyncer.instance.GetLocalTime() % 10 == 0) {
                for(int i = 0; i < (int)GameStatsSyncer.instance.GetLocalTime() % 5 + 3; i++) {
                    StartCoroutine(MovingBucket(cores, index));
                    yield return new WaitForSeconds(1f);
                }
            }
            yield return null;
        }
    }

    private IEnumerator MovingBucket(List<GameObject> cores, int index) {
        Vector3 offset = new Vector3(0, -.75f, 0);
        GameObject bucket = Instantiate(bucketPrefab, cores[0].transform.position + offset, Quaternion.identity);
        for (int i = 1; i < cores.Count; i++) {

            Vector3 target = cores[i].transform.position + offset;
            Vector3 direction = target - bucket.transform.position;
            Vector3 startingLocation = bucket.transform.position;

            float angle = -Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;

            if (index != 2) bucket.transform.rotation = Quaternion.Euler(new Vector3(-90, 90, angle));
            else bucket.transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));

            float distance = Vector3.Distance(bucket.transform.position, target);
            float time = distance / speed;
            float elapsedTime = 0;
            while (elapsedTime < time) {
                bucket.transform.position = Vector3.Lerp(startingLocation, target, elapsedTime / time);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        Destroy(bucket);
    }

    private void Start() {
        StartCoroutine(BucketSpawner(leftTrussCores, 1));
        StartCoroutine(BucketSpawner(centerTrussCores, 2));
        StartCoroutine(BucketSpawner(rightTrussCores, 3));   
    }

    private void Update() {

    }


}
