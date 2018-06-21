package alarm.ashish.com.myapplication;

/**
 * Created by LENOVO on 4/10/2018.
 */

public class MyException extends Exception
{
    int errorCode;
    String message;

    public MyException(int errorCode, String message)
    {
        this.errorCode = errorCode;
        this.message = message;

    }
    public int getErrorCode() {
        return errorCode;
    }

    public void setErrorCode(int errorCode) {
        this.errorCode = errorCode;
    }

    public String getMessage() {
        return message;
    }

    public void setMessage(String message) {
        this.message = message;
    }

    @Override
    public String toString() {
        return "Exception: "+"Code: "+errorCode + ", Message: "+ message;
    }
}
