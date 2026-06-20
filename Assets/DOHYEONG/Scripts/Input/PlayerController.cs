using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

namespace SmallScaleInc.TopDownPixelCharactersPack1
{
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        public AnimationController animationController;
        private Rigidbody2D rb;
        private CircleCollider2D circleCollider;
        private SpriteRenderer spriteRenderer;

        [Header("Movement")]
        public float normalSpeed = 1.0f;
        public float shootingSpeed = 0.5f;

        private float currentSpeed;
        private Vector2 movementDirection;
        private bool isOnStairs;
        private float lastAngle;
        private bool isRunning;

        [Header("Shooting")]
        public float bulletDamage = 1f;
        public float bulletsPerSecond = 3f;
        public float maxShootDistance = 10f;
        public GameObject bulletLinePrefab;
        public float lineDisplayTime = 0.05f;

        private float nextFireTime;

        [Header("Health")]
        public int maxHealth = 100;
        public int currentHealth;
        public bool isDead;
        public Slider healthSlider;
        public GameObject gameOver;

        [Header("Kill Count")]
        public int zombieKillCount;
        public TextMeshProUGUI killCountText;

        private Coroutine pulseCoroutine;
        private Vector3 originalKillTextScale;
        private Color originalColor;

        private static bool IsKeyPressed(Key key)
        {
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard[key].isPressed;
        }

        private static bool WasAttackMousePressedThisFrame()
        {
            Mouse mouse = Mouse.current;
            return mouse != null && mouse.leftButton.wasPressedThisFrame;
        }

        private static bool IsAttackMousePressed()
        {
            Mouse mouse = Mouse.current;
            return mouse != null && mouse.leftButton.isPressed;
        }

        private static Vector2 GetMouseScreenPosition()
        {
            Mouse mouse = Mouse.current;
            return mouse != null ? mouse.position.ReadValue() : Vector2.zero;
        }

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            circleCollider = GetComponent<CircleCollider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (animationController == null)
                animationController = GetComponent<AnimationController>();

            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;

            currentSpeed = normalSpeed;

