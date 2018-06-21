package alarm.ashish.com.myapplication;

import android.app.Activity;
import android.app.ProgressDialog;
import android.content.Intent;
import android.content.pm.ActivityInfo;
import android.hardware.Camera;
import android.media.CamcorderProfile;
import android.media.CameraProfile;
import android.media.MediaRecorder;
import android.os.AsyncTask;
import android.os.Bundle;
import android.os.Environment;
import android.support.annotation.Nullable;
import android.view.Display;
import android.view.Surface;
import android.view.SurfaceHolder;
import android.view.SurfaceView;
import android.view.View;
import android.view.Window;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.ProgressBar;

import java.io.IOException;

/**
 * Created by LENOVO on 4/4/2018.
 */


public class VideoCaptureActivity extends Activity implements View.OnClickListener, SurfaceHolder.Callback
{



    MediaRecorder recorder;
    SurfaceHolder holder;
    boolean recording = false;
    Camera camera;
    public String path = null;
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        requestWindowFeature(Window.FEATURE_NO_TITLE);
        getWindow().setFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN,
                WindowManager.LayoutParams.FLAG_FULLSCREEN);
        setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_PORTRAIT);

        try
        {
            path = AppUtil.getRandomFileName();
        }
        catch (MyException exception)
        {
            Logger.log(exception.toString());
        }

        recorder = new MediaRecorder();

        camera = Camera.open(1);

        camera.setDisplayOrientation(90);
        recorder.setOrientationHint(270);
        recorder.setCamera(camera);
        camera.unlock();

        initRecorder();
        setContentView(R.layout.video_capture_layout);

        SurfaceView cameraView = (SurfaceView) findViewById(R.id.surface_camera);
        holder = cameraView.getHolder();
        holder.addCallback(this);
        holder.setType(SurfaceHolder.SURFACE_TYPE_PUSH_BUFFERS);

        cameraView.setClickable(true);
        cameraView.setOnClickListener(this);
    }



    public int getDisplayOrientationAngle() {
        Logger.log("setDisplayOrientationAngle is call");
        int angle;

        Display display = getWindowManager().getDefaultDisplay();
        int mDisplayRotation = display.getRotation();

        // switch (MeasurementNativeActivity.DisplayRotation) {
        switch (mDisplayRotation) {
            case Surface.ROTATION_0: // This is display orientation
                angle = 90; // This is camera orientation
                break;
            case Surface.ROTATION_90:
                angle = 0;
                break;
            case Surface.ROTATION_180:
                angle = 270;
                break;
            case Surface.ROTATION_270:
                angle = 180;
                break;
            default:
                angle = 90;
                break;
        }
        Logger.log("media recorder displayRotation: " + mDisplayRotation);
        Logger.log("media recorder angle: " + angle);
        return angle;

    }

    private void initRecorder()
    {
        recorder.setAudioSource(MediaRecorder.AudioSource.CAMCORDER);
        recorder.setVideoSource(MediaRecorder.VideoSource.DEFAULT);

        CamcorderProfile cpHigh = CamcorderProfile
                .get(CamcorderProfile.QUALITY_480P);
        recorder.setProfile(cpHigh);
        recorder.setOutputFile(path);
        recorder.setMaxDuration(50000000); // 50 seconds
        recorder.setMaxFileSize(500000000); // Approximately 5 megabytes
    }

    private void prepareRecorder() {
        recorder.setPreviewDisplay(holder.getSurface());

        try {
            recorder.prepare();
        } catch (IllegalStateException e) {
            e.printStackTrace();
            finish();
        } catch (IOException e) {
            e.printStackTrace();
            finish();
        }
    }

    public void onClick(View v) {
        if (recording) {
            recorder.stop();
            recording = false;
//            camera.release();


            camera.release();
            recorder.release();
            // Let's initRecorder so we can record again
//            initRecorder();
//            prepareRecorder();
            setResultAndFinish();
        } else {
//            camera.unlock();
            recording = true;
            recorder.start();
        }
    }

    public void setResultAndFinish()
    {
        Intent intent = new Intent();
        intent.putExtra("filepath", path);
        setResult(RESULT_OK, intent);
        finish();
    }


    public void surfaceCreated(SurfaceHolder holder) {
        prepareRecorder();
    }

    public void surfaceChanged(SurfaceHolder holder, int format, int width,
                               int height) {
    }

    public void surfaceDestroyed(SurfaceHolder holder) {
        if (recording) {
            recorder.stop();
            recording = false;
        }
        recorder.release();
        finish();
    }

    public void setCameraRotation() {
        try {

            Camera.CameraInfo camInfo = new Camera.CameraInfo();

//            if (VideoCaptureActivity.cameraId == 0)
//                Camera.getCameraInfo(Camera.CameraInfo.CAMERA_FACING_BACK, camInfo);
//            else
//                Camera.getCameraInfo(Camera.CameraInfo.CAMERA_FACING_FRONT, camInfo);
            int cameraRotationOffset = camInfo.orientation;
            // ...

            Camera.Parameters parameters = camera.getParameters();


            int rotation = getWindowManager().getDefaultDisplay().getRotation();
            int degrees = 0;
            switch (rotation) {
                case Surface.ROTATION_0:
                    degrees = 0;
                    break; // Natural orientation
                case Surface.ROTATION_90:
                    degrees = 90;
                    break; // Landscape left
                case Surface.ROTATION_180:
                    degrees = 180;
                    break;// Upside down
                case Surface.ROTATION_270:
                    degrees = 270;
                    break;// Landscape right
            }
            int displayRotation;
            if (camInfo.facing == Camera.CameraInfo.CAMERA_FACING_FRONT) {
                displayRotation = (cameraRotationOffset + degrees) % 360;
                displayRotation = (360 - displayRotation) % 360; // compensate
                // the
                // mirror
            } else { // back-facing
                displayRotation = (cameraRotationOffset - degrees + 360) % 360;
            }

            camera.setDisplayOrientation(displayRotation);

            int rotate;
            if (camInfo.facing == Camera.CameraInfo.CAMERA_FACING_FRONT) {
                rotate = (360 + cameraRotationOffset + degrees) % 360;
            } else {
                rotate = (360 + cameraRotationOffset - degrees) % 360;
            }

            parameters.set("orientation", "portrait");
            parameters.setRotation(rotate);
            camera.setParameters(parameters);

        } catch (Exception e) {

        }
    }

}

