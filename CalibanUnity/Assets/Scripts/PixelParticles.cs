using UnityEngine;
using System.Collections;

[RequireComponent (typeof(MeshFilter), typeof(MeshRenderer))]

public class PixelParticles : MonoBehaviour
{
	private Mesh mesh;
	private ParticleSystem system;
	private ParticleSystem.Particle[] particles;

	public enum DrawType
	{
		POINTS,
		LINES,
		LINES_STRIP,
		TRIANGLES
	}

	public DrawType drawType = DrawType.POINTS;

	void Awake ()
	{
	}

	void Start ()
	{
		mesh = new Mesh ();
		GetComponent<MeshFilter> ().mesh = mesh;
	}

	void LateUpdate ()
	{
		InitializeIfNeeded ();

		int amount = system.GetParticles (particles);

		Vector3[] verts = new Vector3[amount];
		Color[] cols = new Color[amount];
		int[] indicies = new int[amount];
		for (int x = 0; x < amount; x++) {
			verts [x] = particles [x].position;
			cols [x] = particles [x].GetCurrentColor (system);
			indicies [x] = x;
		}
		mesh.Clear ();
		mesh.vertices = verts;
		mesh.colors = cols;
		mesh.SetIndices (indicies, GetTopology (), 0);
	}

	void InitializeIfNeeded ()
	{
		if (system == null)
			system = transform.parent.GetComponent<ParticleSystem> ();

		if (particles == null || particles.Length < system.main.maxParticles)
			particles = new ParticleSystem.Particle[system.main.maxParticles]; 
	}

	private MeshTopology GetTopology ()
	{
		switch (drawType) {
		case DrawType.POINTS:
			return MeshTopology.Points;
		case DrawType.LINES:
			return MeshTopology.Lines;
		case DrawType.LINES_STRIP:
			return MeshTopology.LineStrip;
		case DrawType.TRIANGLES:
			return MeshTopology.Triangles;
		default:
			return MeshTopology.Points;
		}
	}
}
