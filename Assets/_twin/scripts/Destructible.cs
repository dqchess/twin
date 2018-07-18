using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Actor))]
public class Destructible : MonoBehaviour
{
    public Vector2 HitPointRange = new Vector2(10.0f, 10.0f);
    public float HitPoints
    {
        get;
        private set;
    }
    public TempEffect DestroyEffect;

    public bool LurchOnImpact = true;
    public bool LurchFreelyOnDestruction = true;
    public Vector2 LurchFreelyDurationRange = new Vector2(0.5f, 1.5f);
    public float LurchRecoveryRate = 0.1f;
    public float LurchRotationStrength = 90.0f;
    public float LurchTranslationStrength = 1.0f;
    public Transform LurchTransform;
    
    public AudioClip[] FatalBlowSound;
    public GameObject[] FatalBlowEffect;

    public void Damage(float Amount, Vector3 ImpactDirection)
    {
        HitPoints -= Amount;
        if (HitPoints <= 0.0f && !explosionImminent)
        {
            if (LurchOnImpact && LurchFreelyOnDestruction)
            {
                GetComponent<AudioSource>().PlayOneShot(RandomUtils.Pick(FatalBlowSound));
                var effect = Instantiate(RandomUtils.Pick(FatalBlowEffect), LurchTransform);
                effect.transform.localPosition = Vector3.zero;
                effect.transform.localRotation = Quaternion.identity;
                Invoke("Destroy", Random.Range(LurchFreelyDurationRange.x, LurchFreelyDurationRange.y));
            }
            else
            {
                Destroy();
            }
            explosionImminent = true;
        }

        if (LurchOnImpact)
        {
            Debug.Assert(LurchTransform != null, "need to set LurchTransform when LurchOnImpact is true for gameObject " + gameObject.name);
            Vector3 lurchAxis = Vector3.Cross(ImpactDirection, Vector3.up).normalized;

            var lurch = new Lurch();
            lurch.direction = ImpactDirection;
            lurch.axis = lurchAxis;
            lurch.amount = 1.0f;
            lurches.Add(lurch);
        }
    }

    void Destroy()
    {
        var effect = Instantiate(DestroyEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    void Start()
    {
        HitPoints = Random.Range(HitPointRange.x, HitPointRange.y);
    }

    void Update()
    {
        if (LurchOnImpact)
        {
            foreach (var lurch in lurches)
            {
                LurchTransform.Rotate(lurch.axis, -lurch.amount * LurchRotationStrength * Time.deltaTime, Space.World);
                transform.Translate(lurch.direction * lurch.amount * LurchTranslationStrength * Time.deltaTime, Space.World);

                if (HitPoints > 0.0f || !LurchFreelyOnDestruction)
                    lurch.amount *= 0.9f;
            }
            lurches.RemoveAll(lurch => lurch.amount < 0.01f);

            if (HitPoints > 0.0f || !LurchFreelyOnDestruction)
                LurchTransform.localRotation = Quaternion.Slerp(LurchTransform.localRotation, Quaternion.identity, 0.1f);
        }
    }

    bool explosionImminent = false;
    class Lurch
    {
        public Vector3 direction;
        public Vector3 axis;
        public float amount;
    };
    List<Lurch> lurches = new List<Lurch>();
}
