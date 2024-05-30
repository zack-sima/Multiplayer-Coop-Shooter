using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// <summary>
/// Starts a single player client on the scene in the absence of a server linker
/// </summary>

public class TestingServerLinker : MonoBehaviour {
	public static TestingServerLinker instance;

	FusionBootstrap bootstrap = null;

	private bool TryFindBootstrap() {
		bootstrap = null;
		bootstrap = FindObjectOfType<FusionBootstrap>();
		return bootstrap != null;
	}
	private void Awake() {
		if (instance != null) {
			Destroy(instance.gameObject);
		}
		instance = this;
	}
	void Start() {
		if (ServerLinker.instance != null) {
			return;
		}

		if (!TryFindBootstrap()) return;

		// Start the game in single-player mode
		bootstrap.StartSinglePlayer();
	}
}
