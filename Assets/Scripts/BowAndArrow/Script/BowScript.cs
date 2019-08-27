using UnityEngine;
using System.Collections;
using System;
using System.CodeDom;
using System.Collections.Generic;
namespace DigitalRuby.BowAndArrow
{
    [RequireComponent(typeof(AudioSource))]

    public class BowScript : MonoBehaviour
    {
        GameObject enemy;
        [Header("Bow Structure")]
        public GameObject BowShaft;

        public GameObject BowString;

        public GameObject TopAnchor;

        public GameObject BottomAnchor;

        public Collider2D DrawStartArea;

        public Collider2D DrawTotalArea;

        public GameObject MinDrawAnchor;

        public GameObject MaxDrawAnchor;

        public GameObject Enemy;

       
        public GameObject Arrow;

        public float MaxRotationAngleRadians = 0.6f;

        public float Cooldown = 0.5f;

        public bool AllowFizzling = true;

        public float FizzleSpeed = 10.0f;

        public float FireSpeed = 80.0f;

        [Header("Bow Sounds")]
        public AudioClip[] KnockClips;

        public AudioClip[] DrawClips;

        public AudioClip[] FireClips;

        private LineRenderer bowStringLineRenderer1;
        private LineRenderer bowStringLineRenderer2;
        private AudioSource audioSource;
        ParticleSystemRingBufferMode firemode;
        private bool drawingBow;
        private GameObject currentArrow;
        private float cooldownTimer;
        private float startAngle;

        private float DifferenceBetweenAngles(float angle1, float angle2)
        {
            float angle = angle1 - angle2;
            return Mathf.Atan2(Mathf.Sin(angle), Mathf.Cos(angle));
        }

        private void RenderBowString(Vector3 arrowPos)
        {
            Vector3 startPoint = TopAnchor.transform.position;
            Vector3 endPoint = BottomAnchor.transform.position;

            if (drawingBow)
            {
                bowStringLineRenderer2.gameObject.SetActive(true);
                bowStringLineRenderer1.SetPosition(0, startPoint);
                bowStringLineRenderer1.SetPosition(1, arrowPos);
                bowStringLineRenderer2.SetPosition(0, arrowPos);
                bowStringLineRenderer2.SetPosition(1, endPoint);
            }
            else
            {
                bowStringLineRenderer2.gameObject.SetActive(false);
                bowStringLineRenderer1.SetPosition(0, startPoint);
                bowStringLineRenderer1.SetPosition(1, endPoint);
            }
        }

        private void PlayRandomSound(AudioClip[] clips)
        {
            if (clips != null && clips.Length != 0)
            {
                audioSource.PlayOneShot(clips[UnityEngine.Random.Range(0, clips.Length)]);
            }
        }

        private Vector3 GetArrowPositionForDraw()
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0.0f;

            Vector3 dirFacing = (MaxDrawAnchor.transform.position - MinDrawAnchor.transform.position).normalized;
            float angleFacing = Mathf.Atan2(MaxDrawAnchor.transform.position.y - MinDrawAnchor.transform.position.y, MaxDrawAnchor.transform.position.x - MinDrawAnchor.transform.position.x);
            float angleClicking = Mathf.Atan2(worldPos.y - MinDrawAnchor.transform.position.y, worldPos.x - MinDrawAnchor.transform.position.x);
            float angleDiff = Mathf.Abs(DifferenceBetweenAngles(angleFacing, angleClicking));
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            
            if (angleDiff >= (Mathf.PI * 0.5f))
            {
                worldPos = MinDrawAnchor.transform.position + (dirFacing * 0.01f);
            }
            else if (!AllowFizzling)
            {
                float maxDistance = Vector3.Distance(MaxDrawAnchor.transform.position, MinDrawAnchor.transform.position);
                float actualDistance = Vector3.Distance(worldPos, MinDrawAnchor.transform.position);
                if (actualDistance > maxDistance)
                {
                    Vector3 dirClicking = (worldPos - MinDrawAnchor.transform.position).normalized;
                    worldPos = MinDrawAnchor.transform.position + (dirClicking * maxDistance);
                }
            }

            return worldPos;
        }

        private void vector(float v1, float v2, float v3)
        {
            throw new NotImplementedException();
        }

        private void BeginBowDraw()
        {
           
            PlayRandomSound(KnockClips);

            currentArrow = GameObject.Instantiate(Arrow);
            currentArrow.transform.rotation = BowShaft.transform.rotation;
            currentArrow.SetActive(true);

            Vector3 pos = GetArrowPositionForDraw();
            currentArrow.transform.position = pos;
            drawingBow = true;
        }

