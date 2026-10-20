using System;
using System.Collections.Generic;
using UnityEngine;

namespace EEGAI.Demo
{
    /// <summary>Creates a fresh, obstacle-checked expedition when a game starts.</summary>
    public class RandomExpeditionLayout : MonoBehaviour
    {
        [SerializeField] private DemoGameState gameState;
        [SerializeField] private Transform player;
        [SerializeField] private Collider ground;
        [SerializeField] private RepairNode[] objectives;
        [SerializeField] private OxygenCanister oxygenTemplate;
        [SerializeField] private float innerRadius = 25f;
        [SerializeField] private float outerRadius = 80f;
        private readonly List<GameObject> supplies = new List<GameObject>();
        private readonly List<Vector3> occupied = new List<Vector3>();
        private readonly List<float> radii = new List<float>();
        public Vector3 Center { get; private set; }
        public int SupplyCount => supplies.Count;

        private void Awake() { Center = player.position; }
        private void OnEnable()
        {
            gameState.OnReset += ClearLayout;
            gameState.OnStarted += BeginLayout;
        }
        private void OnDisable()
        {
            gameState.OnReset -= ClearLayout;
            gameState.OnStarted -= BeginLayout;
        }
        private void BeginLayout()
        {
            // ResetGame has already restored the player's starting pose.
            Center = player.position;
            if (!GenerateLayout(Environment.TickCount))
            {
                Debug.LogError("[Layout] No safe layout found; returning to menu.");
                gameState.ResetGame();
            }
        }
        private void ClearLayout()
        {
            foreach (var supply in supplies)
            {
                supply.SetActive(false);
                Destroy(supply);
            }
            supplies.Clear(); occupied.Clear(); radii.Clear();
            foreach (var node in objectives) node.gameObject.SetActive(false);
            // Keep the original canister as the fixed base refill inside the exclusion radius.
            oxygenTemplate.gameObject.SetActive(true);
            Physics.SyncTransforms();
        }

        public bool GenerateLayout(int seed)
        {
            var random = new System.Random(seed);
            for (int attempt = 0; attempt < 20; attempt++)
            {
                ClearLayout();
                bool valid = true;
                float phase = (float)random.NextDouble() * Mathf.PI * 2f;
                for (int i = 0; i < objectives.Length; i++)
                {
                    var node = objectives[i];
                    node.ResetNode();
                    Vector3 placed;
                    if (!Place(node.gameObject, random, 36f, 74f,
                        phase + i * Mathf.PI * 2f / objectives.Length, 1.2f, null, out placed))
                    { valid = false; break; }
                    node.SetSpawnPosition(placed);
                    node.gameObject.SetActive(true);
                    if (!AddSupply(random, innerRadius, outerRadius, 0, Mathf.PI * 2f, placed))
                    { valid = false; break; }
                }
                // Four angular sectors in each of three rings give distributed refill routes.
                for (int ring = 0; valid && ring < 3; ring++)
                    for (int sector = 0; valid && sector < 4; sector++)
                        valid = AddSupply(random, 28f + ring * 17f, 34f + ring * 19f,
                            phase + sector * Mathf.PI / 2f + ring * 0.25f, 0.7f, null);
                if (valid) { Physics.SyncTransforms(); return true; }
            }
            ClearLayout();
            return false;
        }

        private bool AddSupply(System.Random random, float min, float max,
            float angle, float width, Vector3? near)
        {
            var clone = Instantiate(oxygenTemplate.gameObject);
            clone.SetActive(false);
            clone.name = "Expedition Oxygen " + (supplies.Count + 1);
            supplies.Add(clone);
            Vector3 placed;
            if (!Place(clone, random, min, max, angle, width, near, out placed)) return false;
            clone.SetActive(true);
            Physics.SyncTransforms();
            return true;
        }

        private bool Place(GameObject item, System.Random random, float min, float max,
            float angle, float width, Vector3? near, out Vector3 placed)
        {
            var box = item.GetComponent<BoxCollider>();
            var orientedBounds = new Bounds(Vector3.zero, Vector3.zero);
            for (int corner = 0; corner < 8; corner++)
                orientedBounds.Encapsulate(item.transform.TransformVector(Vector3.Scale(box.size * 0.5f,
                    new Vector3((corner & 1) == 0 ? -1 : 1, (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1))));
            var extents = orientedBounds.extents;
            // Circumscribed horizontal radius also keeps the entire model out of the exclusion zone.
            float radius = new Vector2(extents.x, extents.z).magnitude;
            var offset = item.transform.TransformVector(box.center);
            placed = Vector3.zero;
            for (int trial = 0; trial < 200; trial++)
            {
                float a = angle + ((float)random.NextDouble() - 0.5f) * width;
                float r = Mathf.Sqrt(Mathf.Lerp(min * min, max * max, (float)random.NextDouble()));
                Vector3 candidate = Center + new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * r;
                if (near.HasValue)
                {
                    a = (float)random.NextDouble() * Mathf.PI * 2f;
                    candidate = near.Value + new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * (6f + (float)random.NextDouble() * 3f);
                }
                var planar = new Vector2(candidate.x - Center.x, candidate.z - Center.z);
                if (planar.magnitude < innerRadius + radius + 1f || planar.magnitude > outerRadius - radius - 1f) continue;
                bool clear = true;
                for (int i = 0; i < occupied.Count; i++)
                    if (Vector2.Distance(new Vector2(candidate.x, candidate.z), new Vector2(occupied[i].x, occupied[i].z)) < radius + radii[i] + 3f) { clear = false; break; }
                if (!clear) continue;
                float highest = float.MinValue, lowest = float.MaxValue;
                for (int sample = 0; sample < 5; sample++)
                {
                    Vector3 delta = sample == 0 ? Vector3.zero : new Vector3((sample < 3 ? -1 : 1) * radius, 0, (sample % 2 == 0 ? -1 : 1) * radius);
                    RaycastHit hit;
                    if (!ground.Raycast(new Ray(new Vector3(candidate.x + offset.x, ground.bounds.max.y + 20f, candidate.z + offset.z) + delta, Vector3.down), out hit, ground.bounds.size.y + 50f) || hit.normal.y < 0.85f) { clear = false; break; }
                    highest = Mathf.Max(highest, hit.point.y); lowest = Mathf.Min(lowest, hit.point.y);
                }
                if (!clear || highest - lowest > 0.65f) continue;
                candidate.y = highest + extents.y - offset.y + 0.08f;
                Vector3 checkCenter = candidate + offset;
                foreach (var other in Physics.OverlapBox(checkCenter, new Vector3(radius + 1f, extents.y + 0.3f, radius + 1f), Quaternion.identity, ~0, QueryTriggerInteraction.Ignore))
                    if (other != ground && !other.transform.IsChildOf(item.transform)) { clear = false; break; }
                if (!clear) continue;
                item.transform.position = candidate;
                occupied.Add(candidate); radii.Add(radius);
                placed = candidate;
                return true;
            }
            return false;
        }
    }
}