//public class VideoCaptureActivity extends Activity implements View.OnClickListener
//{
//    @Override
//    protected void onCreate(@Nullable Bundle savedInstanceState) {
//        super.onCreate(savedInstanceState);
//        setContentView(R.layout.video_capture_layout);
//
//        Button btnStop = findViewById(R.id.btnStop);
//        btnStop.setOnClickListener(this);
//
//        //        MediaRecorder recorder = new MediaRecorder();
////        final Camera camera = Camera.open(1);
////        CameraPreview mPreview = new CameraPreview(this, camera);
////
////        RelativeLayout preview = (RelativeLayout) findViewById(R.id.relativeLayout);
////        preview.addView(mPreview);
////        camera.startPreview();
//
////        SurfaceView cameraView = (SurfaceView) findViewById(R.id.CameraView);
////        final SurfaceHolder holder = cameraView.getHolder();
////        holder.addCallback(null);
////        holder.setType(SurfaceHolder.SURFACE_TYPE_PUSH_BUFFERS);
//
////        final MediaRecorder mRecorder= new MediaRecorder();
////        final Camera camera = Camera.open(1);
////        Handler handler = new Handler();
////        handler.postDelayed(new Runnable() {
////            @Override
////            public void run() {
////                    String mFileName = getExternalCacheDir().getAbsolutePath();
////                    mFileName += "/audiorecordtest.mp4";
////
////                    MediaRecorder mRecorder= new MediaRecorder();
////                    Camera camera = Camera.open(1);
////                    camera.unlock();
////                    mRecorder.setCamera(camera);
////
////                    mRecorder.setPreviewDisplay(((SurfaceView)findViewById(R.id.surface_camera)).getHolder().getSurface());
////                    mRecorder.setAudioSource(MediaRecorder.AudioSource.MIC);
////                    mRecorder.setVideoSource(MediaRecorder.VideoSource.CAMERA);
//////                    mRecorder.setAudioSource(MediaRecorder.AudioSource.MIC);
//////                    mRecorder.setOutputFormat(MediaRecorder.OutputFormat.THREE_GPP);
////                    mRecorder.setOutputFile(mFileName);
//////                    mRecorder.setAudioEncoder(MediaRecorder.AudioEncoder.AMR_NB);
////
////                mRecorder.setPreviewDisplay(((SurfaceView)findViewById(R.id.surface_camera)).getHolder().getSurface());
////                    mRecorder.setProfile(CamcorderProfile.get(CamcorderProfile.QUALITY_HIGH));
////
//////                    CamcorderProfile cpHigh = CamcorderProfile
//////                            .get(CamcorderProfile.QUALITY_HIGH);
//////                    mRecorder.setProfile(cpHigh);
////
////                    try {
//////                        mRecorder.setPreviewDisplay(holder.getSurface());
////
////                        mRecorder.prepare();
////                    } catch (IOException e) {
////                        Logger.log("prepare() failed");
////                    }
////
////                    mRecorder.start();
////                }
////
////
////        }, 5000);
////
////        Handler handler1 = new Handler();
////        handler1.postDelayed(new Runnable() {
////            @Override
////            public void run() {
////                mRecorder.stop();
////                mRecorder.release();
////                camera.release();
////            }
////        }, 10000);
//    }
//
//}
