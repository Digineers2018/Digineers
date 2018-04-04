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
    public class UserIdentityControllerOld : ApiController
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

        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }

        private async Task<Face[]> UploadAndDetectFaces(Stream imageFileStream)
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

        private void CreateUserGroup()
        {
            // To use only once

            //try
            //{
            //    await faceServiceClient.DeletePersonGroupAsync(personGroupId);
            //}
            //catch (Exception e)
            //{
            //    string exception = e.ToString();
            //}

            //await faceServiceClient.CreatePersonGroupAsync(personGroupId, groupName);

        }

        private Stream GetImageStream(string imageFilePath)
        {
            Stream imageFileStream = File.OpenRead(imageFilePath);

            return imageFileStream;
        }


        //    /api/VoiceAPI/RegisterUser
        [HttpGet]
        [ActionName("RegisterUser")]
        public void RegisterUser()
        {
            #region TestBed
            //if (userAudioStream == null)
            //{
            //    string audioFilePath = @"C:\Users\Sachin\Desktop\VoiceSamples\brian.wav";
            //    userAudioStream = File.OpenRead(audioFilePath);
            //}
            //if (listUserImages == null)
            //{
            //    listUserImages = new List<Stream>();

            //    string directoryPath = @"C:\Users\Sachin13390\Desktop\Face_Data";

            //    string[] allDirectory = Directory.GetDirectories(directoryPath);

            //    foreach (string dir in allDirectory)
            //    {
            //        personName = Path.GetFileName(dir);
            //        string[] file_Paths = Directory.GetFiles(dir);
            //        foreach (string filePath in file_Paths)
            //        {
            //            listUserImages.Add(GetImageStream(filePath));
            //        }
            //    }
            //}
            #endregion

            try
            {
                Stream userAudioStream = disintegrateVideo(null);

                //var client = new HttpClient();
                //var queryString = HttpUtility.ParseQueryString(string.Empty);

                //// Request headers
                //client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", COGNITIVESPEECHAPIKEY);

                //var identificationProfileURI = "https://westus.api.cognitive.microsoft.com/spid/v1.0/identificationProfiles?" + queryString;
                //HttpResponseMessage response;

                //// Request body
                //byte[] byteData = Encoding.UTF8.GetBytes("{\"locale\":\"en-us\",}");
                //string profileId = string.Empty;
                //string operationUrl = string.Empty;
                //using (var content = new ByteArrayContent(byteData))
                //{
                //    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                //    response = await client.PostAsync(identificationProfileURI, content);
                //    var profileIdResponseData = await response.Content.ReadAsAsync<ExpandoObject>();
                //    if (profileIdResponseData.Count() > 0)
                //    {
                //        profileId = profileIdResponseData.First().Value.ToString();
                //    }
                //}
                //try
                //{
                //    if (string.IsNullOrEmpty(profileId) == false)
                //    {
                //        for (int enrolmentCount = 0; enrolmentCount < 3; enrolmentCount++)
                //        {

                //            client = new HttpClient();

                //            // Request headers
                //            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", COGNITIVESPEECHAPIKEY);
                //            // Request parameters
                //            queryString["shortAudio"] = "true";
                //            var enrollmentProfileUri = string.Format("https://westus.api.cognitive.microsoft.com/spid/v1.0/identificationProfiles/{0}/enroll?{1}", profileId, queryString);

                //            var content = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString("u"));
                //            Byte[] bytes;
                //            using (MemoryStream userAudioStreamRecvd = new MemoryStream())
                //            {
                //                userAudioStream.CopyTo(userAudioStreamRecvd);
                //                userAudioStream.Position = 0;
                //                bytes = ConvertWavTo16000Hz16BitMonoWav(userAudioStreamRecvd.ToArray());
                //                userAudioStreamRecvd.Position = 0;
                //            }

                //            Stream audioStream = new MemoryStream(bytes);
                //            content.Add(new StreamContent(audioStream), "Data", "testFile_" + DateTime.Now.ToString("u"));
                //            response = await client.PostAsync(enrollmentProfileUri, content).ConfigureAwait(false);
                //            if (response.StatusCode == HttpStatusCode.Accepted)
                //            {
                //                IEnumerable<string> operationLocation = response.Headers.GetValues("Operation-Location");
                //                if (operationLocation.Count() == 1)
                //                {
                //                    operationUrl = operationLocation.First();
                //                }
                //                else
                //                {
                //                    return FAILURE;
                //                }
                //            }

                //        }

                //        /////////// 3 STEP operationUrl
                //        ExpandoObject operationStatusResponseData = null;
                //        if (string.IsNullOrEmpty(operationUrl) == false)
                //        {
                //            client = new HttpClient();

                //            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", COGNITIVESPEECHAPIKEY);

                //            response = await client.GetAsync(operationUrl);
                //            operationStatusResponseData = await response.Content.ReadAsAsync<ExpandoObject>();
                //        }

                //        //#region Images
                //        //if (operationStatusResponseData != null && operationStatusResponseData.Count() > 0 && operationStatusResponseData.First().Value.ToString() == "SUCCEEDED" || operationStatusResponseData.First().Value.ToString() == "RUNNING")
                //        //{
                //        //    string personName = SpeechToText(userAudioStream);
                //        //    if (string.IsNullOrEmpty(personName) == false)
                //        //    {
                //        //        CreatePersonResult person = await faceServiceClient.CreatePersonInPersonGroupAsync(personGroupId, personName);

                //        //        foreach (Stream imageStream in listUserImages)
                //        //        {
                //        //            await faceServiceClient.AddPersonFaceInPersonGroupAsync(personGroupId, person.PersonId, imageStream);
                //        //        }

                //        //        await faceServiceClient.TrainPersonGroupAsync(personGroupId);

                //        //        TrainingStatus trainingStatus = null;
                //        //        while (true)
                //        //        {
                //        //            trainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupId);

                //        //            if (trainingStatus.Status != Microsoft.ProjectOxford.Face.Contract.Status.Running)
                //        //            {
                //        //                break;
                //        //            }
                //        //            await Task.Delay(1000);
                //        //        }
                //        //    }
                //        //}
                //        //#endregion

                //        return operationStatusResponseData.First().Value.ToString();
                //    }
            }
            catch (Exception enrollmentException)
            {

            }
            //return SUCCESSFULL;
        }

        //    /api/VoiceAPI/IndentifyUser
        [HttpGet]
        [ActionName("IndentifyUser")]
        public async Task<string> IndentifyUser(Stream userAudioStream, Stream imageStream)
        {
            try
            {
                // Get all Profiles

                string allProfileID = "";
                string matchedProfileID = "0be37623-ad65-45dc-ad42-8dd5b461d3c7";
                string operationUrl = string.Empty;
                #region TestBed
                //if (userAudioStream == null)
                //{
                //    string audioFilePath = @"C:\Users\Sachin\Desktop\Recording_2_.wav";
                //    userAudioStream = File.OpenRead(audioFilePath);
                //}
                //string imageFilePath = @"C:\Users\Sachin13390\Desktop\images.jpg";
                //imageStream = GetImageStream(imageFilePath);
                #endregion

                // Getting All Profile IDS

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
                    userAudioStream.CopyTo(userAudioStreamRecvd);
                    bytes = ConvertWavTo16000Hz16BitMonoWav(userAudioStreamRecvd.ToArray());
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

                #region Image
                Face[] faces = await UploadAndDetectFaces(imageStream);
                var faceIds = faces.Select(face => face.FaceId).ToArray();

                foreach (var identifyResult in await faceServiceClient.IdentifyAsync(personGroupId, faceIds))
                {
                    if (identifyResult.Candidates.Length != 0)
                    {
                        var candidateId = identifyResult.Candidates[0].PersonId;
                        var person = await faceServiceClient.GetPersonInPersonGroupAsync(personGroupId, candidateId);
                        return person.Name;
                    }
                }
                return USERCANNOTBEIDENTIFIED;
                #endregion

            }
            catch (Exception exception)
            {
                return FAILURE;
            }
        }

        //    /api/VoiceAPI/DeleteAllEnrolment
        [HttpGet]
        [ActionName("DeleteAllEnrolment")]
        public async Task<String> DeleteAllEnrolment(Stream userAudio)
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

            return "";
        }

        private string SpeechToText(Stream userAudio)
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
            if(speechInText.ToLower().Contains("my name is") == true)
            {
                speechInText = speechInText.ToLower().Replace("my name is", "");
            }
            return speechInText;
        }
        
    }
}
