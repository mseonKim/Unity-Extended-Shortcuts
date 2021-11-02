using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.ShortcutManagement;

public static class ExtendedShortcuts
{
    private struct SpriteBound
    {
        public float minX, maxX;
        public float minY, maxY;
        public float r;
        private float _rotationZ;  // -90 ~ 90

        public SpriteBound(Transform transform, Sprite sprite)
        {
            float ppu = sprite.pixelsPerUnit;
            this.minX = transform.position.x - (sprite.pivot.x / ppu) * transform.lossyScale.x;
            this.maxX = transform.position.x + (sprite.texture.width - sprite.pivot.x) / ppu * transform.lossyScale.x;
            this.minY = transform.position.y - (sprite.texture.height - sprite.pivot.y) / ppu * transform.lossyScale.y;
            this.maxY = transform.position.y + sprite.pivot.y / ppu * transform.lossyScale.y;
            this._rotationZ = transform.rotation.eulerAngles.z;
            this._rotationZ %= Mathf.Sign(this._rotationZ) * 90f;
            this.r = new Vector2(Mathf.Max(Mathf.Abs(this.minX - transform.position.x), Mathf.Abs(this.maxX - transform.position.x)),
                                 Mathf.Max(Mathf.Abs(this.minY - transform.position.y), Mathf.Abs(this.minY - transform.position.y))).magnitude;

            if (_rotationZ != 0f)
            {
                Vector2 min = _rotationZ > 0f ? new Vector2(this.minX, this.minY) : new Vector2(this.maxX, this.minY);
                min.x -= transform.position.x;
                min.y -= transform.position.y;
                float rad = Mathf.Acos(Vector2.Dot(min.normalized, Vector2.right)) + _rotationZ * Mathf.Deg2Rad;
                float len = min.magnitude;
            }
        }

        public void EditorRaycast2D(Vector2 pos, List<SpriteBound> bounds)
        {
            float distance = 100f;

        }
    }


    [Shortcut("ExtendedShortcuts/StickObject", KeyCode.End)]
    private static void StickObject()
    {
        var currentObj = Selection.activeGameObject;
        // TODO: support multiple selection

        var spriteRenderer = currentObj.GetComponent<SpriteRenderer>();
        if (spriteRenderer)
        {
            // 2D - Use Y Position
            var bound = new SpriteBound(currentObj.transform, spriteRenderer.sprite);
            float nearestY = Mathf.NegativeInfinity;
            bool hasCloseSprite = false;
            foreach (var candidate in SceneAsset.FindObjectsOfType<SpriteRenderer>())
            {
                if (!candidate.gameObject.activeInHierarchy)
                    continue;

                // Skip if included in selection
                if (candidate.GetInstanceID() == spriteRenderer.GetInstanceID())
                    continue;

                // Check if candidate
                var tempBound = new SpriteBound(candidate.transform, candidate.sprite);
                if (tempBound.maxY >= bound.minY)
                    continue;

                if ((bound.minX >= tempBound.minX && bound.minX <= tempBound.maxX)
                    || (bound.maxX >= tempBound.minX && bound.maxX <= tempBound.maxX))
                {
                    // Find nearest sprite
                    if (tempBound.maxY > nearestY)
                    {
                        hasCloseSprite = true;
                        nearestY = tempBound.maxY;
                    }
                }
            }

            // Stick to close sprite
            if (hasCloseSprite)
            {
                Undo.RecordObject(currentObj.transform, "Move object " + currentObj.name);
                currentObj.transform.Translate(new Vector3(0f, nearestY - bound.minY, 0f));
            }
        }
        else
        {
            var meshFilter = currentObj.GetComponent<MeshFilter>();
            if (!meshFilter)
                return;

            // 3D - Use Z Position
            Debug.Log("3d");
            var mesh = meshFilter.sharedMesh;

            // Find vertices that have minimum z
            var testVertices = new List<Vector3>();
            float minZ = mesh.bounds.min.z + 0.01f;
            foreach (var v in mesh.vertices)
            {
                if (v.z < minZ)
                {
                    testVertices.Clear();
                    testVertices.Add(v);
                    minZ = v.z;
                }
                else if (v.z < minZ + 0.001f)
                {
                    testVertices.Add(v);
                }
            }

            // Find nearest mesh
            float nearestZ = Mathf.NegativeInfinity;
            bool hasCloseMesh = false;
            foreach (var candidate in SceneAsset.FindObjectsOfType<MeshFilter>())
            {
                if (meshFilter.GetInstanceID() == candidate.GetInstanceID())
                    continue;
                
                if (candidate.sharedMesh.bounds.min.z > minZ)
                    continue;


            }

            // Stick to close mesh
        }


        // var sceneobjs = SceneAsset.FindObjectsOfType<MeshFilter>();
        // foreach (var item in sceneobjs)
        // {
        //     Debug.Log(item.name);
        // }
    }
}
