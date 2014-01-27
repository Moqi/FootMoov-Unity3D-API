using UnityEngine;
using System.Collections;

public static class ConnectionSettings
{
	//IP SETTINGS
	//public static string ipDevice = "169.254.1.1";
	public static string ipDevice = "1.2.3.4";
	public static int portDevice = 2000;
	
	//STRINGS TO SEND
	public static string UNLOCK_BEGINNING_STRING = "k";
	public static string RAW_DATA_STRING = "s";
	public static string STOP_STRING = "z";
	
	// STRINGS TO RECEIVE
	public static string READY_STRING = "READY";
	public static string HELLO_READY_STRING = "*HELLO";
	public static string CHARGING_BEGINNING_STRING = "P";
	public static string UNLOCK_SUCCESSIFUL_STRING = "OK";
	public static string UNLOCK_FAILED_STRING = "ERROR";

//    RAW DATA CONFIG - BYTE DATA MASK:
//    0.  ! carattere di match per il buffer lato scarpa, può essere ignorato (1 byte ASCII esadecimale)
//    1-3. X Accelerometro (3 byte ASCII esadecimali)
//    4-6. Y Accelerometro (3 byte ASCII esadecimali)
//    7-9. Z Accelerometro (3 byte ASCII esadecimali)
//    10-12.  Pressurometro 1 (3 byte ASCII esadecimali)
//    13-15.  Pressurometro 2 (3 byte ASCII esadecimali)
//    16-18.  Tensione Batteria (3 byte ASCII esadecimali)
	
}
