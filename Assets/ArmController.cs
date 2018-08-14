using UnityEngine;
using UnityEngine.UI;

public class ArmController : MonoBehaviour {

	public Transform table;
	public Transform tripod;
	public Transform body;
	public Transform arm;
	public Transform forearm;
	public Transform gripper;
	public Text text;

	private Plane tableTopPlane;
	private Vector3[][] dof = new Vector3[5][];
	private float[] angles = new float[5];

	void Start () {
		for (int i=0; i<dof.Length; i++) {
			dof[i] = new Vector3[2];
			angles[i] = 0;
		}
		resetDofs();
	}

	void Update() {
		// DOF 0
		if (Input.GetKey(KeyCode.Q)) {
			rotateBody(-1f);
		} else if (Input.GetKey(KeyCode.A)) {
			rotateBody(1f);
		}
		// DOF 1
		else if (Input.GetKey(KeyCode.W)) {
			rotateArm(-1f);
		} else if (Input.GetKey(KeyCode.S))	{
			rotateArm(1f);
		}
		// DOF 2
		else if (Input.GetKey(KeyCode.E)) {
			rotateForearm(-1f);
		} else if (Input.GetKey(KeyCode.D))	{
			rotateForearm(1f);
		}
		// DOF 3
		else if (Input.GetKey(KeyCode.R))
		{
			rotateWrist(-1f);
		}
		else if (Input.GetKey(KeyCode.F))
		{
			rotateWrist(1f);
		}


		/*
		Vector3 center = dof[1][0];
		markPoint(center, Color.green);
		Vector3 basePoint = dof[2][0];
		Vector3 lastPoint = basePoint;
		for (float angle=0; angle<370; angle = angle +10)
		{
			Vector3 newPoint = rotateAroundDof(1, lastPoint, 10);
			Debug.DrawLine(lastPoint, newPoint, Color.red);
			Debug.DrawLine(center, newPoint, Color.cyan);
			print(Vector3.Distance(center, newPoint));
			lastPoint = newPoint;
		}

		
		for (int i=0; i<dof.Length; i++) {
			Debug.DrawLine(dof[i][0], dof[i][1], Color.green);
		}
		markPoint(dof[1][0], Color.red);
		markPoint(dof[1][1], Color.red);
		markPoint(dof[2][0], Color.blue);
		markPoint(dof[2][1], Color.blue);
		markPoint(dof[3][0], Color.cyan);
		markPoint(dof[3][1], Color.cyan);
		*/

		Vector3 gripperTip = getDofCenter(4);
		markPoint(gripperTip, Color.red);
		float distanceToTable = tableTopPlane.GetDistanceToPoint(gripperTip);
		string output = "Gripper tip @" + gripperTip.ToString() +"\n"
			+ "Distance to table: "+ (distanceToTable *10) +" cm\n\n";
		for (int i = 0; i < dof.Length-1; i++) {
			output += "DOF " + i + " angle:\t" + angles[i] + "\n";
		}
		text.text = output;
	}

	private Vector3 rotateAroundDof(int dof, Vector3 point, float angle)
	{
		Vector3 center = getDofCenter(dof);
		return Quaternion.AngleAxis(angle, getDofAxis(dof).normalized) * (point - center) + center;
	}

	private void markPoint(Vector3 point, Color color) {
		Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
		Vector3 up = transform.TransformDirection(Vector3.up) * 10;
		Vector3 right = transform.TransformDirection(Vector3.right) * 10;
		Debug.DrawRay(point, forward, color);
		Debug.DrawRay(point, up, color);
		Debug.DrawRay(point, right, color);
	}

	private void rotateBody(float angle) {
		angles[0] = (angles[0] + angle) % 360;
		Vector3 rotationCenter = getDofCenter(0);
		Vector3 rotationAxis = getDofAxis(0);

		body.RotateAround(rotationCenter, rotationAxis, angle);
		arm.RotateAround(rotationCenter, rotationAxis, angle);
		forearm.RotateAround(rotationCenter, rotationAxis, angle);
		gripper.RotateAround(rotationCenter, rotationAxis, angle);

		for (int i = 1; i < dof.Length; i++) {
			dof[i][0] = rotateAroundDof(0, dof[i][0], angle);
			dof[i][1] = rotateAroundDof(0, dof[i][1], angle);
		}
	}

	private void rotateArm(float angle)	{
		angles[1] = (angles[1] + angle) % 360;
		Vector3 rotationCenter = getDofCenter(1);
		Vector3 rotationAxis = getDofAxis(1);

		arm.RotateAround(rotationCenter, rotationAxis, angle);
		forearm.RotateAround(rotationCenter, rotationAxis, angle);
		gripper.RotateAround(rotationCenter, rotationAxis, angle);

		for (int i = 2; i < dof.Length; i++) {
			dof[i][0] = rotateAroundDof(1, dof[i][0], angle);
			dof[i][1] = rotateAroundDof(1, dof[i][1], angle);
		}
	}

