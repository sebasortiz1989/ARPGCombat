using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using RPG.Combat;
using RPG.Core;
using RPG.Movement;
using System;

namespace RPG.Control
{
    public class AIController : MonoBehaviour
    {
        // Config
        [SerializeField] float chaseDistance = 6f;
        [SerializeField] float timeToWaitSuspiciously = 6f;
        [SerializeField] float waypointDwellTime = 4f;
        [SerializeField] PatrolPath patrolPath;
        [SerializeField] float waypointTolerance = 1.1f;

        // String const
        private const string PLAYER_TAG = "Player";

        // Cached reference
        GameObject player;
        Health health;
        Fighter fighter;
        Mover mover;
        NavMeshAgent navMeshAgent;
        ActionScheduler actionScheduler;

        // Initialize Variables
        float distanceToPlayer;
        Vector3 guardLocation;
        Quaternion guardRotation;
        public float timeSinceLastSawPlayer = Mathf.Infinity;
        public float timeSinceWaypoint = Mathf.Infinity;
        int currentWaypointIndex = 0;
        
        // Start is called before the first frame update
        void Start()
        {
            player = GameObject.FindWithTag(PLAYER_TAG);
            fighter = GetComponent<Fighter>();
            health = GetComponent<Health>();
            mover = GetComponent<Mover>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            actionScheduler = GetComponent<ActionScheduler>();
            guardLocation = transform.position;
            guardRotation = transform.rotation;
        }

        // Update is called once per frame
        void Update()
        {
            if (health.IsDead()) return;
            AttackIfPlayerInRange();
        }

        public bool AttackIfPlayerInRange()
        {           
            distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= chaseDistance && fighter.CanAttack(player))
            {               
                AttackBehaviour();
                return true;
            }
            else if (timeSinceLastSawPlayer < timeToWaitSuspiciously)
            {
                SuspicionBehaviour();
            }
            else
            {
                PatrolGuardBehaviour();
            }
            if (navMeshAgent.velocity.magnitude < 0.1f)
                UpdateTimers();
            return false;
        }

        private void UpdateTimers()
        {
            timeSinceLastSawPlayer += Time.deltaTime;
            timeSinceWaypoint += Time.deltaTime;
        }

        private void PatrolGuardBehaviour()
        {
            fighter.Cancel();

            Vector3 nextPosition = guardLocation;

            if (patrolPath != null)
            {
                if (AtWaypoint())
                {
                    timeSinceWaypoint = 0;
                    CycleWaypoint();
                }
                nextPosition = GetCurrentWaypoint();
            }

            if (timeSinceWaypoint > waypointDwellTime)
                mover.StartMoveAction(nextPosition);

            if (patrolPath == null)
            {
                if (navMeshAgent.velocity.magnitude < 0.05f && transform.rotation != guardRotation)
                {
                    transform.rotation = guardRotation;
                }
            }
        }

        private bool AtWaypoint()
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, GetCurrentWaypoint());
            return distanceToWaypoint < waypointTolerance;
        }


        private void CycleWaypoint()
        {
            currentWaypointIndex = patrolPath.GetNextIndex(currentWaypointIndex);
        }

        private Vector3 GetCurrentWaypoint()
        {
            return patrolPath.GetWaypoint(currentWaypointIndex);
        }

        private void SuspicionBehaviour()
        {
            actionScheduler.CancelCurrentAction();
        }

        private void AttackBehaviour()
        {
            timeSinceLastSawPlayer = 0;
            fighter.Attack(player);
        }

        // Called by Unity
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
    }
}