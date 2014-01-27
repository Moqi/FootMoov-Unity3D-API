using UnityEngine;
using System.Collections;

public class ConnectorReceivedData
{
	public float accelerometerX;
	public float accelerometerY;
	public float accelerometerZ;
	public float pression1;
	public float pression2;
	public float battery;

	public static int		MAXHEIGHT		= 3100;
	
	public static ConnectorReceivedData standValue = new ConnectorReceivedData(2060, 1892, 3055, 0, 0);
	
	public ConnectorReceivedData() {
		accelerometerX = 0;
		accelerometerY = 0;
		accelerometerZ = 0;
		pression1 = 0;
		pression2 = 0;
		battery = 0;
	}
	
	public ConnectorReceivedData(float ax, float ay, float az, float p1, float p2) {
		
		accelerometerX = ax;
		accelerometerY = ay;
		accelerometerZ = az;
		pression1 = p1;
		pression2 = p2;
		
	}
}
