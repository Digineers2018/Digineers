package alarm.ashish.com.myapplication;

import android.app.Activity;
import android.content.Context;
import android.graphics.Bitmap;
import android.media.MediaMetadataRetriever;
import android.net.Uri;
import android.os.Environment;
import android.widget.Toast;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.util.Random;

/**
 * Created by LENOVO on 4/6/2018.
 */

public class AppUtil
{
    public static  int NUMBER_OF_IMAGES_TO_UPLOAD = 1;

    public static boolean isLive = true;
    public static boolean isSendImageToSave = true;

    public static String getRandomFileName() throws MyException {
        String path = "";
        try
        {
            path = Environment.getExternalStorageDirectory()+"/Vid"+ String.valueOf(new Random().nextInt())+".mp4";
            File file  = new File(path);
            if(!file.exists())
            {
               file.createNewFile();
            }
        }
        catch (Exception exception)
        {
            ExceptionManager.dispatchException(ErrorCodes.ERROR_CODE_102, exception.toString(), exception);
        }


        return path;
    }

    public static String getRandomFileForImage() throws MyException {
        String path = "";
        try
        {
            path = Environment.getExternalStorageDirectory()+"/Vid"+ String.valueOf(new Random().nextInt())+".jpg";
            File file  = new File(path);
            if(!file.exists())
            {
                file.createNewFile();
            }
        }
        catch (Exception exception)
        {
            ExceptionManager.dispatchException(ErrorCodes.ERROR_CODE_102, exception.toString(), exception);
        }


        return path;
    }

    public static String uploadImageFromVideo(Activity activityContext, String path, String imageUrl) throws IOException, MyException {
        String responseData = null;
        try
        {
            long frameIndex = 1000;
            for(int i=0; i<AppUtil.NUMBER_OF_IMAGES_TO_UPLOAD; i++)
            {
                final MediaMetadataRetriever mediaMetadataRetriever = new MediaMetadataRetriever();
                mediaMetadataRetriever.setDataSource(activityContext, Uri.fromFile(new File(path)));
                Bitmap bmFrame = mediaMetadataRetriever.getFrameAtTime(frameIndex, MediaMetadataRetriever.OPTION_NEXT_SYNC); //unit in microsecond

                byte[] data = bitmapToBytes(bmFrame);

                if(AppUtil.isSendImageToSave)
                {
                    writeImage(data);
                }
                responseData = ApiInterface.sendData(null, imageUrl, data);

                frameIndex += 1000;
            }
        }
        catch (MyException exception)
        {
           throw  ExceptionManager.dispatchException(ErrorCodes.ERROR_CODE_105, exception.toString(), exception);
        }

        return responseData;
    }

    private static byte[] bitmapToBytes(Bitmap bitmap)
    {
        ByteArrayOutputStream stream = new ByteArrayOutputStream();
        bitmap.compress(Bitmap.CompressFormat.PNG, 100, stream);
        byte[] byteArray = stream.toByteArray();

        return byteArray;
    }

    public static void writeImage(byte[] data) throws MyException
    {
        try {
            File file  = new File(getRandomFileForImage());
            if(!file.exists())
            {
                file.createNewFile();
            }
            FileOutputStream fileOutputStream = new FileOutputStream(file);
            fileOutputStream.write(data);
            fileOutputStream.flush();
            fileOutputStream.close();
        } catch (Exception e) {
          throw   ExceptionManager.dispatchException(ErrorCodes.ERROR_CODE_106, e.toString(), e);
        }

    }

    public static void showToast(Activity activityContext, String message)
    {
        Toast.makeText(activityContext, message, Toast.LENGTH_LONG).show();
    }
}
