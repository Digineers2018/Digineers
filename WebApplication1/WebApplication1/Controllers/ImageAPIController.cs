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

namespace WebApplication1.Controllers
{
    public class ImageAPIController : ApiController
    {
        const string IMAGE_SUBSCRIPTION_KEY = "d51fdfb00e354267bd91d120d92b2f70";
        const string IMAGE_OCP_AICP_SUBSCRIPTION_KEY = "Ocp-Apim-Subscription-Key";
        const string URI_BASE = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/detect";

        public Bitmap input_Face_Image;
        string image_File_Path = null;

        string _groupId = "12111993133902018";
        string _groupName = "Mastek_Digineers";


        const string FACE_API_ENDPOINT = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0";

        private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient(IMAGE_SUBSCRIPTION_KEY, FACE_API_ENDPOINT);


        string[] files;
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

        //    /api/ImageAPI/DUMMY
        [HttpPost]
        [ActionName("DUMMY")]
        public IHttpActionResult DUMMY(string folder_File_Path = "")
        {
            if (folder_File_Path == "")
            {
                folder_File_Path = @"C:\Users\Sachin\Desktop\Face_Data";
            }

            directories = Directory.GetDirectories(folder_File_Path);
            foreach (string directory in directories)
            {
                string s = directory;
            }
            return Ok();

        }


        public void New_Enrollment_Get_Key()
        {
            
        }
    }
}
