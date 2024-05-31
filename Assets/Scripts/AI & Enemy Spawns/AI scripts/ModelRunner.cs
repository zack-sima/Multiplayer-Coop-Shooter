using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using UnityEngine.Rendering;

public class ModelRunner : MonoBehaviour
{
    // Start is called before the first frame update
    public NNModel modelAsset;
    private Model model;
    public List<GameObject> agents = new List<GameObject>();
    protected IEnumerator DecisionTick()
    {
        int batchSize = agents.Count;
        var worker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpBurst, model);
        var inputs = new Dictionary<string, Tensor>();
        inputs["image"] = new Tensor(batchSize, 128, 128, 3);
        inputs["recurrence"] = new Tensor(batchSize, 512);
        worker.Execute(inputs);

        worker.Dispose();
        foreach (var key in inputs.Keys)
        {
            inputs[key].Dispose();
        }
        inputs.Clear();


        yield return new WaitForSeconds(0.1f);
    }
    void Start()
    {
        model = ModelLoader.Load(modelAsset);
        StartCoroutine(DecisionTick());
    }

    // Update is called once per frame
    void Update()
    {

    }
}
