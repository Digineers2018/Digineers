using System;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Drawing;
using System.Web.Http;
using System.IO;
using Newtonsoft.Json;
using Microsoft.ProjectOxford.Face;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.ProjectOxford.Face.Contract;
using System.Linq;
using System.Collections;
using System.Web;
using System.Dynamic;
using Microsoft.ProjectOxford.SpeakerRecognition.Contract;
using Microsoft.ProjectOxford.SpeakerRecognition.Contract.Identification;
using Newtonsoft.Json.Serialization;
using NAudio.Wave;
using System.Net;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace WebApplication1.Controllers
{
    public class UserIdentityController : ApiController
    {
        const string SUCCESSFULL = "SUCCESSFULL";
        const string FAILURE = "FAILURE";
        const string USERCANNOTBEIDENTIFIED = "USER CANNOT BE IDENTIFIED";
        const string BINGSPEECHAPIKEY = "ffb6a06f528441b891be8f0538e67624";
        const string COGNITIVESPEECHAPIKEY = "05d22648c9544427aa99bc5419a6d79c";
        const string IMAGE_SUBSCRIPTION_KEY = "d51fdfb00e354267bd91d120d92b2f70";
        const string IMAGE_OCP_AICP_SUBSCRIPTION_KEY = "Ocp-Apim-Subscription-Key";
        public Bitmap input_Face_Image;
        string personGroupId = "12111993133902018";
        const string FACE_API_ENDPOINT = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0";
        private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient(IMAGE_SUBSCRIPTION_KEY, FACE_API_ENDPOINT);
        const string storedFilePrefix = "http://stoarageaccount.blob.core.windows.net/clips/";

        //    /api/UserIdentityController/ProcessRegistrationVideo
        [HttpPost]
        [ActionName("ProcessRegistrationVideo")]
        public void ProcessRegistrationVideo(Stream userRegisterVideo)
        {
            if(userRegisterVideo == null)
            {
                #region  testbed
                userRegisterVideo = new FileStream("", FileMode.Open);
                #endregion  
            }
            else
            {
                uploadFileToStorage(userRegisterVideo, "userVideo.mp4");
                var userRegistrationAudioLocation = disintegrateVideoToAudio(string.Concat(storedFilePrefix, "userVideo.mp4"));
                List<string> userRegistrationImageLocations = disintegrateVideoToImages(string.Concat(storedFilePrefix, "userVideo.mp4"));
                var textSpoken = speechToText(new FileStream(userRegistrationAudioLocation, FileMode.Open));
            }
        }

        private string disintegrateVideoToAudio(string userRegistrationVideoLocation)
        {
            if (String.IsNullOrWhiteSpace(userRegistrationVideoLocation) == false)
            {
                var userRegistrationAudioLocation = "userAudio.wav";
                var extractHelper = new NReco.VideoConverter.FFMpegConverter();
                Stream audioStream = new MemoryStream();
                extractHelper.ConvertMedia(userRegistrationVideoLocation, audioStream, "wav");
                audioStream.Position = 0;
                uploadFileToStorage(audioStream, userRegistrationAudioLocation);
                return string.Concat(storedFilePrefix, userRegistrationAudioLocation);
            }
            else
            {
                return string.Empty;
            }
        }

        private List<string> disintegrateVideoToImages(string userRegistrationVideoLocation)
        {
            List<string> userRegImageLocations = new List<string>();
            if (String.IsNullOrWhiteSpace(userRegistrationVideoLocation) == false)
            {
                var userRegistrationImageLocation = "userImage_#.jpg";
                var extractHelper = new NReco.VideoConverter.FFMpegConverter();
                List<float> frameLocations = new List<float>() { 0.3f, 1.0F, 1.5f};
                foreach (float frameLocation in frameLocations)
                {
                    using (Stream imageStream = new MemoryStream())
                    {
                        extractHelper.GetVideoThumbnail(userRegistrationVideoLocation, imageStream, frameLocation);
                        imageStream.Position = 0;
                        uploadFileToStorage(imageStream, userRegistrationImageLocation.Replace("#", frameLocations.IndexOf(frameLocation).ToString()));
                    }
                    userRegImageLocations.Add(string.Concat(storedFilePrefix, userRegistrationImageLocation.Replace("#", frameLocations.IndexOf(frameLocation).ToString())));
                }
            }
            return userRegImageLocations;
        }

        private static void uploadFileToStorage(Stream fileStream, string fileName)
        {
            // Create storagecredentials object by reading the values from the configuration (appsettings.json)
            StorageCredentials storageCredentials = new StorageCredentials("stoarageaccount", "586DxL4HL0ayGYspCRCRAEizJTLgm1z7t9wlBcBVxzvwXQ5aJ5SGzk9sQbjZ7HdetuY2lN9GRJbeVawSdOSg0Q==");

            // Create cloudstorage account by passing the storagecredentials
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
            CloudBlobContainer container = blobClient.GetContainerReference("clips");

            // Get the reference to the block blob from the container
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            // Upload the file
            blockBlob.UploadFromStream(fileStream);


        }

        private static byte[] ConvertWavTo16000Hz16BitMonoWav(byte[] inArray)
        {
            {
                using (var mem = new MemoryStream(inArray))
                {
                    using (var reader = new WaveFileReader(mem))
                    {
                        using (var converter = WaveFormatConversionStream.CreatePcmStream(reader))
                        {
                            using (var upsampler = new WaveFormatConversionStream(new WaveFormat(16000, 16, 1), converter))
                            {
                                byte[] data;
                                using (var m = new MemoryStream())
                                {
                                    upsampler.CopyTo(m);
                                    data = m.ToArray();
                                }
                                using (var m = new MemoryStream())
                                {
                                    // to create a propper WAV header (44 bytes), which begins with RIFF 
                                    var w = new WaveFileWriter(m, upsampler.WaveFormat);
                                    // append WAV data body
                                    w.Write(data, 0, data.Length);
                                    w.Dispose();
                                    return m.ToArray();

                                }
                            }
                        }
                    }
                }
            }
        }

        private string speechToText(Stream userAudio)
        {
            string speechInText = string.Empty;

            string requestURI = "https://speech.platform.bing.com/speech/recognition/interactive/cognitiveservices/v1?language=en-US&format=detailed";
            HttpWebRequest request = null;
            request = (HttpWebRequest)HttpWebRequest.Create(requestURI);
            request.SendChunked = true;
            request.Accept = @"application/json;text/xml";
            request.Method = "POST";
            request.ProtocolVersion = HttpVersion.Version11;
            request.ContentType = @"audio/wav; codec=audio/pcm; samplerate=16000";
            request.Headers["Ocp-Apim-Subscription-Key"] = BINGSPEECHAPIKEY;

            // Send an audio file by 1024 byte chunks
            using (FileStream fs = new FileStream(@"C:\Users\Sachin\Desktop\Recording.wav", FileMode.Open, FileAccess.Read))
            {

                /*
                * Open a request stream and write 1024 byte chunks in the stream one at a time.
                */
                byte[] buffer = null;
                int bytesRead = 0;
                using (Stream requestStream = request.GetRequestStream())
                {
                    /*
                    * Read 1024 raw bytes from the input audio file.
                    */
                    buffer = new Byte[checked((uint)Math.Min(1024, (int)fs.Length))];
                    while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        requestStream.Write(buffer, 0, bytesRead);
                    }

                    // Flush
                    requestStream.Flush();
                }
            }
            using (WebResponse response = request.GetResponse())
            {
                Console.WriteLine(((HttpWebResponse)response).StatusCode);

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    speechInText = sr.ReadToEnd();
                }
            }
            if (speechInText.ToLower().Contains("my name is") == true)
            {
                speechInText = speechInText.ToLower().Replace("my name is", "");
            }
            return speechInText;
        }
    }
}