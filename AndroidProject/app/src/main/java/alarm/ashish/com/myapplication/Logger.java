package alarm.ashish.com.myapplication;

import android.util.Log;

/**
 * Created by LENOVO on 3/17/2018.
 */

public class Logger
{
    private static String TAG = "MyApplication";
    public static void log(String message)
    {
        Log.d(TAG, message);
    }
}
