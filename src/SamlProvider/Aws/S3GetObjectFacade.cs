using System;
using System.IO;
using System.Threading.Tasks;
using static System.Text.Json.JsonSerializer;

using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using Amazon.SecurityToken.Model;

namespace Cythral.CloudFormation.Resources.Aws
{
    public class S3GetObjectFacade
    {
        private S3Factory s3Factory = new S3Factory();
        private S3UriFactory s3UriFactory = new S3UriFactory();

        public virtual async Task<string> GetObject(string s3Uri, string? downloaderRoleArn = null)
        {
            var s3Client = await s3Factory.Create(downloaderRoleArn);
            var uri = s3UriFactory.Create(s3Uri);
            var getObjResponse = await s3Client.GetObjectAsync(new GetObjectRequest
            {
                BucketName = uri.Bucket,
                Key = uri.Key
            });

            StreamReader reader = new StreamReader(getObjResponse.ResponseStream);
            return reader.ReadToEnd();
        }
    }
}