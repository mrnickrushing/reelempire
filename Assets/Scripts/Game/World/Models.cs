using UnityEngine;

namespace ReelEmpire.Game
{
    public static class Models
    {
        /// <summary>A free kit model (Resources/Models/{id}: one unit on its longest side, or one unit tall), placed, turned
        /// and sized in the parent's space. Null when it isn't there.</summary>
        public static GameObject Kit(string id, Transform parent, Vector3 at, float yaw, float size)
        {
            var mesh = Resources.Load<Mesh>("Models/" + id);
            if (mesh == null) return null;
            var go = new GameObject(id);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = at;
            go.transform.localRotation = Quaternion.Euler(0, yaw, 0);
            go.transform.localScale = Vector3.one * size;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = Materials.Kit;
            return go;
        }

        /// <summary>An object drawing what <paramref name="build"/> puts into a mesh builder (the game's own shapes).</summary>
        public static GameObject Built(string name, Transform parent, System.Action<MeshBuilder> build)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var mb = new MeshBuilder();
            build(mb);
            go.AddComponent<MeshFilter>().sharedMesh = mb.ToMesh(name);
            go.AddComponent<MeshRenderer>().sharedMaterial = Materials.World;
            return go;
        }
    }
}
