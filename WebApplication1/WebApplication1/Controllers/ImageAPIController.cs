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

namespace WebApplication1.Controllers
{
    public class ImageAPIController : ApiController
    {
        const string IMAGE_SUBSCRIPTION_KEY = "d51fdfb00e354267bd91d120d92b2f70";
        const string IMAGE_OCP_AICP_SUBSCRIPTION_KEY = "Ocp-Apim-Subscription-Key";
        const string URI_BASE = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/detect";

        public Bitmap input_Face_Image;
        string image_File_Path = null;

        string personGroupId = "12111993133902018";
        string groupName = "Mastek_Digineers";


        const string FACE_API_ENDPOINT = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0";

        private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient(IMAGE_SUBSCRIPTION_KEY, FACE_API_ENDPOINT);


        string[] file_Paths;
        string[] directories;


        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }

        static async void MakeFaceDetectRequest(byte[] byteData)
        {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add(IMAGE_OCP_AICP_SUBSCRIPTION_KEY, IMAGE_SUBSCRIPTION_KEY);

            string requestParameters = "returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses,emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";

            string uri = URI_BASE + "?" + requestParameters;

            HttpResponseMessage response;

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json" and "multipart/form-data".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Execute the REST API call.
                response = await client.PostAsync(uri, content);

                // Get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();

                // Display the JSON response.
                dynamic dynObj = JsonConvert.DeserializeObject(contentString);

            }
        }


        private async Task<Face[]> UploadAndDetectFaces(string imageFilePath)
        {
            // The list of Face attributes to return.
            IEnumerable<FaceAttributeType> faceAttributes =
                new FaceAttributeType[] { FaceAttributeType.Gender, FaceAttributeType.Age, FaceAttributeType.Smile, FaceAttributeType.Emotion, FaceAttributeType.Glasses, FaceAttributeType.Hair };

            // Call the Face API.
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    Face[] faces = await faceServiceClient.DetectAsync(imageFileStream, returnFaceId: true, returnFaceLandmarks: false, returnFaceAttributes: faceAttributes);
                    return faces;
                }
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

        //    /api/ImageAPI/FaceDetect
        [HttpGet]
        [ActionName("FaceDetect")]
        public async void FaceDetect(string image_File_Path = "")
        {
            if (image_File_Path == "")
            {
                image_File_Path = @"C:\Users\Sachin\Desktop\GG.jpg";
            }

            byte[] byteData = GetImageAsByteArray(image_File_Path);

            //MakeFaceDetectRequest(byteData);

            Face[] faces = await UploadAndDetectFaces(image_File_Path);

        }


        //    /api/ImageAPI/CreateUserGroup
        [HttpGet]
        [ActionName("CreateUserGroup")]
        public async void CreateUserGroup()
        {
            try
            {
                try
                {
                    await faceServiceClient.DeletePersonGroupAsync(personGroupId);
                }
                catch (Exception e)
                {
                    string exception = e.ToString();
                }

                string folder_File_Path = "";
                if (folder_File_Path == "")
                {
                    folder_File_Path = @"C:\Users\Sachin13390\Desktop\Face_Data";
                }

                directories = Directory.GetDirectories(folder_File_Path);
                foreach (string directory in directories)
                {
                    file_Paths = Directory.GetFiles(directory);

                    await faceServiceClient.CreatePersonGroupAsync(personGroupId, groupName);

                    string personName = Path.GetFileName(directory);
                    CreatePersonResult person = await faceServiceClient.CreatePersonAsync(personGroupId, personName);
                    foreach (string imagePath in Directory.GetFiles(directory))
                    {
                        using (Stream imageStream = File.OpenRead(imagePath))
                        {
                            await faceServiceClient.AddPersonFaceAsync(personGroupId, person.PersonId, imageStream);
                        }
                    }

                    await Task.Delay(1000);
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
        }


        //    /api/ImageAPI/TrainUserGroup
        [HttpGet]
        [ActionName("TrainUserGroup")]
        public async void TrainUserGroup(string personGroupId = "")
        {
            try
            {
                if (personGroupId == "")
                {
                    personGroupId = this.personGroupId;
                }

                await faceServiceClient.TrainPersonGroupAsync(personGroupId);

                TrainingStatus trainingStatus = null;
                while (true)
                {
                    trainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupId);

                    if (trainingStatus.Status != Status.Running)
                    {
                        break;
                    }

                    await Task.Delay(1000);
                }

                //MessageBox.Show("Training successfully completed");
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        //    /api/ImageAPI/IdentifyUser
        [HttpGet]
        [ActionName("IdentifyUser")]
        public async void IdentifyUser(string personGroupId = "", string _imagePath = "")
        {

            if (personGroupId == "")
            {
                personGroupId = this.personGroupId;
            }

            if (_imagePath == "")
            {
                _imagePath = "";
            }

            try
            {
                Face[] faces = await UploadAndDetectFaces(_imagePath);
                var faceIds = faces.Select(face => face.FaceId).ToArray();

                var faceBitmap = new Bitmap(_imagePath);

                using (var g = Graphics.FromImage(faceBitmap))
                {

                    foreach (var identifyResult in await faceServiceClient.IdentifyAsync(personGroupId, faceIds))
                    {
                        if (identifyResult.Candidates.Length != 0)
                        {
                            var candidateId = identifyResult.Candidates[0].PersonId;
                            var person = await faceServiceClient.GetPersonAsync(personGroupId, candidateId);
                        }
                    }
                }

                //imgBox.Image = faceBitmap;
                //MessageBox.Show("Identification successfully completed");

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }

        }

        //    /api/ImageAPI/DUMMY
        [HttpPost]
        [ActionName("DUMMY")]
        public IHttpActionResult DUMMY(string folder_File_Path = "")
        {
            if (folder_File_Path == "")
            {
                folder_File_Path = @"C:\Users\Sachin13390\Desktop\Face_Data";
            }

            directories = Directory.GetDirectories(folder_File_Path);
            foreach (string directory in directories)
            {
                file_Paths = Directory.GetFiles(directory);
            }
            return Ok();

        }


        public void New_Enrollment_Get_Key()
        {
            
        }
    }
}
