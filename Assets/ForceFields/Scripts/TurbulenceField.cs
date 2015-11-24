using UnityEngine;
using System.Collections;

public class TurbulenceField : MonoBehaviour
{
	#region
	enum FieldType
	{
		Box,
		Spherical,
	}

	[SerializeField]
	float _amplitude = 1.0f;

	[SerializeField]
	Vector3 _spatialFreq = Vector3.one;

	[SerializeField]
	Vector3 _effectiveSize = Vector3.one;

	[SerializeField]
	float _effectiveRadius = 1.0f;

	[SerializeField]
	bool _enableGizmos = true;

	[SerializeField]
	bool _visualizeField = false;

	[SerializeField]
	Color _gizmosColor = Color.white;

	#endregion

	// Use this for initialization
	void Start () {
		BoxCollider boxcoll = gameObject.AddComponent<BoxCollider>();
		boxcoll.isTrigger = true;
		boxcoll.size = _effectiveSize;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerStay(Collider coll)
	{
		Rigidbody rb = coll.attachedRigidbody;
		if (rb)
		{
			Debug.Log("Add turbulent force to " + coll.gameObject.name);

			// Generate a turblence field
			Vector3 turblentForce = _amplitude * GetTurbulenceForce(coll.transform.position);
			Debug.Log(turblentForce);
			Debug.DrawLine(
				coll.transform.position, 
				coll.transform.position + Vector3.Normalize(turblentForce),
				Color.black
			);

			rb.AddForce(turblentForce);
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

		if (_enableGizmos)
		{
			Gizmos.color = _gizmosColor;
			Gizmos.DrawWireCube(Vector3.zero, _effectiveSize);
		}

		if (_visualizeField)
		{
			// Visualize field (Stream line)
			const float kPlotStep = 0.01f;	
			Gizmos.matrix = Matrix4x4.identity;
			Color[] axisColors = new Color[] { Color.red, Color.green, Color.blue };
			Vector3[] axisDir = new Vector3[] { Vector3.right, Vector3.up, Vector3.forward };
			for (int axis = 0; axis < 3; ++axis)
			{
				Gizmos.color = axisColors[axis];
				int plotNum = Mathf.CeilToInt(2.0f * _effectiveSize[axis] / kPlotStep);
				Vector3 pos = transform.position - 1.0f * Vector3.Scale(axisDir[axis], _effectiveSize);
				Vector3 nextPos = pos;
				for (int i = 0; i < plotNum; ++i)
				{
					nextPos = pos + kPlotStep * axisDir[axis];
					Vector3 turbulentForce = GetTurbulenceForce(pos);
					turbulentForce[axis] = 0.0f;

					Vector3 turbulentForceNext = GetTurbulenceForce(nextPos);
					turbulentForceNext[axis] = 0.0f;

					Gizmos.DrawLine(
						pos + turbulentForce,
						nextPos + turbulentForceNext
					);
					pos = nextPos;
				}
			}
			
#if false
			// Visualize field (Grid)
			for (int i = 0; i < 10; ++i)
			{
				for (int j = 0; j < 10; ++j)
				{
					for (int k = 0; k < 10; ++k)
					{
						Vector3 point = new Vector3(
							(i - 5) * 0.1f,
							(j - 5) * 0.1f,
							(k - 5) * 0.1f
						);
						point = Vector3.Scale(point, _effectiveSize);
						point += transform.position;

						Vector3 turbulentForce = GetTurbulenceForce(point);
						Vector3 uNorm = 0.5f * (Vector3.Normalize(turbulentForce) + Vector3.one);
						Gizmos.color = new Color(uNorm.x, uNorm.y, uNorm.z);
						Gizmos.DrawLine(point, point + turbulentForce);
					}
				}
			 
			}
#endif
			
		}
	}

	void OnValidate()
	{
		// Clamps the effective size and radius between [0, inf)
		for (int i = 0; i < 3; ++i)
		{
			if (_effectiveSize[i] < 0.0f) _effectiveSize[i] = 0.0f;
		}
		if (_effectiveRadius < 0.0f) _effectiveRadius = 0.0f;

		// Clamps each component of Spatical frequency between [0, inf)
		for (int i = 0; i < 3; ++i)
		{
			if (_spatialFreq[i] < 0.0f) _spatialFreq[i] = 0.0f;
		}
	}

	Vector3 GetTurbulenceForce(Vector3 pos)
	{
		pos = Vector3.Scale(pos, _spatialFreq);
		Vector3 turbulentForce = new Vector3(
			Mathf.PerlinNoise(pos.y + pos.z, Time.time),
			Mathf.PerlinNoise(pos.z + pos.x, Time.time + 0.3f),
			Mathf.PerlinNoise(pos.x + pos.y, Time.time + 0.6f)
		);

		turbulentForce = 2.0f * turbulentForce - Vector3.one;

		return _amplitude * turbulentForce;
	}
}
