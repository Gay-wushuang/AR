using UnityEngine;

namespace EEGAI.Demo
{
    public class MemoryShard : MonoBehaviour
    {
        [Header("Shard Settings")]
        [SerializeField] private string shardName = "Memory Shard";
        [SerializeField] private string shardDescription = "A piece of lost memory";
        [SerializeField] private float collectDistance = 3f;

        [Header("Visual Effects")]
        [SerializeField] private float floatAmplitude = 0.3f;
        [SerializeField] private float floatSpeed = 2f;
        [SerializeField] private float rotationSpeed = 90f;

        [Header("Particles")]
        [SerializeField] private ParticleSystem collectParticles;
        [SerializeField] private ParticleSystem idleParticles;

        public bool IsCollected { get; private set; } = false;

        private Vector3 initialPosition;
        private Transform player;
        private float collectionDelay = 0.5f;

        public event System.Action<MemoryShard> OnCollected;

        private void Awake()
        {
            initialPosition = transform.position;
            player = Camera.main?.transform;
        }

        private void Start()
        {
            if (idleParticles != null)
            {
                idleParticles.Play();
            }
        }

        private void Update()
        {
            if (IsCollected) return;

            AnimateShard();
            CheckCollection();
        }

        private void AnimateShard()
        {
            float floatOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = initialPosition + Vector3.up * floatOffset;

            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }

        private void CheckCollection()
        {
            if (player == null) return;

            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= collectDistance)
            {
                Collect();
            }
        }

        public void Collect()
        {
            if (IsCollected) return;

            IsCollected = true;
            Debug.Log($"[Demo] Collected memory shard: {shardName}");

            if (idleParticles != null)
            {
                idleParticles.Stop();
            }

            if (collectParticles != null)
            {
                collectParticles.Play();
            }

            StartCoroutine(CollectAnimation());
        }

        private System.Collections.IEnumerator CollectAnimation()
        {
            Vector3 startPos = transform.position;
            Vector3 targetPos = player.position + Vector3.up * 2f;
            float elapsed = 0f;
            float duration = 0.8f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = t * t * (3f - 2f * t);

                transform.position = Vector3.Lerp(startPos, targetPos, t);
                transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);

                yield return null;
            }

            transform.localScale = Vector3.zero;

            yield return new WaitForSeconds(collectionDelay);

            if (collectParticles != null)
            {
                collectParticles.Stop();
            }

            OnCollected?.Invoke(this);

            Destroy(gameObject, 0.5f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, collectDistance);
        }

        public string GetShardName() => shardName;
        public string GetShardDescription() => shardDescription;
    }
}