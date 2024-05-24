using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class ModelBuilds : MonoBehaviour
{
    // Start is called before the first frame update
    public NNModel modelAsset;
    private Model m_RuntimeModel;

    void Start()
    {
        m_RuntimeModel = ModelLoader.Load(modelAsset);

    }

    // Update is called once per frame
    void Update()
    {
        var worker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpBurst, m_RuntimeModel);
        Tensor input = new Tensor(16, 64, 64, 4);
        worker.Execute(input);
        var output = worker.PeekOutput();
        print($"BOT5: {output}");
        input.Dispose();
        worker.Dispose();
    }
}
