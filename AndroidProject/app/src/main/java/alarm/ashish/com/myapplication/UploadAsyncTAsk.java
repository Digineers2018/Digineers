package alarm.ashish.com.myapplication;

import android.app.Activity;
import android.app.ProgressDialog;
import android.os.AsyncTask;

/**
 * Created by LENOVO on 4/10/2018.
 */

public class UploadAsyncTAsk extends AsyncTask<Void, Void, String>
{
    ProgressDialog progress = null;
    Exception exception;
    Activity activityContext;
    String filepath;
    String videoUrl;
    AsyncListener asyncListener;
    String imageUrl;

    public UploadAsyncTAsk(Activity activityContext, String filePath, String videoUrl, String imageUrl)
    {
        this.activityContext = activityContext;
        this.filepath = filePath;
        this.videoUrl = videoUrl;
        this.imageUrl = imageUrl;
        if(activityContext instanceof AsyncListener)
        {
            asyncListener = (AsyncListener) activityContext;
        }
    }

    @Override
    protected void onPreExecute()
    {
        super.onPreExecute();
        progress = new ProgressDialog(activityContext);
        progress.setMessage("!Please Wait uploading data...");
        progress.show();
    }




    @Override
    protected String doInBackground(Void... voids)
    {
        String data = null;
        String data2 = null;
        try
        {
            data = ApiInterface.sendData(filepath, videoUrl, null);
            data2 = AppUtil.uploadImageFromVideo(activityContext, filepath, imageUrl);

            Logger.log("data: "+data);
            Logger.log("data2: "+data2);

            if(videoUrl.equalsIgnoreCase(ApiInterface.LOGIN_URL)) {
                boolean loginStatus = false;
                if (data.contains("running") || data.contains("succeeded")) {
                    if (!data2.contains("USER CANNOT BE IDENTIFIED")) {
                        loginStatus = true;
                    }
                }
                if (!loginStatus) {
                    throw new MyException(ErrorCodes.ERROR_CODE_104, "! User Authentication fail");
                }
            }
        }
        catch (Exception exception)
        {
            this.exception = exception;
        }

        return data;
    }

    @Override
    protected void onPostExecute(String responseString) {
        super.onPostExecute(responseString);
        progress.dismiss();

        if(exception == null)
        {
            asyncListener.onSuccess(responseString);
        }
        else
        {
            asyncListener.onFail(exception);
        }
    }
}
