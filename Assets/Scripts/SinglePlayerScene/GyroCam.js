#pragma strict

// Gyroscope-controlled camera for iPhone & Android revised 2.26.12

// Perry Hoberman <hoberman@bway.net>

//

// Usage:

// Attach this script to main camera.

// Note: Unity Remote does not currently support gyroscope.

//

// This script uses three techniques to get the correct orientation out of the gyroscope attitude:

// 1. creates a parent transform (camParent) and rotates it with eulerAngles

// 2. for Android (Samsung Galaxy Nexus) only: remaps gyro.Attitude quaternion values from xyzw to wxyz (quatMap)

// 3. multiplies attitude quaternion by quaternion quatMult

 

// Also creates a grandparent (camGrandparent) which can be rotated with localEulerAngles.y

// This node allows an arbitrary heading to be added to the gyroscope reading

// so that the virtual camera can be facing any direction in the scene, no matter what the phone's heading

 

static var gyroBool : boolean;

private var gyro : Gyroscope;

private var quatMult : Quaternion;

private var quatMap : Quaternion;

 

function Awake() {

    Debug.Log("GyroCam.js::Awake()");

    // find the current parent of the camera's transform

    var currentParent = transform.parent;

    // instantiate a new transform

    var camParent = new GameObject ("camParent");

    // match the transform to the camera position

    camParent.transform.position = transform.position;

    // make the new transform the parent of the camera transform

    transform.parent = camParent.transform;

    // make the original parent the grandparent of the camera transform

    //camParent.transform.parent = currentParent;

    // instantiate a new transform

    var camGrandparent = new GameObject ("camGrandParent");

    // match the transform to the camera position

    camGrandparent.transform.position = transform.position;

    // make the new transform the parent of the camera transform

    camParent.transform.parent = camGrandparent.transform;

    // make the original parent the grandparent of the camera transform

    camGrandparent.transform.parent = currentParent;



    // check whether device supports gyroscope

    #if UNITY_3_4

    gyroBool = Input.isGyroAvailable;

    #else

    gyroBool = SystemInfo.supportsGyroscope;

    #endif


    Debug.Log("GyroCam.js::gyroBool="+gyroBool);


    if (gyroBool) {

        gyro = Input.gyro;

        gyro.enabled = true;

        #if UNITY_IPHONE

                camParent.transform.eulerAngles = Vector3(90,90,0);

           

                    if (Screen.orientation == ScreenOrientation.LandscapeLeft) {

                       quatMult = Quaternion(0,0,1,0); //**

                    } else if (Screen.orientation == ScreenOrientation.LandscapeRight) {

                       quatMult = Quaternion(0,0,1,0); //**

                    } else if (Screen.orientation == ScreenOrientation.Portrait) {

                       quatMult = Quaternion(0,0,1,0); //**

                    } else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown) {

                       quatMult = Quaternion(0,0,1,0); // Unable to build package on upsidedown

                    }

        #endif

        #if UNITY_ANDROID

                camParent.transform.eulerAngles = Vector3(90,90,0);

                Debug.Log("GyroCam.js::Android");


                    if (Screen.orientation == ScreenOrientation.LandscapeLeft) {

                       quatMult = Quaternion(0,0,1,0); //**

                    } else if (Screen.orientation == ScreenOrientation.LandscapeRight) {

                       quatMult = Quaternion(0,0,1,0); //**

                    } else if (Screen.orientation == ScreenOrientation.Portrait) {

                       quatMult = Quaternion(0,0,1,0); //**

                    } else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown) {

                       quatMult = Quaternion(0,0,1,0); // Unable to build package on upsidedown

                    }

        #endif

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

    } else {

        #if UNITY_EDITOR

            //print("NO GYRO");

        #endif

    }

}

 

function Update () {

    if (gyroBool) {

        #if UNITY_IPHONE

            quatMap = gyro.attitude;

        #endif

        #if UNITY_ANDROID

            quatMap = Quaternion(gyro.attitude.x,gyro.attitude.y,gyro.attitude.z,gyro.attitude.w);

        #endif

        transform.localRotation = quatMap * quatMult;

    }

}