package alarm.ashish.com.myapplication;

/**
 * Created by LENOVO on 4/10/2018.
 */

public interface AsyncListener
{
    public void onSuccess(String data);
    public void onFail(Exception exception);
}