            currentHealth = maxHealth;

            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
            }

            if (gameOver != null)
                gameOver.SetActive(false);

            if (killCountText != null)
            {
                originalKillTextScale = killCountText.transform.localScale;
                killCountText.text = zombieKillCount.ToString();
            }
        }

        private void Update()
        {
            if (isDead)
                return;

            UpdateMouseDirection();
            HandleMovementInput();
            HandleShooting();

            bool isMoving =
                IsKeyPressed(Key.W) ||
                IsKeyPressed(Key.A) ||
                IsKeyPressed(Key.S) ||
                IsKeyPressed(Key.D);

            isRunning = isMoving;
        }

        private void FixedUpdate()
        {
            if (isDead)
                return;

            if (rb == null)
                return;

            if (movementDirection != Vector2.zero)
            {
                rb.MovePosition(rb.position + movementDirection * currentSpeed * Time.fixedDeltaTime);
            }
        }

        private void UpdateMouseDirection()
        {
            if (Camera.main == null)
                return;

            Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(GetMouseScreenPosition());
            Vector2 directionToMouse = (mouseWorldPosition - (Vector2)transform.position).normalized;

            if (directionToMouse == Vector2.zero)
                return;

            float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;
            lastAngle = SnapAngleToEightDirections(angle);

            movementDirection = new Vector2(
                Mathf.Cos(lastAngle * Mathf.Deg2Rad),
                Mathf.Sin(lastAngle * Mathf.Deg2Rad)
            );
        }

        private void HandleMovementInput()
        {
            Vector2 input = Vector2.zero;

            if (IsKeyPressed(Key.W))
                input.y += 1f;

            if (IsKeyPressed(Key.S))
                input.y -= 1f;

            if (IsKeyPressed(Key.A))
                input.x -= 1f;

            if (IsKeyPressed(Key.D))
                input.x += 1f;

            if (input.sqrMagnitude > 1f)
                input.Normalize();

            movementDirection = input;
        }

        private void HandleShooting()
        {
            if (IsAttackMousePressed())
            {
                currentSpeed = shootingSpeed;

                float timeBetweenShots = 1f / bulletsPerSecond;

                if (Time.time >= nextFireTime)
                {
                    nextFireTime = Time.time + timeBetweenShots;
                    ShootRay();
                }
            }
            else
            {
                currentSpeed = normalSpeed;
                nextFireTime = 0f;
            }
        }

        private void ShootRay()
        {
            if (Camera.main == null)
                return;

            Vector2 playerPos = transform.position;
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(GetMouseScreenPosition());
            Vector2 direction = (mousePos - playerPos).normalized;

            if (direction == Vector2.zero)
                return;

            Vector2 rayOrigin = playerPos;
            bool shouldContinue = true;

            List<Vector2> hitPoints = new List<Vector2>();
            hitPoints.Add(rayOrigin);

            while (shouldContinue)
            {
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, maxShootDistance);

                if (hit.collider != null)
                {
                    hitPoints.Add(hit.point);

                    ZombieAI zombie = hit.collider.GetComponent<ZombieAI>();

                    if (zombie != null)
                    {
                        zombie.TakeDamage(Mathf.RoundToInt(bulletDamage));

                        // 50% 확률로 관통
                        if (Random.value > 0.5f)
                        {
                            rayOrigin = hit.point + direction * 0.1f;
                        }
                        else
                        {
                            shouldContinue = false;
                        }
                    }
                    else
                    {
                        shouldContinue = false;
                    }
                }
                else
                {
                    hitPoints.Add(rayOrigin + direction * maxShootDistance);
                    shouldContinue = false;
                }
            }

            if (bulletLinePrefab != null)
                StartCoroutine(ShowShotLine(hitPoints));
        }

        private IEnumerator ShowShotLine(List<Vector2> hitPoints)
        {
            GameObject lineObj = Instantiate(bulletLinePrefab, Vector3.zero, Quaternion.identity);
            LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();

            if (lineRenderer != null)
            {
                lineRenderer.positionCount = hitPoints.Count;

                for (int i = 0; i < hitPoints.Count; i++)
                {
                    lineRenderer.SetPosition(i, hitPoints[i]);
                }
            }

            yield return new WaitForSeconds(lineDisplayTime);

            if (lineObj != null)
                Destroy(lineObj);
        }

        public void TakeDamage(int damageAmount)
        {
            if (isDead)
                return;

            currentHealth -= damageAmount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            if (healthSlider != null)
                healthSlider.value = currentHealth;

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                if (animationController != null)
                    animationController.TriggerTakeDamageAnimation();
            }
        }

        private void Die()
        {
            if (isDead)
                return;

            isDead = true;
            movementDirection = Vector2.zero;

            if (circleCollider != null)
                circleCollider.enabled = false;

            if (animationController != null)
                animationController.TriggerDie();

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Static;
            }

            if (gameOver != null)
                gameOver.SetActive(true);

            StartCoroutine(RestartSceneAfterDelay(3f));
        }

        private IEnumerator RestartSceneAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void IncrementZombieKillCount()
        {
            zombieKillCount++;

            if (killCountText != null)
            {
                killCountText.text = zombieKillCount.ToString();

                if (pulseCoroutine != null)
                    StopCoroutine(pulseCoroutine);

                pulseCoroutine = StartCoroutine(PulseTextEffect(killCountText));
            }
        }

        private IEnumerator PulseTextEffect(TextMeshProUGUI text)
        {
            float duration = 0.2f;
            float maxScaleFactor = 1.5f;
            float time = 0f;

            Vector3 maxScale = originalKillTextScale * maxScaleFactor;

            while (time < duration / 2f)
            {
                text.transform.localScale = Vector3.Lerp(originalKillTextScale, maxScale, time / (duration / 2f));
                time += Time.deltaTime;
                yield return null;
            }

            text.transform.localScale = maxScale;
            time = 0f;

            while (time < duration / 2f)
            {
                text.transform.localScale = Vector3.Lerp(maxScale, originalKillTextScale, time / (duration / 2f));
                time += Time.deltaTime;
                yield return null;
            }

            text.transform.localScale = originalKillTextScale;
            pulseCoroutine = null;
        }

        public void FlashGreen()
        {
            if (spriteRenderer != null)
                StartCoroutine(FlashEffect());
        }

        private IEnumerator FlashEffect()
        {
            spriteRenderer.color = Color.green;
            yield return new WaitForSeconds(0.7f);
            spriteRenderer.color = originalColor;
        }

        private float SnapAngleToEightDirections(float angle)
        {
            angle = (angle + 360f) % 360f;

            if (isOnStairs)
            {
                if (angle < 30 || angle >= 330)
                    return 0;
                else if (angle >= 30 && angle < 75)
                    return 60;
                else if (angle >= 75 && angle < 105)
                    return 90;
                else if (angle >= 105 && angle < 150)
                    return 120;
                else if (angle >= 150 && angle < 210)
                    return 180;
                else if (angle >= 210 && angle < 255)
                    return 240;
                else if (angle >= 255 && angle < 285)
                    return 270;
                else if (angle >= 285 && angle < 330)
                    return 300;
            }
            else
            {
                if (angle < 15 || angle >= 345)
                    return 0;
                else if (angle >= 15 && angle < 75)
                    return 30;
                else if (angle >= 75 && angle < 105)
                    return 90;
                else if (angle >= 105 && angle < 165)
                    return 150;
                else if (angle >= 165 && angle < 195)
                    return 180;
                else if (angle >= 195 && angle < 255)
                    return 210;
                else if (angle >= 255 && angle < 285)
                    return 270;
                else if (angle >= 285 && angle < 345)
                    return 330;
            }

            return 0;
        }

        private float GetPerpendicularAngle(float angle, bool isLeft)
        {
            float perpendicularAngle = isLeft ? angle - 90f : angle + 90f;
            perpendicularAngle = (perpendicularAngle + 360f) % 360f;

            return SnapAngleToEightDirections(perpendicularAngle);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Stairs"))
                isOnStairs = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Stairs"))
                isOnStairs = false;
        }
    }
}