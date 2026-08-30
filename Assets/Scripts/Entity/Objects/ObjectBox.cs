using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ObjectBox : MonoBehaviour, IDamageable, IKnockbackable, IGrabbable, IEmitableTarget
{
    private Rigidbody2D rb;

    private Transform target { get; set; }
    private PowerStackUIController powerStackUIController { get; set; }
    public ObjectBoxCollisionDetector objectBoxCollisionDetector;
    public EmitManager emitManager { get; set; }
    public AudioSource myAudioSource;

    [SerializeField] protected ObjectData data;

    public int dmgType { get; set; }

    public int currentPower { get; private set; }
    public float grabForce { get; set; } = 50f;
    public bool transitFlipPower { get; set; }
    public Vector2 FacingDir { get; }       //사용하지 않음

    public Vector2 currentKnockBackForce { get; set; }

    [Header("넉백 관련")]
    public float knockbackRate { get; set; }
    public float strongKnockbackRate { get; set; }
    public float knockbackTime { get; set; }
    public float HitRecoveryTime { get; set; }

    [Header("파워 관련")]
    public float powerResetTime { get; set; }
    public int powerStackWeight { get; set; }

    private float powerTimer { get; set; }

    [Header("사운드 관련")]
    public AudioClip sweepHitSound { get; set; }
    public AudioClip grabHitSound { get; set; }

    private Coroutine knockbackRoutine;
    private Coroutine grabRoutine;

    private void Awake()
    {
        target = Player.InstanceTransform;
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        powerStackUIController = GetComponentInChildren<PowerStackUIController>();
        objectBoxCollisionDetector = GetComponent<ObjectBoxCollisionDetector>();

        if (data == null)
        {
            Debug.LogError($"{gameObject.name} is missing Enemy Data ScriptableObject!!");
        }
        else
        {
            knockbackRate = data.knockbackRate;
            strongKnockbackRate = data.strongKnockbackRate;
            knockbackTime = data.knockbackTime;
            HitRecoveryTime = data.HitRecoveryTime;
            powerResetTime = data.powerResetTime;
            powerStackWeight = data.powerStackWeight;
            sweepHitSound = data.sweepHitSound;
            grabHitSound = data.grabHitSound;
        }
    }

    private void Start()
    {
        emitManager = EmitManager.Instance;
    }

    private void Update()
    {
        CalculatePowerTimer();
    }

    //IDamageable 구현
    public void ReceiveAttack(AttackInfo Info)
    {
        stopKnockbackRoutine();

        powerTimer = 0;
        currentKnockBackForce = Info.knockback;

        if (!Info.attackIsGrab)
        {
            if (isGrabbed())
            {
                stopGrapRoutine();
                transitFlipPower = true;
                FlipPower(Info.knockback.x);
            }
            myAudioSource.PlayOneShot(sweepHitSound);
            ApplyKnockback();
            if (currentPower * Info.power < 0) ResetPower();
            CalculatePower(Info);
        }
        else myAudioSource.PlayOneShot(grabHitSound);

        transitFlipPower = false;
    }

    //power관련
    public void SetPower(int power)
    {
        currentPower = power;
        powerStackUIController.SetStack(currentPower);
    }

    public void CalculatePower(AttackInfo attack)
    {
        if (currentPower < 5 && currentPower > -5 && currentPower * attack.power >= 0)
        {
            SetPower(currentPower + attack.power * powerStackWeight);
        }
    }

    public void ResetPower()
    {
        SetPower(0);
    }

    public void FlipPower(float knockBackForce)
    {
        if (knockBackForce * currentPower < 0)
            SetPower(-currentPower);
    }

    public void CalculatePowerTimer()
    {
        powerTimer += Time.deltaTime;
        if (powerTimer > powerResetTime && !emitManager.isEmitting)
            ResetPower();
    }


    //IKnockbackable 구현
    public void StartOverlapDetect()
    {
        objectBoxCollisionDetector.startDetect();
    }

    public void StopOverlapDetect()
    {
        objectBoxCollisionDetector.stopDetect();
    }



    //IGrabbable 구현
    public void ReceiveGrab()
    {
        float offsetX = (target.position.x > transform.position.x) ? -0.5f : 0.5f;
        Vector2 targetPosition = new Vector2(target.position.x + offsetX, transform.position.y);

        grabRoutine = StartCoroutine(GrabRoutine(targetPosition));
    }

    //IEmitableTarget 구현
    public void SetFacingDirection(Vector2 dir)
    {

    }

    public void Flip()
    {

    }

    public void RootMovement()
    {
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void UnrootMovement()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
    }


    // 고유로직
    private void ApplyKnockback()
    {
        if (transitFlipPower)
        {
            rb.AddForce(currentKnockBackForce * strongKnockbackRate, ForceMode2D.Impulse);

            StartOverlapDetect();
        }
        else
        {
            rb.AddForce(currentKnockBackForce * knockbackRate, ForceMode2D.Impulse);
        }

        knockbackRoutine = StartCoroutine(KnockbackRoutine());
    }

    IEnumerator KnockbackRoutine()
    {
        yield return new WaitForSeconds(knockbackTime);

        rb.linearVelocity = Vector2.zero;
        StopOverlapDetect();

        yield return new WaitForSeconds(HitRecoveryTime);

        knockbackRoutine = null;
    }

    private void stopKnockbackRoutine()
    {
        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
            knockbackRoutine = null;
        }
    }

    IEnumerator GrabRoutine(Vector2 targetPosition)
    {
        rb.linearVelocity = Vector2.zero;

        while (Vector2.Distance(rb.position, targetPosition) > 0.1f)
        {
            Vector2 nextPosition = Vector2.MoveTowards(
                rb.position,
                targetPosition,
                grabForce * Time.fixedDeltaTime
            );

            rb.MovePosition(nextPosition);
            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(targetPosition);
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(HitRecoveryTime);

        grabRoutine = null;
    }

    private void stopGrapRoutine()
    {
        if (grabRoutine != null)
        {
            StopCoroutine(grabRoutine);
            grabRoutine = null;
        }
    }

    private bool isGrabbed() => grabRoutine != null;
}
