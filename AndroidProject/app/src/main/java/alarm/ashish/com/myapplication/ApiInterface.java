package alarm.ashish.com.myapplication;

import android.os.Environment;

import java.io.BufferedInputStream;
import java.io.BufferedOutputStream;
import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;

import retrofit2.Call;
import retrofit2.http.GET;
import retrofit2.http.POST;
import retrofit2.http.Path;

/**
 * Created by LENOVO on 3/17/2018.
 */

public class ApiInterface
{
    public static final String  LOGIN_IMAGES_URL = "http://indentitymanagement.azurewebsites.net/api/UserIdentity/ProcessIdentificationImage";
    public static final String  REGISTER_IMAGES_URL = "http://indentitymanagement.azurewebsites.net/api/UserIdentity/ProcessRegistrationImage";
    public static final String  REGISTER_URL = "http://indentitymanagement.azurewebsites.net/api/UserIdentity/ProcessRegistrationVideo";
    public static final String  LOGIN_URL = "http://indentitymanagement.azurewebsites.net/api/useridentity/ProcessIdentificationVideo";

    public static String sendData(String fileName, String serverUrl, byte[] data) throws MyException
    {
        StringBuilder total = new StringBuilder();
        try
        {
            Logger.log("sendData: fileName: "+fileName);
            Logger.log("sendData: serverUrl: "+serverUrl);



            URL url = new URL(serverUrl);
            HttpURLConnection con = (HttpURLConnection) url.openConnection();
            con.setRequestMethod("POST");
            con.setDoInput(true);
            con.setDoOutput(true);
            con.connect();

            if(data == null)
            {
                data = readFromFile(fileName);
            }

            Logger.log("sendData: data: "+data.length);

            OutputStream outputStream = new BufferedOutputStream(con.getOutputStream());
            outputStream.write(data);
            outputStream.flush();

            int responseCode=con.getResponseCode();
            Logger.log("sendData: ResponseCode: "+responseCode);

            BufferedReader bufferReader = null;
            if(responseCode == 200)
            {
                bufferReader = new BufferedReader(new InputStreamReader(con.getInputStream()));
            }
            else
            {
                bufferReader = new BufferedReader(new InputStreamReader(con.getErrorStream()));
            }

            String line;
            while ((line = bufferReader.readLine()) != null) {
                total.append(line).append('\n');
            }

            Logger.log("sendData: Server Response: "+total.toString());


            bufferReader.close();
            outputStream.close();
        }
        catch (Exception exception)
        {
            throw ExceptionManager.dispatchException(ErrorCodes.ERROR_CODE_100, exception.toString(), exception);
        }

        return total.toString();
    }



    private static byte[] readFromFile(String path) throws MyException {
        byte[] bytes = null;
        try
        {
            File file = null;
            if(AppUtil.isLive)
            {
                file = new File(path);
            }
            else
            {
                file = new File(Environment.getExternalStorageDirectory()+"/Reg.mp4");
            }


            Logger.log("readFromFile: file path: "+file.getPath());
            int size = (int) file.length();
            Logger.log("readFromFile: file length: "+size);

            bytes = new byte[size];

            BufferedInputStream buf = new BufferedInputStream(new FileInputStream(file));
            buf.read(bytes, 0, bytes.length);
            buf.close();
        }
        catch (Exception exception)
        {
           throw ExceptionManager.dispatchException(ErrorCodes.ERROR_CODE_101, exception.toString(), exception);
        }

        return bytes;

    }
}
