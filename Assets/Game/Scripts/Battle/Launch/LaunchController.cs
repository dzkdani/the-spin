using System;
using UnityEngine;

public sealed class LaunchController : MonoBehaviour
{
    [SerializeField] private LaunchTuning tuning;
    [SerializeField] private LaunchOverlay overlay;
    [SerializeField] private Camera targetCamera;

    private BeyController player;
    private ArenaBounds arena;
    private Action onResolved;
    private bool active;
    private bool pulling;
    private float pullStartY;
    private float power;

    public bool Enabled => tuning != null && tuning.enabled;

    public bool IsLaunching => active;

    private void Awake()
    {
        if (overlay != null)
            overlay.Hide();
    }

    public void Begin(
        BeyController newPlayer,
        ArenaBounds newArena,
        Action onResolved)
    {
        player = newPlayer;
        arena = newArena;
        this.onResolved = onResolved;

        if (!Enabled)
        {
            this.onResolved = null;
            onResolved?.Invoke();
            return;
        }

        active = true;
        pulling = false;
        power = 0f;

        if (player != null)
            player.gameObject.SetActive(false);

        if (overlay != null)
            overlay.Show();

        if (player != null && player.Data != null)
            BattleEvents.RaiseLaunchStarted(player.Data);
    }

    public void ApplyEnemyLaunch(BeyData enemy)
    {
        if (enemy == null || tuning == null)
            return;

        float enemyPower = LaunchMath.EnemyPower(
            tuning.enemyPower,
            tuning.enemyVariance);

        enemy.Launch(enemyPower, tuning.minStartFactor);

        BattleEvents.RaiseLaunchResolved(enemy, enemyPower);
    }

    private void Update()
    {
        if (!active)
            return;

        if (!pulling)
        {
            if (Input.GetMouseButtonDown(0))
            {
                pulling = true;
                pullStartY = MouseWorld().y;
                SetPower(0f);
            }

            return;
        }

        if (Input.GetMouseButton(0))
        {
            SetPower(LaunchMath.Power(
                pullStartY - MouseWorld().y,
                tuning.playerPullMax));

            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            pulling = false;
            Resolve();
        }
    }

    private void Resolve()
    {
        active = false;

        if (player != null)
        {
            player.gameObject.SetActive(true);

            if (arena != null && player.Data != null)
            {
                Vector2 spot = arena.Center + new Vector2(
                    0f,
                    arena.Radii.y * tuning.launchSpotFactor);

                player.transform.position = spot;
                player.Data.Stats.RemoveBySourcePrefix("element:");
                player.Data.Launch(power, tuning.minStartFactor);

                Vector2 direction = (arena.Center - spot).normalized;

                if (player.Physics != null)
                    player.Physics.Launch(
                        direction,
                        tuning.launchImpulse * power);

                BattleEvents.RaiseLaunchResolved(player.Data, power);

                if (power >= tuning.perfectThreshold &&
                    player.Combat != null)
                {
                    player.Combat.AddChargedHit(
                        tuning.perfectChargedHits);
                }
            }
        }

        if (overlay != null)
            overlay.Hide();

        Action callback = onResolved;
        onResolved = null;
        callback?.Invoke();
    }

    private void SetPower(float value)
    {
        power = Mathf.Clamp01(value);

        if (overlay != null)
            overlay.SetPower(power);
    }

    private Vector3 MouseWorld()
    {
        Camera cam = targetCamera != null ? targetCamera : Camera.main;

        return cam != null
            ? cam.ScreenToWorldPoint(Input.mousePosition)
            : Vector3.zero;
    }
}
