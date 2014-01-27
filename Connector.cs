using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class Connector: MonoBehaviour
{	
	public enum STATES
	{
		NONE,
		STARTING_CONNECTION,
		TRYING_TO_ESTABILISH_CONNECTION,
		CONNECTION_FAILED,
		CONNECTION_CLOSED,
		CONNECTION_SUCCEDED,
		CHECKING_UNLOCK,
		LOCKED,
		SENDING_START,
		SENDING_CALIBRATION,
		RECEIVING,
		STOPPING_CONNECTION,
		CHARGING
	}
	private STATES state=STATES.NONE;

    const int READ_BUFFER_SIZE = 255;
	const int SINGLE_READ_SIZE = 19;
	const int RECEIVED_DATA_BUFFER_SIZE = 1024;
	
    private TcpClient client;
    private byte[] readBuffer = new byte[READ_BUFFER_SIZE];
	
    public string strMessage=string.Empty;
    public string res=String.Empty;
	public string bufferString = "";
	
	private int loops=0;
	private float framesTick = 0;

	public bool useConn=true;
	
	public bool readyAndReceving=false;
	
	public int[]		offsetGraph = {362, 240, 108, 10, 10};

	private List<ConnectorReceivedData[]> 	samples;
	
	void Awake()
	{
		Globals.connector=this;

		int usethis = PlayerPrefs.GetInt("FootMoov");
		
		if(usethis != 1) {
			Destroy(this);
		}
	}
	
	void Start()
	{
		Globals.connector.StartConnection(true);
	}
	
	public void StartConnection(bool _graphEnabled)
	{
		SetState(STATES.STARTING_CONNECTION);
	}
	
	private ConnectorReceivedData lastReceivedData=null;
	void Update()
	{
		switch(state)
		{
			case STATES.NONE:
				break;
			case STATES.STARTING_CONNECTION:
				SetState(STATES.TRYING_TO_ESTABILISH_CONNECTION);
				TryToEstabilishConnection(ConnectionSettings.ipDevice, ConnectionSettings.portDevice);
				break;
			case STATES.TRYING_TO_ESTABILISH_CONNECTION:
				break;
			case STATES.CONNECTION_FAILED:
				if(IsTimerOver(2))
					SetState(STATES.STARTING_CONNECTION);
				break;
			case STATES.CONNECTION_CLOSED:
				break;
			case STATES.CONNECTION_SUCCEDED:
				SetState(STATES.CHECKING_UNLOCK);
				break;
			case STATES.CHECKING_UNLOCK:
				break;
			case STATES.LOCKED:
				break;
			case STATES.CHARGING:
				break;
			case STATES.SENDING_START:
				SetState(STATES.RECEIVING);
				break;
			case STATES.SENDING_CALIBRATION:
				break;
			case STATES.RECEIVING:
				
			
				lastReceivedData = UnpackData(NextData(ref bufferString));
			
				if (Time.time >= framesTick) {
					framesTick = Time.time + 1;
				}
			
				break;
			case STATES.STOPPING_CONNECTION:
				break;
		}
	}
	
	public ConnectorReceivedData GetLastData()
	{
		return lastReceivedData;
	}
	
	private void SetState(STATES State)
	{
		//Debug.Log("CONNECTOR SET STATE: " + State);
		switch(State)
		{
			case STATES.NONE:
				ResetTimer();
				break;
			case STATES.STARTING_CONNECTION:
				break;
			case STATES.TRYING_TO_ESTABILISH_CONNECTION:
				break;
			case STATES.CONNECTION_FAILED:
				ResetTimer();
				break;
			case STATES.CONNECTION_CLOSED:
				break;
			case STATES.CONNECTION_SUCCEDED:
				//HELLO RECEIVED
				break;
			case STATES.CHECKING_UNLOCK:
				//MODALITA' APP
				SendData(ConnectionSettings.RAW_DATA_STRING);
				readBuffer = new byte[READ_BUFFER_SIZE];
				client.GetStream().BeginRead(readBuffer, 0, READ_BUFFER_SIZE, new AsyncCallback(DoReadLockCheck), null);
				break;
			case STATES.LOCKED:
				ResetTimer();
				break;
			case STATES.CHARGING:
				break;
			case STATES.SENDING_START:
				SendData(ConnectionSettings.RAW_DATA_STRING);
				break;
			case STATES.SENDING_CALIBRATION:
				break;
			case STATES.RECEIVING:
				client.GetStream().BeginRead(readBuffer, 0, SINGLE_READ_SIZE, new AsyncCallback(DoReadData), null);
				break;
			case STATES.STOPPING_CONNECTION:
				break;
		}
		state=State;
	}

	public STATES GetState()
	{
		return state;
	}
	
	private float elapsedTime=0;
	private void ResetTimer()
	{
		elapsedTime=0;
	}
	
	private bool IsTimerOver(float TimeToWait)
	{
		elapsedTime+=Time.deltaTime;
		if(elapsedTime>TimeToWait)
			return true;
		else 
			return false;
	}
	
	private void TryToEstabilishConnection(string sNetIP, int iPORT_NUM)
	{
        try 
        {
            client = new TcpClient(sNetIP, iPORT_NUM);
			readBuffer = new byte[READ_BUFFER_SIZE];
            client.GetStream().BeginRead(readBuffer, 0, READ_BUFFER_SIZE, new AsyncCallback(DoReadConnectionResult), null);
    		//Debug.Log("fnConnectionResult: " + System.Text.ASCIIEncoding.ASCII.GetString(readBuffer) );
            SetState(STATES.CONNECTION_SUCCEDED);
        } 
        catch(Exception ex)
        {
			Debug.Log("TryToEstabilishConnection ERROR:" + ex.Message);
			SetState(STATES.CONNECTION_FAILED);
        }
	}

    private void DoReadConnectionResult(IAsyncResult ar)
	{
        int BytesRead;
        try
        {
            BytesRead = client.GetStream().EndRead(ar);
            if (BytesRead < 1) 
            {
                // if no bytes were read server has close.  
				SetState(STATES.CONNECTION_FAILED);
                return;
            }
            // Convert the byte array the message was saved into, minus two for the
            // Chr(13) and Chr(10)

			//*HELLO*			
			strMessage = System.Text.ASCIIEncoding.ASCII.GetString(readBuffer).Substring(0, BytesRead);
			Debug.Log("DoReadConnectionResult: " + strMessage + " " + BytesRead);
			
			if (strMessage.StartsWith(ConnectionSettings.HELLO_READY_STRING)) {
				SetState(STATES.CONNECTION_SUCCEDED);
			} else {
				SetState(STATES.CONNECTION_FAILED);
			}
        } 
        catch
        {
			SetState(STATES.CONNECTION_FAILED);
        }
	}

	
    private void DoReadLockCheck(IAsyncResult ar)
	{
        int BytesRead;
        try
        {
            BytesRead = client.GetStream().EndRead(ar);
            if (BytesRead < 1) 
            {
				SetState(STATES.CONNECTION_CLOSED);
                return;
            }
			strMessage = System.Text.ASCIIEncoding.ASCII.GetString(readBuffer).Substring(0, BytesRead);
			Debug.Log("CHECK LOCK MSG (" + BytesRead + "): '" + strMessage + "'");
			
			if (strMessage.StartsWith(ConnectionSettings.READY_STRING)) {
				SetState(STATES.SENDING_START);
				return;
			}
			else if (strMessage.StartsWith(ConnectionSettings.CHARGING_BEGINNING_STRING)) {
				SetState(STATES.CHARGING);
				return;
			} else if (strMessage == "") {
				SetState(STATES.SENDING_START);
				return;
			} else {
				SetState(STATES.LOCKED);
				return;
			}
        } 
        catch
        {
			SetState(STATES.CONNECTION_CLOSED);
        }
	}
	
	private void DoReadData(IAsyncResult ar)
	{
        int BytesRead;
        try
        {
            BytesRead = client.GetStream().EndRead(ar);
            if (BytesRead < 1) 
            {
				SetState(STATES.CONNECTION_CLOSED);
				readyAndReceving=false;
                return;
            }
			
			strMessage = System.Text.ASCIIEncoding.ASCII.GetString(readBuffer, 0, SINGLE_READ_SIZE);
			
			bufferString += strMessage;
			// Debug.Log("Lunghezza buffer: " + strMessage.Length + ", Lunghezza read: " + BytesRead);
			
			readyAndReceving=true;
			//Debug.Log("DoReadData: " + strMessage);
			SetState(STATES.RECEIVING);
        } 
        catch
        {
			readyAndReceving=false;
			SetState(STATES.CONNECTION_CLOSED);
        }
	}

    // Use a StreamWriter to send a message to server.
    private void SendData(string data)
    {
		if(client==null)
			return;
		
        StreamWriter writer = new StreamWriter(client.GetStream());
        //writer.Write(data + (char) 13);
        writer.Write(data);
        writer.Flush();
    }

	
	void OnApplicationQuit()
	{
		if(client==null)
			return;
		SendData(ConnectionSettings.STOP_STRING);
		client.GetStream().Close();
		client.Close();
		client=null;
	}
	
	public float startAccelerometerX=-1;
	public float startAccelerometerY=-1;
	public float startAccelerometerZ=-1;
	public float accelerometerX=0;
	public float accelerometerY=0;
	public float accelerometerZ=0;
	public float pression1=0;
	public float pression2=0;
	public float startPression1=0;
	public float startPression2=0;
	public float battery=0;

	public ConnectorReceivedData calibratedData=new ConnectorReceivedData();
	public ConnectorReceivedData UnpackData(string dataStr)
	{
		string 	fullstring = dataStr;
		int		bytePerValue = 3;
		
		// Debug.Log("Data string: " + dataString);
		
		if (dataStr == "") {
			return(null);
		}
		
		try
		{
			dataStr=dataStr.Substring(dataStr.IndexOf("!") + 1);
			string accX=dataStr.Substring(0,bytePerValue);
			accelerometerX=int.Parse(accX, System.Globalization.NumberStyles.AllowHexSpecifier);
			if(loops<5)
				startAccelerometerX=accelerometerX;
				
			dataStr=dataStr.Substring(bytePerValue);
			string accY=dataStr.Substring(0,bytePerValue);
			accelerometerY=int.Parse(accY, System.Globalization.NumberStyles.AllowHexSpecifier);
			if(loops<5)
				startAccelerometerY=accelerometerY;
			
			dataStr=dataStr.Substring(bytePerValue);
			string accZ=dataStr.Substring(0,bytePerValue);
			accelerometerZ=int.Parse(accZ, System.Globalization.NumberStyles.AllowHexSpecifier);
			if(loops<5)
				startAccelerometerZ=accelerometerZ;
	
	
			dataStr=dataStr.Substring(bytePerValue);
			string Press1=dataStr.Substring(0,bytePerValue);
			pression1=int.Parse(Press1, System.Globalization.NumberStyles.AllowHexSpecifier);
			if(loops<5)
				startPression1=pression1;
	
			dataStr=dataStr.Substring(bytePerValue);
			string Press2=dataStr.Substring(0,bytePerValue);
			pression2=int.Parse(Press2, System.Globalization.NumberStyles.AllowHexSpecifier);
			if(loops<5)
				startPression2=pression2;
	
			dataStr=dataStr.Substring(bytePerValue);
			string Batt=dataStr.Substring(0,bytePerValue);
			battery=int.Parse(Batt, System.Globalization.NumberStyles.AllowHexSpecifier);
	
			loops++;

			ConnectorReceivedData connData=new ConnectorReceivedData();
			connData.accelerometerX=accelerometerX;
			connData.accelerometerY=accelerometerY;
			connData.accelerometerZ=accelerometerZ;
			connData.pression1=pression1;
			connData.pression2=pression2;
			connData.battery=100-battery;
			
			//Debug.Log(connData.accelerometerX +" "+ connData.accelerometerY +" "+ connData.accelerometerZ +" "+ connData.pression1 +" "+ connData.pression2 +" "+ connData.battery);
			return connData;
		}
	    catch(Exception ex)
        {
			Debug.Log("ERROR ON RECEIVING DATA: " + ex.Message + "\nds: " + dataStr + "\nfs:" + fullstring);
			return new ConnectorReceivedData();
        }
		
	}	
	
	public string NextData(ref string buffer) {
		int p1, p2;
		
		string dataString = "";
		if (buffer != "") {
			// Cerca il primo !
			p1 = buffer.IndexOf("!");
			// Assicurati che sia completo
			if ((p1 != -1) && (buffer.Length >= p1 + SINGLE_READ_SIZE)) {
				dataString = buffer.Substring(p1, SINGLE_READ_SIZE);

				p2 = dataString.LastIndexOf("!");
				if (p2 > 0) {
					// Debug.Log("Posizione scorretta" + dataString);
					
					// Inizio in posizione non corretta, skippa tutto
					dataString = "";
					p2 = buffer.IndexOf("!", p1 + 1);
					if (p2 != -1) {
						buffer = buffer.Substring(p2);
						// Debug.Log("Buffer sistemato: " + buffer);
						// buffer = "";
					}
					
				} else {
					// Elimina il pacchetto letto
					buffer = buffer.Substring(p1 + SINGLE_READ_SIZE);
					// buffer = "";
				}
				
				// Debug.Log(buffer);
			}
		}
		
		return(dataString);

	}
}