        private void ContinueBowDraw()
        {
            
            if (!AllowFizzling || DrawTotalArea.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition)))
            {
                Vector3 pos = GetArrowPositionForDraw();
                if (MaxRotationAngleRadians > 0.0f)
                {
                    float angle = Mathf.Atan2(pos.y - BowShaft.transform.position.y, pos.x - BowShaft.transform.position.x);
                    float angleDiff = Mathf.Abs(DifferenceBetweenAngles(angle, startAngle));
                    if (angleDiff <= MaxRotationAngleRadians)
                    {
                        BowShaft.transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
                    }
                }

                currentArrow.GetComponent<Rigidbody2D>().MovePosition(pos);
                currentArrow.GetComponent<Rigidbody2D>().MoveRotation(BowShaft.transform.rotation.eulerAngles.z);
                RenderBowString(pos);
            }
            else
            {
                FireArrow(true);
            }
        }

        private void FireArrow(bool fizzle)
        {
            PlayRandomSound(FireClips);
            float speed;
            cooldownTimer = Cooldown;

            if (fizzle)
            {
                speed = UnityEngine.Random.Range(-FizzleSpeed, FizzleSpeed);
            }
            else
            {
                Vector3 pos = GetArrowPositionForDraw();
                float baseDistance = Vector3.Distance(MaxDrawAnchor.transform.position, MinDrawAnchor.transform.position);
                float clickDistance = Vector3.Distance(MinDrawAnchor.transform.position, pos);
                float speedBoost = clickDistance / baseDistance;
                float clickDone = clickDistance * baseDistance;
                float angleFromCenter = Mathf.Rad2Deg * Mathf.Atan2(pos.y - MinDrawAnchor.transform.position.y, pos.x - MinDrawAnchor.transform.position.x);
                angleFromCenter -= BowShaft.transform.rotation.eulerAngles.z;
                angleFromCenter *= Mathf.Deg2Rad;
                angleFromCenter = Mathf.Abs(DifferenceBetweenAngles(0.0f, angleFromCenter));
                angleFromCenter = Mathf.Abs(NewMethod(angleFromCenter));
                speedBoost = Mathf.Clamp(speedBoost - (angleFromCenter * 2.0f), 0.0f, 1.2f);


                speed = FireSpeed * speedBoost;
            }

            currentArrow.GetComponent<Rigidbody2D>().isKinematic = false;
            currentArrow.transform.GetChild(0).gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
            currentArrow.GetComponent<Rigidbody2D>().velocity =
                currentArrow.transform.GetChild(0).gameObject.GetComponent<Rigidbody2D>().velocity = BowShaft.transform.rotation * new Vector2(-speed, 0.0f);

            drawingBow = false;
            RenderBowString(Vector3.zero);
        }

        private float NewMethod(float angleFromCenter)
        { float speed = 20;
            return diffrenceBetweenAngles(1f * Time.deltaTime, angleFromCenter*speed);
        }

        private float diffrenceBetweenAngles(float v, float angleFromCenter)
        {
            throw new NotImplementedException();
        }

        private void Start()
        {
            bowStringLineRenderer1 = BowString.transform.GetChild(0).GetComponent<LineRenderer>();
            bowStringLineRenderer2 = BowString.transform.GetChild(1).GetComponent<LineRenderer>();
            audioSource = GetComponent<AudioSource>();
            RenderBowString(Vector3.zero);
            startAngle = BowShaft.transform.eulerAngles.z;
        }

        private void Update()
        {
            cooldownTimer -= Time.deltaTime;

            
            if (Input.GetMouseButtonDown(0))
            {
                if (cooldownTimer <= 0.0f)
                {
                    Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    if (DrawStartArea.OverlapPoint(worldPos))
                    {
                        BeginBowDraw();
                        _ = DrawStartArea.gameObject.tag == "done";
                    }
                }
            }
            else if (Input.GetMouseButton(0))
            {
                if (drawingBow)
                {
                    ContinueBowDraw();
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (drawingBow)
                {
                    FireArrow(false);
                }
            }
        }
        protected void OnEnterCollsion2d(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("enemy")) {
                Destroy(enemy, 2f);
            }
        }

        protected void OnTriggerEnter2D(Collider2D collision)
        {
#pragma warning disable CS0168 // Variable is declared but never used
            GameObject collectStars;
            GameObject gameObject1;
#pragma warning restore CS0168 // Variable is declared but never used
            

        }
        
    }
    }


    
