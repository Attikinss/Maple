using Maple.Blackboards;
using Maple.Nodes;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Maple
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Agent : MonoBehaviour, INoiseListener
    {
        /* --<| VARIABLES |>-- */
        [Tooltip("A behaviour tree asset created from the Maple tree editor is placed here and it will be cloned for use at runtime."), SerializeField]
        private BehaviourTree m_Tree;

        [Header("Components")]
        [Tooltip("If true an AudioSource component (with default settings) will be added to this agent if one isn't already attached."), SerializeField]
        private bool m_ForceAddAudioSource = true;

        [Tooltip("If true an Animator component (with default settings) will be added to this agent if one isn't already attached."), SerializeField]
        private bool m_ForceAddAnimator = true;

        /* --<| COMPONENTS |>-- */
        [SerializeField]
        private AudioSource m_AudioSource;
        
        [SerializeField]
        private Animator m_Animator;

        [SerializeField]
        private NavMeshAgent m_NavAgent;

        [SerializeField]
        private NoiseEmitter m_NoiseEmitter;

        /* --<| PROPERTIES |>-- */
        public BehaviourTree RuntimeTree { get; private set; }
        public AudioSource AudioSource { get => m_AudioSource; }
        public Animator Animator { get => m_Animator; }
        public NoiseEmitter NoiseEmitter { get => m_NoiseEmitter; }
        public Vector3 Destination { get => m_NavAgent.destination; }
        public bool IsActive { get; private set; }

        private void Awake() => Initialise();
        protected virtual void Start() => StartCoroutine(StartTree());

        protected void Initialise()
        {
            // Cache components for later use
            m_NavAgent = GetComponent<NavMeshAgent>();

            // If AudioSource component is required and not already
            // attached to agent, add it to the agent gameObject
            if (m_ForceAddAudioSource && !TryGetComponent(out m_AudioSource))
                m_AudioSource = gameObject.AddComponent<AudioSource>();

            // If Animator component is required and not already
            // attached to agent, add it to the agent gameObject
            if (m_ForceAddAnimator && !TryGetComponent(out m_Animator))
                m_Animator = gameObject.AddComponent<Animator>();

            // Notify user that a behaviour tree asset has not been assigned to the agent
            if (m_Tree == null)
            {
                Debug.LogWarning($"A behaviour tree has not been assigned for {gameObject.name}!");
                return;
            }
            
            // Deep clone tree
            RuntimeTree = m_Tree.Clone(gameObject.name, this);
        }

        public void DetectNoise(object source, float loudness)
        {
            Agent agent = source as Agent;
            if (agent)
            {
                // Ignore noise event if source was self
                if (agent == this)
                    return;

                // Find closest point on navmesh and then raycast to noise location
                // If the length end point of the ray is roughly the where the noise
                // event was triggered, then it is in range to hear
            }

            // Check other types and do something
        }

        public void Move(Vector3 direction) => m_NavAgent.Move(direction);
        public bool MoveTo(GameObject target, bool allowPartialPath) => MoveTo(target.transform.position, allowPartialPath);
        public bool MoveTo(Transform target, bool allowPartialPath) => MoveTo(target.position, allowPartialPath);
        public bool MoveTo(Vector3 target, bool allowPartialPath)
        {
            m_NavAgent.isStopped = false;
            m_NavAgent.SetDestination(target);

            if (m_NavAgent.pathStatus != NavMeshPathStatus.PathInvalid)
            {
                // Move only if partial paths are allowed or the path was completed
                if (allowPartialPath || m_NavAgent.pathStatus == NavMeshPathStatus.PathComplete)
                {
                    // If path was completed or is partially complete and
                    // partial paths are allowed, the agent can move
                    return true;
                }
            }

            // Agent could not find a suitable path
            m_NavAgent.isStopped = true;
            m_NavAgent.ResetPath();
            return false;
        }

        public void LookAt(GameObject target) => LookAt(target.transform.position);
        public void LookAt(Transform target) => LookAt(target.position);
        public void LookAt(Vector3 target)
        {
            // Level out the vertical difference so that the agent doesn't jitter
            target.y = transform.position.y;
            m_NavAgent.updateRotation = false;

            Vector3 direction = target - transform.position;
            float difference = Vector3.Angle(direction, transform.forward) / 10.0f;

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, difference * m_NavAgent.angularSpeed * Time.deltaTime);
        }

        public bool TargetReachable(GameObject target) => TargetReachable(target.transform.position);
        public bool TargetReachable(Transform target) => TargetReachable(target.position);
        public bool TargetReachable(Vector3 target)
        {
            NavMeshPath path = new NavMeshPath();
            return m_NavAgent.CalculatePath(target, path)
                && path.status == NavMeshPathStatus.PathComplete;
        }

        public bool AtTarget(GameObject target, bool useAgentRadius = false) => AtTarget(target.transform.position, useAgentRadius);
        public bool AtTarget(Transform target, bool useAgentRadius = false) => AtTarget(target.position, useAgentRadius);
        public bool AtTarget(Vector3 target, bool useAgentRadius = false)
        {
            float threshold = m_NavAgent.stoppingDistance;
            if (useAgentRadius)
                threshold += m_NavAgent.radius;

            return Vector3.Distance(transform.position, target) <= threshold;
        }

        public void PlayAnimation(int hashID, int layer = -1, bool crossFade = false)
        {
            if (Animator)
            {
                if (crossFade)
                    Animator.CrossFade(hashID, 0.5f, layer);
                else
                    Animator.Play(hashID, layer);
            }
            else
            {
                // Alert user that an animator isn't attached to the agent
                Debug.LogWarning($"({gameObject.name}): Agent does not have access to an Animator!");
            }
        }

        public void PlayAnimation(string name, int layer = -1, bool crossFade = false)
        {
            if (Animator)
            {
                if (crossFade)
                    Animator.CrossFade(name, 0.5f, layer);
                else
                    Animator.Play(name, layer);
            }
            else
            {
                // Alert user that an animator isn't attached to the agent
                Debug.LogWarning($"({gameObject.name}): Agent does not have access to an Animator!");
            }
        }

        public virtual void AttachTree(BehaviourTree tree) => RuntimeTree = tree;       
        public virtual void DetachTree() => RuntimeTree = null;

        public IEnumerator StartTree(float tickInvertalSeconds = 0.0f)
        {
            float time = 0.0f;
            RuntimeTree?.Start();

            while (RuntimeTree != null)
            {
                if (Time.time - time <= tickInvertalSeconds)
                    RuntimeTree.Tick();
                
                time += Time.deltaTime;
                yield return null;
            }
        }
    }
}