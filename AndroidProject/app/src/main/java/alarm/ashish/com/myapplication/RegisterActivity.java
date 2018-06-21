package alarm.ashish.com.myapplication;

import android.app.ProgressDialog;
import android.content.Intent;
import android.graphics.Bitmap;

import android.graphics.drawable.BitmapDrawable;
import android.graphics.drawable.Drawable;
import android.hardware.Camera;
import android.media.MediaMetadataRetriever;
import android.media.MediaPlayer;
import android.media.MediaRecorder;
import android.net.Uri;
import android.os.AsyncTask;
import android.os.Bundle;
import android.os.Environment;
import android.os.Handler;
import android.provider.MediaStore;
import android.support.annotation.Nullable;
import android.support.v7.app.AppCompatActivity;
import android.view.View;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.util.Random;

/**
 * Created by LENOVO on 3/17/2018.
 */

public class RegisterActivity extends AppCompatActivity implements View.OnClickListener, AsyncListener
{
    private Uri videoPathUri;
    private TextView btnTakePicture;
    private TextView btnRecordVoice;
    private ImageView imageViewUserPicture;
    private TextView textViewReTake;
    private TextView btnLRegister;
    private MediaPlayer mPlayer;
    private String mFileName;

    MediaRecorder mediaRecorder;
    String AudioSavePathInDevice;
    String RandomAudioFileName = "ABCDEFGHIJKLMNOP";
    Random random  = new Random();

    private final int REQUEST_CODE_ON_ACTIVITY_RESULT = 100;
    private String videoPath = null;

    private MediaRecorder mRecorder;
//    private String filepath = null;

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.register_activity);

        btnTakePicture = findViewById(R.id.btnTakePicture);
        btnRecordVoice = findViewById(R.id.btnRecordVoice);

        imageViewUserPicture = findViewById(R.id.imageViewUserPicture);
        textViewReTake = findViewById(R.id.textViewReTake);
        btnLRegister = findViewById(R.id.btnLRegister);
        btnLRegister.setSelected(true);

        btnTakePicture.setOnClickListener(this);
        imageViewUserPicture.setOnClickListener(this);
        textViewReTake.setOnClickListener(this);
        btnLRegister.setOnClickListener(this);
        btnRecordVoice.setOnClickListener(this);


        mFileName = Environment.getExternalStorageDirectory().getAbsolutePath();
        mFileName += "/audiorecordtest112.3gp";
        Logger.log("Path: "+mFileName);
       // startPlaying();
    }

    @Override
    public void onClick(View v) {
        int viewId = v.getId();
        switch (viewId)
        {
            case R.id.btnTakePicture:
                onClick_TakePicture();
                break;
            case R.id.imageViewUserPicture:
                onClick_ImageViewUserPicture();
                break;
            case R.id.textViewReTake:
                onClick_TakePicture();
                break;
            case R.id.btnLRegister:
                onClick_BtnRegister();
                break;
            case R.id.btnRecordVoice:
                //onClick_BtnRecordAudio();
                break;
        }
    }


    public void MediaRecorderReady(){
        mediaRecorder=new MediaRecorder();
        mediaRecorder.setAudioSource(MediaRecorder.AudioSource.MIC);
        mediaRecorder.setOutputFormat(MediaRecorder.OutputFormat.THREE_GPP);
        mediaRecorder.setAudioEncoder(MediaRecorder.OutputFormat.AMR_NB);
        mediaRecorder.setOutputFile(AudioSavePathInDevice);
    }

    private void onClick_TakePicture()
    {
        startActivityForResult(new Intent(this, VideoCaptureActivity.class), REQUEST_CODE_ON_ACTIVITY_RESULT);
    }

    private void onClick_ImageViewUserPicture()
    {
        Intent intent = new Intent(Intent.ACTION_VIEW, videoPathUri);
        intent.setDataAndType(videoPathUri, "video/mp4");
        startActivity(intent);
    }
    private void onClick_TextViewReTake()
    {
        startActivityForResult(new Intent(this, VideoCaptureActivity.class), REQUEST_CODE_ON_ACTIVITY_RESULT);
    }

    private void onClick_BtnRegister()
    {
        new UploadAsyncTAsk(this, videoPath, ApiInterface.REGISTER_URL, ApiInterface.REGISTER_IMAGES_URL).execute();
    }


    @Override
    protected void onActivityResult(int requestCode, int resultCode, final Intent data) {

        try
        {
            if (requestCode == REQUEST_CODE_ON_ACTIVITY_RESULT && resultCode == RESULT_OK)
            {
                videoPath = data.getStringExtra("filepath");
                Logger.log("Path Received: "+videoPath);
                final MediaMetadataRetriever mediaMetadataRetriever = new MediaMetadataRetriever();
                FileInputStream inputStream = new FileInputStream(videoPath);
                mediaMetadataRetriever.setDataSource(this, Uri.fromFile(new File(videoPath)));
                Bitmap bmFrame = mediaMetadataRetriever.getFrameAtTime(1000, MediaMetadataRetriever.OPTION_NEXT_SYNC); //unit in microsecond
                imageViewUserPicture.setImageBitmap(bmFrame);

                Drawable d = new BitmapDrawable(getResources(), bmFrame);

                imageViewUserPicture.setBackground(d);
                imageViewUserPicture.setVisibility(View.VISIBLE);
                btnLRegister.setEnabled(true);
                btnLRegister.setClickable(true);
                btnLRegister.setSelected(false);

                textViewReTake.setVisibility(View.VISIBLE);
            }
        }
        catch(Exception exception)
        {
            Logger.log("Exception: "+ exception.toString());
        }
    }

    @Override
    public void onSuccess(String data) {
        AppUtil.showToast(this, "You Are Register Successfully: "+data);
        finish();
    }

    @Override
    public void onFail(Exception exception) {
        AppUtil.showToast(this, "Error: "+exception.toString());
        finish();
    }


//    public class UploadVideoAsyncTask extends AsyncTask<Void, Void, String>
//    {
//        ProgressDialog progress = null;
//        Exception exception;
//
//        @Override
//        protected void onPreExecute() {
//            super.onPreExecute();
//            progress = new ProgressDialog(RegisterActivity.this);
//            progress.show();
//        }
//
//        @Override
//        protected String doInBackground(Void... voids) {
//            String data = null;
//            try
//            {
//                data = ApiInterface.sendData(videoPath, ApiInterface.REGISTER_URL, null);
//                AppUtil.uploadImageFromVideo(RegisterActivity.this, data);
//            }
//            catch (Exception exception)
//            {
//                this.exception = exception;
//                Logger.log(exception.toString());
//            }
//
//
//            return data;
//        }
//
//        @Override
//        protected void onPostExecute(String responseString) {
//            super.onPostExecute(responseString);
//            progress.dismiss();
//
//            if(exception == null) {
//                Toast.makeText(RegisterActivity.this, "You Are Register Successfully: "+responseString, Toast.LENGTH_LONG).show();
//                RegisterActivity.this.finish();
//            }
//            else
//            {
//                Toast.makeText(RegisterActivity.this, "Error: "+ exception.toString(), Toast.LENGTH_LONG).show();
//            }
//        }
//    }


}
