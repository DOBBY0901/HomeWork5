using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SmallScaleInc.TopDownPixelCharactersPack1
{
    public class AnimationController : MonoBehaviour
    {
        private Animator animator;
        private PlayerController playerController;

        [Header("Muzzle")]
        public Animator muzzleAnimator;
        public SpriteRenderer muzzleFlashRenderer;

        [Header("Direction")]
        public string currentDirection = "isEast";
        public bool isCurrentlyRunning;

        [Header("Hit Effects")]
        [SerializeField] private List<GameObject> bloodPrefabs = new List<GameObject>();
        [SerializeField] private List<GameObject> radiatedPrefabs = new List<GameObject>();
        public bool isRadiated = false;

        private bool isAttacking;
        private bool isDying;

        private readonly HashSet<string> animatorParameters = new HashSet<string>();
        private readonly HashSet<string> muzzleParameters = new HashSet<string>();

        private static bool IsKeyPressed(Key key)
        {
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard[key].isPressed;
        }

        private static bool IsAttackMousePressed()
        {
            Mouse mouse = Mouse.current;
            return mouse != null && mouse.leftButton.isPressed;
        }

        private static bool WasAttackMouseReleasedThisFrame()
        {
            Mouse mouse = Mouse.current;
            return mouse != null && mouse.leftButton.wasReleasedThisFrame;
        }

        private static Vector2 GetMouseScreenPosition()
        {
            Mouse mouse = Mouse.current;
            return mouse != null ? mouse.position.ReadValue() : Vector2.zero;
        }

        private void Start()
        {
            animator = GetComponent<Animator>();
            playerController = GetComponent<PlayerController>();

            CacheAnimatorParameters();

            SetBoolSafe("isEast", true);
            SetBoolSafe("isWalking", false);
            SetBoolSafe("isRunning", false);

            if (muzzleFlashRenderer != null)
                muzzleFlashRenderer.sortingOrder = -1;
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy)
                return;

            if (isDying)
                return;

            HandleMovement();
            HandleAttack();
        }

        private void CacheAnimatorParameters()
        {
            animatorParameters.Clear();
            muzzleParameters.Clear();

            if (animator != null)
            {
                foreach (AnimatorControllerParameter parameter in animator.parameters)
                {
                    animatorParameters.Add(parameter.name);
                }
            }

            if (muzzleAnimator != null)
            {
                foreach (AnimatorControllerParameter parameter in muzzleAnimator.parameters)
                {
                    muzzleParameters.Add(parameter.name);
                }
            }
        }

        private void SetBoolSafe(string parameterName, bool value)
        {
            if (animator == null)
                return;

            if (!animatorParameters.Contains(parameterName))
                return;

            animator.SetBool(parameterName, value);
        }

        private void SetMuzzleBoolSafe(string parameterName, bool value)
        {
            if (muzzleAnimator == null)
                return;

            if (!muzzleParameters.Contains(parameterName))
                return;

            muzzleAnimator.SetBool(parameterName, value);
        }

        private void PlayStateSafe(string stateName)
        {
            if (animator == null)
                return;

            animator.Play(stateName, 0);
        }

        private void HandleMovement()
        {
            string newDirection = GetDirectionFromMouse();

            if (newDirection != currentDirection)
            {
                UpdateDirection(newDirection);
            }

            string directionName = newDirection.Substring(2);

            bool moveForward = IsKeyPressed(Key.W);
            bool moveBackward = IsKeyPressed(Key.S);
            bool strafeLeft = IsKeyPressed(Key.A);
            bool strafeRight = IsKeyPressed(Key.D);

            isCurrentlyRunning = moveForward || moveBackward || strafeLeft || strafeRight;

            ResetAllMovementBools();

            SetBoolSafe("isRunning", isCurrentlyRunning);
            SetBoolSafe("isWalking", false);

            if (!isCurrentlyRunning)
                return;

            if (moveForward)
            {
                SetMovementAnimation("Move", directionName);
            }
            else if (moveBackward || strafeLeft || strafeRight)
            {
                SetMovementAnimation("Move", directionName);
            }
        }

        private string GetDirectionFromMouse()
        {
            if (Camera.main == null)
                return currentDirection;

            Vector3 mouseScreenPosition = GetMouseScreenPosition();
            mouseScreenPosition.z = Camera.main.transform.position.z - transform.position.z;

            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
            Vector3 directionToMouse = mouseWorldPosition - transform.position;

            if (directionToMouse == Vector3.zero)
                return currentDirection;

            directionToMouse.Normalize();

            float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;

            if (angle < 0)
                angle += 360f;

            return DetermineDirectionFromAngle(angle);
        }

        private string DetermineDirectionFromAngle(float angle)
        {
            angle = (angle + 360f) % 360f;

            if (angle < 15f || angle >= 345f)
                return "isEast";
            else if (angle >= 15f && angle < 75f)
                return "isNorthEast";
            else if (angle >= 75f && angle < 105f)
                return "isNorth";
            else if (angle >= 105f && angle < 165f)
                return "isNorthWest";
            else if (angle >= 165f && angle < 195f)
                return "isWest";
            else if (angle >= 195f && angle < 255f)
                return "isSouthWest";
            else if (angle >= 255f && angle < 285f)
                return "isSouth";
            else if (angle >= 285f && angle < 345f)
                return "isSouthEast";

            return "isEast";
        }

        private void UpdateDirection(string newDirection)
        {
            string[] directions =
            {
                "isWest",
                "isEast",
                "isSouth",
                "isSouthWest",
                "isNorthEast",
                "isSouthEast",
                "isNorth",
                "isNorthWest"
            };

            foreach (string direction in directions)
            {
                SetBoolSafe(direction, direction == newDirection);
            }

            if (currentDirection != newDirection)
            {
                isAttacking = false;
                ResetAttackParameters();
            }

            currentDirection = newDirection;
        }

        private void SetMovementAnimation(string baseKey, string direction)
        {
            string animationKey = baseKey + direction;
            SetBoolSafe(animationKey, true);
        }

        private bool HasAnimatorParameter(string parameterName)
        {
            return animatorParameters.Contains(parameterName);
        }

        private void ResetAllMovementBools()
        {
            string[] directions =
            {
                "North",
                "South",
                "East",
                "West",
                "NorthEast",
                "NorthWest",
                "SouthEast",
                "SouthWest"
            };

            string[] movementKeys =
            {
                "Move",
                "RunBackwards",
                "StrafeLeft",
                "StrafeRight",
                "CrouchRun"
            };

            foreach (string key in movementKeys)
            {
                foreach (string direction in directions)
                {
                    SetBoolSafe(key + direction, false);
                }
            }
        }

        private void HandleAttack()
        {
            if (IsAttackMousePressed())
            {
                if (isAttacking == false)
                {
                    isAttacking = true;
                    ResetAttackParameters();
                }

                string newDirection = GetDirectionFromMouse();

                if (newDirection != currentDirection)
                {
                    ResetAttackParameters();
                    ResetAllGunFireBools();
                    UpdateDirection(newDirection);
                }

                string directionName = newDirection.Substring(2);

                bool moving =
                    IsKeyPressed(Key.W) ||
                    IsKeyPressed(Key.S) ||
                    IsKeyPressed(Key.A) ||
                    IsKeyPressed(Key.D);

                if (muzzleFlashRenderer != null)
                    muzzleFlashRenderer.sortingOrder = 150;

                SetBoolSafe("isAttackRunning", false);
                SetBoolSafe("isAttackAttacking", !moving);

                if (!moving)
                    SetBoolSafe("AttackAttack" + directionName, true);

                ResetAllGunFireBools();
                SetMuzzleBoolSafe("Gunfire" + directionName, true);
            }
            else if (WasAttackMouseReleasedThisFrame())
            {
                isAttacking = false;

                ResetAttackParameters();
                ResetAllGunFireBools();

                if (muzzleFlashRenderer != null)
                    muzzleFlashRenderer.sortingOrder = -1;

                SetBoolSafe("isAttackRunning", false);
                SetBoolSafe("isAttackAttacking", false);
            }
        }

        private void ResetAttackParameters()
        {
            string[] directions =
            {
                "North",
                "South",
                "East",
                "West",
                "NorthEast",
                "NorthWest",
                "SouthEast",
                "SouthWest"
            };

            foreach (string direction in directions)
            {
                SetBoolSafe("AttackAttack" + direction, false);
                SetBoolSafe("Attack2" + direction, false);
                SetBoolSafe("AttackRun" + direction, false);
            }

            SetBoolSafe("isAttackAttacking", false);
            SetBoolSafe("isAttackRunning", false);
        }

        private void ResetAllGunFireBools()
        {
            string[] directions =
            {
                "North",
                "South",
                "East",
                "West",
                "NorthEast",
                "NorthWest",
                "SouthEast",
                "SouthWest"
            };

            foreach (string direction in directions)
            {
                SetMuzzleBoolSafe("Gunfire" + direction, false);
            }
        }

        public void TriggerTakeDamageAnimation()
        {
            if (!gameObject.activeInHierarchy)
                return;

            SpawnEffect();

            SetBoolSafe("isTakeDamage", true);

            string directionName = currentDirection.Substring(2);
            SetBoolSafe("TakeDamage" + directionName, true);

            StartCoroutine(ResetTakeDamageParameters());
        }

        private IEnumerator ResetTakeDamageParameters()
        {
            yield return new WaitForSeconds(0.5f);

            string[] directions =
            {
                "North",
                "South",
                "East",
                "West",
                "NorthEast",
                "NorthWest",
                "SouthEast",
                "SouthWest"
            };

            SetBoolSafe("isTakeDamage", false);

            foreach (string direction in directions)
            {
                SetBoolSafe("TakeDamage" + direction, false);
            }
        }

        private void SpawnEffect()
        {
            List<GameObject> prefabsToUse = isRadiated ? radiatedPrefabs : bloodPrefabs;

            if (prefabsToUse == null || prefabsToUse.Count == 0)
                return;

            GameObject selectedPrefab = prefabsToUse[Random.Range(0, prefabsToUse.Count)];

            if (selectedPrefab == null)
                return;

            GameObject effectInstance = Instantiate(selectedPrefab, transform.position, Quaternion.identity);
            StartCoroutine(UpdateSpriteOrder(effectInstance));
        }

        private IEnumerator UpdateSpriteOrder(GameObject effectInstance)
        {
            if (effectInstance == null)
                yield break;

            yield return new WaitForSeconds(0.5f);

            SpriteRenderer spriteRenderer = effectInstance.GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = 40;
        }

        public void TriggerDie()
        {
            if (!gameObject.activeInHierarchy)
                return;

            if (isDying)
                return;

            isDying = true;

            ResetAttackParameters();
            ResetAllMovementBools();
            ResetAllGunFireBools();

            SetBoolSafe("isDie", true);

            string directionName = currentDirection.Substring(2);
            SetBoolSafe("die" + directionName, true);

            if (muzzleFlashRenderer != null)
                muzzleFlashRenderer.sortingOrder = -1;
        }
    }
}