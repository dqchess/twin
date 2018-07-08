using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtGravityReceiver))]
	public class SgtGravityReceiver_Editor : SgtEditor<SgtGravityReceiver>
	{
		protected override void OnInspector()
		{
			EditorGUILayout.HelpBox("This component applies force to the attached Rigidbody based on nearby SgtGravitySource components.", MessageType.Info);
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component applies force to the attached Rigidbody based on nearby SgtGravitySource components.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(Rigidbody))]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Gravity Receiver")]
	public class SgtGravityReceiver : MonoBehaviour
	{
		[System.NonSerialized]
		private Rigidbody cachedRigidbody;

		[System.NonSerialized]
		private bool cachedRigidbodySet;

		protected virtual void FixedUpdate()
		{
			if (cachedRigidbodySet == false)
			{
				cachedRigidbody    = GetComponent<Rigidbody>();
				cachedRigidbodySet = true;
			}

			var gravitySource = SgtGravitySource.FirstInstance;

			for (var i = 0; i < SgtGravitySource.InstanceCount; i++)
			{
				// Avoid self gravity
				if (gravitySource.transform != transform)
				{
					var totalMass  = cachedRigidbody.mass * gravitySource.Mass;
					var vector     = gravitySource.transform.position - transform.position;
					var distanceSq = vector.sqrMagnitude;

					if (distanceSq > 0.0f)
					{
						var force = totalMass / distanceSq;

						cachedRigidbody.AddForce(vector.normalized * force * Time.fixedDeltaTime, ForceMode.Acceleration);
					}
				}

				gravitySource = gravitySource.NextInstance;
			}
		}
	}
}