	private void rotateForearm(float angle)	{
		angles[2] = (angles[2] + angle) % 360;
		Vector3 rotationCenter = getDofCenter(2);
		Vector3 rotationAxis = getDofAxis(2);

		forearm.RotateAround(rotationCenter, rotationAxis, angle);
		gripper.RotateAround(rotationCenter, rotationAxis, angle);

		for (int i = 3; i < dof.Length; i++)
		{
			dof[i][0] = rotateAroundDof(2, dof[i][0], angle);
			dof[i][1] = rotateAroundDof(2, dof[i][1], angle);
		}
	}

	private void rotateWrist(float angle)
	{
		angles[3] = (angles[3] + angle) % 360;
		Vector3 rotationCenter = getDofCenter(3);
		Vector3 rotationAxis = getDofAxis(3);
		
		gripper.RotateAround(rotationCenter, rotationAxis, angle);

		for (int i = 4; i < dof.Length; i++)
		{
			dof[i][0] = rotateAroundDof(3, dof[i][0], angle);
			dof[i][1] = rotateAroundDof(3, dof[i][1], angle);
		}
	}

	private void resetDofs() {
		Vector3 tableSize = getSize(table);
		Vector3 tripodSize = getSize(tripod);
		Vector3 bodySize = getSize(body);
		Vector3 armSize = getSize(arm);
		Vector3 forearmSize = getSize(forearm);
		Vector3 gripperSize = getSize(gripper);

		body.position = new Vector3(tripod.position.x, tripod.position.y + tripodSize.y / 2 + bodySize.y / 2, tripod.position.z);

		dof[0][0] = new Vector3(body.position.x, body.position.y - bodySize.y / 2, body.position.z);
		dof[0][1] = new Vector3(body.position.x, body.position.y + bodySize.y / 2, body.position.z);

		dof[1][0] = new Vector3(body.position.x + bodySize.x / 2, body.position.y + bodySize.y / 2 - armSize.y / 2, body.position.z - bodySize.z / 2);
		dof[1][1] = new Vector3(body.position.x + bodySize.x / 2, body.position.y + bodySize.y / 2 - armSize.y / 2, body.position.z + bodySize.z / 2);
		arm.position = new Vector3(dof[1][0].x + armSize.x / 2, dof[1][0].y + armSize.y / 2, (dof[1][0].z + dof[1][1].z) / 2);

		dof[2][0] = new Vector3(arm.position.x + armSize.x / 2, arm.position.y, arm.position.z - armSize.z / 2);
		dof[2][1] = new Vector3(arm.position.x + armSize.x / 2, arm.position.y, arm.position.z + armSize.z / 2);
		forearm.position = new Vector3(dof[2][0].x + forearmSize.x / 2, dof[2][0].y, (dof[2][0].z + dof[2][1].z) / 2);

		dof[3][0] = new Vector3(forearm.position.x + forearmSize.x / 2, forearm.position.y, forearm.position.z - forearmSize.z / 2);
		dof[3][1] = new Vector3(forearm.position.x + forearmSize.x / 2, forearm.position.y, forearm.position.z + forearmSize.z / 2);
		gripper.position = new Vector3(dof[3][0].x + gripperSize.x / 2, dof[3][0].y, (dof[3][0].z + dof[3][1].z) / 2);

		dof[4][0] = new Vector3(gripper.position.x + gripperSize.x / 2, gripper.position.y, gripper.position.z - gripperSize.z / 2);
		dof[4][1] = new Vector3(gripper.position.x + gripperSize.x / 2, gripper.position.y, gripper.position.z + gripperSize.z / 2);

		// Create invisible plane on top of table
		tableTopPlane = new Plane(Vector3.up, new Vector3(table.position.x, table.position.y + tableSize.y / 2, table.position.z));

		// Report ground distance between table edge and ARM
		float tableArmGroundDist = table.position.x - tableSize.x/2 - tripod.position.x;
		Debug.Log("Ground distance between table edge and ARM: "+ (tableArmGroundDist*10) +" cm");
	}

	private Vector3 getDofCenter(int i) {
		return (dof[i][0] + dof[i][1]) / 2;
	}

	private Vector3 getDofAxis(int i) {
		return dof[i][0] - dof[i][1];
	}

	private Vector3 getSize(Transform transform) {
		return transform.gameObject.GetComponent<Renderer>().bounds.size;
	}
}
