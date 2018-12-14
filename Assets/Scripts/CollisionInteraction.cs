using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionInteraction : MonoBehaviour {

    public float mass = 1.0f;
    static float v_elasticity = 1.0f;
    public float speed = 5.0f;
    public bool randDirection = true;
    public Vector3 direction = Vector3.forward;
    public float resistancePP = 10.0f;
    public float rotationResistancePP = 0.1f;
    public float givenNumber = 0.5f;

    Vector3 v;
    bool hitOnce = false;

    float radius;
    Vector3 axis;
    float angleVelocity;

	// Use this for initialization
	void Start () {
        if (randDirection)
            direction = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
        direction = direction.normalized;
        v_elasticity = Mathf.Clamp(v_elasticity, 0.0f, 1.0f);
        v = speed * direction;
        givenNumber = Mathf.Clamp(givenNumber, 0.0f, 1.0f);
        if (gameObject.name.Contains("Sphere"))
            radius = GetComponent<SphereCollider>().radius;
        else if (gameObject.name.Contains("Cube"))
            radius = GetComponent<BoxCollider>().extents.x * 0.5f;
        axis = Vector3.zero;
        angleVelocity = 0.0f;
    }
	
	// Update is called once per frame
	void Update () {
        transform.position += v * Time.deltaTime;
        if (axis != Vector3.zero)
        {
            transform.Rotate(axis, angleVelocity * Time.deltaTime);
            angleVelocity -= angleVelocity * (rotationResistancePP / 100.0f);
            if (angleVelocity < 1.0f)
                angleVelocity = 0.0f;
        }
        if (Vector3.Distance(Vector3.zero, v) < 1.0f)
            v = Vector3.zero;
	}

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Plane")
        {
            // Normalize normal vector of collision
            Vector3 normalized = Vector3.Normalize(collision.contacts[0].normal);
            // Get projection vector
            float projection = Vector3.Dot(-v, normalized);
            Vector3 vp = projection * normalized;

            // Get changed velocity
            v = 2 * vp + v;
            SetVelocity(v - v * (resistancePP / 100.0f));
        }
        else if (collision.gameObject.tag == "Sphere")
        {
            if (!hitOnce)
            {
                CollisionInteraction obj = collision.gameObject.GetComponent<CollisionInteraction>();
                Vector3 ke1 = direction * speed * mass;
                Vector3 ke2 = obj.direction * obj.speed * obj.mass;
                Vector3 eq1 = ke1 + ke2;

                Vector3 eq2 = -v_elasticity * (direction * speed - obj.direction * obj.speed);

                Vector3 v1f = (eq1 + obj.mass * eq2) / (mass + obj.mass);
                Vector3 v2f = (v1f - eq2);

                Vector3 cp = collision.contacts[0].point;
                Vector3 sum = Vector3.zero;
                foreach (ContactPoint contact in collision.contacts)
                {
                    sum += contact.point;
                }
                // Calculate Center of Contact Points
                if (Vector3.Distance(Vector3.zero, sum) > Mathf.Epsilon)
                    cp = sum / collision.contacts.Length;

                // Calculate distance btw Contact point n position
                radius = Vector3.Distance(cp, transform.position);
                obj.radius = Vector3.Distance(cp, obj.transform.position);

                // Calculate axis of rotation
                axis = Vector3.Cross(direction, cp - transform.position);
                obj.axis = Vector3.Cross(obj.direction,cp - obj.transform.position);

                float angle = Vector3.Angle(direction, cp - obj.transform.position);//obj.direction);
                if ((angle > 178.0f && angle < 182.0f) || (angle > -2.0f && angle < 2.0f))
                {
                    obj.angleVelocity = 0.0f;
                }
                else
                {
                    // F = ma, a = v / t
                    Vector3 Newton = mass * v / Time.deltaTime;
                    // Toruqe = F * r
                    float torque = Vector3.Distance(Vector3.zero, Newton) * obj.radius;
                    // Inertia = givenNumber(Decide by shape) * mass * radius * radius
                    float I = obj.givenNumber * obj.mass * obj.radius * obj.radius;
                    // a = torque / I
                    float alpha = torque / I;
                    obj.angleVelocity = alpha * Time.deltaTime;
                }

                angle = Vector3.Angle(obj.direction, cp - transform.position);
                if ((angle > 178.0f && angle < 182.0f) || (angle > -2.0f && angle < 2.0f))
                {
                    angleVelocity = 0.0f;
                }
                else
                {
                    Vector3 Newton = obj.mass * obj.v / Time.deltaTime;
                    float torque = Vector3.Distance(Vector3.zero, Newton) * radius;
                    float I = givenNumber * mass * radius * radius;
                    float alpha = torque / I;
                    angleVelocity = alpha * Time.deltaTime;
                }

                SetVelocity(v1f - v1f * (resistancePP / 100.0f));
                obj.SetVelocity(v2f - v2f * (obj.resistancePP / 100.0f));
                obj.hitOnce = true;
            }
            else
            {
                hitOnce = false;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + v);
    }

    void SetVelocity(Vector3 velocity)
    {
        v = velocity;
        direction = v.normalized;
        speed = Vector3.Distance(Vector3.zero, v);
    }
}
