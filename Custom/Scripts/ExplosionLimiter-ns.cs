using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace ExtraltodeusPlugin {
	public class ExplosionLimiterPlugin : MVRScript {

		void speedLimiter(){
			foreach (ForceReceiver fr in containingAtom.forceReceivers){
				Rigidbody rb = fr.GetComponent<Rigidbody>();
				if (rb != null) {
					Vector3 newVelocity = rb.velocity;
					for (int i = 0; i < 2; i++) {
						if (Mathf.Abs(rb.velocity[i]) > 7f)
							newVelocity[i] = 0;
						rb.velocity = newVelocity;
					}
				}
			}
		}

		void FixedUpdate() {
			try {
				speedLimiter();
			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}
	}
}
