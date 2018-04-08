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
using Newtonsoft.Json.Linq;

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

        //    /api/UserIdentity/ProcessRegistrationVideo
        [HttpPost]
        [ActionName("ProcessRegistrationVideo")]
        public async Task<String> ProcessRegistrationVideo()
        {
            Stream userRegisterVideo = null;
            userRegisterVideo = await this.Request.Content.ReadAsStreamAsync();
            userRegisterVideo.Position = 0;
            string speechText = string.Empty;
            if (userRegisterVideo == null)
            {
                #region  testbed
                
                #endregion
            }
            else
            {

                Stream userRegistrationAudio = null;
                uploadFileToStorage(userRegisterVideo, "userVideo.mp4");

                #region Disintegrate incoming Video Stream into Audio and Image

                userRegistrationAudio = disintegrateVideoToAudio(string.Concat(storedFilePrefix, "userVideo.mp4"));
                userRegistrationAudio.Position = 0;
                //userRegistrationAudio = downloadFileFromStorage(userRegistrationAudioFileName, userRegistrationAudio);

                //List<string> userRegistrationImageLocations = disintegrateVideoToImages(string.Concat(storedFilePrefix, "userVideo.mp4"));
                #endregion

                #region Register AudioProfile

                var client = new HttpClient();
                var queryString = HttpUtility.ParseQueryString(string.Empty);

                // Request headers
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", COGNITIVESPEECHAPIKEY);

                var identificationProfileURI = "https://westus.api.cognitive.microsoft.com/spid/v1.0/identificationProfiles?" + queryString;
                HttpResponseMessage response;

                // Request body
                byte[] byteData = Encoding.UTF8.GetBytes("{\"locale\":\"en-us\",}");
                string profileId = string.Empty;
                string operationUrl = string.Empty;
                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await client.PostAsync(identificationProfileURI, content);
                    var profileIdResponseData = await response.Content.ReadAsAsync<ExpandoObject>();
                    if (profileIdResponseData.Count() > 0)
                    {
                        profileId = profileIdResponseData.First().Value.ToString();
                    }
                }
                try
                {
                    if (string.IsNullOrEmpty(profileId) == false)
                    {
                        for (int enrolmentCount = 0; enrolmentCount < 3; enrolmentCount++)
                        {

                            client = new HttpClient();

                            // Request headers
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", COGNITIVESPEECHAPIKEY);
                            // Request parameters
                            queryString["shortAudio"] = "true";
                            var enrollmentProfileUri = string.Format("https://westus.api.cognitive.microsoft.com/spid/v1.0/identificationProfiles/{0}/enroll?{1}", profileId, queryString);

                            var content = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString("u"));
                            Byte[] bytes;
                            using (MemoryStream userAudioStreamRecvd = new MemoryStream())
                            {
                                userRegistrationAudio.CopyTo(userAudioStreamRecvd);
                                userRegistrationAudio.Position = 0;
                                bytes = ConvertWavTo16000Hz16BitMonoWav(userAudioStreamRecvd.ToArray());
                                userAudioStreamRecvd.Position = 0;
                            }

                            Stream audioStream = new MemoryStream(bytes);
                            content.Add(new StreamContent(audioStream), "Data", "testFile_" + DateTime.Now.ToString("u"));
                            response = await client.PostAsync(enrollmentProfileUri, content).ConfigureAwait(false);
                            if (response.StatusCode == HttpStatusCode.Accepted)
                            {
                                IEnumerable<string> operationLocation = response.Headers.GetValues("Operation-Location");
                                if (operationLocation.Count() == 1)
                                {
                                    operationUrl = operationLocation.First();
                                }
                                else
                                {
                                    return FAILURE;
                                }
                            }

                        }

                        /////////// 3 STEP operationUrl
                        ExpandoObject operationStatusResponseData = null;
                        if (string.IsNullOrEmpty(operationUrl) == false)
                        {
                            client = new HttpClient();

                            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", COGNITIVESPEECHAPIKEY);

                            response = await client.GetAsync(operationUrl);
                            operationStatusResponseData = await response.Content.ReadAsAsync<ExpandoObject>();
                        }
                    }
                }
                catch(Exception ex)
                {
                    return ex.Message;
                }

                #endregion

                #region Images
                //if (operationStatusResponseData != null && operationStatusResponseData.Count() > 0 && operationStatusResponseData.First().Value.ToString() == "SUCCEEDED" || operationStatusResponseData.First().Value.ToString() == "RUNNING")
                //{
                //    string personName = SpeechToText(userAudioStream);
                //    if (string.IsNullOrEmpty(personName) == false)
                //    {
                //        CreatePersonResult person = await faceServiceClient.CreatePersonInPersonGroupAsync(personGroupId, personName);

                //        foreach (Stream imageStream in listUserImages)
                //        {
                //            await faceServiceClient.AddPersonFaceInPersonGroupAsync(personGroupId, person.PersonId, imageStream);
                //        }

                //        await faceServiceClient.TrainPersonGroupAsync(personGroupId);

                //        TrainingStatus trainingStatus = null;
                //        while (true)
                //        {
                //            trainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupId);

                //            if (trainingStatus.Status != Microsoft.ProjectOxford.Face.Contract.Status.Running)
                //            {
                //                break;
                //            }
                //            await Task.Delay(1000);
                //        }
                //    }
                //}
                #endregion

                userRegistrationAudio.Position = 0;
                speechText = speechToText(userRegistrationAudio);
            }
            return speechText;
        }

        //    /api/UserIdentity/DeleteAllVoiceRegistrations
        [HttpGet]
        [ActionName("DeleteAllVoiceRegistrations")]
        public async Task<String> DeleteAllVoiceRegistrations()
        {
            try
            {
                var client = new HttpClient();
                string allProfileID = "";
                var queryString = HttpUtility.ParseQueryString(string.Empty);

                // Request headers
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", COGNITIVESPEECHAPIKEY);

                var enrollmentVerifyProfileUri = "https://westus.api.cognitive.microsoft.com/spid/v1.0/identificationProfiles?";

                var response = await client.GetAsync(enrollmentVerifyProfileUri);

                var profileIdResponseData = await response.Content.ReadAsAsync<List<Object>>();

                foreach (var foundProfileID in profileIdResponseData)
                {
                    allProfileID = (foundProfileID as Newtonsoft.Json.Linq.JObject)["identificationProfileId"].ToString();

                    client = new HttpClient();

                    // Request headers
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", COGNITIVESPEECHAPIKEY);

                    var uri = "https://westus.api.cognitive.microsoft.com/spid/v1.0/identificationProfiles/" + allProfileID;

                    response = await client.DeleteAsync(uri);
                    var deleteOperationStatus = await response.Content.ReadAsAsync<ExpandoObject>();
                }
                return "Success";
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
            
        }

        //    /api/UserIdentity/ProcessIdentificationVideo
        [HttpPost]
        [ActionName("ProcessIdentificationVideo")]
        public async Task<String> ProcessIdentificationVideo()
        {
            Stream userRegisterVideo = null;
            userRegisterVideo = await this.Request.Content.ReadAsStreamAsync();
            userRegisterVideo.Position = 0;
            string speechText = string.Empty;
            if (userRegisterVideo == null)
            {
                #region  testbed

                #endregion
            }
            else
            {
                Stream userRegistrationAudio = null;
                uploadFileToStorage(userRegisterVideo, "userVideo.mp4");
                #region Disintegrate incoming Video Stream into Audio and Image

                userRegistrationAudio = disintegrateVideoToAudio(string.Concat(storedFilePrefix, "userVideo.mp4"));
                userRegistrationAudio.Position = 0;
                //userRegistrationAudio = downloadFileFromStorage(userRegistrationAudioFileName, userRegistrationAudio);

                //List<string> userRegistrationImageLocations = disintegrateVideoToImages(string.Concat(storedFilePrefix, "userVideo.mp4"));
                #endregion

                #region Identify Profile

                string allProfileID = "";
                string operationUrl = string.Empty;
                try
                {
                    var client = new HttpClient();
                    var queryString = HttpUtility.ParseQueryString(string.Empty);

                    // Request headers
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", COGNITIVESPEECHAPIKEY);

                    var enrollmentVerifyProfileUri = "https://westus.api.cognitive.microsoft.com/spid/v1.0/identificationProfiles?" + queryString;

                    var response = await client.GetAsync(enrollmentVerifyProfileUri);

                    var profileIdResponseData = await response.Content.ReadAsAsync<List<Object>>();

                    foreach (var foundProfileID in profileIdResponseData)
                    {
                        allProfileID = allProfileID + "," + (foundProfileID as Newtonsoft.Json.Linq.JObject)["identificationProfileId"].ToString();
                    }

                    allProfileID = allProfileID.Substring(1);

                    client = new HttpClient();
                    queryString = HttpUtility.ParseQueryString(string.Empty);

                    // Request headers
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", COGNITIVESPEECHAPIKEY);

                    // Request parameters
                    queryString["shortAudio"] = "true";
                    enrollmentVerifyProfileUri = string.Format("https://westus.api.cognitive.microsoft.com/spid/v1.0/identify?identificationProfileIds={0}&{1}", allProfileID, queryString);

                    var content = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString("u"));

                    Byte[] bytes;
                    using (MemoryStream userAudioStreamRecvd = new MemoryStream())
                    {
                        userRegistrationAudio.CopyTo(userAudioStreamRecvd);
                        userRegistrationAudio.Position = 0;
                        bytes = ConvertWavTo16000Hz16BitMonoWav(userAudioStreamRecvd.ToArray());
                        userAudioStreamRecvd.Position = 0;
                    }
                    Stream audioStream = new MemoryStream(bytes);

                    content.Add(new StreamContent(audioStream), "Data", "testFile_" + DateTime.Now.ToString("u"));
                    response = await client.PostAsync(enrollmentVerifyProfileUri, content).ConfigureAwait(false);
                    if (response.StatusCode == HttpStatusCode.Accepted)
                    {
                        IEnumerable<string> operationLocation = response.Headers.GetValues("Operation-Location");
                        if (operationLocation.Count() == 1)
                        {
                            operationUrl = operationLocation.First();
                        }
                        else
                        {
                            return FAILURE;
                        }
                    }

                    if (string.IsNullOrEmpty(operationUrl) == false)
                    {
                        client = new HttpClient();

                        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", COGNITIVESPEECHAPIKEY);

                        response = await client.GetAsync(operationUrl);
                        var operationStatusResponseData = await response.Content.ReadAsAsync<ExpandoObject>();
                        return operationStatusResponseData.First().Value.ToString();
                    }
                    userRegistrationAudio.Position = 0;
                    speechText = speechToText(userRegistrationAudio);

                    #region Image
                    //Face[] faces = await uploadAndDetectFaces(imageStream);
                    //var faceIds = faces.Select(face => face.FaceId).ToArray();

                    //foreach (var identifyResult in await faceServiceClient.IdentifyAsync(personGroupId, faceIds))
                    //{
                    //    if (identifyResult.Candidates.Length != 0)
                    //    {
                    //        var candidateId = identifyResult.Candidates[0].PersonId;
                    //        var person = await faceServiceClient.GetPersonInPersonGroupAsync(personGroupId, candidateId);
                    //        return person.Name;
                    //    }
                    //}
                    //return USERCANNOTBEIDENTIFIED;
                    #endregion


                }
                catch (Exception ex)
                {
                    speechText = ex.Message;
                }

                #endregion
            }
            return speechText;
        }


        //    /api/UserIdentity/ProcessRegistrationVideoTest
        [HttpPost]
        [ActionName("ProcessRegistrationVideoTest")]
        public String ProcessRegistrationVideoTest()
        {
            string speechText = string.Empty;
            Stream userRegisterVideo = null;
            if (userRegisterVideo == null)
            {
                #region  testbed
                string videoFilePath = @"https://stoarageaccount.blob.core.windows.net/clips/Reg.mp4";
                var userRegistrationAudioLocation = disintegrateVideoToAudioTest(videoFilePath);

                //List<string> userRegistrationImageLocations = disintegrateVideoToImages(videoFilePath);

                Stream userRegistrationAudio = new MemoryStream();
                userRegistrationAudio = downloadFileFromStorage(userRegistrationAudioLocation, userRegistrationAudio);
                userRegistrationAudio.Position = 0;
                speechText = speechToText(userRegistrationAudio);
                #endregion
            }
            else
            {
                ////uploadFileToStorage(userRegisterVideo, "userVideo.mp4");
                //var userRegistrationAudioLocation = disintegrateVideoToAudio(string.Concat(storedFilePrefix, "userVideo.mp4"));
                //List<string> userRegistrationImageLocations = disintegrateVideoToImages(string.Concat(storedFilePrefix, "userVideo.mp4"));
                //Stream userRegistrationAudio = null;
                //userRegistrationAudio = downloadFileFromStorage("https://stoarageaccount.blob.core.windows.net/clips/VID_20180403_153456090.mp4", userRegistrationAudio);
                //userRegistrationAudio.Position = 0;
                //var textSpoken = speechToText(userRegistrationAudio);
            }
            return speechText;
        }

        private Stream disintegrateVideoToAudio(string userRegistrationVideoLocation)
        {
            if (String.IsNullOrWhiteSpace(userRegistrationVideoLocation) == false)
            {
                //var userRegistrationAudioFileName = "userAudio.wav";
                var extractHelper = new NReco.VideoConverter.FFMpegConverter();
                Stream audioStream = new MemoryStream();
                extractHelper.ConvertMedia(userRegistrationVideoLocation, audioStream, "wav");
                audioStream.Position = 0;
                //uploadFileToStorage(audioStream, userRegistrationAudioFileName);
                //extractHelper.Abort();
                return audioStream;
            }
            else
            {
                return null;
            }
        }

        private string disintegrateVideoToAudioTest(string userRegistrationVideoLocation)
        {
            if (String.IsNullOrWhiteSpace(userRegistrationVideoLocation) == false)
            {
                var userRegistrationAudioLocation = "userAudioTest.wav";
                var extractHelper = new NReco.VideoConverter.FFMpegConverter();
                Stream audioStream = new MemoryStream();
                extractHelper.ConvertMedia(userRegistrationVideoLocation, audioStream, "wav");
                audioStream.Position = 0;
                uploadFileToStorage(audioStream, userRegistrationAudioLocation);
                extractHelper.Abort();
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
                var extractHelper = new NReco.VideoConverter.FFMpegConverter();
                Stream imageStream = new MemoryStream() { Position = 0 };
                extractHelper.GetVideoThumbnail(userRegistrationVideoLocation, imageStream, 0);
                imageStream.Position = 0;
                uploadFileToStorage(imageStream, "userImage.jpg");
                //var userRegistrationImageLocation = "userImage_#.jpg";
                //var extractHelper = new NReco.VideoConverter.FFMpegConverter();
                //List<float> frameLocations = new List<float>() { 0.3f, 1.0F, 1.5f};
                //foreach (float frameLocation in frameLocations)
                //{
                //    using (Stream imageStream = new MemoryStream())
                //    {
                //        extractHelper.GetVideoThumbnail(userRegistrationVideoLocation, imageStream, frameLocation);
                //        imageStream.Position = 0;
                //        uploadFileToStorage(imageStream, userRegistrationImageLocation.Replace("#", frameLocations.IndexOf(frameLocation).ToString()));
                //    }
                //    userRegImageLocations.Add(string.Concat(storedFilePrefix, userRegistrationImageLocation.Replace("#", frameLocations.IndexOf(frameLocation).ToString())));
                //}
            }
            return userRegImageLocations;
        }

        private static void uploadFileToStorage(Stream fileStream, string fileName)
        {
            try
            {
                // Create storagecredentials object by reading the values from the configuration (appsettings.json)
                StorageCredentials storageCredentials = new StorageCredentials("stoarageaccount", "586DxL4HL0ayGYspCRCRAEizJTLgm1z7t9wlBcBVxzvwXQ5aJ5SGzk9sQbjZ7HdetuY2lN9GRJbeVawSdOSg0Q==");

                // Create cloudstorage account by passing the storagecredentials
                CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

                // Create the blob client.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
                CloudBlobContainer container = blobClient.GetContainerReference("clips");
                container.CreateIfNotExists();

                // Get the reference to the block blob from the container
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

                // Upload the file
                blockBlob.UploadFromStream(fileStream);
            }
            catch(Exception uploadException)
            {

            }
        }

        private static Stream downloadFileFromStorage(string fileName, Stream downloadedStream)
        {
            fileName = fileName.Replace(storedFilePrefix, "");
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

            // Download to stream
            
            blockBlob.DownloadToStream(downloadedStream);
            downloadedStream.Position = 0;
            return downloadedStream;
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
            Byte[] bytes;
            using (MemoryStream userAudioStreamRecvd = new MemoryStream())
            {
                userAudioStreamRecvd.Position = 0;
                userAudio.CopyTo(userAudioStreamRecvd);

                bytes = ConvertWavTo16000Hz16BitMonoWav(userAudioStreamRecvd.ToArray());
            }

            userAudio = new MemoryStream(bytes);
            userAudio.Position = 0;

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


            byte[] buffer = null;
            int bytesRead = 0;
            using (Stream requestStream = request.GetRequestStream())
            {
                /*
                * Read 1024 raw bytes from the input audio file.
                */
                buffer = new Byte[checked((uint)Math.Min(1024, (int)userAudio.Length))];
                while ((bytesRead = userAudio.Read(buffer, 0, buffer.Length)) != 0)
                {
                    requestStream.Write(buffer, 0, bytesRead);
                }
                // Flush
                requestStream.Flush();
            }

            using (WebResponse response = request.GetResponse())
            {
                Console.WriteLine(((HttpWebResponse)response).StatusCode);

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    speechInText = sr.ReadToEnd();
                }
            }

            JObject json = JObject.Parse(speechInText);
            string text = json["NBest"].Children().First()["Display"].ToString();
            List<string> registrationUserAudioText = text.Split(' ').ToList();
            return string.Format("{0} {1}", registrationUserAudioText[registrationUserAudioText.IndexOf("is") + 1], registrationUserAudioText[registrationUserAudioText.IndexOf("is") + 2]);
        }

        private async Task<Face[]> uploadAndDetectFaces(Stream imageFileStream)
        {
            // The list of Face attributes to return.
            IEnumerable<FaceAttributeType> faceAttributes =
                new FaceAttributeType[] { FaceAttributeType.Gender, FaceAttributeType.Age, FaceAttributeType.Smile, FaceAttributeType.Emotion, FaceAttributeType.Glasses, FaceAttributeType.Hair };

            // Call the Face API.
            try
            {
                Face[] faces = await faceServiceClient.DetectAsync(imageFileStream, returnFaceId: true, returnFaceLandmarks: false, returnFaceAttributes: faceAttributes);
                return faces;
            }
            // Catch and display Face API errors.
            catch (FaceAPIException f)
            {
                //MessageBox.Show(f.ErrorMessage, f.ErrorCode);
                return new Face[0];
            }
            // Catch and display all other errors.
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Error");
                return new Face[0];
            }
        }

    }
}