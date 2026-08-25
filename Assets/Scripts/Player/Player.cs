using UnityEngine;

[RequireComponent(typeof(PlayerState))]
// [RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerAnimation))]
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    public static Transform InstanceTransform { get; private set; }

    public Rigidbody2D Rb { get; private set; }
    public Animator animator { get; private set; }
    public SpriteRenderer spriteRenderer { get; private set; }

    public static PlayerState State { get; private set; }
    public static PlayerInputHandler InputHandler { get; private set; }
    //public static PlayerMovement Movement { get; private set; }
    public static TempPlayerMovement TempMovement { get; private set; }
    public static PlayerCombat Combat { get; private set; }
    public static PlayerAnimation Animation { get; private set; }
    public static DashEffectController DashEffect { get; private set; }
    public static GrapEffect GrapEffect { get; private set; }
    public static PlayerHealth PlayerHealth { get; private set; }
    public static PlayerUIController UIController { get; private set; }

    public AudioClip walkSound;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip dashSound;
    public AudioClip sweepSound;
    public AudioClip grabThrowSound1;
    public AudioClip grabThrowSound2;
    public AudioClip grabRetreiveSound;
    public AudioClip canEmitSound;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InstanceTransform = this.transform;

        Rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        State = GetComponent<PlayerState>();
        InputHandler = GetComponent<PlayerInputHandler>();
        //Movement = GetComponent<PlayerMovement>();
        TempMovement = GetComponent<TempPlayerMovement>();
        Combat = GetComponent<PlayerCombat>();
        Animation = GetComponent<PlayerAnimation>();
        DashEffect = GetComponent<DashEffectController>();
        GrapEffect = GetComponent<GrapEffect>();
        PlayerHealth = GetComponent<PlayerHealth>();
        UIController = GetComponent<PlayerUIController>();
    }
}
