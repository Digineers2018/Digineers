using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace WebApplication1.Controllers
{
    public class TableStorageController : ApiController
    {
        const string TABLECONNECTIONSTRING = "DefaultEndpointsProtocol=https;AccountName=stoarageaccount;AccountKey=586DxL4HL0ayGYspCRCRAEizJTLgm1z7t9wlBcBVxzvwXQ5aJ5SGzk9sQbjZ7HdetuY2lN9GRJbeVawSdOSg0Q==;EndpointSuffix=core.windows.net";

        //    /api/VoiceAPI/RegisterUser
        [HttpGet]
        [ActionName("RegisterUser")]
        public void RegisterUser()
        {
            CloudStorageAccount csa = CloudStorageAccount.Parse(TABLECONNECTIONSTRING);

            CloudTableClient tableClient = csa.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("person");

            // Create the table if it doesn't exist.
            table.CreateIfNotExists();
        }

    }


    public class PersonEntity : TableEntity
    {
        public PersonEntity(string lastName, string firstName)
        {
            this.PartitionKey = lastName;
            this.RowKey = firstName;
        }

        public PersonEntity() { }

        public string FaceProfileID { get; set; }

        public string AudioProfileID { get; set; }
    }
}
