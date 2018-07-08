using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtTerrainFace))]
	public class SgtTerrainFace_Editor : SgtEditor<SgtTerrainFace>
	{
		protected override void OnInspector()
		{
			BeginDisabled();
				DrawDefault("Terrain");
				DrawDefault("Parent");
				DrawDefault("Side");
				DrawDefault("Depth");
				DrawDefault("Bounds");

				Separator();

				DrawDefault("NeighbourL");
				DrawDefault("NeighbourR");
				DrawDefault("NeighbourB");
				DrawDefault("NeighbourT");

				Separator();

				DrawDefault("CornerBL");
				DrawDefault("CornerBR");
				DrawDefault("CornerTL");
				DrawDefault("CornerTR");

				Separator();

				DrawDefault("Split");
				DrawDefault("ChildBL");
				DrawDefault("ChildBR");
				DrawDefault("ChildTL");
				DrawDefault("ChildTR");

				Separator();

				DrawDefault("CoordBL");
				DrawDefault("CoordBR");
				DrawDefault("CoordTL");
				DrawDefault("CoordTR");
			EndDisabled();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class SgtTerrainFace : MonoBehaviour
	{
		public enum StateType
		{
			Invalid,
			Building,
			Visible
		}

		[System.NonSerialized]
		public StateType State;

		public SgtTerrain     Terrain;
		public SgtTerrainFace Parent;
		public CubemapFace    Side;
		public int            Depth;
		public SgtBoundsD     Bounds;

		public bool           Split;
		public SgtTerrainFace ChildBL;
		public SgtTerrainFace ChildBR;
		public SgtTerrainFace ChildTL;
		public SgtTerrainFace ChildTR;

		public SgtTerrainFace GetChild(int index)
		{
			if (index == 0) return ChildBL;
			if (index == 1) return ChildBR;
			if (index == 2) return ChildTL;
			return ChildTR;
		}

		public SgtVector3D CornerBL;
		public SgtVector3D CornerBR;
		public SgtVector3D CornerTL;
		public SgtVector3D CornerTR;

		public SgtVector2D CoordBL;
		public SgtVector2D CoordBR;
		public SgtVector2D CoordTL;
		public SgtVector2D CoordTR;

		public SgtTerrainNeighbour NeighbourL;
		public SgtTerrainNeighbour NeighbourR;
		public SgtTerrainNeighbour NeighbourB;
		public SgtTerrainNeighbour NeighbourT;

		public void SetNeighbour(int index, SgtTerrainFace face)
		{
			if (index == 0) NeighbourL.Face = face;
			else if (index == 1) NeighbourR.Face = face;
			else if (index == 2) NeighbourB.Face = face;
			else NeighbourT.Face = face;
		}

		public SgtVector3D[] positions;
		public Vector3[] vertices;
		public Vector3[] normals1;
		public Vector3[] normals2;
		public Vector4[] tangents1;
		public Vector4[] tangents2;
		public Vector2[] coords1;
		public Vector2[] coords2;
		public Color[]   colors;
		public Mesh      mesh;

		[System.NonSerialized]
		private MeshFilter cachedMeshFilter;

		[System.NonSerialized]
		private MeshRenderer cachedMeshRenderer;

		[System.NonSerialized]
		private MeshCollider cachedMeshCollider;

		// Used to assign renderer.sharedMaterials
		private static Material[] materials0 = new Material[0];
		private static Material[] materials1 = new Material[1];
		private static Material[] materials2 = new Material[2];

		[ContextMenu("Update Renderers")]
		public void UpdateRenderers()
		{
			UpdateRenderer();

			if (Split == true)
			{
				ChildBL.UpdateRenderers();
				ChildBR.UpdateRenderers();
				ChildTL.UpdateRenderers();
				ChildTR.UpdateRenderers();
			}
		}

		[ContextMenu("Update Renderer")]
		public void UpdateRenderer()
		{
			if (cachedMeshRenderer == null) cachedMeshRenderer = GetComponent<MeshRenderer>();

			var material = Terrain.Material;

			if (Terrain.OnCalculateMaterial != null)
			{
				Terrain.OnCalculateMaterial(this, ref material);
			}

			if (SgtHelper.Enabled(Terrain.SharedMaterial) == true)
			{
				materials2[0] = material;
				materials2[1] = Terrain.SharedMaterial.Material;

				cachedMeshRenderer.sharedMaterials = materials2;
			}
			else if (material != null)
			{
				materials1[0] = material;

				cachedMeshRenderer.sharedMaterials = materials1;
			}
			else
			{
				cachedMeshRenderer.sharedMaterials = materials0;
			}
		}

		[ContextMenu("Update Colliders")]
		public void UpdateColliders()
		{
			UpdateCollider();

			if (Split == true)
			{
				ChildBL.UpdateColliders();
				ChildBR.UpdateColliders();
				ChildTL.UpdateColliders();
				ChildTR.UpdateColliders();
			}
		}

		[ContextMenu("Update Collider")]
		public void UpdateCollider()
		{
			var shouldExist = false;

			if (Depth < Terrain.MaxColliderDepth)
			{
				if (Split == false || (Terrain.MaxColliderDepth - 1) == Depth)
				{
					shouldExist = true;
				}
			}

			if (cachedMeshCollider == null) cachedMeshCollider = GetComponent<MeshCollider>();

			if (shouldExist == true)
			{
				if (cachedMeshCollider == null) cachedMeshCollider = gameObject.AddComponent<MeshCollider>();

				cachedMeshCollider.enabled = false;

				if (cachedMeshCollider.sharedMesh == mesh)
				{
					cachedMeshCollider.sharedMesh = null;
				}

				cachedMeshCollider.sharedMesh = mesh;

				cachedMeshCollider.enabled = true;
			}
			else if (cachedMeshCollider != null)
			{
				cachedMeshCollider.enabled = false;
			}
		}

		public bool WriteVertex(int x, int y)
		{
			if (Depth == 0)
			{
				return true;
			}

			if (y % 2 == 1)
			{
				return true;
			}

			return x % 2 == 1;
		}

		private void ValidateVertices()
		{
			var verts = (2 << Terrain.Subdivisions) + 1;
			var size  = verts * verts;

			if (vertices == null || vertices.Length != size)
			{
				positions = new SgtVector3D[size];
				vertices  = new Vector3[size];
				normals1  = new Vector3[size];
				normals2  = new Vector3[size];
				tangents1 = new Vector4[size];
				tangents2 = new Vector4[size];
				coords1   = new Vector2[size];
				coords2   = new Vector2[size];
				colors    = new Color[size];
			}
		}

		public void BuildData()
		{
			var verts   = (2 << Terrain.Subdivisions) + 1;
			var step    = 1.0 / (verts - 1);
			var cornerU = (CornerBR - CornerBL) * step;
			var cornerV = (CornerTL - CornerBL) * step;
			var cornerS = SgtVector3D.Cross(cornerU, cornerV).normalized * cornerU.magnitude;
			var coordU  = (CoordBR - CoordBL) * step;
			var coordV  = (CoordTL - CoordBL) * step;

			ValidateVertices();

			Bounds.Clear();

			for (var y = 0; y < verts; y++)
			{
				var cubeY = CornerBL + cornerV * y;

				for (var x = 0; x < verts; x++)
				{
					var cube     = cubeY + cornerU * x;
					var normal   = cube.normalized;
					var height   = Terrain.GetLocalHeight(normal);
					var position = normal * height;

					Bounds.Add(position);

					//if (WriteVertex(x, y) == true)
					{
						var index   = x + y * verts;
						var coord1  = CoordBL + coordU * x + coordV * y;
						var coord2  = new SgtVector2D(x * step, y * step);
						var tangent = default(SgtVector3D);
						var color   = Color.white;

						tangent = SgtVector3D.Cross(normal, cornerU).normalized;

						if (Terrain.OnCalculateColor != null)
						{
							Terrain.OnCalculateColor(position, height, ref color);
						}

						positions[index] = position;
						vertices[index]  = (Vector3)position;
						coords1[index]   = (Vector2)coord1;
						coords2[index]   = (Vector2)coord2;
						colors[index]    = color;
					}
				}
			}

			if (Terrain.Normals == SgtTerrain.NormalsType.Normalized)
			{
				for (var y = 0; y < verts; y++)
				{
					var cubeY = CornerBL + cornerV * y;

					for (var x = 0; x < verts; x++)
					{
						var index   = x + y * verts;
						var cubeA   = cubeY + cornerU * x;
						var cubeB   = GetPosition(cubeA, cornerU, cornerS);
						var normal  = cubeA.normalized;
						var tangent = (Vector3)(cubeB.normalized - normal).normalized;

						normals1[index] = (Vector3)normal;
						tangents1[index] = new Vector4(tangent.x, tangent.y, tangent.z, -1.0f);
					}
				}
			}

			if (Terrain.Normals == SgtTerrain.NormalsType.Hierarchical)
			{
				for (var y = 0; y < verts; y++)
				{
					var cubeY = CornerBL + cornerV * y;

					for (var x = 0; x < verts; x++)
					{
						var index = x + y * verts;
						var cube  = cubeY + cornerU * x;

						// Get position from vertex or recalc off edge
						var positionL = x == 0         ? GetPosition(cube, -cornerU, cornerS) : positions[index - 1    ];
						var positionR = x == verts - 1 ? GetPosition(cube,  cornerU, cornerS) : positions[index + 1    ];
						var positionB = y == 0         ? GetPosition(cube, -cornerV, cornerS) : positions[index - verts];
						var positionT = y == verts - 1 ? GetPosition(cube,  cornerV, cornerS) : positions[index + verts];

						// Calc normal and tangent
						var positionH = positionR - positionL;
						var positionV = positionT - positionB;
						var normal    = SgtVector3D.Cross(positionV, positionH).normalized;
						var tangent   = (Vector3)positionH.normalized;

						normals1[index] = normals2[index] = (Vector3)normal;
						tangents1[index] = tangents2[index] = new Vector4(tangent.x, tangent.y, tangent.z, -1.0f);
					}
				}
			}

			if (Terrain.OnPostProcessVertices != null)
			{
				Terrain.OnPostProcessVertices(this);
			}

			if (Split == true)
			{
				CopyVertiesToChildren();
			}
		}

		private SgtVector3D GetPosition(SgtVector3D origin, SgtVector3D sideDelta, SgtVector3D edgeDelta)
		{
			var eps      = 0.00000001;
			var position = origin + sideDelta;

			// Wrap position around cube if it goes off the edge
			if (position.x < -1.0 - eps || position.x > 1.0 + eps || position.y < -1.0 - eps || position.y > 1.0 + eps || position.z < -1.0 - eps || position.z > 1.0 + eps)
			{
				position = origin + edgeDelta;
			}

			position = position.normalized * Terrain.GetLocalHeight(position);

			return position;
		}

		private void CopyVertiesToChildren()
		{
			var mid   = (1 << Terrain.Subdivisions);
			var half  = (1 << Terrain.Subdivisions) + 1;
			var verts = (2 << Terrain.Subdivisions) + 1;

			ChildBL.ValidateVertices();
			ChildBR.ValidateVertices();
			ChildTL.ValidateVertices();
			ChildTR.ValidateVertices();

			CopyVertiesToChildren(ChildBL, verts,   0,  half,   0,  half,   0,   0);
			CopyVertiesToChildren(ChildBR, verts, mid, verts,   0,  half, mid,   0);
			CopyVertiesToChildren(ChildTL, verts,   0,  half, mid, verts,   0, mid);
			CopyVertiesToChildren(ChildTR, verts, mid, verts, mid, verts, mid, mid);
		}

		private void CopyVertiesToChildren(SgtTerrainFace child, int verts, int minX, int maxX, int minY, int maxY, int offX, int offY)
		{
			for (var y = minY; y < maxY; y++)
			{
				for (var x = minX; x < maxX; x++)
				{
					var x2 = (x - offX) * 2;
					var y2 = (y - offY) * 2;
					var src = x + y * verts;
					var dst = x2 + y2 * verts;

					child.vertices[dst] = vertices[src];
					child.coords1[dst] = coords1[src];
					child.coords2[dst] = coords2[src];
					//child.normals[dst] = normals[src];
					//child.tangents[dst] = tangents[src];
				}
			}
		}

		public void BuildVertices()
		{
			var mesh = GetMesh();

			mesh.Clear(false);
			mesh.vertices = vertices;
			mesh.normals  = normals1;
			mesh.tangents = tangents1;
			mesh.uv       = coords1;
			mesh.uv2      = coords2;

			if (Terrain.Tangents == true)
			{
				mesh.tangents = tangents1;
			}

			if (Terrain.CenterBounds == true)
			{
				mesh.bounds = new Bounds(Vector3.zero, (Vector3)(Bounds.Extents * 2.0));
			}
			else
			{
				mesh.bounds = (Bounds)Bounds;
			}
		}

		private static List<int> buildingIndices = new List<int>();

		[ContextMenu("Build Indices")]
		public void BuildIndices()
		{
			var level  = SgtTerrainLevel.Levels[Terrain.Subdivisions];
			var indexL = level.GetIndex(Depth - NeighbourL.Face.Depth);
			var indexR = level.GetIndex(Depth - NeighbourR.Face.Depth);
			var indexB = level.GetIndex(Depth - NeighbourB.Face.Depth);
			var indexT = level.GetIndex(Depth - NeighbourT.Face.Depth);

			buildingIndices.Clear();

			buildingIndices.AddRange(level.Indices);
			buildingIndices.AddRange(level.IndicesL[indexL]);
			buildingIndices.AddRange(level.IndicesR[indexR]);
			buildingIndices.AddRange(level.IndicesB[indexB]);
			buildingIndices.AddRange(level.IndicesT[indexT]);

			//link to parent

			mesh.SetTriangles(buildingIndices, 0);

			UpdateCollider();
		}

		public static SgtTerrainFace Create(string name, int layer, Transform parent)
		{
			return SgtComponentPool<SgtTerrainFace>.Pop(parent, name, layer);
		}

		public static SgtTerrainFace Pool(SgtTerrainFace face)
		{
			if (face != null)
			{
				face.State   = StateType.Invalid;
				face.Terrain = null;

				if (face.Split == true)
				{
					face.Split   = false;
					face.ChildBL = Pool(face.ChildBL);
					face.ChildBR = Pool(face.ChildBR);
					face.ChildTL = Pool(face.ChildTL);
					face.ChildTR = Pool(face.ChildTR);
				}

				SgtComponentPool<SgtTerrainFace>.Add(face);
			}

			return null;
		}

		public static SgtTerrainFace MarkForDestruction(SgtTerrainFace face)
		{
			if (face != null)
			{
				face.Terrain = null;

				face.gameObject.SetActive(true);
			}

			return null;
		}

		public void UpdateStates()
		{
			switch (State)
			{
				case StateType.Invalid:
				{
					UpdateInvalid();

					if (Split == true)
					{
						UpdateInvalidChildren();
					}
				}
				break;

				case StateType.Visible:
				{
					var distance = GetDistance();

					// Split?
					if (distance < Terrain.LocalDistances[Depth])
					{
						if (Split == false && CanSplit == true)
						{
							SplitNow();
						}
					}
					// Unsplit?
					else
					{
						if (Split == true && CanUnsplit == true)
						{
							UnsplitNow();
						}
					}

					if (Split == true)
					{
						ChildBL.UpdateStates();
						ChildBR.UpdateStates();
						ChildTL.UpdateStates();
						ChildTR.UpdateStates();
					}
				}
				break;
			}
		}

		private bool CanSplit
		{
			get
			{
				return NeighbourL.Face.Depth >= Depth && NeighbourR.Face.Depth >= Depth && NeighbourB.Face.Depth >= Depth && NeighbourT.Face.Depth >= Depth;
			}
		}

		private bool CanUnsplit
		{
			get
			{
				if (NeighbourL.Face.Split == true) if (ChildBL.NeighbourL.Face.Split == true || ChildTL.NeighbourL.Face.Split == true) return false;
				if (NeighbourR.Face.Split == true) if (ChildBR.NeighbourR.Face.Split == true || ChildTR.NeighbourR.Face.Split == true) return false;
				if (NeighbourB.Face.Split == true) if (ChildBL.NeighbourB.Face.Split == true || ChildBR.NeighbourB.Face.Split == true) return false;
				if (NeighbourT.Face.Split == true) if (ChildTL.NeighbourT.Face.Split == true || ChildTR.NeighbourT.Face.Split == true) return false;

				return true;
			}
		}

		protected virtual void Update()
		{
			if (Terrain == null)
			{
				Pool(this);
			}
		}

		/*
		protected virtual void OnDrawGizmosSelected()
		{
			for (var i = positions.Length - 1; i >= 0; i--)
			{
				var position = positions[i];
				var normal   = normals[i];
				var tangent  = tangents[i];

				Gizmos.DrawRay(position, normal * 0.05f);
				//Gizmos.DrawRay(position, tangent * 0.01f);
			}
		}
		*/
		private double GetDistance()
		{
			var bestDistance = double.PositiveInfinity;

			for (var i = Terrain.LocalPoints.Count - 1; i >= 0; i--)
			{
				var dist = (Bounds.Center - Terrain.LocalPoints[i]).sqrMagnitude;

				if (dist < bestDistance)
				{
					bestDistance = dist;
				}
			}

			return bestDistance;
		}

		public void UpdateInvalid()
		{
			BuildData();
			BuildVertices();
			BuildIndices();
			UpdateRenderer();
			UpdateCollider();

			State = StateType.Visible;

			if (Split == true)
			{
				UpdateInvalidChildren();
			}

			if (Terrain.OnSpawnFace != null)
			{
				Terrain.OnSpawnFace.Invoke(this);
			}
		}

		public override int GetHashCode()
		{
			return Side.GetHashCode() ^ CoordBL.x.GetHashCode() ^ (CoordBL.y + 1.0).GetHashCode();
		}

		private void SplitNow()
		{
			var cornerLL = (CornerBL + CornerTL) * 0.5f;
			var cornerRR = (CornerBR + CornerTR) * 0.5f;
			var cornerTT = (CornerTL + CornerTR) * 0.5f;
			var cornerBB = (CornerBL + CornerBR) * 0.5f;
			var cornerMM = (CornerBL + CornerTR) * 0.5f;
			var coordLL = (CoordBL + CoordTL) * 0.5f;
			var coordRR = (CoordBR + CoordTR) * 0.5f;
			var coordTT = (CoordTL + CoordTR) * 0.5f;
			var coordBB = (CoordBL + CoordBR) * 0.5f;
			var coordMM = (CoordBL + CoordTR) * 0.5f;

			Split   = true;
			ChildBL = Create("ChildBL", CornerBL, cornerBB, cornerLL, cornerMM, CoordBL, coordBB, coordLL, coordMM);
			ChildBR = Create("ChildBR", cornerBB, CornerBR, cornerMM, cornerRR, coordBB, CoordBR, coordMM, coordRR);
			ChildTL = Create("ChildTL", cornerLL, cornerMM, CornerTL, cornerTT, coordLL, coordMM, CoordTL, coordTT);
			ChildTR = Create("ChildTR", cornerMM, cornerRR, cornerTT, CornerTR, coordMM, coordRR, coordTT, CoordTR);

			CopyVertiesToChildren();

			// Link children
			ChildBL.NeighbourR.Set(ChildBR, 1, 1, 3, 0, 0, 2, 0);
			ChildBL.NeighbourT.Set(ChildTL, 3, 2, 3, 2, 0, 1, 2);
			ChildBR.NeighbourL.Set(ChildBL, 0, 0, 2, 1, 1, 3, 1);
			ChildBR.NeighbourT.Set(ChildTR, 3, 2, 3, 2, 0, 1, 2);
			ChildTL.NeighbourR.Set(ChildTR, 1, 1, 3, 0, 0, 2, 0);
			ChildTL.NeighbourB.Set(ChildBL, 2, 0, 1, 3, 2, 3, 3);
			ChildTR.NeighbourL.Set(ChildTL, 0, 0, 2, 1, 1, 3, 1);
			ChildTR.NeighbourB.Set(ChildBR, 2, 0, 1, 3, 2, 3, 3);

			// Link to neighbours
			LinkNeighbours(ChildBL, ChildTL, ref ChildBL.NeighbourL, ref ChildTL.NeighbourL, ref NeighbourL);
			LinkNeighbours(ChildBR, ChildTR, ref ChildBR.NeighbourR, ref ChildTR.NeighbourR, ref NeighbourR);
			LinkNeighbours(ChildBL, ChildBR, ref ChildBL.NeighbourB, ref ChildBR.NeighbourB, ref NeighbourB);
			LinkNeighbours(ChildTL, ChildTR, ref ChildTL.NeighbourT, ref ChildTR.NeighbourT, ref NeighbourT);

			UpdateInvalidChildren();

			UpdateNeighbourData(NeighbourL.Face, ref NeighbourL);
			UpdateNeighbourData(NeighbourR.Face, ref NeighbourR);
			UpdateNeighbourData(NeighbourB.Face, ref NeighbourB);
			UpdateNeighbourData(NeighbourT.Face, ref NeighbourT);

			cachedMeshRenderer.enabled = false;
		}

		private void LinkNeighbours(SgtTerrainFace childA, SgtTerrainFace childB, ref SgtTerrainNeighbour childNeighbourA, ref SgtTerrainNeighbour childNeighbourB, ref SgtTerrainNeighbour neighbour)
		{
			if (neighbour.Face.Split == true)
			{
				var neighbourChildC = neighbour.Face.GetChild(neighbour.C);
				var neighbourChildD = neighbour.Face.GetChild(neighbour.D);

				neighbourChildC.SetNeighbour(neighbour.O, childA);
				neighbourChildD.SetNeighbour(neighbour.O, childB);

				childNeighbourA.Set(neighbourChildC, neighbour.I, neighbour.A, neighbour.B, neighbour.O, neighbour.C, neighbour.D, neighbour.Z);
				childNeighbourB.Set(neighbourChildD, neighbour.I, neighbour.A, neighbour.B, neighbour.O, neighbour.C, neighbour.D, neighbour.Z);
			}
			else
			{
				childNeighbourA.Set(neighbour.Face, neighbour.I, neighbour.A, neighbour.B, neighbour.O, neighbour.C, neighbour.D, neighbour.Z);
				childNeighbourB.Set(neighbour.Face, neighbour.I, neighbour.A, neighbour.B, neighbour.O, neighbour.C, neighbour.D, neighbour.Z);
			}
		}

		private void UpdateNeighbourData(SgtTerrainFace face, ref SgtTerrainNeighbour neighbour)
		{
			if (face.Split == true)
			{
				var childC = face.GetChild(neighbour.C);
				var childD = face.GetChild(neighbour.D);

				childC.BuildIndices();
				childD.BuildIndices();

				UpdateNeighbourData(childC, ref neighbour);
				UpdateNeighbourData(childD, ref neighbour);
			}
		}

		private void UpdateInvalidChildren()
		{
			ChildBL.UpdateInvalid();
			ChildBR.UpdateInvalid();
			ChildTL.UpdateInvalid();
			ChildTR.UpdateInvalid();
		}

		private void UnsplitNow()
		{
			if (ChildBL.Split == true) ChildBL.UnsplitNow();
			if (ChildBR.Split == true) ChildBR.UnsplitNow();
			if (ChildTL.Split == true) ChildTL.UnsplitNow();
			if (ChildTR.Split == true) ChildTR.UnsplitNow();

			if (Terrain.OnDespawnFace != null)
			{
				Terrain.OnDespawnFace.Invoke(ChildBL);
				Terrain.OnDespawnFace.Invoke(ChildBR);
				Terrain.OnDespawnFace.Invoke(ChildTL);
				Terrain.OnDespawnFace.Invoke(ChildTR);
			}

			Split   = false;
			ChildBL = Pool(ChildBL);
			ChildBR = Pool(ChildBR);
			ChildTL = Pool(ChildTL);
			ChildTR = Pool(ChildTR);

			BuildIndices();

			UnlinkNeighbours(ref NeighbourL);
			UnlinkNeighbours(ref NeighbourR);
			UnlinkNeighbours(ref NeighbourB);
			UnlinkNeighbours(ref NeighbourT);

			cachedMeshRenderer.enabled = true;
		}

		private void UnlinkNeighbours(ref SgtTerrainNeighbour childNeighbour)
		{
			if (childNeighbour.Face.Split == true)
			{
				var childC = childNeighbour.Face.GetChild(childNeighbour.C);
				var childD = childNeighbour.Face.GetChild(childNeighbour.D);

				childC.SetNeighbour(childNeighbour.O, this);
				childD.SetNeighbour(childNeighbour.O, this);

				childC.BuildIndices();
				childD.BuildIndices();
			}
		}

		private SgtTerrainFace Create(string childName, SgtVector3D cornerBL, SgtVector3D cornerBR, SgtVector3D cornerTL, SgtVector3D cornerTR, SgtVector2D coordBL, SgtVector2D coordBR, SgtVector2D coordTL, SgtVector2D coordTR)
		{
			var face = Create(childName, gameObject.layer, transform);

			face.State    = StateType.Invalid;
			face.Terrain  = Terrain;
			face.Side     = Side;
			face.Depth    = Depth + 1;
			face.CornerBL = cornerBL;
			face.CornerBR = cornerBR;
			face.CornerTL = cornerTL;
			face.CornerTR = cornerTR;
			face.CoordBL  = coordBL;
			face.CoordBR  = coordBR;
			face.CoordTL  = coordTL;
			face.CoordTR  = coordTR;

			return face;
		}

		private Mesh GetMesh()
		{
			if (mesh == null)
			{
				mesh = SgtObjectPool<Mesh>.Pop() ?? new Mesh();
#if UNITY_EDITOR
				mesh.hideFlags = HideFlags.DontSave;
#endif
				mesh.name = "Terrain";
			}

			if (cachedMeshFilter == null) cachedMeshFilter = GetComponent<MeshFilter>();

			cachedMeshFilter.sharedMesh = mesh;

			return mesh;
		}
	}
}