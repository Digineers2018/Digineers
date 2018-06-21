package alarm.ashish.com.myapplication;

/**
 * Created by LENOVO on 4/4/2018.
 */



import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.hardware.Camera;
import android.hardware.Camera.CameraInfo;
import android.hardware.Camera.Parameters;
import android.media.AudioManager;
import android.media.FaceDetector;
import android.media.FaceDetector.Face;
import android.os.IBinder;
import android.util.Log;
import android.view.SurfaceHolder;
import android.view.SurfaceView;

public class CameraService extends Service implements SurfaceHolder.Callback{

    //a variable to store a reference to the Surface View at the main.xml file
    private SurfaceView sv;
    boolean previewing = false;
    //a bitmap to display the captured image


    //Camera variables
    //a surface holder
    private SurfaceHolder sHolder;
    //a variable to control the camera
    private Camera mCamera=null;
    //the camera parameters
    private Parameters parameters;

    /** Called when the activity is first created. */
    @Override
    public void onCreate()
    {
        super.onCreate();
        Camera.CameraInfo cameraInfo = new Camera.CameraInfo();
        Log.d("No of cameras",Camera.getNumberOfCameras()+"");
        for (int camNo = 0; camNo < Camera.getNumberOfCameras(); camNo++) {
            CameraInfo camInfo = new CameraInfo();
            Camera.getCameraInfo(camNo, camInfo);

            if (camInfo.facing==(Camera.CameraInfo.CAMERA_FACING_FRONT)) {
                mCamera = Camera.open(camNo);
            }
        }
        if (mCamera == null) {
            // no front-facing camera, use the first back-facing camera instead.
            // you may instead wish to inform the user of an error here...
            mCamera = Camera.open();
        }

//        params.
        sv = new SurfaceView(getApplicationContext());
        Log.d("Entered","dfdf");

        //get the Surface View at the main.xml file

    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        Log.d("Entered onstart","dfdf");

//          Toast.makeText(getApplicationContext(), "wrking ", Toast.LENGTH_LONG).show();
        try {



            //set camera parameters
           // mCamera.setParameters(parameters);
            mCamera.startPreview();
            mCamera.takePicture(null, null, mCall);

        } catch (Exception e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
        }


        //Get a surface
        sHolder = sv.getHolder();

        //add the callback interface methods defined below as the Surface View callbacks


        //tells Android that this surface will have its data constantly replaced
        sHolder.setType(SurfaceHolder.SURFACE_TYPE_PUSH_BUFFERS);// TODO Auto-generated method stub



        return super.onStartCommand(intent, flags, startId);
    }


    Camera.PictureCallback mCall = new Camera.PictureCallback()
    {




        public void onPictureTaken(byte[] data, Camera camera)
        {
            //decode the data obtained by the camera into a Bitmap
            FileOutputStream outStream = null;
            try{
                outStream = new FileOutputStream("/sdcard/Image.jpg");
                outStream.write(data);
                outStream.close();
            } catch (FileNotFoundException e){
                Log.d("CAMERA", e.getMessage());
            } catch (IOException e){
                Log.d("CAMERA", e.getMessage());
            }


        }
    };

    @Override
    public IBinder onBind(Intent intent) {
        // TODO Auto-generated method stub
        return null;
    }

    public void surfaceChanged(SurfaceHolder holder, int format, int width,
                               int height) {
        if(previewing){
            mCamera.stopPreview();
            previewing = false;
        }

    }

    public void surfaceCreated(SurfaceHolder holder) {
        // TODO Auto-generated method stub

    }

    public void surfaceDestroyed(SurfaceHolder holder) {
        mCamera.stopPreview();
        mCamera.release();
        mCamera = null;
        previewing = false;

    }

}