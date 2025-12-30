using FishNet.Object;
using FishNet.Managing;
using UnityEngine;
using UnityEngine.SceneManagement;
using FishNet;

//Based on the official FishNet example;
//https://fish-networking.gitbook.io/docs/guides/features/scene-management/scene-stacking#separating-physics
public class BasicScenePhysicsSync : NetworkBehaviour
{

    private bool _sync3D, _sync2D;
    private bool _hooked;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        if (_hooked) return;
        Scene scene = gameObject.scene;

        _sync3D = (scene.GetPhysicsScene() != Physics.defaultPhysicsScene);
        _sync2D = (scene.GetPhysicsScene2D() != Physics2D.defaultPhysicsScene);
        if (!_sync3D && !_sync2D) return;

        InstanceFinder.TimeManager.OnPrePhysicsSimulation += OnPrePhysics;
        _hooked = true;
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();

        if (!_hooked) return;
        InstanceFinder.TimeManager.OnPrePhysicsSimulation -= OnPrePhysics;
        _hooked = false;
    }

    private void OnPrePhysics(float delta)
    {
        Scene scene = gameObject.scene;
        if (_sync3D)
            scene.GetPhysicsScene().Simulate(delta);
        if (_sync2D)
            scene.GetPhysicsScene2D().Simulate(delta);
    }
}
