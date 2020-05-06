using System.Threading.Tasks;

using Amazon.S3;
using Amazon.S3.Util;
using Amazon.IdentityManagement;
using Amazon.SecurityToken.Model;

namespace Cythral.CloudFormation.Resources.Aws
{
    public class S3UriFactory
    {
        public virtual AmazonS3Uri Create(string uri)
        {
            return new AmazonS3Uri(uri);
        }
    }
}