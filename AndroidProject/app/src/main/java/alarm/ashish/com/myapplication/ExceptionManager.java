package alarm.ashish.com.myapplication;

/**
 * Created by LENOVO on 4/10/2018.
 */

public class ExceptionManager
{
    public static MyException dispatchException(int errorCode, String message, Exception exception) throws MyException
    {
        if(exception instanceof MyException)
        {
            throw (MyException) exception;
        }

        throw new MyException(errorCode, message);
    }
}
